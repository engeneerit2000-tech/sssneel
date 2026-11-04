using AutoMapper;
using Medallion.Threading;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPOI.POIFS.Properties;
using NPOI.SS.Formula.Functions;
using SNEL.WalletPlatform.Application;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.BillingServices.Models.Internal;
using SNEL.WalletPlatform.Application.BillingServices.Models.Output;
using SNEL.WalletPlatform.Application.Caching;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Enums;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Invoices.Models.Input;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.BillingServices.ConnectAfrica;
using SNEL.WalletPlatform.Infrastructure.BillingServices.ConnectAfrica.Models;
using SNEL.WalletPlatform.Infrastructure.Configuration;
using SNEL.WalletPlatform.Infrastructure.Oracle;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection.Emit;
using System.Text.Json;
using System.Threading;

namespace SNEL.WalletPlatform.Infrastructure.BillingServices.Connectors;

public sealed class SnelBillerConnector(
    IMapper mapper,
    IUnitOfWork unitOfWork,
    IStringLocalizer<SharedResource> localizer,
    ILogger<SnelBillerConnector> logger,
    IOptions<SnelOracleLoggingOptions> loggingOptions,
    ISnelConfigService configService,
    ISnelOracleApplyCommandBuilder applyCommandBuilder,
    ConnectAfricaHttpClient connectAfricaHttpClient,
    IOracleSnelConnectionFactory oracleFactory,
    IDistributedLockProvider distributedLockProvider) : BillerConnector(localizer)
{
    //private const string SupportedAreaCode = "06-05,06-03";
    private const string MeterTokenDelimiter = "|";

    public override string BillerCode => "SNEL";
    private const string BillerCodeStatic = "SNEL";

    private enum CustomActionCodesEnum {
        LAST_TRANSACTION_STATUS,
        REPRINT,
        LIST_TRANSACTIONS,
    }
    private enum CustomActionVariableCodesEnum
    {
        RECEIPT_NO,
        COUNT
    }


    public override async Task<Result<InquireOutputModel>> HandleInquireAsync(
        BillerInquiryRequestModel request,
        CancellationToken cancellationToken = default)
    {
        var invoices = await unitOfWork.InvoiceRepository.GetByBillingServiceIdAndBillingRefAsync(
            request.BillingServiceId,
            request.BillingRef,
            request.UnpaidOnly,
            cancellationToken);

        if (invoices.Count == 0)
        {
            var emptyOutput = new InquireOutputModel(
                BillerCode: BillerCode,
                ServiceCode: request.ServiceCode,
                BillingRef: request.BillingRef,
                BillingRefForDisplay: request.BillingRef.Contains('-') ? request.BillingRef.Substring(request.BillingRef.LastIndexOf('-') + 1) : request.BillingRef,
                Balances: null,
                Bills: Array.Empty<InquireBillOutputModel>(),
                TotalOutstanding: Array.Empty<InquireTotalOutstandingByCurrencyOutputModel>(),
                RetrievedAtUtc: DateTimeOffset.UtcNow);

            return Result<InquireOutputModel>.Success(emptyOutput);

        }

        var bills = mapper.Map<InquireBillOutputModel[]>(invoices);

        var output = new InquireOutputModel(
            BillerCode: BillerCode,
            ServiceCode: request.ServiceCode,
            BillingRef: request.BillingRef,
            BillingRefForDisplay: request.BillingRef.Contains('-') ? request.BillingRef.Substring(request.BillingRef.LastIndexOf('-')+1) : request.BillingRef,
            Balances: null,
            Bills: bills,
            TotalOutstanding: InquireOutputTotals.CalculateTotalOutstandingByCurrency(bills),
            RetrievedAtUtc: DateTimeOffset.UtcNow);

        return Result<InquireOutputModel>.Success(output);
    }

    public override async Task<Result<PagedInquireOutputModel>> HandleInquirePagedAsync(
        BillerPagedInquiryRequestModel request,
        CancellationToken cancellationToken = default)
    {
        var query = new InvoiceInquiryQueryInternalModel(
            request.BillingServiceId,
            request.BillingRef,
            request.UnpaidOnly,
            request.PageNumber,
            request.PageSize,
            request.Search,
            request.SortBy,
            request.IsAscending);

        var (totalCount, totalOutstanding) = await unitOfWork.InvoiceRepository
            .GetInquirySummaryAsync(query, cancellationToken);

        var offset = ((long)request.PageNumber - 1L) * request.PageSize;
        var invoices = offset >= totalCount
            ? Array.Empty<InvoiceEntity>()
            : await unitOfWork.InvoiceRepository.GetInquiryPageAsync(query, cancellationToken);

        var bills = mapper.Map<InquireBillOutputModel[]>(invoices);
        var output = new PagedInquireOutputModel(
            BillerCode: BillerCode,
            ServiceCode: request.ServiceCode,
            BillingRef: request.BillingRef,
            BillingRefForDisplay: request.BillingRef.Contains('-')
                ? request.BillingRef[(request.BillingRef.LastIndexOf('-') + 1)..]
                : request.BillingRef,
            Balances: null,
            Bills: new PagedResult<InquireBillOutputModel>(bills, totalCount),
            TotalOutstanding: totalOutstanding
                .Select(total => new InquireTotalOutstandingByCurrencyOutputModel(total.Key, total.Value))
                .OrderBy(total => total.CurrencyCode, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            RetrievedAtUtc: DateTimeOffset.UtcNow);

        return Result<PagedInquireOutputModel>.Success(output);
    }

    public override async Task<Result<(bool Status, string? Info)>> HandleCheckSubscriptionExistsAsync(BillerSubscriptionExistsRequestModel request)
    {
        var billingService = await unitOfWork.BillingServiceRepository.GetByIdWithRelationsAsync(request.BillingServiceId);
        if (billingService is null)
        {
            return Result<(bool, string?)>.Failure(
                localizer[LocalizationKeys.BillingInquiryServiceIdNotFound, request.BillingServiceId],
                AppHttpStatusCodes.BadRequest);
        }

        if (!string.Equals(billingService.Biller.Code, BillerCode, StringComparison.OrdinalIgnoreCase))
        {
            return Result<(bool Status, string? Info)>.Failure(
                localizer[LocalizationKeys.BillingInquiryInvalidRequest],
                AppHttpStatusCodes.BadRequest);
        }
        switch (billingService.Code)
        {
            case "SNEL":
                return await CheckCustomerExistAsync(billingService.BillerId, request.AreaCode, request.SubscriptionNumber);                 
            case "CONNECT-AFRICA":
                {
                    return await CheckMeterRechargeExistAsync(request.SubscriptionNumber);
                }
            default:
                throw new NotImplementedException($"Billing service code '{billingService.Code}' is not supported.");
        }
        
    }

    private async Task<Result<(bool Status, string? Info)>> CheckCustomerExistAsync(long billerId, string areaCode, string subscriptionNumber)
    {
        if (string.IsNullOrEmpty(subscriptionNumber))
        {
            return Result<(bool Status, string? Info)>.Failure(
                localizer[LocalizationKeys.CustomerSubscriptionSubscriptionNumberRequired],
                AppHttpStatusCodes.BadRequest);
        }

        var normalizedNumber = string.IsNullOrEmpty(areaCode) ? subscriptionNumber : $"{areaCode}-{subscriptionNumber}";

        var exists = await unitOfWork.BillerCustomerRepository.GetByBillerIdAndNumberAsync(
            billerId,            
            normalizedNumber);

        if (exists is null)
        {
            return Result<(bool Status, string? Info)>.Failure(
                localizer[LocalizationKeys.CustomerSubscriptionInvalidSubscriptionNumber, subscriptionNumber],
                AppHttpStatusCodes.BadRequest);
        }

        return Result<(bool Status, string? Info)>.Success((true, exists.Name));
    }

    public override async Task<Result<int>> ApplyPaymentsAsync(IReadOnlyCollection<PaymentOrderItemEntity> pois)
    {
        var billEntries = pois
            .Where(x => x.ItemType == OrderItemType.PayBill)
            .ToArray();
        var meterEntries = pois
            .Where(x => x.ItemType == OrderItemType.MeterCharge)
            .ToArray();
        int billApplyResult = 0;
        int meterApplyResult = 0;
        if (billEntries.Length > 0)
        {
            billApplyResult += (await ExecuteBillApplyAsync(billEntries)).Value;
            
        }

        if (meterEntries.Length > 0)
        {
            var meterApply = await ExecuteMeterChargeApplyAsync(meterEntries);
            if (!meterApply.IsSuccess)
            {
                return Result<int>.Failure(meterApply.Message!, meterApply.StatusCode);
            }

            meterApplyResult += meterApply.Value;
        }

        return Result<int>.Success(billApplyResult + meterApplyResult);
    }

    public override async Task<Result<(bool,string)>> HandleIsEligible(IsEligibleRequestModel model)
    {
        switch (model.ServiceCode.Trim())
        {
            case "SNEL":
                var lockKey = $"{LockNames.SNEL_DAILY_JOB}";
                var locker = await distributedLockProvider.TryAcquireLockAsync(lockKey, TimeSpan.FromSeconds(0));
                if (locker is null)
                {
                    logger.LogError($"Failed to initiate payment, SNEL journal is locked");
                    return Result<(bool, string)>.Failure(
                        localizer[LocalizationKeys.PaymentBillerDailyJobLocked],
                        AppHttpStatusCodes.Conflict);
                }
                await locker.DisposeAsync();
                return Result<(bool, string)>.Success((true, string.Empty));
            case "CONNECT-AFRICA":
                {
                    return await CheckMeterRechargeAmount(model.BillingRef, model.Amount);
                }
            default:
                throw new NotImplementedException($"Service code '{model.ServiceCode}' is not supported.");
        }
        
    }

    private async Task<Result<(bool Status, string? Info)>> CheckMeterRechargeAmount(string billingRef, decimal amount)
    {
        try
        {
            logger.LogInformation(
                "Connect Africa CheckAsync request (amount check). Meter={Meter}, Amount={Amount}",
                billingRef.Trim(),
                amount);
            var checkResponse = await connectAfricaHttpClient.CheckAsync(
                new ConnectAfricaCheckRequest(
                    Meter: billingRef.Trim(),
                    Amount: amount));
            logger.LogInformation(
                "Connect Africa CheckAsync response (amount check). IsSuccess={IsSuccess}, MinimumAllowedChargeValue={Min}, CustomerName={CustName}, Message={Message}",
                checkResponse?.IsSuccess,
                checkResponse?.MinimumAllowedChargeValue,
                checkResponse?.CustomerName,
                checkResponse?.Message);

            if (checkResponse == null) {
                return Result<(bool Status, string? Info)>.Failure(
                    localizer[LocalizationKeys.BillingInquirySubscriptionNotFound, billingRef],
                    AppHttpStatusCodes.BadRequest);
            }
            if (checkResponse.IsSuccess)
            {
                if ((checkResponse.MinimumAllowedChargeValue ?? 0m) > amount)
                {
                    var minimumAmount = (checkResponse.MinimumAllowedChargeValue ?? 0m).ToString("0.00", CultureInfo.InvariantCulture);
                    return Result<(bool Status, string? Info)>.Failure(
                        localizer[LocalizationKeys.PaymentInitializeAmountBelowMin, minimumAmount],
                        AppHttpStatusCodes.BadRequest);
                }
                else
                {
                    return Result<(bool Status, string? Info)>.Success((true, string.Empty));
                }
            }
            else {
                return Result<(bool Status, string? Info)>.Failure(
                    localizer[LocalizationKeys.BillingInquirySubscriptionNotFound, billingRef],
                    AppHttpStatusCodes.BadRequest);
            }
                            
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Error occurred while checking meter recharge amount for billing ref '{billingRef}' and amount '{amount}'.");
            return Result<(bool Status, string? Info)>.Failure(
                localizer[LocalizationKeys.AmountCheckFailed],
                AppHttpStatusCodes.Failed);
        }
    }
    private async Task<Result<(bool Status, string? Info)>> CheckMeterRechargeExistAsync(string billingRef)
    {
        try
        {
            logger.LogInformation(
                "Connect Africa CheckAsync request (subscription exists). Meter={Meter}, Amount={Amount}",
                billingRef.Trim(),
                1m);
            var checkResponse = await connectAfricaHttpClient.CheckAsync(
                new ConnectAfricaCheckRequest(
                    Meter: billingRef.Trim(),
                    Amount: 1m));
            logger.LogInformation(
                "Connect Africa CheckAsync response (subscription exists). IsSuccess={IsSuccess}, MinimumAllowedChargeValue={Min}, CustomerName={CustName}, Message={Message}",
                checkResponse?.IsSuccess,
                checkResponse?.MinimumAllowedChargeValue,
                checkResponse?.CustomerName,
                checkResponse?.Message);

            if (checkResponse == null || !checkResponse.IsSuccess) {
                return Result<(bool Status, string Info)>.Failure(
                localizer[LocalizationKeys.BillingInquirySubscriptionNotFound, billingRef],
                AppHttpStatusCodes.BadRequest);
            }

            return Result<(bool Status, string Info)>.Success(new (checkResponse!.IsSuccess, checkResponse.CustomerName));

        }
        catch (Exception e)
        {
            logger.LogError(e, $"Error occurred while checking meter recharge existence for billing ref '{billingRef}'.");
            return Result<(bool Status, string Info)>.Failure(
                localizer[LocalizationKeys.MeterRechargeExistenceCheckFailed, billingRef],
                AppHttpStatusCodes.Failed);
        }
    }
    private async Task<Result<int>> ExecuteBillApplyAsync(
        IReadOnlyList<PaymentOrderItemEntity> billPOIs,
        CancellationToken cancellationToken=default)
    {
        var itemsByArea = new Dictionary<string, List<PaymentOrderItemEntity>>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in billPOIs)
        {
            var addResult = TryAddItemToAreaGroup(itemsByArea, entry);
            if (!addResult.IsSuccess)
            {
                logger.LogError($"Failed to group SNEL bill item '{entry.Id}' by area. Error: {addResult.Message}");
                return Result<int>.Failure(
                    addResult.Message!,
                    addResult.StatusCode,0);
            }
        }

        int result = 0;        
        if (itemsByArea.Count > 0) {
            
            foreach (var areaItems in itemsByArea)
            {
                using var connection = oracleFactory.CreateConnection(areaItems.Key);
                connection.Open();
                var executeResult =
                    await ExecuteAreaApplyAsync(connection, areaItems.Key, areaItems.Value, cancellationToken);
                if (!executeResult.IsSuccess)
                {
                    logger.LogError($"Failed to apply SNEL bill for area '{areaItems.Key}'. Error: {executeResult.Message}");
                    //return Result<bool>.Failure(
                    //    executeResult.Error!,
                    //    executeResult.StatusCode);
                }
                result+= executeResult.Value;
            }
        }        
        return Result<int>.Success(result);
    }

    private async Task<Result<int>> ExecuteMeterChargeApplyAsync(
        IReadOnlyList<PaymentOrderItemEntity> meterEntries)
    {
        int result = 0;
        string? lastFailureMessage = null;
        foreach (var entry in meterEntries)
        {
            var meter = entry.BillingRefValue?.Trim();
            if (string.IsNullOrWhiteSpace(meter))
            {
                lastFailureMessage = $"Meter charge item '{entry.Id}' has no meter number configured.";
                logger.LogError(lastFailureMessage);
                continue;
            }

            var customerPartnerId = entry.PaymentOrder?.Customer?.PartnerId;
            var customerUser = customerPartnerId.HasValue
                ? (await unitOfWork.UserRepository.GetByPartnerIdsWithNotificationsDependenciesAsync([customerPartnerId.Value]))
                    .FirstOrDefault(x => x.PartnerId == customerPartnerId.Value)
                : null;
            var customerPhone = customerUser?.MobileNo?.Trim();
            if (string.IsNullOrWhiteSpace(customerPhone))
            {
                lastFailureMessage =
                    $"PaymentOrder '{entry.PaymentOrder?.Id}' has no customer mobile number for Connect Africa vend.";
                logger.LogError(lastFailureMessage);
                continue;
            }

            ConnectAfricaVendResponse vendResponse;
            try
            {
                logger.LogInformation(
                    "Connect Africa VendAsync request. Meter={Meter}, Amount={Amount}, CustomerPhone={CustomerPhone}",
                    meter,
                    entry.Amount,
                    customerPhone);
                vendResponse = await connectAfricaHttpClient.VendAsync(
                    new ConnectAfricaVendRequest(
                        Meter: meter,
                        Amount: entry.Amount,
                        CustomerPhone: customerPhone));
            }
            catch (Exception ex)
            {
                lastFailureMessage = ex.Message;
                logger.LogError(ex, "Connect Africa vend threw for item '{EntryId}'.", entry.Id);
                continue;
            }

            logger.LogInformation(
                "Connect Africa VendAsync response. IsSuccess={IsSuccess}, ResponseCode={ResponseCode}, RequestBody={RequestBody}, ResponseBody={ResponseBody}",
                vendResponse.IsSuccess,
                vendResponse.ResponseCode,
                vendResponse.RequestBody,
                vendResponse.ResponseBody);

            if (!vendResponse.IsSuccess)
            {
                lastFailureMessage = DescribeConnectAfricaVendFailure(vendResponse);
                logger.LogError(
                    "Connect Africa vend failed for item '{EntryId}'. {FailureMessage}",
                    entry.Id,
                    lastFailureMessage);
                continue;
            }
            logger.LogInformation($"Connect Africa vend succeeded for item '{entry.Id}'.Request: {vendResponse.RequestBody}, Response: {vendResponse.ResponseBody}");

            if (string.IsNullOrWhiteSpace(vendResponse.ReceiptNumber))
            {
                logger.LogError($"Connect Africa vend did not return a receipt number for item '{entry.Id}'. Response: {vendResponse.ResponseBody}");
                continue;
                //return Result<bool>.Failure(
                //    $"Connect Africa vend did not return a receipt number for item '{entry.Id}'.",
                //    AppHttpStatusCodes.Failed);
            }

            if (vendResponse.Tokens.Count == 0)
            {
                logger.LogError($"Connect Africa vend did not return any tokens for item '{entry.Id}'. Response: {vendResponse.ResponseBody}");
                continue;
                //return Result<bool>.Failure(
                //    $"Connect Africa vend did not return any tokens for item '{entry.Id}'.",
                //    AppHttpStatusCodes.Failed);
            }              

            logger.LogInformation("Connect Africa AckAsync request. ReceiptNumber={ReceiptNumber}", vendResponse.ReceiptNumber);
            var ackResponse = await connectAfricaHttpClient.AckAsync(new ConnectAfricaAckRequest(vendResponse.ReceiptNumber));
            logger.LogInformation(
                "Connect Africa AckAsync response. IsSuccess={IsSuccess}, RequestBody={RequestBody}, ResponseBody={ResponseBody}",
                ackResponse.IsSuccess,
                ackResponse.RequestBody,
                ackResponse.ResponseBody);
            ////TODO: Shaker, this place has nothing to do with the payment at all.
            //var ackTxn = PaymentTxnEntity.Create(
            //    paymentId: entry.Payment.Id,
            //    type: TransactionType.Initiate,
            //    status: TransactionStatus.Processing,
            //    currencyId: entry.Payment.CurrencyId,
            //    amount: entry.Item.Amount,
            //    externalRef: vendResponse.ReceiptNumber,
            //    reqBody: ackResponse.RequestBody,
            //    resBody: ackResponse.ResponseBody,
            //    resCode: ackResponse.ResponseCode,
            //    resMessage: "ConnectAfrica ack");
            //await unitOfWork.PaymentTxnRepository.AddAsync(ackTxn);

            if (!ackResponse.IsSuccess)
            {

                logger.LogError($"Connect Africa ack failed for item '{entry.Id}'. Response: {ackResponse.ResponseBody}");
                continue;
                //return Result<bool>.Failure(
                //    $"Connect Africa ack failed for item '{entry.Id}'.",
                //    AppHttpStatusCodes.Failed);
            }
            var receipt = vendResponse.Receipt;
            var tokens = receipt?.Tokens.Count > 0 ? receipt.Tokens : vendResponse.Tokens;
            var receiptNumber = receipt?.ReceiptNumber ?? vendResponse.ReceiptNumber;
            logger.LogInformation(
                "Connect Africa meter charge response saved. PaymentOrderItemId={PaymentOrderItemId}, ReceiptNumber={ReceiptNumber}, WebResponse={WebResponse}",
                entry.Id,
                receiptNumber,
                vendResponse.ResponseBody);
            entry.SetWebResponse(vendResponse.ResponseBody);

            entry.SetExternalRef(JsonSerializer.Serialize(new
            {
                ReceiptNo = string.IsNullOrWhiteSpace(receiptNumber) ? null : receiptNumber.Trim(),
                Tokens = tokens,
                Acknowledged = ackResponse.IsSuccess,
            }));
            entry.ChangeStatus(PaymentOrderItemStatus.Applied);
            result +=1;
        }

        if (result == 0 && meterEntries.Count > 0)
        {
            return Result<int>.Failure(
                lastFailureMessage ?? "Connect Africa vend failed.",
                AppHttpStatusCodes.Failed);
        }

        return Result<int>.Success(result);
    }

    private static string DescribeConnectAfricaVendFailure(ConnectAfricaVendResponse vendResponse)
    {
        if (!string.IsNullOrWhiteSpace(vendResponse.ResponseCode) &&
            !string.Equals(vendResponse.ResponseCode, "1", StringComparison.OrdinalIgnoreCase))
        {
            return $"Connect Africa vend failed (code {vendResponse.ResponseCode}).";
        }

        if (!string.IsNullOrWhiteSpace(vendResponse.ResponseBody))
        {
            return $"Connect Africa vend failed: {vendResponse.ResponseBody}";
        }

        return "Connect Africa vend failed.";
    }

    private Result<bool> TryAddItemToAreaGroup(
        IDictionary<string, List<PaymentOrderItemEntity>> itemsByArea,
        PaymentOrderItemEntity item)
    {
        var areaCodeResult = TryGetAreaCode(item);
        if (!areaCodeResult.IsSuccess)
        {
            return Result<bool>.Failure(
                areaCodeResult.Message!,
                areaCodeResult.StatusCode);
        }

        var areaCode = areaCodeResult.Value!;
        if (!itemsByArea.TryGetValue(areaCode, out var items))
        {
            items = new List<PaymentOrderItemEntity>();
            itemsByArea[areaCode] = items;
        }

        items.Add(item);
        return Result<bool>.Success(true);
    }

    private async Task<Result<int>> ExecuteAreaApplyAsync(
        IDbConnection connection,
        string areaCode,
        IReadOnlyList<PaymentOrderItemEntity> items,
        CancellationToken cancellationToken)
    {
        //var activeCenters = await unitOfWork.CenterRepository.GetActiveAsync();
        var activeCenters = CachingFactory.GetValue<CenterEntity>(c => c.Active);           
        if (activeCenters == null || activeCenters.Count == 0)
            return Result<int>.Failure(
                localizer[LocalizationKeys.SnelBillerAreaNotSupported, areaCode, ""],
                AppHttpStatusCodes.BadRequest, 0);
        var SupportedAreaCode = activeCenters!.Select(x => x.Code);
        if(!SupportedAreaCode.Contains(areaCode)) {        
            return Result<int>.Failure(
                localizer[LocalizationKeys.SnelBillerAreaNotSupported, areaCode,  string.Join(',', SupportedAreaCode)],
                AppHttpStatusCodes.BadRequest, 0);
        }
        var originalConfigs = await configService.GetConfigAsync(areaCode, cancellationToken);
        var lockKey = $"{LockNames.SNEL_BILL_PAYMENT_APPLY}:{areaCode}";
        await using var locker = await distributedLockProvider.TryAcquireLockAsync(lockKey, TimeSpan.FromSeconds(15));
        if (locker is null)
        {
            return Result<int>.Failure(localizer[LocalizationKeys.SnelBillerAreaJournalLocked, areaCode], AppHttpStatusCodes.Conflict);
        }        
        var config = await configService.ReserveApplyConfigAsync(items.Count, areaCode, cancellationToken);
        var startItemNumber = config.JournalItemNumber;
        var endItemNumber = startItemNumber + items.Count - 1;
        var tr = connection.BeginTransaction();
        try
        {
            var commandResult = applyCommandBuilder.Build(
                connection,
                items,
                config.JournalNumber,
                startItemNumber,
                config.PosCode,
                config.PosId,
                config.PaymentMode);
            if (!commandResult.IsSuccess)
            {
                return Result<int>.Failure(
                    commandResult.Message!,
                    commandResult.StatusCode, 0);
            }

            using var command = commandResult.Value!;
            var affectedRows = await ExecuteNonQueryAsync(command, cancellationToken);
            locker.Dispose();
            if (affectedRows <= 0)
            {
                return Result<int>.Failure(
                    localizer[LocalizationKeys.BillingApplySnelInsertFailed],
                    AppHttpStatusCodes.Failed, 0);
            }

            var externalRefsResult = await LoadExternalRefsByJournalRangeAsync(
                connection,
                config.JournalNumber,
                config.PosId,
                startItemNumber,
                endItemNumber,
                items.Count,
                cancellationToken);
            if (!externalRefsResult.IsSuccess)
            {
                return Result<int>.Failure(
                    externalRefsResult.Message!,
                    externalRefsResult.StatusCode, 0);
            }

            int affectedResult = 0;
            for (var i = 0; i < items.Count; i++)
            {
                var itemNumber = startItemNumber + i;
                if (!externalRefsResult.Value!.TryGetValue(itemNumber, out var externalRef))
                {
                    return Result<int>.Failure(
                        localizer[LocalizationKeys.SnelBillerReglementIdeMissing, config.JournalNumber, itemNumber],
                        AppHttpStatusCodes.Failed, 0);
                }

                items[i].SetExternalRef(JsonSerializer.Serialize(new { ExternalRef = externalRef }));
                items[i].ChangeStatus(PaymentOrderItemStatus.Applied);
                items[i].Invoice!.ApplyPayment(items[i].Amount);
                affectedResult += 1;
            }
            tr.Commit();
            return Result<int>.Success(affectedResult);
        }
        catch (Exception ex)
        {
            tr.Rollback();
            logger.LogError(ex.StackTrace);
            return Result<int>.Failure(
                    ex.Message,
                    AppHttpStatusCodes.Failed, 0);
        }
    }

    private async Task<Result<IReadOnlyDictionary<decimal, string>>> LoadExternalRefsByJournalRangeAsync(
        IDbConnection connection,
        decimal journalNumber,
        decimal posId,
        decimal startItemNumber,
        decimal endItemNumber,
        int expectedCount,
        CancellationToken cancellationToken)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT
                NUM_ITEM,
                IDE_REGLEMENT
            FROM gco.GCO_REGLEMENTS
            WHERE NUM_JOURNAL = :numJournal
              AND PPRS_IDE_PPR = :pprsIdePpr
              AND NUM_ITEM BETWEEN :startItemNumber AND :endItemNumber
            ORDER BY NUM_ITEM
            """;

        AddParameter(command, "numJournal", journalNumber);
        AddParameter(command, "pprsIdePpr", posId);
        AddParameter(command, "startItemNumber", startItemNumber);
        AddParameter(command, "endItemNumber", endItemNumber);

        var externalRefs = new Dictionary<decimal, string>();

        using var reader = await ExecuteReaderAsync(command, cancellationToken);
        while (await ReadAsync(reader, cancellationToken))
        {
            var itemNumber = Convert.ToDecimal(reader["NUM_ITEM"]);
            var externalRef = Convert.ToString(reader["IDE_REGLEMENT"]);

            if (string.IsNullOrWhiteSpace(externalRef))
            {
                return Result<IReadOnlyDictionary<decimal, string>>.Failure(
                    localizer[LocalizationKeys.SnelBillerReglementIdeEmpty, journalNumber, posId, itemNumber],
                    AppHttpStatusCodes.Failed);
            }

            if (!externalRefs.TryAdd(itemNumber, externalRef))
            {
                return Result<IReadOnlyDictionary<decimal, string>>.Failure(
                    localizer[LocalizationKeys.SnelBillerReglementDuplicateItem, itemNumber, journalNumber, posId],
                    AppHttpStatusCodes.Failed);
            }
        }

        if (externalRefs.Count != expectedCount)
        {
            return Result<IReadOnlyDictionary<decimal, string>>.Failure(
                localizer[LocalizationKeys.SnelBillerReglementRowCountMismatch, externalRefs.Count, journalNumber, posId, startItemNumber, endItemNumber, expectedCount],
                AppHttpStatusCodes.Failed);
        }

        return Result<IReadOnlyDictionary<decimal, string>>.Success(externalRefs);
    }

    private Result<string> TryGetAreaCode(PaymentOrderItemEntity item)
    {
        var billingRef = item.Invoice?.BillingRef?.Trim();
        if (string.IsNullOrWhiteSpace(billingRef))
        {
            return Result<string>.Failure(
                localizer[LocalizationKeys.BillingApplySnelSettingsInvalid],
                AppHttpStatusCodes.BadRequest);
        }

        var separatorIndex = billingRef.LastIndexOf('-');
        if (separatorIndex <= 0)
        {
            return Result<string>.Failure(
                localizer[LocalizationKeys.SnelBillerBillingRefInvalid, billingRef],
                AppHttpStatusCodes.BadRequest);
        }

        var areaCode = billingRef[..separatorIndex].Trim();
        if (string.IsNullOrWhiteSpace(areaCode))
        {
            return Result<string>.Failure(
                localizer[LocalizationKeys.SnelBillerBillingRefAreaEmpty, billingRef],
                AppHttpStatusCodes.BadRequest);
        }

        return Result<string>.Success(areaCode);
    }

    private static void AddParameter(IDbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    private async Task<IDataReader> ExecuteReaderAsync(IDbCommand command, CancellationToken ct)
    {
        SnelOracleCommandLogger.LogCommand(logger, loggingOptions.Value.Enabled, "ExecuteReader", command);

        if (command is DbCommand dbCommand)
            return await dbCommand.ExecuteReaderAsync(ct);

        return command.ExecuteReader();
    }

    private async Task<int> ExecuteNonQueryAsync(IDbCommand command, CancellationToken ct)
    {
        SnelOracleCommandLogger.LogCommand(logger, loggingOptions.Value.Enabled, "ExecuteNonQuery", command);

        if (command is DbCommand dbCommand)
            return await dbCommand.ExecuteNonQueryAsync(ct);

        return command.ExecuteNonQuery();
    }

    private static async Task<bool> ReadAsync(IDataReader reader, CancellationToken ct)
    {
        if (reader is DbDataReader dbReader)
            return await dbReader.ReadAsync(ct);

        return reader.Read();
    }

    private sealed record EligibleApplyEntry(
        PaymentEntity Payment,
        PaymentOrderItemEntity Item);

    public override Result<List<CustomActionsOutputModel>> GetCustomActions(string billingServiceCode)
    {
        switch (billingServiceCode)
        {
            case "SNEL":
                return Result<List<CustomActionsOutputModel>>.Success(new List<CustomActionsOutputModel>());
            case "CONNECT-AFRICA":
                {
                    return Result<List<CustomActionsOutputModel>>.Success(new List<CustomActionsOutputModel>() {
                        new CustomActionsOutputModel( 
                            CustomActionCodesEnum.LAST_TRANSACTION_STATUS.ToString(),
                            localizer[LocalizationKeys.BillerCustomKeys.SNEL.LastTransactionStatus],
                            new List<CustomActionsParamsOutputModel>() { 
                            
                            }
                        ),
                        new CustomActionsOutputModel(
                            CustomActionCodesEnum.LIST_TRANSACTIONS.ToString(),
                            localizer[LocalizationKeys.BillerCustomKeys.SNEL.ListTransactions],
                            new List<CustomActionsParamsOutputModel>() {
                                new CustomActionsParamsOutputModel( 
                                    localizer[LocalizationKeys.BillerCustomKeys.SNEL.CountOfTransactionsToList],
                                    CustomActionVariableCodesEnum.COUNT.ToString(),
                                    "int",
                                    5
                                ),
                            }
                        ),
                        new CustomActionsOutputModel(
                            CustomActionCodesEnum.REPRINT.ToString(),
                            localizer[LocalizationKeys.BillerCustomKeys.SNEL.Reprint],
                            new List<CustomActionsParamsOutputModel>() {
                                new CustomActionsParamsOutputModel(
                                    localizer[LocalizationKeys.BillerCustomKeys.SNEL.ReceiptNo],
                                    CustomActionVariableCodesEnum.RECEIPT_NO.ToString(),
                                    "text",
                                    String.Empty
                                ),
                            }
                        ),
                    });
                }
            default:
                throw new NotImplementedException($"Custom actions for Billing service '{billingServiceCode}' are not supported.");
        }
    }

    public override async Task<Result<object>> ExecuteCustomAction(string billingServiceCode, string billingRef, string actionCode, Dictionary<string, object> parameters)
    {
     
        switch (billingServiceCode)
        {
            case "SNEL":
                throw new NotImplementedException();
            case "CONNECT-AFRICA":
                {
                    if (Enum.TryParse<CustomActionCodesEnum>(actionCode, out CustomActionCodesEnum action))
                    {
                        switch (action)
                        {
                            case CustomActionCodesEnum.LAST_TRANSACTION_STATUS:
                                logger.LogInformation(
                                    "Connect Africa LastTransactionStatusAsync request. Meter={Meter}",
                                    billingRef.Trim());
                                var lastTransactionResponse = await connectAfricaHttpClient.LastTransactionStatusAsync(
                                    new ConnectAfricaLastTransactionStatusRequest(Meter: billingRef.Trim())
                                    );
                                logger.LogInformation(
                                    "Connect Africa LastTransactionStatusAsync response: TransactionId={TransactionId}, Status={Status}",
                                    lastTransactionResponse?.TransactionId,
                                    lastTransactionResponse?.Status);

                                if (lastTransactionResponse == null)
                                {
                                    return Result<object>.Failure(
                                    localizer[LocalizationKeys.BillerCustomKeys.Actions.Error, actionCode],
                                    AppHttpStatusCodes.BadRequest);
                                }

                                return Result<object>.Success(new Dictionary<string, object>() {
                                    { localizer[LocalizationKeys.BillerCustomKeys.SNEL.TransactionId],lastTransactionResponse.TransactionId!},
                                    { localizer[LocalizationKeys.BillerCustomKeys.SNEL.TransactionStatus],lastTransactionResponse.Status!}

                                });
                            case CustomActionCodesEnum.REPRINT:

                                var receiptNumbner = parameters[CustomActionVariableCodesEnum.RECEIPT_NO.ToString()] + "";
                                if (string.IsNullOrWhiteSpace(receiptNumbner))
                                {
                                    return Result<object>.Failure(
                                    localizer[LocalizationKeys.BillerCustomKeys.Actions.Error, actionCode],
                                    AppHttpStatusCodes.BadRequest);
                                }
                                else
                                {
                                }
                                logger.LogInformation(
                                    "Connect Africa ReprintAsync request. ReceiptNumber={ReceiptNumber}",
                                    receiptNumbner);
                                var reprintResponse = await connectAfricaHttpClient.ReprintAsync(
                                    new ConnectAfricaReprintRequest(receiptNumbner));
                                logger.LogInformation(
                                    "Connect Africa ReprintAsync response. ReceiptNumber={ReceiptNumber}, Tokens count={TokenCount}",
                                    reprintResponse?.ReceiptNumber,
                                    reprintResponse?.Tokens?.Count);

                                if (reprintResponse == null)
                                {
                                    return Result<object>.Failure(
                                    localizer[LocalizationKeys.BillerCustomKeys.Actions.Error, actionCode],
                                    AppHttpStatusCodes.BadRequest);
                                }

                                return Result<object>.Success(new Dictionary<string, object>() {
                                    { localizer[LocalizationKeys.BillerCustomKeys.SNEL.ReceiptNumber],reprintResponse.ReceiptNumber!},
                                    { localizer[LocalizationKeys.BillerCustomKeys.SNEL.Tokens],reprintResponse.Tokens!}

                                });
                            case CustomActionCodesEnum.LIST_TRANSACTIONS:
                                if (!int.TryParse(parameters[CustomActionVariableCodesEnum.COUNT.ToString()] + "", out int count))
                                {

                                    return Result<object>.Failure(
                                        localizer[LocalizationKeys.BillerCustomKeys.Actions.Error, actionCode],
                                        AppHttpStatusCodes.BadRequest);
                                }
                                if (count <= 0)
                                {
                                    count = 5;
                                }
                                logger.LogInformation(
                                    "Connect Africa ListAsync request. BillingRef={BillingRef}, Count={Count}, Page={Page}",
                                    billingRef,
                                    count,
                                    1);
                                var listResponse = await connectAfricaHttpClient.ListAsync(
                                    new ConnectAfricaTokenListRequest(billingRef, count, 1));
                                logger.LogInformation(
                                    "Connect Africa ListAsync response. Tokens count={Count}",
                                    listResponse?.Tokens?.Count);

                                if (listResponse == null)
                                {
                                    return Result<object>.Failure(
                                    localizer[LocalizationKeys.BillerCustomKeys.Actions.Error, actionCode],
                                    AppHttpStatusCodes.BadRequest);
                                }

                                return Result<object>.Success(new Dictionary<string, object>() {
                                    { localizer[LocalizationKeys.BillerCustomKeys.SNEL.LastNTransactions,count],listResponse.Tokens.Select(x=> new Dictionary<string,object>() {
                                        {localizer[LocalizationKeys.BillerCustomKeys.SNEL.TransactionReceiptNo],x.ReceiptNumber },
                                        {localizer[LocalizationKeys.BillerCustomKeys.SNEL.TransactionAmount],x.Amount},

                                    }).ToList()}

                                });
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        return Result<object>.Failure(localizer[LocalizationKeys.BillerCustomKeys.Actions.Unknown, actionCode], AppHttpStatusCodes.BadRequest);
                    }
                }
            default:
                throw new NotImplementedException($"Custom action {actionCode} for Billing service '{billingServiceCode}' are not supported.");
        }
    }
}
