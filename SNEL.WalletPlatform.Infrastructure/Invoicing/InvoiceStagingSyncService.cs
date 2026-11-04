using System.Diagnostics;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SNEL.WalletPlatform.Application.Invoicing;
using SNEL.WalletPlatform.Application.Invoicing.Interfaces;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Jobs;
using SNEL.WalletPlatform.Infrastructure.Persistence;

namespace SNEL.WalletPlatform.Infrastructure.Invoicing;

/// <summary>
/// Reads Oracle <see cref="InvoiceStagingEntity"/> rows and writes projected invoice payloads to
/// <see cref="InvoiceSecondaryStagingEntity"/>. New invoices: <see cref="InvoiceSecondaryStagingEntity.Updated"/> false.
/// Existing invoices: <see cref="InvoiceSecondaryStagingEntity.Updated"/> true only when staging
/// <see cref="InvoiceStagingEntity.EffectiveInvoiceUpdateDate"/> is greater than <see cref="InvoiceEntity.InvoiceUpdateDate"/>.
/// <see cref="InvoiceSecondaryStagingEntity.PaymentMismatchFlag"/> follows the same rule as the former direct sync:
/// staging paid + paid POI amount vs invoice total (existing invoice), or staging paid vs staging total (new invoice).
/// Invoice-shaped fields are derived from <see cref="InvoiceStagingEntity"/> (not from <see cref="InvoiceEntity"/> mapping).
/// After secondary rows are written, applies them to <see cref="InvoiceEntity"/> (SQL on PostgreSQL; EF path for InMemory tests).
/// </summary>
public sealed class InvoiceStagingSyncService : IInvoiceStagingSyncService
{
    private const int BatchSize = 10000;
    private const int ApplyBatchSize = 20000;
    private const int StagingDeleteBatchSize = 100000;
    private const string SystemActor = "SYSTEM";

    /// <summary>Biller code used to resolve the default BillingService for Oracle-synced invoices (e.g. SNEL).</summary>
    private const string OracleSyncBillerCode = "SNEL";

    /// <summary>Billing service code under the biller for Oracle-synced invoices (e.g. SNEL).</summary>
    private const string OracleSyncBillingServiceCode = "SNEL";

    private readonly ApplicationDbContext _db;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InvoiceStagingSyncService> _logger;

