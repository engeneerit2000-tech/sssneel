using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SNEL.WalletPlatform.Application.Invoicing.Interfaces;
using SNEL.WalletPlatform.Infrastructure.Persistence;

namespace SNEL.WalletPlatform.Infrastructure.Invoicing;

/// <summary>
/// Same pattern as invoice apply-from-secondary-staging: set staging <c>Updated</c> vs <c>BillerCustomers</c>, then raw SQL UPDATE, then raw SQL INSERT.
/// All statements are scoped by <paramref name="jobKey"/> on <c>BillerCustomerStagings</c>.
/// </summary>
public sealed class BillerCustomerStagingMergeService : IBillerCustomerStagingMergeService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<BillerCustomerStagingMergeService> _logger;

    public BillerCustomerStagingMergeService(
        ApplicationDbContext db,
        ILogger<BillerCustomerStagingMergeService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(int Inserted, int Updated)> MergeFromStagingAsync(long billerId, string jobKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobKey))
            throw new ArgumentException("jobKey must be non-empty.", nameof(jobKey));

        var job = jobKey.Trim();

        // 1) Staging.Updated = true when (BillerId, CustomerId) already exists in BillerCustomers; else false.
        var markUpdatedSql = """
            UPDATE "BillerCustomerStagings" AS s
            SET "Updated" = EXISTS (
                SELECT 1
                FROM "BillerCustomers" AS bc
                WHERE bc."BillerId" = s."BillerId"
                  AND bc."CustomerId" = s."CustomerId"
                  AND bc."Number" = s."Number"
            )
            WHERE s."BillerId" = {0}
              AND s."JobKey" = {1}
            """;

        await _db.Database.ExecuteSqlRawAsync(markUpdatedSql, new object[] { billerId, job }, cancellationToken);

        // 2) UPDATE main table from staging rows classified as existing (Updated = TRUE).
        var updateSql = """
            UPDATE "BillerCustomers" AS bc
            SET "Name" = s."Name"
            FROM "BillerCustomerStagings" AS s
            WHERE bc."BillerId" = s."BillerId"
              AND bc."CustomerId" = s."CustomerId"
              AND bc."Number" = s."Number"
              AND s."BillerId" = {0}
              AND s."JobKey" = {1}
              AND s."Updated" = TRUE
            """;

        // 3) INSERT new rows from staging (Updated = FALSE).
        var insertSql = """
            INSERT INTO "BillerCustomers" ("Id", "BillerId", "CustomerId", "Number", "Name")
            SELECT gen_random_uuid(),
                   s."BillerId",
                   s."CustomerId",
                   s."Number",
                   s."Name"
            FROM "BillerCustomerStagings" AS s
            WHERE s."BillerId" = {0}
              AND s."JobKey" = {1}
              AND s."Updated" = FALSE
            """;

        var updatedRows = await _db.Database.ExecuteSqlRawAsync(
            updateSql,
            new object[] { billerId, job },
            cancellationToken);
        var insertedRows = await _db.Database.ExecuteSqlRawAsync(
            insertSql,
            new object[] { billerId, job },
            cancellationToken);

        _logger.LogInformation(
            "BillerCustomer merge (SQL): BillerId={BillerId}, JobKey={JobKey}, staging Updated flags set, SQL update rows={UpdatedRows}, insert rows={InsertedRows}.",
            billerId, job, updatedRows, insertedRows);

        return (insertedRows, updatedRows);
    }
}