    public InvoiceStagingSyncService(
        ApplicationDbContext db,
        IUnitOfWork unitOfWork,
        ILogger<InvoiceStagingSyncService> logger)
    {
        _db = db;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SyncInvoicesFromStagingAsync(
        string jobKey,
        string sourceSystem,
        bool registerNotificationOnPull = false,
        TimeSpan? notificationTriggerTime = null)
    {
        if (string.IsNullOrWhiteSpace(jobKey))
            throw new ArgumentException("jobKey must be non-empty.", nameof(jobKey));
        if (string.IsNullOrWhiteSpace(sourceSystem))
            throw new ArgumentException("sourceSystem must be non-empty.", nameof(sourceSystem));

        var updated = 0;
        var inserted = 0;
        var skipped = 0;
        var skippedNoChange = 0;
        var skippedEmptyRef = 0;
        var skippedEmptyCurrency = 0;
        var skippedCurrencyNotFound = 0;
        var skippedDuplicateInBatch = 0;
        var batchNumber = 0;
        var processedStagingRows = 0;
        Guid? cursor = null;

        var syncStopwatch = Stopwatch.StartNew();
        _logger.LogInformation(
            "InvoiceStagingSyncService: Stage=Started. JobKey={JobKey}, SourceSystem={SourceSystem}, BatchSize={BatchSize}, RegisterNotificationOnPull={RegisterNotificationOnPull}, NotificationTriggerTime={NotificationTriggerTime}.",
            jobKey, sourceSystem, BatchSize, registerNotificationOnPull, notificationTriggerTime);

        await ClearSecondaryStagingForJobAsync(jobKey);

        _logger.LogInformation(
            "InvoiceStagingSyncService: Stage=LoadCurrencies started. JobKey={JobKey}, SourceSystem={SourceSystem}.",
            jobKey, sourceSystem);
        var currencies = await _db.Currencies
            .AsNoTracking()
            .ToListAsync();
        var currencyByAbbreviationOrCode = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in currencies)
        {
            if (!string.IsNullOrEmpty(c.Abbreviation))
                currencyByAbbreviationOrCode[c.Abbreviation] = c.Id;
            if (!string.IsNullOrEmpty(c.Code))
                currencyByAbbreviationOrCode[c.Code] = c.Id;
        }

        var defaultCurrencyId = currencies.Count > 0 ? currencies[0].Id : 0;
        _logger.LogInformation(
            "InvoiceStagingSyncService: Stage=LoadCurrencies completed. JobKey={JobKey}, SourceSystem={SourceSystem}, CurrencyCount={CurrencyCount}, CurrencyLookupKeyCount={CurrencyLookupKeyCount}, DefaultCurrencyId={DefaultCurrencyId}.",
            jobKey, sourceSystem, currencies.Count, currencyByAbbreviationOrCode.Count, defaultCurrencyId);

        long? defaultBillingServiceId = null;

        while (true)
        {
            batchNumber++;
            var batchStopwatch = Stopwatch.StartNew();
            _logger.LogInformation(
                "InvoiceStagingSyncService: Stage=LoadPrimaryStagingBatch started. JobKey={JobKey}, SourceSystem={SourceSystem}, BatchNumber={BatchNumber}, Cursor={Cursor}.",
                jobKey, sourceSystem, batchNumber, cursor);
            var query = _db.InvoiceStagings
                .AsNoTracking()
                .Where(stagingRow => stagingRow.JobKey == jobKey && stagingRow.SourceSystem == sourceSystem)
                .Where(stagingRow => !string.IsNullOrEmpty(stagingRow.InvoiceNum));
            if (cursor.HasValue)
                query = query.Where(stagingRow => stagingRow.Id.CompareTo(cursor.Value) > 0);
            var stagingBatch = await query
                .OrderBy(stagingRow => stagingRow.Id)
                .Take(BatchSize)
                .ToListAsync();

            if (stagingBatch.Count == 0)
            {
                _logger.LogInformation(
                    "InvoiceStagingSyncService: Stage=LoadPrimaryStagingBatch completed with no rows. JobKey={JobKey}, SourceSystem={SourceSystem}, BatchNumber={BatchNumber}, ProcessedStagingRows={ProcessedStagingRows}.",
                    jobKey, sourceSystem, batchNumber, processedStagingRows);
                break;
            }

            processedStagingRows += stagingBatch.Count;
            _logger.LogInformation(
                "InvoiceStagingSyncService: Stage=LoadPrimaryStagingBatch completed. JobKey={JobKey}, SourceSystem={SourceSystem}, BatchNumber={BatchNumber}, RowsLoaded={RowsLoaded}, ProcessedStagingRows={ProcessedStagingRows}, ElapsedMs={ElapsedMs}.",
                jobKey, sourceSystem, batchNumber, stagingBatch.Count, processedStagingRows,
                batchStopwatch.ElapsedMilliseconds);

            var distinctInvoiceRefs = stagingBatch
                .Select(stagingRow => (stagingRow.InvoiceNum ?? "").Trim().ToLower())
                .Where(invoiceRef => !string.IsNullOrEmpty(invoiceRef))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var loadExistingInvoicesStopwatch = Stopwatch.StartNew();
            _logger.LogInformation(
                "InvoiceStagingSyncService: Stage=LoadExistingInvoices started. JobKey={JobKey}, SourceSystem={SourceSystem}, BatchNumber={BatchNumber}, DistinctInvoiceRefs={DistinctInvoiceRefs}.",
                jobKey, sourceSystem, batchNumber, distinctInvoiceRefs.Count);
            var invoices = distinctInvoiceRefs.Count == 0
                ? []
                : await _db.Invoices
                    .AsNoTracking()
                    .Where(invoice => distinctInvoiceRefs.Contains(invoice.InvoiceRef.ToLower()))
                    .ToListAsync();

            var invoicesByRef = invoices.ToDictionary(invoice => invoice.InvoiceRef, invoice => invoice,
                StringComparer.OrdinalIgnoreCase);
            loadExistingInvoicesStopwatch.Stop();
            _logger.LogInformation(
                "InvoiceStagingSyncService: Stage=LoadExistingInvoices completed. JobKey={JobKey}, SourceSystem={SourceSystem}, BatchNumber={BatchNumber}, ExistingInvoices={ExistingInvoices}, ElapsedMs={ElapsedMs}.",
                jobKey, sourceSystem, batchNumber, invoices.Count,
                loadExistingInvoicesStopwatch.ElapsedMilliseconds);

            var secondaryBatch = new List<InvoiceSecondaryStagingEntity>();
            var batchRefs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var invoiceIds = invoices.Select(invoice => invoice.Id).ToList();
            _logger.LogInformation(
                "InvoiceStagingSyncService: Stage=LoadPaidPaymentOrderTotals started. JobKey={JobKey}, SourceSystem={SourceSystem}, BatchNumber={BatchNumber}, ExistingInvoiceIds={ExistingInvoiceIds}.",
                jobKey, sourceSystem, batchNumber, invoiceIds.Count);
            var paidPoiTotalByInvoiceId = invoiceIds.Count > 0
                ? (await _unitOfWork.PaymentOrderItemRepository
                    .GetAllQueryable(poi =>
                        poi.InvoiceId != null && invoiceIds.Contains(poi.InvoiceId.Value) &&
                        poi.Status == PaymentOrderItemStatus.Paid)
                    .Select(poi => new { poi.InvoiceId, poi.Amount })
                    .ToListAsync())
                .Where(poi => poi.InvoiceId.HasValue)
                .GroupBy(poi => poi.InvoiceId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(poi => poi.Amount))
                : new Dictionary<Guid, decimal>();
            _logger.LogInformation(
                "InvoiceStagingSyncService: Stage=LoadPaidPaymentOrderTotals completed. JobKey={JobKey}, SourceSystem={SourceSystem}, BatchNumber={BatchNumber}, InvoiceTotalsLoaded={InvoiceTotalsLoaded}.",
                jobKey, sourceSystem, batchNumber, paidPoiTotalByInvoiceId.Count);

            var updatedBeforeBatch = updated;
            var insertedBeforeBatch = inserted;
            var skippedBeforeBatch = skipped;
            _logger.LogInformation(
                "InvoiceStagingSyncService: Stage=ProjectSecondaryStaging started. JobKey={JobKey}, SourceSystem={SourceSystem}, BatchNumber={BatchNumber}, RowsToProject={RowsToProject}.",
                jobKey, sourceSystem, batchNumber, stagingBatch.Count);

            foreach (var stagingRow in stagingBatch)
            {
                var invoiceRef = (stagingRow.InvoiceNum ?? "").Trim();
                if (string.IsNullOrEmpty(invoiceRef))
                {
                    skipped++;
                    skippedEmptyRef++;
                    continue;
                }

                invoicesByRef.TryGetValue(invoiceRef, out var invoice);

                if (invoice != null)
                {
                    if (!stagingRow.EffectiveInvoiceUpdateDate.HasValue)
                    {
                        skipped++;
                        skippedNoChange++;
                        continue;
                    }

                    if (!StagingEffectiveIsNewerThanInvoiceBusinessUpdate(
                            stagingRow.EffectiveInvoiceUpdateDate.Value,
                            invoice.InvoiceUpdateDate))
                    {
                        skipped++;
                        skippedNoChange++;
                        continue;
                    }

                    var currencyCode = (stagingRow.InvoiceCurrencyCode ?? "").Trim();
                    if (string.IsNullOrEmpty(currencyCode))
                    {
                        skipped++;
                        skippedEmptyCurrency++;
                        continue;
                    }

                    if (!currencyByAbbreviationOrCode.TryGetValue(currencyCode, out var resolvedCurrencyId))
                    {
                        if (defaultCurrencyId == 0)
                        {
                            _logger.LogWarning(
                                "Currency not found for code {Code} and no default currency in DB, skipping invoice {InvoiceRef}.",
                                currencyCode, stagingRow.InvoiceNum);
                            skipped++;
                            skippedCurrencyNotFound++;
                            continue;
                        }

                        _logger.LogWarning(
                            "Currency not found for code {Code}, using default currency Id={CurrencyId} for invoice {InvoiceRef}.",
                            currencyCode, defaultCurrencyId, stagingRow.InvoiceNum);
                        resolvedCurrencyId = defaultCurrencyId;
                    }

                    var isUsd = string.Equals(stagingRow.InvoiceCurrencyCode, "USD", StringComparison.OrdinalIgnoreCase);
                    var totalPaidFromStaging = isUsd ? (stagingRow.PaidUsd ?? 0) : (stagingRow.PaidFc ?? 0);
                    var paidPoiTotal = paidPoiTotalByInvoiceId.TryGetValue(invoice.Id, out var paidSum) ? paidSum : 0;
                    var overPaid = totalPaidFromStaging + paidPoiTotal > invoice.AmountTotal;

                    var invoiceInfoJson = OracleInvoiceMapper.FromStaging(stagingRow).ToInvoiceInfoJson();
                    secondaryBatch.Add(InvoiceSecondaryStagingEntity.FromInvoiceStaging(
                        stagingRow,
                        invoice.BillingServiceId,
                        resolvedCurrencyId,
                        jobKey,
                        updated: true,
                        overPaid,
                        invoiceInfoJson));
                    updated++;
                }
                else
                {
                    if (defaultBillingServiceId == null)
                    {
                        var normalizedBillerCode = OracleSyncBillerCode.Trim().ToLowerInvariant();
                        var normalizedServiceCode = OracleSyncBillingServiceCode.Trim().ToLowerInvariant();

                        var billerId = await _db.Billers
                            .AsNoTracking()
                            .Where(b => b.Code.ToLower() == normalizedBillerCode)
                            .Select(b => (long?)b.Id)
                            .FirstOrDefaultAsync();

                        if (!billerId.HasValue)
                        {
                            throw new InvalidOperationException(
                                $"Biller not found for Oracle invoice sync. Ensure a Biller exists with Code='{OracleSyncBillerCode}'.");
                        }

                        var defaultBillingService = await _db.BillingServices
                            .AsNoTracking()
                            .Where(bs => bs.BillerId == billerId.Value && bs.Code.ToLower() == normalizedServiceCode)
                            .Select(bs => new { bs.Id })
                            .FirstOrDefaultAsync();

                        if (defaultBillingService == null)
                        {
                            throw new InvalidOperationException(
                                $"BillingService not found for Oracle invoice sync. Ensure a BillingService exists with Biller.Code='{OracleSyncBillerCode}' and BillingService.Code='{OracleSyncBillingServiceCode}'.");
                        }

                        defaultBillingServiceId = defaultBillingService.Id;
                    }

                    var currencyCode = (stagingRow.InvoiceCurrencyCode ?? "").Trim();
                    if (string.IsNullOrEmpty(currencyCode))
                    {
                        skipped++;
                        skippedEmptyCurrency++;
                        continue;
                    }

                    if (!currencyByAbbreviationOrCode.TryGetValue(currencyCode, out var currencyId))
                    {
                        if (defaultCurrencyId == 0)
                        {
                            _logger.LogWarning(
                                "Currency not found for code {Code} and no default currency in DB, skipping invoice {InvoiceRef}.",
                                currencyCode, stagingRow.InvoiceNum);
                            skipped++;
                            skippedCurrencyNotFound++;
                            continue;
                        }

                        _logger.LogWarning(
                            "Currency not found for code {Code}, using default currency Id={CurrencyId} for invoice {InvoiceRef}.",
                            currencyCode, defaultCurrencyId, stagingRow.InvoiceNum);
                        currencyId = defaultCurrencyId;
                    }

                    if (!batchRefs.Add(invoiceRef))
                    {
                        _logger.LogDebug("Skipping duplicate InvoiceRef {InvoiceRef} within batch.", invoiceRef);
                        skipped++;
                        skippedDuplicateInBatch++;
                        continue;
                    }

                    var isUsdNew = string.Equals(stagingRow.InvoiceCurrencyCode, "USD", StringComparison.OrdinalIgnoreCase);
                    var totalPaidFromStagingNew = isUsdNew ? (stagingRow.PaidUsd ?? 0) : (stagingRow.PaidFc ?? 0);
                    var amountTotalStaging = isUsdNew ? (stagingRow.AmountUsd ?? 0) : (stagingRow.AmountFc ?? 0);
                    var overPaid = totalPaidFromStagingNew > amountTotalStaging;

                    var invoiceInfoJsonNew = OracleInvoiceMapper.FromStaging(stagingRow).ToInvoiceInfoJson();
                    secondaryBatch.Add(InvoiceSecondaryStagingEntity.FromInvoiceStaging(
                        stagingRow,
                        defaultBillingServiceId.Value,
                        currencyId,
                        jobKey,
                        updated: false,
                        overPaid,
                        invoiceInfoJsonNew));
                    inserted++;
                }
            }

            _logger.LogInformation(
                "InvoiceStagingSyncService: Stage=ProjectSecondaryStaging completed. JobKey={JobKey}, SourceSystem={SourceSystem}, BatchNumber={BatchNumber}, SecondaryRows={SecondaryRows}, Updates={Updates}, Inserts={Inserts}, Skipped={Skipped}.",
                jobKey, sourceSystem, batchNumber, secondaryBatch.Count, updated - updatedBeforeBatch,
                inserted - insertedBeforeBatch, skipped - skippedBeforeBatch);

            if (secondaryBatch.Count > 0)
            {
                var now = DateTimeOffset.UtcNow;
                foreach (var row in secondaryBatch)
                {
                    row.CreatedAtUtc = now;
                    row.UpdatedAtUtc = now;
                }

                _logger.LogInformation(
                    "InvoiceStagingSyncService: Stage=SaveSecondaryStaging started. JobKey={JobKey}, SourceSystem={SourceSystem}, BatchNumber={BatchNumber}, RowsToSave={RowsToSave}.",
                    jobKey, sourceSystem, batchNumber, secondaryBatch.Count);
                await _db.BulkInsertAsync(secondaryBatch);
                _logger.LogInformation(
                    "InvoiceStagingSyncService: Stage=SaveSecondaryStaging completed. JobKey={JobKey}, SourceSystem={SourceSystem}, BatchNumber={BatchNumber}, RowsSaved={RowsSaved}, ElapsedMs={ElapsedMs}.",
                    jobKey, sourceSystem, batchNumber, secondaryBatch.Count, batchStopwatch.ElapsedMilliseconds);

            }
            else
            {
                _logger.LogInformation(
                    "InvoiceStagingSyncService: Stage=SaveSecondaryStaging skipped because no rows require migration. JobKey={JobKey}, SourceSystem={SourceSystem}, BatchNumber={BatchNumber}.",
                    jobKey, sourceSystem, batchNumber);
            }

            cursor = stagingBatch[^1].Id;
            batchStopwatch.Stop();
            _logger.LogInformation(
                "InvoiceStagingSyncService: Stage=BatchCompleted. JobKey={JobKey}, SourceSystem={SourceSystem}, BatchNumber={BatchNumber}, Cursor={Cursor}, ElapsedMs={ElapsedMs}.",
                jobKey, sourceSystem, batchNumber, cursor, batchStopwatch.ElapsedMilliseconds);
        }

        var effectiveNotificationTriggerTime = notificationTriggerTime ?? TimeSpan.Zero;
        _logger.LogInformation(
            "InvoiceStagingSyncService: Stage=ApplySecondaryStaging started. JobKey={JobKey}, SourceSystem={SourceSystem}.",
            jobKey, sourceSystem);
        await ApplyInvoicesFromSecondaryStagingAsync(jobKey, registerNotificationOnPull, effectiveNotificationTriggerTime);
        _logger.LogInformation(
            "InvoiceStagingSyncService: Stage=ApplySecondaryStaging completed. JobKey={JobKey}, SourceSystem={SourceSystem}.",
            jobKey, sourceSystem);

        var stagingCount = await _db.InvoiceStagings
            .CountAsync(x => x.JobKey == jobKey && x.SourceSystem == sourceSystem);

        await ClearInvoiceStagingsForJobAsync(jobKey, sourceSystem);
        
        await ClearSecondaryStagingForJobAsync(jobKey);

        syncStopwatch.Stop();
        _logger.LogInformation(
            "InvoiceStagingSyncService: Stage=Completed. Processed {StagingCount} staging rows for JobKey={JobKey} SourceSystem={SourceSystem}, secondary updated {Updated}, secondary inserted {Inserted}, skipped {Skipped} (skippedOracleNotNewerOrNoDate={SkippedNoChange}, emptyRef={EmptyRef}, emptyCurrency={EmptyCurrency}, currencyNotFound={CurrencyNotFound}, duplicateInBatch={DuplicateInBatch}), ElapsedMs={ElapsedMs}.",
            stagingCount, jobKey, sourceSystem, updated, inserted, skipped, skippedNoChange, skippedEmptyRef, skippedEmptyCurrency,
            skippedCurrencyNotFound, skippedDuplicateInBatch, syncStopwatch.ElapsedMilliseconds);
    }

    private async Task ClearSecondaryStagingForJobAsync(string jobKey)
    {
        _logger.LogInformation(
            "InvoiceStagingSyncService: Stage=ClearSecondaryStaging started. JobKey={JobKey}, BatchSize={BatchSize}.",
            jobKey, StagingDeleteBatchSize);

        var batchNumber = 0;
        var totalDeletedRows = 0;
        int deletedRows;

        do
        {
            batchNumber++;
            var batchStopwatch = Stopwatch.StartNew();
            deletedRows = await _db.Database.ExecuteSqlRawAsync(
                """
                WITH batch AS (
                    SELECT ctid
                    FROM "InvoiceSecondaryStagings"
                    WHERE "JobKey" = {0}
                    LIMIT {1}
                )
                DELETE FROM "InvoiceSecondaryStagings" AS staging
                USING batch
                WHERE staging.ctid = batch.ctid
                """,
                new object[] { jobKey, StagingDeleteBatchSize });
            batchStopwatch.Stop();
            totalDeletedRows += deletedRows;

            _logger.LogInformation(
                "InvoiceStagingSyncService: Stage=ClearSecondaryStaging batch completed. JobKey={JobKey}, BatchNumber={BatchNumber}, RowsDeleted={RowsDeleted}, TotalRowsDeleted={TotalRowsDeleted}, ElapsedMs={ElapsedMs}.",
                jobKey, batchNumber, deletedRows, totalDeletedRows, batchStopwatch.ElapsedMilliseconds);
        } while (deletedRows == StagingDeleteBatchSize);

        _logger.LogInformation(
            "InvoiceStagingSyncService: Stage=ClearSecondaryStaging completed. JobKey={JobKey}, BatchCount={BatchCount}, TotalRowsDeleted={TotalRowsDeleted}.",
            jobKey, batchNumber, totalDeletedRows);
    }

    private async Task ClearInvoiceStagingsForJobAsync(string jobKey, string sourceSystem)
    {
        _logger.LogInformation(
            "InvoiceStagingSyncService: Stage=ClearPrimaryStaging started. JobKey={JobKey}, SourceSystem={SourceSystem}, BatchSize={BatchSize}.",
            jobKey, sourceSystem, StagingDeleteBatchSize);

        var batchNumber = 0;
        var totalDeletedRows = 0;
        int deletedRows;

        do
        {
            batchNumber++;
            var batchStopwatch = Stopwatch.StartNew();
            deletedRows = await _db.Database.ExecuteSqlRawAsync(
                """
                WITH batch AS (
                    SELECT ctid
                    FROM "InvoiceStagings"
                    WHERE "JobKey" = {0}
                      AND "SourceSystem" = {1}
                    LIMIT {2}
                )
                DELETE FROM "InvoiceStagings" AS staging
                USING batch
                WHERE staging.ctid = batch.ctid
                """,
                new object[] { jobKey, sourceSystem, StagingDeleteBatchSize });
            batchStopwatch.Stop();
            totalDeletedRows += deletedRows;

            _logger.LogInformation(
                "InvoiceStagingSyncService: Stage=ClearPrimaryStaging batch completed. JobKey={JobKey}, SourceSystem={SourceSystem}, BatchNumber={BatchNumber}, RowsDeleted={RowsDeleted}, TotalRowsDeleted={TotalRowsDeleted}, ElapsedMs={ElapsedMs}.",
                jobKey, sourceSystem, batchNumber, deletedRows, totalDeletedRows, batchStopwatch.ElapsedMilliseconds);
        } while (deletedRows == StagingDeleteBatchSize);

        _logger.LogInformation(
            "InvoiceStagingSyncService: Stage=ClearPrimaryStaging completed. JobKey={JobKey}, SourceSystem={SourceSystem}, BatchCount={BatchCount}, TotalRowsDeleted={TotalRowsDeleted}.",
            jobKey, sourceSystem, batchNumber, totalDeletedRows);
    }

    private async Task ApplyInvoicesFromSecondaryStagingAsync(
        string jobKey,
        bool registerNotificationOnPull,
        TimeSpan notificationTriggerTime)
    {
        var applyStopwatch = Stopwatch.StartNew();
        var updateSql = """
            UPDATE "Invoices" AS i
            SET "BillingServiceId" = s."BillingServiceId",
                "BillingRef" = s."BillingRef",
                "OriginInvoiceId" = s."OriginInvoiceId",
                "AmountOutstanding" = s."AmountOutstanding",
                "AmountPaid" = s."AmountPaid",
                "AmountTotal" = s."AmountTotal",
                "CurrencyId" = s."CurrencyId",
                "IssueDate" = s."IssueDate",
                "DueDate" = s."DueDate",
                "Status" = s."Status",
                "InvoiceInfo" = s."InvoiceInfo",
                "OriginCustomerId" = s."OriginCustomerId",
                "PaymentMismatchFlag" = s."PaymentMismatchFlag",
                "InvoiceUpdateDate" = s."InvoiceUpdateDate",
                "UpdatedAtUtc" = NOW(),
                "UpdatedBy" = {1}
            FROM "InvoiceSecondaryStagings" AS s
            WHERE i."InvoiceRef" = s."InvoiceRef"
              AND s."JobKey" = {0}
              AND s."Updated" = TRUE
              AND s."Id" > {2}
              AND s."Id" <= {3}
            """;

        var insertSql = """
            INSERT INTO "Invoices" (
                "Id",
                "BillingServiceId",
                "BillingRef",
                "OriginInvoiceId",
                "InvoiceRef",
                "AmountOutstanding",
                "AmountPaid",
                "AmountTotal",
                "CurrencyId",
                "IssueDate",
                "DueDate",
                "Status",
                "InvoiceInfo",
                "OriginCustomerId",
                "PaymentMismatchFlag",
                "InvoiceUpdateDate",
                "CreatedAtUtc",
                "UpdatedAtUtc",
                "CreatedBy",
                "UpdatedBy"
            )
            SELECT gen_random_uuid(),
                s."BillingServiceId",
                s."BillingRef",
                s."OriginInvoiceId",
                s."InvoiceRef",
                s."AmountOutstanding",
                s."AmountPaid",
                s."AmountTotal",
                s."CurrencyId",
                s."IssueDate",
                s."DueDate",
                s."Status",
                s."InvoiceInfo",
                s."OriginCustomerId",
                s."PaymentMismatchFlag",
                s."InvoiceUpdateDate",
                s."CreatedAtUtc",
                s."UpdatedAtUtc",
                COALESCE(s."CreatedBy", {1}),
                COALESCE(s."UpdatedBy", {1})
            FROM "InvoiceSecondaryStagings" AS s
            WHERE s."JobKey" = {0}
              AND s."Updated" = FALSE
              AND s."Id" > {2}
              AND s."Id" <= {3}
            """;

        _logger.LogInformation(
            "ApplyInvoicesFromSecondaryStaging: Stage=UpdateInvoices started. JobKey={JobKey}, BatchSize={BatchSize}.",
            jobKey, ApplyBatchSize);
        var updatedRows = await UpdateExistingInvoicesInBatchesAsync(jobKey, updateSql);
        _logger.LogInformation(
            "ApplyInvoicesFromSecondaryStaging: Stage=UpdateInvoices completed. JobKey={JobKey}, RowsAffected={RowsAffected}, ElapsedMs={ElapsedMs}.",
            jobKey, updatedRows, applyStopwatch.ElapsedMilliseconds);

        _logger.LogInformation(
            "ApplyInvoicesFromSecondaryStaging: Stage=InsertInvoices started. JobKey={JobKey}, BatchSize={BatchSize}.",
            jobKey, ApplyBatchSize);
        var insertedRows = await InsertNewInvoicesInBatchesAsync(jobKey, insertSql);
        _logger.LogInformation(
            "ApplyInvoicesFromSecondaryStaging: Stage=InsertInvoices completed. JobKey={JobKey}, RowsAffected={RowsAffected}, ElapsedMs={ElapsedMs}.",
            jobKey, insertedRows, applyStopwatch.ElapsedMilliseconds);

        var notificationRows = 0;
        if (registerNotificationOnPull)
        {
            _logger.LogInformation(
                "ApplyInvoicesFromSecondaryStaging: Stage=InsertNotifications started. JobKey={JobKey}, NotificationTriggerTime={NotificationTriggerTime}, BatchSize={BatchSize}.",
                jobKey, notificationTriggerTime, ApplyBatchSize);
            notificationRows = await InsertNewInvoiceNotificationRowsAsync(jobKey, notificationTriggerTime);
            _logger.LogInformation(
                "ApplyInvoicesFromSecondaryStaging: Stage=InsertNotifications completed. JobKey={JobKey}, RowsAffected={RowsAffected}, ElapsedMs={ElapsedMs}.",
                jobKey, notificationRows, applyStopwatch.ElapsedMilliseconds);
        }
        else
        {
            _logger.LogInformation(
                "ApplyInvoicesFromSecondaryStaging: Stage=InsertNotifications skipped because notification registration is disabled. JobKey={JobKey}.",
                jobKey);
        }

        applyStopwatch.Stop();
        _logger.LogInformation(
            "ApplyInvoicesFromSecondaryStaging: Stage=Completed. JobKey={JobKey}, SQL update rows affected={UpdatedRows}, insert rows affected={InsertedRows}, notification rows affected={NotificationRows}, ElapsedMs={ElapsedMs}.",
            jobKey, updatedRows, insertedRows, notificationRows, applyStopwatch.ElapsedMilliseconds);
    }

    private async Task<int> UpdateExistingInvoicesInBatchesAsync(string jobKey, string updateSql)
    {
        var totalUpdatedRows = 0;
        var batchNumber = 0;
        var lastProcessedId = Guid.Empty;

        while (true)
        {
            var batchIds = await _db.InvoiceSecondaryStagings
                .AsNoTracking()
                .Where(row => row.JobKey == jobKey && row.Updated && row.Id.CompareTo(lastProcessedId) > 0)
                .OrderBy(row => row.Id)
                .Select(row => row.Id)
                .Take(ApplyBatchSize)
                .ToListAsync();

            if (batchIds.Count == 0)
                break;

            batchNumber++;
            var batchStopwatch = Stopwatch.StartNew();
            var batchLastId = batchIds[^1];

            _logger.LogInformation(
                "ApplyInvoicesFromSecondaryStaging: Stage=UpdateInvoiceBatch started. JobKey={JobKey}, BatchNumber={BatchNumber}, RowsSelected={RowsSelected}, FirstExclusiveId={FirstExclusiveId}, LastInclusiveId={LastInclusiveId}.",
                jobKey, batchNumber, batchIds.Count, lastProcessedId, batchLastId);

            var updatedRows = await _db.Database.ExecuteSqlRawAsync(
                updateSql,
                new object[] { jobKey, SystemActor, lastProcessedId, batchLastId });

            totalUpdatedRows += updatedRows;
            lastProcessedId = batchLastId;
            batchStopwatch.Stop();

            _logger.LogInformation(
                "ApplyInvoicesFromSecondaryStaging: Stage=UpdateInvoiceBatch completed. JobKey={JobKey}, BatchNumber={BatchNumber}, RowsSelected={RowsSelected}, RowsUpdated={RowsUpdated}, TotalRowsUpdated={TotalRowsUpdated}, ElapsedMs={ElapsedMs}.",
                jobKey, batchNumber, batchIds.Count, updatedRows, totalUpdatedRows,
                batchStopwatch.ElapsedMilliseconds);
        }

        _logger.LogInformation(
            "ApplyInvoicesFromSecondaryStaging: Stage=UpdateInvoiceBatches completed. JobKey={JobKey}, BatchCount={BatchCount}, TotalRowsUpdated={TotalRowsUpdated}.",
            jobKey, batchNumber, totalUpdatedRows);

        return totalUpdatedRows;
    }

    private async Task<int> InsertNewInvoicesInBatchesAsync(string jobKey, string insertSql)
    {
        var totalInsertedRows = 0;
        var batchNumber = 0;
        var lastProcessedId = Guid.Empty;

        while (true)
        {
            var batchIds = await _db.InvoiceSecondaryStagings
                .AsNoTracking()
                .Where(row => row.JobKey == jobKey && !row.Updated && row.Id.CompareTo(lastProcessedId) > 0)
                .OrderBy(row => row.Id)
                .Select(row => row.Id)
                .Take(ApplyBatchSize)
                .ToListAsync();

            if (batchIds.Count == 0)
                break;

            batchNumber++;
            var batchStopwatch = Stopwatch.StartNew();
            var batchLastId = batchIds[^1];

            _logger.LogInformation(
                "ApplyInvoicesFromSecondaryStaging: Stage=InsertInvoiceBatch started. JobKey={JobKey}, BatchNumber={BatchNumber}, RowsSelected={RowsSelected}, FirstExclusiveId={FirstExclusiveId}, LastInclusiveId={LastInclusiveId}.",
                jobKey, batchNumber, batchIds.Count, lastProcessedId, batchLastId);

            var insertedRows = await _db.Database.ExecuteSqlRawAsync(
                insertSql,
                new object[] { jobKey, SystemActor, lastProcessedId, batchLastId });

            totalInsertedRows += insertedRows;
            lastProcessedId = batchLastId;
            batchStopwatch.Stop();

            _logger.LogInformation(
                "ApplyInvoicesFromSecondaryStaging: Stage=InsertInvoiceBatch completed. JobKey={JobKey}, BatchNumber={BatchNumber}, RowsSelected={RowsSelected}, RowsInserted={RowsInserted}, TotalRowsInserted={TotalRowsInserted}, ElapsedMs={ElapsedMs}.",
                jobKey, batchNumber, batchIds.Count, insertedRows, totalInsertedRows,
                batchStopwatch.ElapsedMilliseconds);
        }

        _logger.LogInformation(
            "ApplyInvoicesFromSecondaryStaging: Stage=InsertInvoiceBatches completed. JobKey={JobKey}, BatchCount={BatchCount}, TotalRowsInserted={TotalRowsInserted}.",
            jobKey, batchNumber, totalInsertedRows);

        return totalInsertedRows;
    }

    private async Task<int> InsertNewInvoiceNotificationRowsAsync(string jobKey, TimeSpan notificationTriggerTime)
    {
        var insertNotificationsSql = """
            INSERT INTO "NewInvoiceNotifications" (
                "Id",
                "InvoiceId",
                "NotificationTriggerTime",
                "Processed",
                "ProcessedAtUtc",
                "CreatedAtUtc",
                "UpdatedAtUtc",
                "CreatedBy",
                "UpdatedBy"
            )
            SELECT
                gen_random_uuid(),
                i."Id",
                {1},
                FALSE,
                NULL,
                NOW(),
                NOW(),
                {2},
                {2}
            FROM "InvoiceSecondaryStagings" AS s
            INNER JOIN "Invoices" AS i
                ON i."InvoiceRef" = s."InvoiceRef"
            WHERE s."JobKey" = {0}
              AND s."Updated" = FALSE
              AND s."Id" > {3}
              AND s."Id" <= {4}
              AND NOT EXISTS (
                  SELECT 1
                  FROM "NewInvoiceNotifications" AS n
                  WHERE n."InvoiceId" = i."Id"
              )
            """;

        var totalInsertedRows = 0;
        var batchNumber = 0;
        var lastProcessedId = Guid.Empty;

        while (true)
        {
            var batchIds = await _db.InvoiceSecondaryStagings
                .AsNoTracking()
                .Where(row => row.JobKey == jobKey && !row.Updated && row.Id.CompareTo(lastProcessedId) > 0)
                .OrderBy(row => row.Id)
                .Select(row => row.Id)
                .Take(ApplyBatchSize)
                .ToListAsync();

            if (batchIds.Count == 0)
                break;

            batchNumber++;
            var batchStopwatch = Stopwatch.StartNew();
            var batchLastId = batchIds[^1];

            _logger.LogInformation(
                "ApplyInvoicesFromSecondaryStaging: Stage=InsertNotificationBatch started. JobKey={JobKey}, BatchNumber={BatchNumber}, RowsSelected={RowsSelected}, FirstExclusiveId={FirstExclusiveId}, LastInclusiveId={LastInclusiveId}.",
                jobKey, batchNumber, batchIds.Count, lastProcessedId, batchLastId);

            var insertedRows = await _db.Database.ExecuteSqlRawAsync(
                insertNotificationsSql,
                new object[]
                {
                    jobKey,
                    notificationTriggerTime,
                    SystemActor,
                    lastProcessedId,
                    batchLastId
                });

            totalInsertedRows += insertedRows;
            lastProcessedId = batchLastId;
            batchStopwatch.Stop();

            _logger.LogInformation(
                "ApplyInvoicesFromSecondaryStaging: Stage=InsertNotificationBatch completed. JobKey={JobKey}, BatchNumber={BatchNumber}, RowsSelected={RowsSelected}, RowsInserted={RowsInserted}, TotalRowsInserted={TotalRowsInserted}, ElapsedMs={ElapsedMs}.",
                jobKey, batchNumber, batchIds.Count, insertedRows, totalInsertedRows,
                batchStopwatch.ElapsedMilliseconds);
        }

        _logger.LogInformation(
            "ApplyInvoicesFromSecondaryStaging: Stage=InsertNotificationBatches completed. JobKey={JobKey}, BatchCount={BatchCount}, TotalRowsInserted={TotalRowsInserted}.",
            jobKey, batchNumber, totalInsertedRows);

        return totalInsertedRows;
    }

    /// <summary>
    /// True when Oracle effective update is strictly after the invoice business <see cref="InvoiceEntity.InvoiceUpdateDate"/>.
    /// If <paramref name="invoiceInvoiceUpdateDate"/> is null, treats staging as newer (row never had a business sync date).
    /// </summary>
    private static bool StagingEffectiveIsNewerThanInvoiceBusinessUpdate(
        DateTime stagingEffectiveOracle,
        DateTime? invoiceInvoiceUpdateDate)
    {
        var stagingInstant = ToComparableUtcOffset(stagingEffectiveOracle);
        if (!invoiceInvoiceUpdateDate.HasValue)
            return true;
        var invoiceInstant = ToComparableUtcOffset(invoiceInvoiceUpdateDate.Value);
        return stagingInstant > invoiceInstant;
    }

    private static DateTimeOffset ToComparableUtcOffset(DateTime dt)
    {
        return dt.Kind switch
        {
            DateTimeKind.Utc => new DateTimeOffset(dt, TimeSpan.Zero),
            DateTimeKind.Local => new DateTimeOffset(dt.ToUniversalTime(), TimeSpan.Zero),
            _ => new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc), TimeSpan.Zero)
        };
    }
}
