using AutoMapper;
using Medallion.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using SNEL.WalletPlatform.Application.Accounting.Interfaces;
using SNEL.WalletPlatform.Application.Billers.Interfaces;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.BillingServices.Models.Input;
using SNEL.WalletPlatform.Application.Caching;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Enums;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Common.Models.Input;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.FileExport.Services;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Invoicing.Models;
using SNEL.WalletPlatform.Application.Jobs;
using SNEL.WalletPlatform.Application.Lookups.Interfaces;
using SNEL.WalletPlatform.Application.Notification;
using SNEL.WalletPlatform.Application.Notification.Enums;
using SNEL.WalletPlatform.Application.Notification.Models.Internal;
using SNEL.WalletPlatform.Application.Notification.Services;
using SNEL.WalletPlatform.Application.Partners.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Configurations;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Mapper;
using SNEL.WalletPlatform.Application.Payments.Models.ConnectAfrica;
using SNEL.WalletPlatform.Application.Payments.Models.Input;
using SNEL.WalletPlatform.Application.Payments.Models.Internal;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Application.Reporting.Interfaces;
using SNEL.WalletPlatform.Application.Reporting.Models;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Accounting.Enums;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Domain.Users;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using static SNEL.WalletPlatform.Application.Payments.Services.PaymentService;


namespace SNEL.WalletPlatform.Application.Payments.Services;

public sealed class PaymentService(
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUserContext,
    IMapper mapper,
    IStringLocalizer<SharedResource> localizer,
    IBillerService billerService,
    IPaymentProviderService paymentProviderService,
    IBillingServiceService billingServiceService,
    IPspConnectorResolver pspConnectorResolver,
    IBillerConnectorResolver billerConnectorResolver,
    IInitPaymentServiceResolver initPaymentServiceResolver,
    IPaymentRefGenerator paymentRefGenerator,
    IAccountingService accountingService,
    ISchedulerFactory schedulerFactory,
    ITemplateRednerer templateRenderer,
    IHtmlToPdfGenerator htmlToPdfConverter,
    IDistributedLockProvider lockProvider,
ILogger<PaymentService> logger,
PaymentHistoryMapper paymentHistoryMapper,
NotificationService notificationService,
ExcelFileExportService excelFileExportService,
IDistributedLockProvider distributedLockProvider,
    IPaymentProviderAccessPolicy paymentProviderAccessPolicy,
    IBillerAccessPolicy billerAccessPolicy,
    IOptions<PaymentPrintOptions> paymentPrintOptions,
    IAkibaPayStatusPollingScheduler akibaPayStatusPollingScheduler,
    IEasypayStatusPollingScheduler easypayStatusPollingScheduler,
    IAfrimonyStatusPollingScheduler afrimonyStatusPollingScheduler,
    IAkibaPayPaymentInitContext akibaPayPaymentInitContext,
    IEasypayPaymentInitContext easypayPaymentInitContext,
    IAfrimonyPaymentInitContext afrimonyPaymentInitContext,
    IMpesaPaymentInitContext mpesaPaymentInitContext,
    IOrangePaymentInitContext orangePaymentInitContext,
    IPlatformConstantService platformConstantService,
    IBillingInvoiceRefResolver billingInvoiceRefResolver) : IPaymentService

{
    private const string WebChannelCode = "WEB";
    private const string MeterChargeCurrencyCode = "976"; // CDF
    private const string PostpaidBillingServiceTypeCode = "POSTPAID";
    private const string PrepaidBillingServiceTypeCode = "PREPAID";
    private const string AkibaPayProviderCode = "AKIPA-PAY";
    private const string EasypayProviderCode = "EASYPAY";
    private const string EasypayCardMethodCode = "CARD_COLLECTIONS";
    private const string AfrimonyProviderCode = "AFRIMONEY";
    private const string MpesaProviderCode = "MPESA";
    private const string OrangeProviderCode = "ORANGE";
    private const string PlatformPartnerTypeCode = "PLATFORM";
    private const string DefaultPaymentMethodCode = "TELECOM_COMPANIES";
    private sealed record UserPartnerAccess(long PartnerId, bool IsPlatform);

    #region Public Methods
    public async Task<Result<bool>> TestLock(string name, TimeSpan timeout)
    {
        await using var locker = await lockProvider.TryAcquireLockAsync(name, timeout);
        if (locker is null)
        {
            return Result<bool>.Failure(localizer[LocalizationKeys.PaymentLockAcquireFailed], AppHttpStatusCodes.Conflict);
        }
        Thread.Sleep(10000);

        return Result<bool>.Success(true);
    }
    public async Task<Result<PagedResult<PaymentWithOrderAndItemsOutputModel>>> GetPaymentHistoryAsync(
        string languageCode,
        PaymentHistoryQueryInputModel model,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currentUserContext.UserId))
        {
            return Result<PagedResult<PaymentWithOrderAndItemsOutputModel>>.Failure(
                localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.Unauthorized);
        }

        var customer = await GetCurrentCustomerByPartnerAsync(cancellationToken);
        if (customer is null)
        {
            return Result<PagedResult<PaymentWithOrderAndItemsOutputModel>>.Failure(
                localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.Unauthorized);
        }

        var normalizedQuery = NormalizePaymentHistoryQuery(model, customer.Id);
        var payments =
            await unitOfWork.PaymentRepository.GetPaymentHistoryByCustomerIdAsync(
                normalizedQuery,
                cancellationToken);
        var totalCount = await unitOfWork.PaymentRepository.CountPaymentHistoryByCustomerIdAsync(
            normalizedQuery,
            cancellationToken);
        var items = paymentHistoryMapper.Map(payments,languageCode);

        return Result<PagedResult<PaymentWithOrderAndItemsOutputModel>>.Success(
            new PagedResult<PaymentWithOrderAndItemsOutputModel>(items, totalCount));
    }

    public async Task<Result<PagedResult<PaymentWithOrderAndItemsOutputModel>>> GetScopePaymentHistoryAsync(
        string languageCode,
        PaymentHistoryQueryInputModel model,
        CancellationToken cancellationToken = default)
    {
        var partnerAccess = await GetCurrentUserPartnerAccessAsync(cancellationToken);
        if (partnerAccess is null)
        {
            return Result<PagedResult<PaymentWithOrderAndItemsOutputModel>>.Failure(
                localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.NotFound);
        }

        var normalizedQuery = NormalizePaymentHistoryQuery(model, Guid.Empty);
        long? scopedBillerPartnerId = partnerAccess.IsPlatform ? null : partnerAccess.PartnerId;
        var payments = await unitOfWork.PaymentRepository.GetPaymentHistoryByScopeAsync(
            normalizedQuery,
            scopedBillerPartnerId,
            cancellationToken);
        var totalCount = await unitOfWork.PaymentRepository.CountPaymentHistoryByScopeAsync(
            normalizedQuery,
            scopedBillerPartnerId,
            cancellationToken);
        var items = paymentHistoryMapper.Map(payments, languageCode);

        return Result<PagedResult<PaymentWithOrderAndItemsOutputModel>>.Success(
            new PagedResult<PaymentWithOrderAndItemsOutputModel>(items, totalCount));
    }

    public async Task<Result<FileContentResult>> ExportScopePaymentHistoryAsync(
        string languageCode,
        PaymentHistoryQueryInputModel model,
        CancellationToken cancellationToken = default)
    {
        var partnerAccess = await GetCurrentUserPartnerAccessAsync(cancellationToken);
        if (partnerAccess is null)
        {
            return Result<FileContentResult>.Failure(
                localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.NotFound);
        }
        model.PageNumber = 1;
        model.PageSize = int.MaxValue;
        var normalizedQuery = NormalizePaymentHistoryQuery(model, Guid.Empty);
        long? scopedBillerPartnerId = partnerAccess.IsPlatform ? null : partnerAccess.PartnerId;
        var payments = await unitOfWork.PaymentRepository.GetPaymentHistoryByScopeAsync(
            normalizedQuery,
            scopedBillerPartnerId,
            cancellationToken);
        var items = paymentHistoryMapper.MapPaymentExcel(payments, languageCode);
        var data_objects = mapper.Map<List<object>>(items);
        var export = await excelFileExportService.ExportExcelFileAsync("PaymentHistory", data_objects, typeof(PaymentExcelOutputModel));
        if (!export.IsSuccess)
        {
            return export;
        }

        return Result<FileContentResult>.Success(
            export.Value!,
            localizer[LocalizationKeys.PaymentTopupRateExportSuccess],
            AppHttpStatusCodes.Ok);
    }
    public async Task<Result<PagedResult<PaymentWithOrderAndItemsOutputModel>>> GetTopUpPaymentHistoryAsync(
        string languageCode,
        TopUpPaymentHistoryQueryInputModel model,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currentUserContext.UserId))
        {
            return Result<PagedResult<PaymentWithOrderAndItemsOutputModel>>.Failure(
                localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.Unauthorized);
        }

        var customer = await GetCurrentCustomerByPartnerAsync(cancellationToken);
        if (customer is null)
        {
            return Result<PagedResult<PaymentWithOrderAndItemsOutputModel>>.Failure(
                localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.Unauthorized);
        }

        var normalizedQuery = NormalizeTopUpPaymentHistoryQuery(model, customer.Id);
        var payments =
            await unitOfWork.PaymentRepository.GetTopUpPaymentHistoryByCustomerIdAsync(
                normalizedQuery,
                cancellationToken);
        var totalCount = await unitOfWork.PaymentRepository.CountTopUpPaymentHistoryByCustomerIdAsync(
            normalizedQuery,
            cancellationToken);
        var items = paymentHistoryMapper.Map(payments, languageCode);

        return Result<PagedResult<PaymentWithOrderAndItemsOutputModel>>.Success(
            new PagedResult<PaymentWithOrderAndItemsOutputModel>(items, totalCount));
    }

    public async Task<Result<PagedResult<PaymentWithOrderAndItemsOutputModel>>> GetScopeTopUpPaymentHistoryAsync(
        string languageCode,
        TopUpPaymentHistoryQueryInputModel model,
        CancellationToken cancellationToken = default)
    {
        var partnerAccess = await GetCurrentUserPartnerAccessAsync(cancellationToken);
        if (partnerAccess is null)
        {
            return Result<PagedResult<PaymentWithOrderAndItemsOutputModel>>.Failure(
                localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.NotFound);
        }

        var normalizedQuery = NormalizeTopUpPaymentHistoryQuery(model, Guid.Empty);
        long? scopedProviderPartnerId = partnerAccess.IsPlatform ? null : partnerAccess.PartnerId;
        var payments = await unitOfWork.PaymentRepository.GetTopUpPaymentHistoryByScopeAsync(
            normalizedQuery,
            scopedProviderPartnerId,
            cancellationToken);
        var totalCount = await unitOfWork.PaymentRepository.CountTopUpPaymentHistoryByScopeAsync(
            normalizedQuery,
            scopedProviderPartnerId,
            cancellationToken);
        var items = paymentHistoryMapper.Map(payments, languageCode);

        return Result<PagedResult<PaymentWithOrderAndItemsOutputModel>>.Success(
            new PagedResult<PaymentWithOrderAndItemsOutputModel>(items, totalCount));
    }

    public async Task<Result<FileContentResult>> ExportScopeTopUpPaymentHistoryAsync(
        string languageCode,
        TopUpPaymentHistoryQueryInputModel model,
        CancellationToken cancellationToken = default)
    {
        var partnerAccess = await GetCurrentUserPartnerAccessAsync(cancellationToken);
        if (partnerAccess is null)
        {
            return Result<FileContentResult>.Failure(
                localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.NotFound);
        }
        model.PageNumber = 1;
        model.PageSize = int.MaxValue;
        var normalizedQuery = NormalizeTopUpPaymentHistoryQuery(model, Guid.Empty);
        long? scopedProviderPartnerId = partnerAccess.IsPlatform ? null : partnerAccess.PartnerId;        
        var payments = await unitOfWork.PaymentRepository.GetTopUpPaymentHistoryByScopeAsync(
            normalizedQuery,
            scopedProviderPartnerId,
            cancellationToken);
        var items = paymentHistoryMapper.MapTopupExcel(payments, languageCode);
        var data_objects = mapper.Map<List<object>>(items);
        var export = await excelFileExportService.ExportExcelFileAsync("TopupHistory", data_objects, typeof(TopupExcelOutputModel));
        if (!export.IsSuccess)
        {
            return export;
        }

        return Result<FileContentResult>.Success(
            export.Value!,
            localizer[LocalizationKeys.PaymentTopupRateExportSuccess],
            AppHttpStatusCodes.Ok);        
    }

    public async Task<Result<PaymentWithOrderAndItemsOutputModel>> GetPaymentDetailsAsync(
        string languageCode,
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var payment = await unitOfWork.PaymentRepository.GetByIdWithHistoryContextAsync(paymentId, cancellationToken);
        if (payment is null)
        {
            return Result<PaymentWithOrderAndItemsOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentNotFound],
                AppHttpStatusCodes.NotFound);
        }

        var output = paymentHistoryMapper.Map(payment, languageCode);
        return Result<PaymentWithOrderAndItemsOutputModel>.Success(output);
    }

    public async Task<Result<PagedResult<PaymentTxnOutputModel>>> GetPaymentTransactionsAsync(
        Guid paymentId,
        PaymentTxnQueryInputModel model,
        CancellationToken cancellationToken = default)
    {
        if (paymentId == Guid.Empty)
        {
            return Result<PagedResult<PaymentTxnOutputModel>>.Failure(
                localizer[LocalizationKeys.PaymentIdRequired],
                AppHttpStatusCodes.BadRequest);
        }

        var partnerAccess = await GetCurrentUserPartnerAccessAsync(cancellationToken);
        if (partnerAccess is null)
        {
            return Result<PagedResult<PaymentTxnOutputModel>>.Failure(
                localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.NotFound);
        }

        var isBiller = await billerAccessPolicy.GetBillersReadScopeAsync(cancellationToken);
        var isPsp = await paymentProviderAccessPolicy.GetPaymentProvidersReadScopeAsync(cancellationToken);

        var accessContext = await unitOfWork.PaymentRepository.GetPaymentAccessContextAsync(paymentId, cancellationToken);
        if ((accessContext is null
            || (!partnerAccess.IsPlatform && accessContext.ProviderPartnerId != partnerAccess.PartnerId))
            && !isBiller.IsSuccess && !isPsp.IsSuccess
            )
        {
            return Result<PagedResult<PaymentTxnOutputModel>>.Failure(
                localizer[LocalizationKeys.PaymentNotFound],
                AppHttpStatusCodes.NotFound);
        }

        var normalizedQuery = NormalizePaymentTxnQuery(model);
        var paymentTxns = await unitOfWork.PaymentTxnRepository.GetByPaymentIdAsync(
            paymentId,
            normalizedQuery.PageNumber,
            normalizedQuery.PageSize,
            cancellationToken);
        var totalCount = await unitOfWork.PaymentTxnRepository.CountByPaymentIdAsync(paymentId, cancellationToken);
        var output = mapper.Map<IReadOnlyList<PaymentTxnOutputModel>>(paymentTxns);

        return Result<PagedResult<PaymentTxnOutputModel>>.Success(
            new PagedResult<PaymentTxnOutputModel>(output, totalCount));
    }

    public async Task<Result<byte[]>> GetPaymentPdfAsync(Guid paymentId, string? acceptLanguage,
        CancellationToken cancellationToken = default)
    {
        if (paymentId == Guid.Empty)
        {
            return Result<byte[]>.Failure(localizer[LocalizationKeys.PaymentIdRequired], AppHttpStatusCodes.BadRequest);
        }

        var modelResult = await BuildPaymentPrintModelAsync(paymentId, acceptLanguage, cancellationToken);
        if (!modelResult.IsSuccess)
            return Result<byte[]>.Failure(modelResult.Message, modelResult.StatusCode);

        var templateName = ResolvePaymentPrintTemplate(modelResult.Value!);
        var html = await templateRenderer.RenderAsync(templateName, modelResult.Value!, cancellationToken);
        // Top-up uses A4 layout; prepaid/postpaid use 80mm thermal.
        var pageOptions = templateName == "Payment/payment-topup"
            ? null
            : PdfPageOptions.Thermal80Mm;
        var pdfBytes = await htmlToPdfConverter.GenerateFromHtmlAsync(
            html,
            cancellationToken,
            pageOptions);
        return Result<byte[]>.Success(
            pdfBytes,
            localizer[LocalizationKeys.FileDownloadSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<PaymentInitializationOutputModel>> InitTopupPayment(PaymentInitializeInputModel model)
    {
        try
        {
            #region Validation and fetching related entities
            var topupModelValidationResult = await ValidatePaymentTopupModel(model);
            if (!topupModelValidationResult.IsSuccess)
            {
                return topupModelValidationResult.ToFailure<PaymentInitializationOutputModel>();
            }

            (
        CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        bool phoneNumberRequired
        ) = topupModelValidationResult.Value;

            if(phoneNumberRequired)
            {
                var previousNumbers = unitOfWork.PaymentRepository.GetAllQueryable(x => x.Direction == PaymentDirectionEnum.IN && !string.IsNullOrEmpty(x.PhoneNumber) && x.CreatedBy == currentUserContext.UserId).OrderByDescending(x => x.UpdatedAtUtc).Distinct().Take(5).Select(x => x.PhoneNumber).ToArray();
                return Result<PaymentInitializationOutputModel>.Success(
                             new PaymentInitializationOutputModel(
                                     PaymentId: Guid.Empty,
                                     PaymentInternalReference: null,
                                     Items: [],
                                     NextStepInfo: new PaymentNextActionOutputModel(PaymentNextActionEnum.ExtraInfoRequired, new ExtraInfoNextActionModel(
                                         Message: localizer[LocalizationKeys.PaymentPspInitNeedPhoneNumber],
                                         PreviousNumbers: previousNumbers
                                     ))),
                             localizer[LocalizationKeys.PaymentPspInitNeedPhoneNumber],
                             AppHttpStatusCodes.Ok);
            }

            #endregion

            // 2. Execution
            // 2.a resolve the psp connector
            #region Creating Payment Artifacts in a transaction
            var payment = await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                //var effectivePaymentType = IsEasypayCardMethod(
                //    paymentMethodCurrency.PaymentMethod?.PaymentProvider?.Code,
                //    paymentMethodCurrency.PaymentMethod?.Code)
                //    ? PaymentTypeEnum.BANK_CARD
                //    : model.PaymentType;
                var payment = await CreateTopupPaymentArtifactsAsync(
                    customer,
                    channel,
                    customerWalletAccount,
                    paymentMethodCurrency,
                    model.PaymentType,
                    model.Amount);
                payment.SetPhoneNumber(model.FromNumber);
                await unitOfWork.CompleteAsync();
                return payment;
            });
            #endregion

            #region Initializing payment with PSP
            if (payment == null)
            {
                logger.LogError("Failed to create payment artifacts.");
                return Result<PaymentInitializationOutputModel>.Failure(
                    localizer[LocalizationKeys.PaymentTopupInitNoPaymentRecord],
                    AppHttpStatusCodes.Failed);
            }
            if (string.Equals(
                pspConnector.PaymentServiceProviderCode,
                AkibaPayProviderCode,
                StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(model.FromNumber))
            {
                akibaPayPaymentInitContext.FromNumber = NormalizeFromNumber(model.FromNumber);
            }
            if (string.Equals(
                pspConnector.PaymentServiceProviderCode,
                EasypayProviderCode,
                StringComparison.OrdinalIgnoreCase))
            {
                easypayPaymentInitContext.FromNumber = string.IsNullOrWhiteSpace(model.FromNumber)
                    ? null
                    : NormalizeFromNumber(model.FromNumber);
                easypayPaymentInitContext.Email = model.Email;
                easypayPaymentInitContext.Description = model.Description;
                easypayPaymentInitContext.ReturnUrl = model.ReturnUrl;
                easypayPaymentInitContext.SendPaymentLinkByEmail = model.SendPaymentLinkByEmail;
            }
            if (string.Equals(
                pspConnector.PaymentServiceProviderCode,
                AfrimonyProviderCode,
                StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(model.FromNumber))
            {
                afrimonyPaymentInitContext.FromNumber = NormalizeFromNumber(model.FromNumber);
            }
            if (string.Equals(
                pspConnector.PaymentServiceProviderCode,
                MpesaProviderCode,
                StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(model.FromNumber))
            {
                mpesaPaymentInitContext.FromNumber = NormalizeFromNumber(model.FromNumber);
            }
            if (string.Equals(
                pspConnector.PaymentServiceProviderCode,
                OrangeProviderCode,
                StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(model.FromNumber))
            {
                orangePaymentInitContext.FromNumber = NormalizeFromNumber(model.FromNumber);
            }
            return await InititalizePaymentwithPSP(payment, paymentMethodCurrency, pspConnector);
            #endregion            
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error initializing topup payment for customer {CustomerId} with amount {Amount} and currency {CurrencyCode}",
                currentUserContext.UserId, model.Amount, model.CurrencyCode);
            return Result<PaymentInitializationOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentTopupInitUnexpectedError],
                AppHttpStatusCodes.Failed);
        }
    }

    private async Task<Result<PaymentInitializationOutputModel>> InititalizePaymentwithPSP(PaymentEntity payment, PaymentMethodCurrencyEntity paymentMethodCurrency, PspConnector connector)
    {
        var paymentMethod = payment.PaymentMethod!;
        switch ((PaymentInitModeEnum)payment.PaymentMethod!.PaymentInitModeId)
        {
            case PaymentInitModeEnum.CLIENT_SIDE:
                {
                    var paymentInitiationResult = await connector.GetDataForPaymentInitiation(payment);
                    if (!paymentInitiationResult.IsSuccess)
                    {
                        await unitOfWork.ExecuteInTransactionAsync(async ct =>
                        {
                            //TODO: Delete the artifactes (PO,POI,Payment) instead of just marking as failed
                            payment.ChangePaymentArtifactStatus(PaymentStatus.Failed);
                            await HandlePaymentControlsAsync(payment.Id);
                            await unitOfWork.CompleteAsync();
                        });
                        return paymentInitiationResult.ToFailure<PaymentInitializationOutputModel>();
                    }
                    else
                    {
                        if (payment.PaymentMethod!.CompletionModeId == (int)CompletionModeEnum.POLLING)
                        {
                            await TriggerPollingJob(payment.Id, payment.PaymentMethod.Config);
                        }
                        return Result<PaymentInitializationOutputModel>.Success(
                            new PaymentInitializationOutputModel(
                                    PaymentId: payment.Id,
                                    PaymentInternalReference: payment.PaymentRef,
                                    Items: PaymentOrderItemSummaryListMapper.MapItems(mapper, payment.PaymentOrder!.Items, payment.Direction),
                                    NextStepInfo: new PaymentNextActionOutputModel(PaymentNextActionEnum.InitializeClientSideFromWeb, paymentInitiationResult.Value.Data)),
                            localizer[LocalizationKeys.PaymentPspInitClientPrepared],
                            AppHttpStatusCodes.Ok);
                    }
                }
            case PaymentInitModeEnum.SERVER_SIDE:
                {
                    //initiate payment with PSP and fail payment-artifacts if initiation fails.
                    var paymentInitiationResult = await connector.InitiatePayment(payment);
                    if (!paymentInitiationResult.IsSuccess)
                    {
                        await unitOfWork.ExecuteInTransactionAsync(async ct =>
                        {
                            //TODO: Delete the artifactes (PO,POI,Payment) instead of just marking as failed
                            payment.ChangePaymentArtifactStatus(PaymentStatus.Failed);
                            await HandlePaymentControlsAsync(payment.Id);
                            await unitOfWork.CompleteAsync();
                        });
                        return paymentInitiationResult.ToFailure<PaymentInitializationOutputModel>();
                    }
                    await unitOfWork.ExecuteInTransactionAsync(async ct =>
                    {
                        payment.ChangePaymentArtifactStatus(PaymentStatus.Processing);
                        await unitOfWork.CompleteAsync();
                    });

                    // now payment is intitated with PSP successfully, we need to check the confirmation mode and act accordingly
                    switch ((ConfirmationModeEnum)paymentMethod.ConfirmationModeId)
                    {
                        case ConfirmationModeEnum.EXTERNAL://confirmation happen on PSP side, we just need to poll for the status.
                            {
                                if (payment.PaymentMethod!.CompletionModeId == (int)CompletionModeEnum.POLLING)
                                {
                                    await TriggerPollingJob(payment.Id, payment.PaymentMethod.Config);
                                }
                                else if (string.Equals(
                                    connector.PaymentServiceProviderCode,
                                    AkibaPayProviderCode,
                                    StringComparison.OrdinalIgnoreCase))
                                {
                                    akibaPayStatusPollingScheduler.Schedule(payment.Id);
                                }
                                else if (string.Equals(
                                    connector.PaymentServiceProviderCode,
                                    EasypayProviderCode,
                                    StringComparison.OrdinalIgnoreCase))
                                {
                                    easypayStatusPollingScheduler.Schedule(payment.Id);
                                }
                                else if (string.Equals(
                                    connector.PaymentServiceProviderCode,
                                    AfrimonyProviderCode,
                                    StringComparison.OrdinalIgnoreCase))
                                {
                                    afrimonyStatusPollingScheduler.Schedule(payment.Id);
                                }
                                return Result<PaymentInitializationOutputModel>.Success(
                                    new PaymentInitializationOutputModel(
                                        PaymentId: payment.Id,
                                        PaymentInternalReference: payment.PaymentRef,
                                        Items: PaymentOrderItemSummaryListMapper.MapItems(mapper, payment.PaymentOrder!.Items, payment.Direction),
                                        NextStepInfo: new PaymentNextActionOutputModel(
                                            paymentInitiationResult.Value!.PaymentNextAction,
                                            paymentInitiationResult.Value.Data)),
                                    localizer[LocalizationKeys.PaymentPspInitProviderPending],
                                    AppHttpStatusCodes.Ok);
                            }
                        case ConfirmationModeEnum.PLATFORM://confirmation happen on platform side, we need to check the confirmation type and act accordingly
                            {
                                switch ((ConfirmationTypeEnum)paymentMethod.ConfirmationTypeId)
                                {
                                    case ConfirmationTypeEnum.NONE:
                                        {
                                            var confirmResult = await connector.ConfirmPayment(payment.Id);
                                            if (!confirmResult.IsSuccess)
                                            {
                                                return confirmResult.ToFailure<PaymentInitializationOutputModel>();
                                            }
                                            await unitOfWork.ExecuteInTransactionAsync(async ct =>
                                            {
                                                payment.SetExternalRef(confirmResult.Value!.PaymentExternalRef);
                                                await unitOfWork.CompleteAsync();
                                            });

                                            switch ((CompletionModeEnum)paymentMethod.CompletionModeId)
                                            {
                                                case CompletionModeEnum.IMMEDIATE:
                                                    {
                                                        bool isTransactional = false;
                                                        if (payment.Direction == PaymentDirectionEnum.OUT)
                                                        {
                                                            isTransactional = payment.PaymentOrder!.Items.Any(x => x.BillingService!.IsTransactional);
                                                            var isOnPay = payment.PaymentOrder!.Items.Any(x => x.BillingService!.Currencies.Any(z => z.CurrencyId == payment.CurrencyId && z.ApplyType == ApplyTypeEnum.ON_PAY));
                                                            isTransactional &= isOnPay;
                                                        }
                                                        await unitOfWork.ExecuteInTransactionAsync(async ct =>
                                                        {
                                                            payment.ChangePaymentArtifactStatus(PaymentStatus.Completed);
                                                            await HandlePaymentControlsAsync(payment.Id);
                                                            var journalsForPaymentResult = await CreateJournalsForPayment(payment);
                                                            var (journals, accounts, platformInvoices) = journalsForPaymentResult.Value;
                                                            accounts = await unitOfWork.AccountRepository.GetByIdsWithLockAsync(accounts.Select(a => a.Id).ToArray());
                                                            foreach (var journal in journals)
                                                            {
                                                                foreach (JournalLineEntity line in journal.Lines)
                                                                {
                                                                    var account = accounts.First(x => x.Id == line.AccountId);
                                                                    line.Balance = account.Balance + line.Credit - line.Debit;
                                                                    account.SetBalance(line.Balance);
                                                                }
                                                            }

                                                            await TrackPlatformInvoicesAndJournalsForPaymentAsync(platformInvoices, journals, ct);
                                                            unitOfWork.AccountRepository.UpdateRange(accounts);
                                                            if (isTransactional)
                                                            {
                                                                var applyResult = await billingServiceService.ApplyPaymentAfterCompleteAsync(payment);
                                                                if (applyResult.IsSuccess && applyResult.Value)
                                                                    await unitOfWork.CompleteAsync();                                                                
                                                                else if(!applyResult.IsSuccess)                                                                
                                                                    throw new Exception(applyResult.ToFailure<bool>().Message);
                                                                else
                                                                    throw new Exception(localizer[LocalizationKeys.UnexpectedError]);
                                                            }                                                            
                                                            await unitOfWork.CompleteAsync();
                                                        });

                                                        await SendNotificationForPaymentAsync(payment);
                                                        if (!isTransactional)
                                                        {
                                                            var applyResult = await billingServiceService.ApplyPaymentAfterCompleteAsync(payment);
                                                            if (applyResult.IsSuccess)
                                                            {
                                                                await unitOfWork.CompleteAsync();
                                                            }
                                                        }
                                                        ////poi status and external-ref will be updated in the ApplyPayment method
                                                        //await unitOfWork.CompleteAsync();

                                                        return Result<PaymentInitializationOutputModel>.Success(
                                                            new PaymentInitializationOutputModel(
                                                            PaymentId: payment.Id,
                                                            Items: PaymentOrderItemSummaryListMapper.MapItems(mapper, payment.PaymentOrder.Items, payment.Direction),
                                                            PaymentInternalReference: payment.PaymentRef!,
                                                            NextStepInfo: new PaymentNextActionOutputModel(PaymentNextActionEnum.None, new NoActionNextActionModel(localizer[LocalizationKeys.PaymentCompletedAndConfirmedSuccessfully]))),
                                                            localizer[LocalizationKeys.PaymentPspInitCompletedInSession],
                                                            AppHttpStatusCodes.Ok);

                                                    }
                                                case CompletionModeEnum.POLLING:
                                                    {
                                                        await TriggerPollingJob(payment.Id, payment.PaymentMethod.Config);
                                                        return Result<PaymentInitializationOutputModel>.Success(
                                                            new PaymentInitializationOutputModel(
                                                        PaymentId: payment.Id,
                                                        PaymentInternalReference: payment.PaymentRef!,
                                                        Items: PaymentOrderItemSummaryListMapper.MapItems(mapper, payment.PaymentOrder!.Items, payment.Direction),
                                                        NextStepInfo: new PaymentNextActionOutputModel(PaymentNextActionEnum.None, new NoActionNextActionModel(localizer[LocalizationKeys.PaymentConfirmedAndWillCompletedExternally]))),
                                                            localizer[LocalizationKeys.PaymentPspInitProviderPending],
                                                            AppHttpStatusCodes.Ok);
                                                    }
                                                case CompletionModeEnum.WEBHOOK:
                                                    {
                                                        return Result<PaymentInitializationOutputModel>.Success(
                                                            new PaymentInitializationOutputModel(
                                                        PaymentId: payment.Id,
                                                        PaymentInternalReference: payment.PaymentRef!,
                                                        Items: PaymentOrderItemSummaryListMapper.MapItems(mapper, payment.PaymentOrder!.Items, payment.Direction),
                                                        NextStepInfo: new PaymentNextActionOutputModel(PaymentNextActionEnum.None, new NoActionNextActionModel(localizer[LocalizationKeys.PaymentConfirmedAndWillCompletedExternally]))),
                                                            localizer[LocalizationKeys.PaymentPspInitProviderPending],
                                                            AppHttpStatusCodes.Ok);
                                                    }
                                                default:
                                                    throw new NotImplementedException($"Payment completion mode '{payment.PaymentMethod!.CompletionMode}' is not implemented.");
                                            }
                                        }
                                        break;
                                    case ConfirmationTypeEnum.OTP:
                                    case ConfirmationTypeEnum.PROMPT://for both OTP and PROMPT, we will return the next step info to the client and client will call confirm API to confirm the payment, so we need to get the next step info here.
                                        {
                                            var nextStepInfoResult = await connector.GetNextStepInfo(payment);
                                            if (!nextStepInfoResult.IsSuccess)
                                            {
                                                return nextStepInfoResult.ToFailure<PaymentInitializationOutputModel>();
                                            }
                                            return Result<PaymentInitializationOutputModel>.Success(
                                                new PaymentInitializationOutputModel(
                                                    PaymentId: payment.Id,
                                                    PaymentInternalReference: payment.PaymentRef,
                                                    Items: PaymentOrderItemSummaryListMapper.MapItems(mapper, payment.PaymentOrder.Items, payment.Direction),
                                                    NextStepInfo: new PaymentNextActionOutputModel(PaymentNextActionEnum.Confirm, nextStepInfoResult.Value.Data)),
                                                localizer[LocalizationKeys.PaymentPspInitProviderPending],
                                                AppHttpStatusCodes.Ok);
                                        }
                                    default:
                                        throw new NotImplementedException($"Payment confirmation type '{(ConfirmationTypeEnum)payment.PaymentMethod!.ConfirmationTypeId}' is not implemented.");
                                }
                            }
                        case ConfirmationModeEnum.NONE:
                            {
                                switch ((CompletionModeEnum)paymentMethod.CompletionModeId)
                                {
                                    case CompletionModeEnum.IMMEDIATE:
                                        {
                                            bool isTransactional = false;
                                            if (payment.Direction == PaymentDirectionEnum.OUT)
                                            {
                                                isTransactional = payment.PaymentOrder!.Items.Any(x => x.BillingService!.IsTransactional);
                                                var isOnPay = payment.PaymentOrder!.Items.Any(x => x.BillingService!.Currencies.Any(z => z.CurrencyId == payment.CurrencyId && z.ApplyType == ApplyTypeEnum.ON_PAY));
                                                isTransactional &= isOnPay;
                                            }
                                            await unitOfWork.ExecuteInTransactionAsync(async ct =>
                                            {
                                                payment.ChangePaymentArtifactStatus(PaymentStatus.Completed);
                                                await HandlePaymentControlsAsync(payment.Id);
                                                var journalsForPaymentResult = await CreateJournalsForPayment(payment);
                                                var (journals, accounts, platformInvoices) = journalsForPaymentResult.Value;
                                                accounts = await unitOfWork.AccountRepository.GetByIdsWithLockAsync(accounts.Select(a => a.Id).ToArray());
                                                foreach (var journal in journals)
                                                {
                                                    foreach (JournalLineEntity line in journal.Lines)
                                                    {
                                                        var account = accounts.First(x => x.Id == line.AccountId);
                                                        line.Balance = account.Balance + line.Credit - line.Debit;
                                                        account.SetBalance(line.Balance);
                                                    }
                                                }

                                                await TrackPlatformInvoicesAndJournalsForPaymentAsync(platformInvoices, journals, ct);
                                                unitOfWork.AccountRepository.UpdateRange(accounts);
                                                if (isTransactional)
                                                {
                                                    var applyResult = await billingServiceService.ApplyPaymentAfterCompleteAsync(payment);
                                                    if (applyResult.IsSuccess && applyResult.Value)
                                                        await unitOfWork.CompleteAsync();
                                                    else if (!applyResult.IsSuccess)
                                                        throw new Exception(applyResult.ToFailure<bool>().Message);
                                                    else
                                                        throw new Exception(localizer[LocalizationKeys.UnexpectedError]);
                                                }
                                                await unitOfWork.CompleteAsync();
                                            });
                                            await SendNotificationForPaymentAsync(payment);
                                            if (!isTransactional)
                                            {
                                                var applyResult = await billingServiceService.ApplyPaymentAfterCompleteAsync(payment);
                                                if (applyResult.IsSuccess)
                                                {
                                                    await unitOfWork.CompleteAsync();
                                                }
                                            }

                                            return Result<PaymentInitializationOutputModel>.Success(
                                                new PaymentInitializationOutputModel(
                                                PaymentId: payment.Id,
                                                Items: PaymentOrderItemSummaryListMapper.MapItems(mapper, payment.PaymentOrder!.Items, payment.Direction),
                                                PaymentInternalReference: payment.PaymentRef!,
                                                NextStepInfo: new PaymentNextActionOutputModel(PaymentNextActionEnum.None, new NoActionNextActionModel(localizer[LocalizationKeys.PaymentCompletedAndConfirmedSuccessfully]))),
                                                localizer[LocalizationKeys.PaymentPspInitCompletedInSession],
                                                AppHttpStatusCodes.Ok);
                                        }
                                    case CompletionModeEnum.POLLING:
                                        {
                                            await TriggerPollingJob(payment.Id, payment.PaymentMethod.Config);
                                            return Result<PaymentInitializationOutputModel>.Success(
                                                new PaymentInitializationOutputModel(
                                            PaymentId: payment.Id,
                                            PaymentInternalReference: payment.PaymentRef!,
                                            Items: PaymentOrderItemSummaryListMapper.MapItems(mapper, payment.PaymentOrder!.Items, payment.Direction),
                                            NextStepInfo: new PaymentNextActionOutputModel(PaymentNextActionEnum.None, new NoActionNextActionModel(localizer[LocalizationKeys.PaymentConfirmedAndWillCompletedExternally]))),
                                            localizer[LocalizationKeys.PaymentPspInitProviderPending],
                                            AppHttpStatusCodes.Ok);
                                        }
                                    case CompletionModeEnum.WEBHOOK:
                                        {
                                            return Result<PaymentInitializationOutputModel>.Success(
                                                new PaymentInitializationOutputModel(
                                            PaymentId: payment.Id,
                                            PaymentInternalReference: payment.PaymentRef!,
                                            Items: PaymentOrderItemSummaryListMapper.MapItems(mapper, payment.PaymentOrder!.Items, payment.Direction),
                                            NextStepInfo: new PaymentNextActionOutputModel(PaymentNextActionEnum.None, new NoActionNextActionModel(localizer[LocalizationKeys.PaymentConfirmedAndWillCompletedExternally]))),
                                            localizer[LocalizationKeys.PaymentPspInitProviderPending],
                                            AppHttpStatusCodes.Ok);
                                        }
                                    default:
                                        throw new NotImplementedException($"Payment completion mode '{payment.PaymentMethod!.CompletionMode}' is not implemented.");
                                }
                            }
                        default:
                            throw new NotImplementedException($"Payment confirmation mode '{payment.PaymentMethod!.ConfirmationMode}' is not implemented.");
                    }
                }
            default: throw new NotImplementedException($"Payment initiation mode '{(PaymentInitModeEnum)payment.PaymentMethod!.PaymentInitModeId}' is not implemented.");
        }
    }

    public async Task<Result<PaymentSummaryOutputModel>> TopupSummary(PaymentInitializeInputModel model)
    {
        #region Validation and fetching related entities
        logger.LogInformation("START_SUMMARY_1");
        var topupModelValidationResult = await ValidatePaymentTopupModel(model);
        logger.LogInformation("START_SUMMARY_1");
        if (!topupModelValidationResult.IsSuccess)
        {
            return topupModelValidationResult.ToFailure<PaymentSummaryOutputModel>();
        }

        (
            CustomerEntity customer,
            AccountEntity customerWalletAccount,
            ChannelEntity channel,
            PaymentMethodCurrencyEntity paymentMethodCurrency,
            PspConnector pspConnector,
            bool phoneNumberRequired
        ) = topupModelValidationResult.Value;


        var resolvedCodesResult = await ResolveTopupPaymentMethodCodesAsync(model);
        if (!resolvedCodesResult.IsSuccess)
        {
            return resolvedCodesResult.ToFailure<PaymentSummaryOutputModel>();
        }
        
        var (paymentServiceProviderCode, paymentMethodCode, phoneNumberRequired1) = resolvedCodesResult.Value;

        #endregion

        var chargeRecord = paymentMethodCurrency.CalculateCharges(model.Amount);

        return Result<PaymentSummaryOutputModel>.Success(
            new PaymentSummaryOutputModel
        {
            CurrencyAbbreviation = paymentMethodCurrency.Currency.Abbreviation,
            PaymentServiceProviderCode = paymentServiceProviderCode,
            PaymentMethodCode = paymentMethodCode,
            TotalAmount = chargeRecord.TotalAmount,
            Amount = chargeRecord.Amount,
            Fees = chargeRecord.Fees,
            FeesTax = chargeRecord.FeesTax,
            Items = new List<PaymentSummaryItemModel>(),
            RequirePhoneNumber = phoneNumberRequired1
        },
            localizer[LocalizationKeys.PaymentTopupSummaryCalculated],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<PaymentInitializationOutputModel>> InitMeterChargePayment(
        InitMeterChargePaymentInputModel model)
    {
        #region Validation and fetching related entities
        var paymentModelValidationCheck = await ValidatePaymentMeterRechargeModel(model);
        if (!paymentModelValidationCheck.IsSuccess) {
            return paymentModelValidationCheck.ToFailure<PaymentInitializationOutputModel>();
        }
        (
        CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        BillingServiceCurrencyEntity billingServiceCurrency) = paymentModelValidationCheck.Value;


        #endregion
        #region Creating Payment Artifacts in a transaction
        var payment = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var payment = await CreateMeterChargePaymentArtifactsAsync(
                customer,
                channel,
                customerWalletAccount,
                paymentMethodCurrency,
                billingServiceCurrency,
                 model.MeterNumber.Trim(),
                model.Amount);
            await unitOfWork.CompleteAsync();
            return payment;
        });
        #endregion
        #region Initializing payment with PSP
        if (payment == null)
        {
            logger.LogError("Failed to create payment artifacts.");
            return Result<PaymentInitializationOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentMeterInitNoPaymentRecord],
                AppHttpStatusCodes.Failed);
        }
        return await InititalizePaymentwithPSP(payment, paymentMethodCurrency, pspConnector);
        #endregion
        //return await InitPayment(
        //    account: account,
        //    paymentMethod: paymentMethod,
        //    amount: model.Amount,
        //    currency: currency,
        //    customer: customer,
        //    connector: pspConnector,
        //    preArtifacts: null,
        //    createArtifacts: () => CreateMeterChargePaymentArtifactsAsync(
        //        customer.Id,
        //        channel.Id,
        //        account,
        //        paymentMethod,
        //        billingServiceCurrency,
        //        model.MeterNumber.Trim(),
        //        model.Amount));
    }

    public async Task<Result<PaymentSummaryOutputModel>> MeterRechargePaymentSummary(InitMeterChargePaymentInputModel model)
    {
        var paymentModelValidationCheck = await ValidatePaymentMeterRechargeModel(model);
        if (!paymentModelValidationCheck.IsSuccess) {
            return paymentModelValidationCheck.ToFailure<PaymentSummaryOutputModel>();
        }

        (
        CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        BillingServiceCurrencyEntity billingServiceCurrency) = paymentModelValidationCheck.Value;


        var chargeRecord = billingServiceCurrency.CalculateCharges(model.Amount);
        return Result<PaymentSummaryOutputModel>.Success(
            new PaymentSummaryOutputModel
        {
            CurrencyAbbreviation = paymentMethodCurrency.Currency.Abbreviation,
            TotalAmount = chargeRecord.TotalAmount,
            Amount = chargeRecord.Amount,
            Fees = chargeRecord.Fees,
            FeesTax = chargeRecord.FeesTax,
            Items = new List<PaymentSummaryItemModel>()
        },
            localizer[LocalizationKeys.PaymentMeterSummaryCalculated],
            AppHttpStatusCodes.Ok);
    }

    public Task<Result<bool>> ConfirmTopupPayment(Guid paymentId)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<PaymentInitializationOutputModel>> InitBillPayment(PayBillsInputModel model)
    {
        #region Check Can Pay

        var lockKey = $"{LockNames.SNEL_DAILY_JOB}";
        var locker = await distributedLockProvider.TryAcquireLockAsync(lockKey, TimeSpan.FromSeconds(0));
        if (locker is null)
        {
            logger.LogError($"Failed to initiate payment, SNEL journal is locked");
            return Result<PaymentInitializationOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentLockAcquireFailed],
                AppHttpStatusCodes.Conflict);
        }
        await locker.DisposeAsync();

        #endregion
        #region Validation and fetching related entities

        var paymentModelValidationResult = await ValidatePaymentBillModel(model);
        if (!paymentModelValidationResult.IsSuccess) {
            return paymentModelValidationResult.ToFailure<PaymentInitializationOutputModel>();
        }

        (CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        IReadOnlyList<InvoicePaymentDetailInternalModel> bills) = paymentModelValidationResult.Value;        

        #endregion

        #region Creating Payment Artifacts in a transaction
        logger.LogInformation(
            "InitBillPayment creating artifacts for customer {CustomerId}, invoices {InvoiceCount}, amount {Amount}, currency {CurrencyCode}, paymentMethod {PaymentMethodCode}, psp {PspCode}",
            customer.Id,
            bills.Count,
            model.Amount,
            paymentMethodCurrency.Currency!.Code,
            paymentMethodCurrency.PaymentMethod!.Code,
            paymentMethodCurrency.PaymentMethod.PaymentProvider?.Code);

        var payment = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var payment = await CreateBillPaymentArtifactsAsync(
                customer,
                channel,
                customerWalletAccount,
                paymentMethodCurrency,
                bills,
                model.Amount);
            await unitOfWork.CompleteAsync();
            return payment;
        });
        #endregion
        #region Initializing payment with PSP
        if (payment == null)
        {
            logger.LogError(
                "Failed to create payment artifacts in InitBillPayment for customer {CustomerId}, invoices {InvoiceIds}, amount {Amount}. Transaction returned null.",
                customer.Id,
                model.InvoicesIds is { Count: > 0 } ids
                    ? string.Join(",", ids)
                    : string.Join(",", model.InvoiceRefs ?? []),
                model.Amount);
            return Result<PaymentInitializationOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentBillInitNoPaymentRecord],
                AppHttpStatusCodes.Conflict);
        }
        return await InititalizePaymentwithPSP(payment, paymentMethodCurrency, pspConnector);
        #endregion

    }

    public async Task<Result<PaymentSummaryOutputModel>> BillPaymentSummary(PayBillsInputModel model)
    {
        //var lockKey = $"{LockNames.SNEL_DAILY_JOB}";
        //var locker = await distributedLockProvider.TryAcquireLockAsync(lockKey, TimeSpan.FromSeconds(0));
        //if (locker is null)
        //{
        //    logger.LogError($"Failed to get summary, SNEL journal is locked");
        //    return Result<PaymentSummaryOutputModel>.Failure(
        //        localizer[LocalizationKeys.PaymentLockAcquireFailed],
        //        AppHttpStatusCodes.Conflict);
        //}
        //await locker.DisposeAsync();
        var paymentModelValidationResult = await ValidatePaymentBillModel(model);
        if (!paymentModelValidationResult.IsSuccess)
        {
            return paymentModelValidationResult.ToFailure<PaymentSummaryOutputModel>();
        }

        (CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        IReadOnlyList<InvoicePaymentDetailInternalModel> bills) = paymentModelValidationResult.Value;


        BillingChargesRecord chargesRecord = new BillingChargesRecord(
            Fees: bills.Sum(x => x.ChargesRecord.Fees),
            FeesTax: bills.Sum(x => x.ChargesRecord.FeesTax),
            Cost: bills.Sum(x => x.ChargesRecord.Cost),
            CostTax: bills.Sum(x => x.ChargesRecord.CostTax),
            Commission: bills.Sum(x => x.ChargesRecord.Commission),
            CommissionTax: bills.Sum(x => x.ChargesRecord.CommissionTax),
            Amount: bills.Sum(x => x.ChargesRecord.Amount),
            TotalAmount: bills.Sum(x => x.ChargesRecord.TotalAmount)
        );
        return Result<PaymentSummaryOutputModel>.Success(
            new PaymentSummaryOutputModel
        {
            CurrencyAbbreviation = bills.First().Currency.Abbreviation,
            TotalAmount = chargesRecord.TotalAmount,
            Amount = chargesRecord.Amount,
            Fees = chargesRecord.Fees,
            FeesTax = chargesRecord.FeesTax,
            Items = bills.Select(x =>
                new PaymentSummaryItemModel()
                {
                    Reference = x.Invoice.InvoiceRef,
                    Date = x.Invoice.IssueDate,
                    Amount = x.Amount
                }).ToList()
        },
            localizer[LocalizationKeys.PaymentBillSummaryCalculated],
            AppHttpStatusCodes.Ok);
    }



    public async Task<Result<PaymentConfirmationOutputModel>> ConfirmBillPayment(Guid paymentId)
    {
        string lockName = $"{LockNames.PAYMENT}:{paymentId}";
        await using var locker = await lockProvider.TryAcquireLockAsync(lockName, TimeSpan.FromSeconds(0));
        if (locker is null)
        {
            return Result<PaymentConfirmationOutputModel>.Failure(localizer[LocalizationKeys.PaymentLockAcquireFailed], AppHttpStatusCodes.Conflict);
        }

        if (paymentId == Guid.Empty)
        {
            return Result<PaymentConfirmationOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentIdRequired],
                AppHttpStatusCodes.BadRequest);
        }

        var customerValidation = await ValidateCustomerAsync();
        if (!customerValidation.IsSuccess)
        {
            return customerValidation.ToFailure<PaymentConfirmationOutputModel>();
        }

        var customer = customerValidation.Value;



        //var result = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        //{
        var payment = await unitOfWork.PaymentRepository.GetByIdWithConfirmContextAsync(paymentId);
        var validationResult = ValidatePaymentForPspSync(payment);
        if (!validationResult.IsSuccess)
        {
            return Result<PaymentConfirmationOutputModel>.Failure(
                validationResult.Message,
                validationResult.StatusCode);
        }
        if (payment.PaymentOrder is null || payment.PaymentOrder.CustomerId != customer.Id)
        {
            return Result<PaymentConfirmationOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentNotFound],
                AppHttpStatusCodes.NotFound);
        }

        if (payment.Direction != PaymentDirectionEnum.OUT)
        {
            return Result<PaymentConfirmationOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentConfirmNotOutgoingBillOrMeter, paymentId],
                AppHttpStatusCodes.BadRequest);
        }

        if (payment.Status is not (PaymentStatus.Pending or PaymentStatus.Processing))
        {
            return Result<PaymentConfirmationOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentConfirmInvalidStatus, paymentId, payment.Status],
                AppHttpStatusCodes.BadRequest);
        }
        var paymentMethodValidation = await ValidatePaymentMethodAgainstCurrencyAndAmountAsync(payment.PaymentMethod!.Code,
                payment.PaymentMethod.PaymentProvider!.Code, payment.Currency!, payment.Amount);
        if (!paymentMethodValidation.IsSuccess)
        {
            return paymentMethodValidation.ToFailure<PaymentConfirmationOutputModel>();
        }

        var (paymentMethod, paymentMethodCurrency, connector) = paymentMethodValidation.Value;

        var confirmResult = await connector.ConfirmPayment(payment.Id);
        if (!confirmResult.IsSuccess)
        {
            return confirmResult.ToFailure<PaymentConfirmationOutputModel>();
        }
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            payment.SetExternalRef(confirmResult.Value!.PaymentExternalRef);
            await unitOfWork.CompleteAsync();
        });

        switch ((CompletionModeEnum)payment.PaymentMethod!.CompletionModeId)
        {
            case CompletionModeEnum.IMMEDIATE:
                {
                    bool isTransactional = false;
                    if (payment.Direction == PaymentDirectionEnum.OUT)
                    {
                        isTransactional = payment.PaymentOrder!.Items.Any(x => x.BillingService!.IsTransactional);
                        var isOnPay = payment.PaymentOrder!.Items.Any(x => x.BillingService!.Currencies.Any(z => z.CurrencyId == payment.CurrencyId && z.ApplyType == ApplyTypeEnum.ON_PAY));
                        isTransactional &= isOnPay;
                    }
                    logger.LogInformation("Payment '{0}' is Transactional = {1}", payment.Id, isTransactional);
                    await unitOfWork.ExecuteInTransactionAsync(async ct =>
                    {
                        payment.ChangePaymentArtifactStatus(PaymentStatus.Completed);
                        await HandlePaymentControlsAsync(payment.Id);
                        var journalsForPaymentResult = await CreateJournalsForPayment(payment);
                        var (journals, accounts, platformInvoices) = journalsForPaymentResult.Value;
                        accounts = await unitOfWork.AccountRepository.GetByIdsWithLockAsync(accounts.Select(a => a.Id).ToArray());
                        foreach (var journal in journals)
                        {
                            foreach (JournalLineEntity line in journal.Lines)
                            {
                                var account = accounts.First(x => x.Id == line.AccountId);
                                line.Balance = account.Balance + line.Credit - line.Debit;
                                account.SetBalance(line.Balance);
                            }
                        }

                        await TrackPlatformInvoicesAndJournalsForPaymentAsync(platformInvoices, journals, ct);
                        unitOfWork.AccountRepository.UpdateRange(accounts);
                        if (isTransactional)
                        {
                            var applyResult = await billingServiceService.ApplyPaymentAfterCompleteAsync(payment);
                            if (applyResult.IsSuccess && applyResult.Value)
                                await unitOfWork.CompleteAsync();
                            else if (!applyResult.IsSuccess)
                                throw new Exception(applyResult.ToFailure<bool>().Message);
                            else
                                throw new Exception(localizer[LocalizationKeys.UnexpectedError]);
                        }
                        await unitOfWork.CompleteAsync();
                    });
                    await SendNotificationForPaymentAsync(payment);
                    if (!isTransactional)
                    {
                        var applyResult = await billingServiceService.ApplyPaymentAfterCompleteAsync(payment);
                        if (applyResult.IsSuccess)
                        {
                            await unitOfWork.CompleteAsync();
                        }
                    }
                    //var applyResult = await billingServiceService.ApplyPaymentAfterCompleteAsync(payment);
                    //if (applyResult.IsSuccess)
                    //{
                        //poi status and external-ref will be updated in the ApplyPayment method
                    //    await unitOfWork.CompleteAsync();
                    //}

                    return Result<PaymentConfirmationOutputModel>.Success(
                        new PaymentConfirmationOutputModel(
                        PaymentId: payment.Id,
                        PaymentInternalReference: payment.PaymentRef!,
                        PaymentExternalReference: payment.ExternalRef!,
                                                Items: PaymentOrderItemSummaryListMapper.MapItems(mapper, payment.PaymentOrder!.Items, payment.Direction)),
                        localizer[LocalizationKeys.PaymentBillConfirmSuccess],
                        AppHttpStatusCodes.Ok);
                }
            case CompletionModeEnum.POLLING:
                {
                    await TriggerPollingJob(payment.Id, payment.PaymentMethod.Config);
                    return Result<PaymentConfirmationOutputModel>.Success(
                        new PaymentConfirmationOutputModel(
                        PaymentId: payment.Id,
                        PaymentInternalReference: payment.PaymentRef!,
                        PaymentExternalReference: payment.ExternalRef!,
                                                Items: PaymentOrderItemSummaryListMapper.MapItems(mapper, payment.PaymentOrder!.Items, payment.Direction)),
                        localizer[LocalizationKeys.PaymentBillConfirmSuccess],
                        AppHttpStatusCodes.Ok);
                }
            case CompletionModeEnum.WEBHOOK:
                {
                    return Result<PaymentConfirmationOutputModel>.Success(
                        new PaymentConfirmationOutputModel(
                        PaymentId: payment.Id,
                        PaymentInternalReference: payment.PaymentRef!,
                        PaymentExternalReference: payment.ExternalRef!,
                                                Items: PaymentOrderItemSummaryListMapper.MapItems(mapper, payment.PaymentOrder!.Items, payment.Direction)),
                        localizer[LocalizationKeys.PaymentBillConfirmSuccess],
                        AppHttpStatusCodes.Ok);
                }
            default:
                throw new NotImplementedException($"Payment completion mode '{payment.PaymentMethod!.CompletionMode}' is not implemented.");
        }
        //});
    }

    public async Task<Result<PaymentConfirmationOutputModel>> TerminatePaymentAfterInitAsync(
        Guid paymentId,
        PaymentStatus terminationStatus = PaymentStatus.Cancelled)
    {
        if (!terminationStatus.IsTermination())
        {
            return Result<PaymentConfirmationOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentCancelInvalidFinalStatus],
                AppHttpStatusCodes.BadRequest);
        }

        string lockName = $"{LockNames.PAYMENT}:{paymentId}";
        await using var locker = await lockProvider.TryAcquireLockAsync(lockName, TimeSpan.FromSeconds(0));
        if (locker is null)
        {
            return Result<PaymentConfirmationOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentCancelLockConflict, paymentId],
                AppHttpStatusCodes.Conflict);
        }

        if (paymentId == Guid.Empty)
        {
            return Result<PaymentConfirmationOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentIdRequired],
                AppHttpStatusCodes.BadRequest);
        }
        var payment = await unitOfWork.PaymentRepository.GetByIdWithConfirmContextAsync(paymentId);
        if (payment is null)
        {
            return Result<PaymentConfirmationOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentNotFound],
                AppHttpStatusCodes.NotFound);
        }
        if (!PaymentStatusExtensions.ExpirableStatuses.Contains(payment.Status))
        {
            return Result<PaymentConfirmationOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentCancelNotAllowed],
                AppHttpStatusCodes.BadRequest);
        }
        if (payment.PaymentOrder is null)
        {
            return Result<PaymentConfirmationOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentNotFound],
                AppHttpStatusCodes.NotFound);
        }

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            payment.ChangePaymentArtifactStatus(terminationStatus);
            await HandlePaymentControlsAsync(payment.Id);
            await unitOfWork.CompleteAsync();
        });
        return Result<PaymentConfirmationOutputModel>.Success(
            new PaymentConfirmationOutputModel(
                PaymentId: payment.Id,
                PaymentInternalReference: payment.PaymentRef!,
                PaymentExternalReference: payment.ExternalRef ?? string.Empty,
                Items: PaymentOrderItemSummaryListMapper.MapItems(mapper, payment.PaymentOrder!.Items, payment.Direction)),
            localizer[LocalizationKeys.PaymentCancelSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<PaymentStatus>> ManualCompletePaymentAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        string lockName = $"{LockNames.PAYMENT}:{paymentId}";
        await using var locker = await lockProvider.TryAcquireLockAsync(lockName, TimeSpan.FromSeconds(0));
        if (locker is null)
        {
            return Result<PaymentStatus>.Failure(localizer[LocalizationKeys.PaymentLockAcquireFailed], AppHttpStatusCodes.Conflict);
        }

        if (paymentId == Guid.Empty)
        {
            return Result<PaymentStatus>.Failure(localizer[LocalizationKeys.PaymentIdRequired], AppHttpStatusCodes.BadRequest);
        }

        var partnerAccess = await GetCurrentUserPartnerAccessAsync(cancellationToken);
        if (partnerAccess is null || !partnerAccess.IsPlatform)
        {
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentManualCompletePlatformOnly],
                AppHttpStatusCodes.BadRequest);
        }

        var payment = await unitOfWork.PaymentRepository.GetByIdWithConfirmContextAsync(paymentId);
        var validationResult = ValidatePaymentForPspSync(payment);
        if (!validationResult.IsSuccess)
        {
            return validationResult.ToFailure<PaymentStatus>();
        }

        var completeResult = await CompletePaymentArtifactsAsync(payment!);
        if (!completeResult.IsSuccess)
        {
            return completeResult.ToFailure<PaymentStatus>();
        }

        return Result<PaymentStatus>.Success(PaymentStatus.Completed);
    }

    public async Task<Result<bool>> ManualApplyPaymentAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        string lockName = $"{LockNames.PAYMENT}:{paymentId}:apply";
        await using var locker = await lockProvider.TryAcquireLockAsync(lockName, TimeSpan.FromSeconds(0));
        if (locker is null)
        {
            return Result<bool>.Failure(localizer[LocalizationKeys.PaymentLockAcquireFailed], AppHttpStatusCodes.Conflict);
        }

        if (paymentId == Guid.Empty)
        {
            return Result<bool>.Failure(localizer[LocalizationKeys.PaymentIdRequired], AppHttpStatusCodes.BadRequest);
        }

        var partnerAccess = await GetCurrentUserPartnerAccessAsync(cancellationToken);
        if (partnerAccess is null || !partnerAccess.IsPlatform)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.PaymentManualApplyPlatformOnly],
                AppHttpStatusCodes.BadRequest);
        }

        return await billingServiceService.ApplyPayment(paymentId: paymentId);
    }

    public async Task<Result<bool>> ManualReconcilePoiAsync(
        Guid paymentId,
        string externalRef,
        CancellationToken cancellationToken = default)
    {
        string lockName = $"{LockNames.PAYMENT}:{paymentId}:reconcile-poi";
        await using var locker = await lockProvider.TryAcquireLockAsync(lockName, TimeSpan.FromSeconds(0));
        if (locker is null)
        {
            return Result<bool>.Failure(localizer[LocalizationKeys.PaymentLockAcquireFailed], AppHttpStatusCodes.Conflict);
        }

        if (paymentId == Guid.Empty)
        {
            return Result<bool>.Failure(localizer[LocalizationKeys.PaymentIdRequired], AppHttpStatusCodes.BadRequest);
        }

        if (string.IsNullOrWhiteSpace(externalRef))
        {
            return Result<bool>.Failure(localizer[LocalizationKeys.PaymentExternalRefRequired], AppHttpStatusCodes.BadRequest);
        }

        var partnerAccess = await GetCurrentUserPartnerAccessAsync(cancellationToken);
        if (partnerAccess is null || !partnerAccess.IsPlatform)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.PaymentManualReconcilePlatformOnly],
                AppHttpStatusCodes.BadRequest);
        }

        var payment = await unitOfWork.PaymentRepository.GetByIdAsync(paymentId);
        if (payment is null)
        {
            return Result<bool>.Failure(localizer[LocalizationKeys.PaymentNotFound], AppHttpStatusCodes.NotFound);
        }

        payment.SetExternalRef(externalRef);
        await unitOfWork.CompleteAsync();
        return Result<bool>.Success(true);
    }

    public async Task<Result<PaymentStatus>> SyncPaymentStatusWithPsp(
        Guid paymentId,
        int? statusPollingAttempt = null)
    {
        string lockName = $"{LockNames.PAYMENT}:{paymentId}";
        await using var locker = await lockProvider.TryAcquireLockAsync(lockName, TimeSpan.FromSeconds(0));
        if (locker is null)
        {
            logger.LogError($"Failed to acquire lock for payment status sync for payment {paymentId}. Another process may be syncing the same payment. Please check if there are any hanging sync processes and consider increasing the lock timeout if this issue happens frequently.", paymentId);
            return Result<PaymentStatus>.Failure(localizer[LocalizationKeys.PaymentLockAcquireFailed], AppHttpStatusCodes.Conflict);
        }

        var payment = await unitOfWork.PaymentRepository.GetByIdWithConfirmContextAsync(paymentId);
        var validationResult = ValidatePaymentForPspSync(payment);
        if (!validationResult.IsSuccess)
        {
            logger.LogInformation($"Payment {paymentId} failed validation for PSP sync with message: {validationResult.Message}", paymentId, validationResult.Message);
            return Result<PaymentStatus>.Failure(
                validationResult.Message,
                validationResult.StatusCode);
        }

        var paymentServiceProviderCode = validationResult.Value;

        // Resolve biller connector
        var connector = pspConnectorResolver.Resolve(paymentServiceProviderCode);
        if (connector is null)
        {
            logger.LogError($"PSP connector '{paymentServiceProviderCode}' is not registered for payment polling while syncing payment {paymentId} status.", paymentServiceProviderCode, paymentId);
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentPspConnectorNotRegisteredForPolling, paymentServiceProviderCode],
                AppHttpStatusCodes.BadRequest);
        }

        var paymentStatusContext = BuildPaymentTransactionStatusContext(payment!, statusPollingAttempt);

        // Get the Transaction status from PSP
        var paymentCheckStatusResult = await connector.GetTransactionStatus(paymentStatusContext);
        if (!paymentCheckStatusResult.IsSuccess)
        {
            logger.LogInformation($"Failed to get transaction status from PSP for payment {paymentId} with message: {paymentCheckStatusResult.Message}", paymentId, paymentCheckStatusResult.Message);
            return Result<PaymentStatus>.Failure(
                paymentCheckStatusResult.Message,
                paymentCheckStatusResult.StatusCode);
        }

        var paymentCheckStatusModel = paymentCheckStatusResult.Value;


        if (payment.Status == paymentCheckStatusModel!.Status)
        {
            logger.LogInformation($"Payment {paymentId} status is already '{paymentCheckStatusModel.Status}'. No update is needed.", paymentId, paymentCheckStatusModel.Status);
            return ToPaymentStatusResult(paymentCheckStatusModel);
        }


        //stop polling job if payment is no more in pending or processing status.
        if (paymentCheckStatusModel!.Status is not (PaymentStatus.Pending or PaymentStatus.Processing))
        {
            try
            {
                logger.LogInformation($"Going to stop transaction status for payment {payment.Id}, because its new status is {paymentCheckStatusModel.Status}");
                await StopPollingJobAsync(paymentId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to stop polling job for payment {PaymentId} after status sync. Please check and stop the job manually if it is still running.", paymentId);
            }
        }        
        // Update payment status, payment controls, and accounting in one DB transaction with a single SaveChanges.
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            payment.ChangePaymentArtifactStatus(paymentCheckStatusModel.Status);
            payment.SetExternalRef(paymentCheckStatusModel.PaymentExternalRef);
            if (paymentCheckStatusModel.Status.IsFinal())
            {
                await HandlePaymentControlsAsync(payment.Id);
            }
            if (paymentCheckStatusModel.Status == PaymentStatus.Completed)
            {
                var journalsForPaymentResult = await CreateJournalsForPayment(payment);
                var (journals, accounts, platformInvoices) = journalsForPaymentResult.Value;
                foreach (var journal in journals)
                {
                    foreach (JournalLineEntity line in journal.Lines)
                    {
                        var account = accounts.First(x => x.Id == line.AccountId);
                        line.Balance = account.Balance + line.Credit - line.Debit;
                        account.SetBalance(line.Balance);
                    }
                }
                await TrackPlatformInvoicesAndJournalsForPaymentAsync(platformInvoices, journals, ct);                
            }

            await unitOfWork.CompleteAsync();
        });
        await SendNotificationForPaymentAsync(payment);
        return ToPaymentStatusResult(paymentCheckStatusModel);
    }

    public async Task<Result<PaymentStatus>> ExpireStalePendingPaymentAsync(Guid paymentId)
    {
        if (paymentId == Guid.Empty)
        {
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentIdRequired],
                AppHttpStatusCodes.BadRequest);
        }

        var payment = await unitOfWork.PaymentRepository.GetByIdWithConfirmContextAsync(paymentId);
        if (payment is null)
        {
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentNotFound],
                AppHttpStatusCodes.NotFound);
        }

        if (!PaymentStatusExtensions.ExpirableStatuses.Contains(payment.Status))
        {
            return Result<PaymentStatus>.Success(payment.Status);
        }

        var validationResult = ValidatePaymentForPspSync(payment);
        if (validationResult.IsSuccess)
        {
            var connector = pspConnectorResolver.Resolve(validationResult.Value);
            if (connector is not null && connector.SupportsTransactionStatusPolling)
            {
                var syncResult = await SyncPaymentStatusWithPsp(paymentId);
                if (!syncResult.IsSuccess)
                {
                    return syncResult;
                }

                if (!PaymentStatusExtensions.ExpirableStatuses.Contains(syncResult.Value))
                {
                    return syncResult;
                }
            }
        }

        var terminateResult = await TerminatePaymentAfterInitAsync(paymentId, PaymentStatus.Expired);
        if (!terminateResult.IsSuccess)
        {
            return Result<PaymentStatus>.Failure(
                terminateResult.Message,
                terminateResult.StatusCode);
        }

        return Result<PaymentStatus>.Success(PaymentStatus.Expired);
    }

    public async Task<Result<int>> SyncInPendingPaymentsStatusWithPspAsync(
        int pageSize,
        int minimumAgeMinutes,
        CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0)
        {
            return Result<int>.Failure(localizer[LocalizationKeys.CommonValidationPageSizePositive], AppHttpStatusCodes.BadRequest);
        }
        if (minimumAgeMinutes <= 0)
        {
            return Result<int>.Failure(localizer[LocalizationKeys.CommonValidationMinimumAgePositive], AppHttpStatusCodes.BadRequest);
        }

        var targetStatuses = new[] { PaymentStatus.Pending, PaymentStatus.Processing };
        var totalSynced = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var createdBeforeUtc = DateTimeOffset.UtcNow.AddMinutes(-minimumAgeMinutes);
            var paymentIds = await unitOfWork.PaymentRepository.GetInPaymentIdsByStatusesAsync(
                targetStatuses,
                createdBeforeUtc,
                pageSize,
                cancellationToken);

            if (paymentIds.Count == 0)
            {
                break;
            }

            foreach (var paymentId in paymentIds)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                string lockName = $"{LockNames.PAYMENT}:{paymentId}";
                await using var locker = await lockProvider.TryAcquireLockAsync(lockName, TimeSpan.FromSeconds(0));
                if (locker is null)
                {
                    logger.LogError(
                        "Failed to acquire lock for payment status sync for payment {PaymentId}. Another process may be syncing the same payment.",
                        paymentId);
                    continue;
                }

                var payment = await unitOfWork.PaymentRepository.GetByIdWithConfirmContextAsync(paymentId);
                var validationResult = ValidatePaymentForPspSync(payment);
                if (!validationResult.IsSuccess)
                {
                    logger.LogInformation(
                        "Payment {PaymentId} failed validation for bulk PSP sync with message: {Message}",
                        paymentId,
                        validationResult.Message);
                    continue;
                }

                var paymentServiceProviderCode = validationResult.Value;
                var connector = pspConnectorResolver.Resolve(paymentServiceProviderCode);
                if (connector is null)
                {
                    logger.LogError(
                        "PSP connector '{PspCode}' is not registered for payment polling while syncing payment {PaymentId} status in bulk job.",
                        paymentServiceProviderCode,
                        paymentId);
                    continue;
                }

                var paymentStatusContext = new PaymentTransactionStatusContextInternalModel(
                    PaymentId: payment!.Id,
                    CurrencyId: payment.CurrencyId,
                    PaymentTransactionReference: payment.PaymentRef!,
                    PaymentExternalReference: payment.ExternalRef!,
                    PaymentMethod: payment.PaymentMethod!,
                    Amount: payment.Total);

                var paymentCheckStatusResult = await connector.GetTransactionStatus(paymentStatusContext);
                if (!paymentCheckStatusResult.IsSuccess)
                {
                    logger.LogInformation(
                        "Failed to get transaction status from PSP for payment {PaymentId} in bulk job with message: {Message}",
                        paymentId,
                        paymentCheckStatusResult.Message);
                    continue;
                }

                var paymentCheckStatusModel = paymentCheckStatusResult.Value;
                if (payment.Status == paymentCheckStatusModel!.Status)
                {
                    logger.LogInformation(
                        "Payment {PaymentId} status is already '{PaymentStatus}' in bulk job. No update is needed.",
                        paymentId,
                        paymentCheckStatusModel.Status);
                    totalSynced++;
                    continue;
                }

                if (paymentCheckStatusModel.Status is not (PaymentStatus.Pending or PaymentStatus.Processing))
                {
                    try
                    {
                        logger.LogInformation(
                            "Going to stop transaction status polling for payment {PaymentId} in bulk job, because its new status is {PaymentStatus}",
                            payment.Id,
                            paymentCheckStatusModel.Status);
                        await StopPollingJobAsync(paymentId);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(
                            ex,
                            "Failed to stop polling job for payment {PaymentId} after status sync in bulk job.",
                            paymentId);
                    }
                }                
                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    payment.ChangePaymentArtifactStatus(paymentCheckStatusModel.Status);
                    payment.SetExternalRef(paymentCheckStatusModel.PaymentExternalRef);
                    if (paymentCheckStatusModel.Status.IsFinal())
                    {
                        await HandlePaymentControlsAsync(payment.Id);
                    }

                    if (paymentCheckStatusModel.Status == PaymentStatus.Completed)
                    {
                        var journalsForPaymentResult = await CreateJournalsForPayment(payment);
                        var (journals, accounts, platformInvoices) = journalsForPaymentResult.Value;
                        foreach (var journal in journals)
                        {
                            foreach (JournalLineEntity line in journal.Lines)
                            {
                                var account = accounts.First(x => x.Id == line.AccountId);
                                line.Balance = account.Balance + line.Credit - line.Debit;
                                account.SetBalance(line.Balance);
                            }
                        }

                        await TrackPlatformInvoicesAndJournalsForPaymentAsync(platformInvoices, journals, ct);                        
                    }

                    await unitOfWork.CompleteAsync();
                }, cancellationToken);
                await SendNotificationForPaymentAsync(payment);
                totalSynced++;
            }
        }

        return Result<int>.Success(totalSynced);
    }

    public Task<Result<PaymentStatus>> CompletePaymentFromWebhook(Guid paymentId) =>
        ApplyAkibaPayWebhookOutcomeAsync(paymentId, "SUCCESS", akibaReference: null);

    public async Task<Result<PaymentStatus>> ApplyAkibaPayWebhookOutcomeAsync(
        Guid paymentId,
        string transactionStatus,
        string? akibaReference,
        CancellationToken cancellationToken = default)
    {
        if (paymentId == Guid.Empty)
        {
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentIdRequired],
                AppHttpStatusCodes.BadRequest);
        }

        if (!TryMapAkibaPayTransactionStatusToPaymentStatus(transactionStatus, out var newStatus))
        {
            return Result<PaymentStatus>.Failure(
                $"Unsupported AkibaPay transaction status '{transactionStatus}'.",
                AppHttpStatusCodes.BadRequest);
        }

        var payment = await unitOfWork.PaymentRepository.GetByIdWithPspContextAsync(paymentId);
        if (payment is null)
        {
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentNotFound],
                AppHttpStatusCodes.NotFound);
        }

        if (!string.IsNullOrWhiteSpace(akibaReference))
        {
            payment.SetExternalRef(akibaReference.Trim());
        }

        switch (newStatus)
        {
            case PaymentStatus.Completed:
                {
                    switch (payment.Status)
                    {
                        case PaymentStatus.Completed:
                            {
                                akibaPayStatusPollingScheduler.Cancel(paymentId);
                                return Result<PaymentStatus>.Success(PaymentStatus.Completed);
                            }
                            break;
                        case PaymentStatus.Processing:
                        case PaymentStatus.Pending:
                            {
                                var completeResult = await CompletePaymentArtifactsAsync(payment);
                                if (completeResult.IsSuccess)
                                {
                                    akibaPayStatusPollingScheduler.Cancel(paymentId);
                                }

                                return completeResult.IsSuccess
                                    ? Result<PaymentStatus>.Success(PaymentStatus.Completed)
                                    : Result<PaymentStatus>.Failure(completeResult.Message!, completeResult.StatusCode);
                            }
                            break;
                        case PaymentStatus.Expired:
                        case PaymentStatus.Cancelled:
                        case PaymentStatus.Failed:
                            {
                                var result = await SyncAkibaPayStatusFromManagementAsync(paymentId, true, cancellationToken);
                                if (!result.IsSuccess)
                                {
                                    return Result<PaymentStatus>.Failure(
                                        localizer[LocalizationKeys.PaymentWebhookCannotCompleteInStatus, paymentId, payment.Status],
                                        AppHttpStatusCodes.BadRequest);
                                }
                                else if (result.Value != PaymentStatus.Completed)
                                {
                                    akibaPayStatusPollingScheduler.Cancel(paymentId);
                                    return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                                }
                                else
                                {
                                    var completeResult = await CompletePaymentArtifactsAsync(payment);
                                    if (completeResult.IsSuccess)
                                    {
                                        akibaPayStatusPollingScheduler.Cancel(paymentId);
                                    }

                                    return completeResult.IsSuccess
                                        ? Result<PaymentStatus>.Success(PaymentStatus.Completed)
                                        : Result<PaymentStatus>.Failure(completeResult.Message!, completeResult.StatusCode);
                                }
                            }
                            break;
                    }
                }
                break;            
            case PaymentStatus.Failed:
                {
                    switch (payment.Status)
                    {
                        case PaymentStatus.Completed:
                            {
                                var result = await SyncAkibaPayStatusFromManagementAsync(paymentId, true, cancellationToken);
                                if (!result.IsSuccess)
                                {
                                    return Result<PaymentStatus>.Failure(
                                        localizer[LocalizationKeys.PaymentWebhookCannotCompleteInStatus, paymentId, payment.Status],
                                        AppHttpStatusCodes.BadRequest);
                                }
                                if (result.Value != PaymentStatus.Completed)
                                {
                                    await unitOfWork.ExecuteInTransactionAsync(async _ =>
                                    {
                                        //payment.ChangePaymentArtifactStatus(PaymentStatus.Failed);
                                        payment.SetNeedsReview(true);
                                        await HandlePaymentControlsAsync(payment.Id);
                                        await unitOfWork.CompleteAsync();
                                    }, cancellationToken);

                                    akibaPayStatusPollingScheduler.Cancel(paymentId);
                                    return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                                }
                                else
                                {
                                    akibaPayStatusPollingScheduler.Cancel(paymentId);
                                    return Result<PaymentStatus>.Success(PaymentStatus.Completed);
                                }
                                break;
                            }
                        case PaymentStatus.Failed:
                        case PaymentStatus.Cancelled:
                        case PaymentStatus.Expired:
                            {
                                akibaPayStatusPollingScheduler.Cancel(paymentId);
                                return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                            }
                        case PaymentStatus.Processing:
                        case PaymentStatus.Pending:
                            {
                                await unitOfWork.ExecuteInTransactionAsync(async _ =>
                                {
                                    payment.ChangePaymentArtifactStatus(PaymentStatus.Failed);
                                    await HandlePaymentControlsAsync(payment.Id);
                                    await unitOfWork.CompleteAsync();
                                }, cancellationToken);

                                akibaPayStatusPollingScheduler.Cancel(paymentId);
                                await SendNotificationForPaymentAsync(payment);
                                return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                            }
                    }
                    break;
                }
            case PaymentStatus.Pending:
                {
                    switch (payment.Status)
                    {
                        case PaymentStatus.Completed:
                            {
                                var result = await SyncAkibaPayStatusFromManagementAsync(paymentId, true, cancellationToken);
                                if (!result.IsSuccess)
                                {
                                    return Result<PaymentStatus>.Failure(
                                        localizer[LocalizationKeys.PaymentWebhookCannotCompleteInStatus, paymentId, payment.Status],
                                        AppHttpStatusCodes.BadRequest);
                                }
                                if (result.Value != PaymentStatus.Completed)
                                {
                                    await unitOfWork.ExecuteInTransactionAsync(async _ =>
                                    {
                                        //payment.ChangePaymentArtifactStatus(PaymentStatus.Failed);
                                        payment.SetNeedsReview(true);
                                        await HandlePaymentControlsAsync(payment.Id);
                                        await unitOfWork.CompleteAsync();
                                    }, cancellationToken);

                                    akibaPayStatusPollingScheduler.Cancel(paymentId);
                                    return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                                }
                                else
                                {
                                    akibaPayStatusPollingScheduler.Cancel(paymentId);
                                    return Result<PaymentStatus>.Success(PaymentStatus.Completed);
                                }
                                break;
                            }
                        case PaymentStatus.Failed:
                        case PaymentStatus.Cancelled:
                        case PaymentStatus.Expired:
                            {
                                var result = await SyncAkibaPayStatusFromManagementAsync(paymentId, true, cancellationToken);
                                if (!result.IsSuccess)
                                {
                                    return Result<PaymentStatus>.Failure(
                                        localizer[LocalizationKeys.PaymentWebhookCannotCompleteInStatus, paymentId, payment.Status],
                                        AppHttpStatusCodes.BadRequest);
                                }
                                if (result.Value == PaymentStatus.Completed)
                                {
                                    var completeResult = await CompletePaymentArtifactsAsync(payment);
                                    if (completeResult.IsSuccess)
                                    {
                                        akibaPayStatusPollingScheduler.Cancel(paymentId);
                                    }

                                    return completeResult.IsSuccess
                                        ? Result<PaymentStatus>.Success(PaymentStatus.Completed)
                                        : Result<PaymentStatus>.Failure(completeResult.Message!, completeResult.StatusCode);
                                }
                                else if (result.Value == PaymentStatus.Pending)
                                {
                                    //await unitOfWork.ExecuteInTransactionAsync(async _ =>
                                    //{
                                    //    payment.SetNeedsReview(true);
                                    //    await HandlePaymentControlsAsync(payment.Id);
                                    //    await unitOfWork.CompleteAsync();
                                    //}, cancellationToken);

                                    //akibaPayStatusPollingScheduler.Cancel(paymentId);
                                    return Result<PaymentStatus>.Success(PaymentStatus.Pending);
                                }
                                else
                                {
                                    akibaPayStatusPollingScheduler.Cancel(paymentId);
                                    return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                                }
                                
                            }
                        case PaymentStatus.Processing:
                        case PaymentStatus.Pending:
                            {
                                akibaPayStatusPollingScheduler.Cancel(paymentId);
                                return Result<PaymentStatus>.Success(PaymentStatus.Failed);                                
                            }
                    }
                    break;
                }
        }
        return Result<PaymentStatus>.Success(newStatus);

        //if (newStatus == PaymentStatus.Completed)
        //{
        //    if (payment.Status == PaymentStatus.Completed)
        //    {
        //        akibaPayStatusPollingScheduler.Cancel(paymentId);
        //        return Result<PaymentStatus>.Success(PaymentStatus.Completed);
        //    }            

        //    if (payment.Status is not (PaymentStatus.Processing or PaymentStatus.Pending))
        //    {                
        //        var result = await SyncAkibaPayStatusFromManagementAsync(paymentId, true, cancellationToken);
        //        if (!result.IsSuccess || result.Value != PaymentStatus.Completed)
        //        {
        //            return Result<PaymentStatus>.Failure(
        //                localizer[LocalizationKeys.PaymentWebhookCannotCompleteInStatus, paymentId, payment.Status],
        //                AppHttpStatusCodes.BadRequest);
        //        }
        //    }

        //    var completeResult = await CompletePaymentArtifactsAsync(payment);
        //    if (completeResult.IsSuccess)
        //    {
        //        akibaPayStatusPollingScheduler.Cancel(paymentId);
        //    }

        //    return completeResult.IsSuccess
        //        ? Result<PaymentStatus>.Success(PaymentStatus.Completed)
        //        : Result<PaymentStatus>.Failure(completeResult.Message!, completeResult.StatusCode);
        //}

        //if (payment.Status == PaymentStatus.Completed)
        //{
        //    var result = await SyncAkibaPayStatusFromManagementAsync(paymentId, true, cancellationToken);
        //    if (!result.IsSuccess || result.Value != PaymentStatus.Completed)
        //    {
        //        return Result<PaymentStatus>.Failure(
        //            localizer[LocalizationKeys.PaymentWebhookCannotCompleteInStatus, paymentId, payment.Status],
        //            AppHttpStatusCodes.BadRequest);
        //    }
        //    akibaPayStatusPollingScheduler.Cancel(paymentId);
        //    return Result<PaymentStatus>.Success(PaymentStatus.Completed);
        //}

        //if (payment.Status == PaymentStatus.Failed)
        //{
        //    akibaPayStatusPollingScheduler.Cancel(paymentId);
        //    return Result<PaymentStatus>.Success(PaymentStatus.Failed);
        //}

        //await unitOfWork.ExecuteInTransactionAsync(async _ =>
        //{
        //    payment.ChangePaymentArtifactStatus(PaymentStatus.Failed);
        //    await HandlePaymentControlsAsync(payment.Id);
        //    await unitOfWork.CompleteAsync();
        //}, cancellationToken);

        //akibaPayStatusPollingScheduler.Cancel(paymentId);
        //return Result<PaymentStatus>.Success(PaymentStatus.Failed);
    }    

    public async Task<Result<PaymentStatus>> ApplyEasypayWebhookOutcomeAsync(
        Guid paymentId,
        string transactionStatus,
        string? easypayReference,
        CancellationToken cancellationToken = default)
    {
        if (paymentId == Guid.Empty)
        {
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentIdRequired],
                AppHttpStatusCodes.BadRequest);
        }

        if (!TryMapEasypayTransactionStatusToPaymentStatus(transactionStatus, out var newStatus))
        {
            return Result<PaymentStatus>.Failure(
                $"Unsupported Easypay transaction status '{transactionStatus}'.",
                AppHttpStatusCodes.BadRequest);
        }

        var payment = await unitOfWork.PaymentRepository.GetByIdWithPspContextAsync(paymentId);
        if (payment is null)
        {
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentNotFound],
                AppHttpStatusCodes.NotFound);
        }

        if (!string.IsNullOrWhiteSpace(easypayReference))
        {
            payment.SetExternalRef(easypayReference.Trim());
        }

        switch (newStatus)
        {
            case PaymentStatus.Completed:
                {
                    switch (payment.Status)
                    {
                        case PaymentStatus.Completed:
                            {
                                easypayStatusPollingScheduler.Cancel(paymentId);
                                return Result<PaymentStatus>.Success(PaymentStatus.Completed);
                            }
                            break;
                        case PaymentStatus.Processing:
                        case PaymentStatus.Pending:
                            {
                                var completeResult = await CompletePaymentArtifactsAsync(payment);
                                if (completeResult.IsSuccess)
                                {
                                    easypayStatusPollingScheduler.Cancel(paymentId);
                                }

                                return completeResult.IsSuccess
                                    ? Result<PaymentStatus>.Success(PaymentStatus.Completed)
                                    : Result<PaymentStatus>.Failure(completeResult.Message!, completeResult.StatusCode);
                            }
                            break;
                        case PaymentStatus.Expired:
                        case PaymentStatus.Cancelled:
                        case PaymentStatus.Failed:
                            {
                                var result = await SyncEasypayStatusFromManagementAsync(paymentId, true, cancellationToken);
                                if (!result.IsSuccess)
                                {
                                    return Result<PaymentStatus>.Failure(
                                        localizer[LocalizationKeys.PaymentWebhookCannotCompleteInStatus, paymentId, payment.Status],
                                        AppHttpStatusCodes.BadRequest);
                                }
                                else if (result.Value != PaymentStatus.Completed)
                                {
                                    easypayStatusPollingScheduler.Cancel(paymentId);
                                    return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                                }
                                else
                                {
                                    var completeResult = await CompletePaymentArtifactsAsync(payment);
                                    if (completeResult.IsSuccess)
                                    {
                                        easypayStatusPollingScheduler.Cancel(paymentId);
                                    }

                                    return completeResult.IsSuccess
                                        ? Result<PaymentStatus>.Success(PaymentStatus.Completed)
                                        : Result<PaymentStatus>.Failure(completeResult.Message!, completeResult.StatusCode);
                                }
                            }
                            break;
                    }
                }
                break;
            case PaymentStatus.Failed:
                {
                    switch (payment.Status)
                    {
                        case PaymentStatus.Completed:
                            {
                                var result = await SyncEasypayStatusFromManagementAsync(paymentId, true, cancellationToken);
                                if (!result.IsSuccess)
                                {
                                    return Result<PaymentStatus>.Failure(
                                        localizer[LocalizationKeys.PaymentWebhookCannotCompleteInStatus, paymentId, payment.Status],
                                        AppHttpStatusCodes.BadRequest);
                                }
                                if (result.Value != PaymentStatus.Completed)
                                {
                                    await unitOfWork.ExecuteInTransactionAsync(async _ =>
                                    {
                                        //payment.ChangePaymentArtifactStatus(PaymentStatus.Failed);
                                        payment.SetNeedsReview(true);
                                        await HandlePaymentControlsAsync(payment.Id);
                                        await unitOfWork.CompleteAsync();
                                    }, cancellationToken);

                                    easypayStatusPollingScheduler.Cancel(paymentId);
                                    return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                                }
                                else
                                {
                                    easypayStatusPollingScheduler.Cancel(paymentId);
                                    return Result<PaymentStatus>.Success(PaymentStatus.Completed);
                                }
                                break;
                            }
                        case PaymentStatus.Failed:
                        case PaymentStatus.Cancelled:
                        case PaymentStatus.Expired:
                            {
                                easypayStatusPollingScheduler.Cancel(paymentId);
                                return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                            }
                        case PaymentStatus.Processing:
                        case PaymentStatus.Pending:
                            {
                                await unitOfWork.ExecuteInTransactionAsync(async _ =>
                                {
                                    payment.ChangePaymentArtifactStatus(PaymentStatus.Failed);
                                    await HandlePaymentControlsAsync(payment.Id);
                                    await unitOfWork.CompleteAsync();
                                }, cancellationToken);

                                easypayStatusPollingScheduler.Cancel(paymentId);
                                await SendNotificationForPaymentAsync(payment);
                                return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                            }
                    }
                    break;
                }
            case PaymentStatus.Pending:
                {
                    switch (payment.Status)
                    {
                        case PaymentStatus.Completed:
                            {
                                var result = await SyncEasypayStatusFromManagementAsync(paymentId, true, cancellationToken);
                                if (!result.IsSuccess)
                                {
                                    return Result<PaymentStatus>.Failure(
                                        localizer[LocalizationKeys.PaymentWebhookCannotCompleteInStatus, paymentId, payment.Status],
                                        AppHttpStatusCodes.BadRequest);
                                }
                                if (result.Value != PaymentStatus.Completed)
                                {
                                    await unitOfWork.ExecuteInTransactionAsync(async _ =>
                                    {
                                        //payment.ChangePaymentArtifactStatus(PaymentStatus.Failed);
                                        payment.SetNeedsReview(true);
                                        await HandlePaymentControlsAsync(payment.Id);
                                        await unitOfWork.CompleteAsync();
                                    }, cancellationToken);

                                    easypayStatusPollingScheduler.Cancel(paymentId);
                                    return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                                }
                                else
                                {
                                    easypayStatusPollingScheduler.Cancel(paymentId);
                                    return Result<PaymentStatus>.Success(PaymentStatus.Completed);
                                }
                                break;
                            }
                        case PaymentStatus.Failed:
                        case PaymentStatus.Cancelled:
                        case PaymentStatus.Expired:
                            {
                                var result = await SyncEasypayStatusFromManagementAsync(paymentId, true, cancellationToken);
                                if (!result.IsSuccess)
                                {
                                    return Result<PaymentStatus>.Failure(
                                        localizer[LocalizationKeys.PaymentWebhookCannotCompleteInStatus, paymentId, payment.Status],
                                        AppHttpStatusCodes.BadRequest);
                                }
                                if (result.Value == PaymentStatus.Completed)
                                {
                                    var completeResult = await CompletePaymentArtifactsAsync(payment);
                                    if (completeResult.IsSuccess)
                                    {
                                        easypayStatusPollingScheduler.Cancel(paymentId);
                                    }

                                    return completeResult.IsSuccess
                                        ? Result<PaymentStatus>.Success(PaymentStatus.Completed)
                                        : Result<PaymentStatus>.Failure(completeResult.Message!, completeResult.StatusCode);
                                }
                                else if (result.Value == PaymentStatus.Pending)
                                {
                                    //await unitOfWork.ExecuteInTransactionAsync(async _ =>
                                    //{
                                    //    payment.SetNeedsReview(true);
                                    //    await HandlePaymentControlsAsync(payment.Id);
                                    //    await unitOfWork.CompleteAsync();
                                    //}, cancellationToken);

                                    //easypayStatusPollingScheduler.Cancel(paymentId);
                                    return Result<PaymentStatus>.Success(PaymentStatus.Pending);
                                }
                                else
                                {
                                    easypayStatusPollingScheduler.Cancel(paymentId);
                                    return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                                }

                            }
                        case PaymentStatus.Processing:
                        case PaymentStatus.Pending:
                            {
                                easypayStatusPollingScheduler.Cancel(paymentId);
                                return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                            }
                    }
                    break;
                }
        }
        return Result<PaymentStatus>.Success(newStatus);

        //if (newStatus == PaymentStatus.Completed)
        //{
        //    if (payment.Status == PaymentStatus.Completed)
        //    {
        //        easypayStatusPollingScheduler.Cancel(paymentId);
        //        return Result<PaymentStatus>.Success(PaymentStatus.Completed);
        //    }

        //    if (payment.Status is not (PaymentStatus.Processing or PaymentStatus.Pending))
        //    {
        //        return Result<PaymentStatus>.Failure(
        //            localizer[LocalizationKeys.PaymentWebhookCannotCompleteInStatus, paymentId, payment.Status],
        //            AppHttpStatusCodes.BadRequest);
        //    }

        //    var completeResult = await CompletePaymentArtifactsAsync(payment);
        //    if (completeResult.IsSuccess)
        //    {
        //        easypayStatusPollingScheduler.Cancel(paymentId);
        //    }

        //    return completeResult.IsSuccess
        //        ? Result<PaymentStatus>.Success(PaymentStatus.Completed)
        //        : Result<PaymentStatus>.Failure(completeResult.Message!, completeResult.StatusCode);
        //}

        //if (payment.Status == PaymentStatus.Completed)
        //{
        //    easypayStatusPollingScheduler.Cancel(paymentId);
        //    return Result<PaymentStatus>.Success(PaymentStatus.Completed);
        //}

        //if (payment.Status == PaymentStatus.Failed)
        //{
        //    easypayStatusPollingScheduler.Cancel(paymentId);
        //    return Result<PaymentStatus>.Success(PaymentStatus.Failed);
        //}

        //await unitOfWork.ExecuteInTransactionAsync(async _ =>
        //{
        //    payment.ChangePaymentArtifactStatus(PaymentStatus.Failed);
        //    await HandlePaymentControlsAsync(payment.Id);
        //    await unitOfWork.CompleteAsync();
        //}, cancellationToken);

        //easypayStatusPollingScheduler.Cancel(paymentId);
        //return Result<PaymentStatus>.Success(PaymentStatus.Failed);
    }

    public async Task<Result<PaymentStatus>> SyncAkibaPayStatusFromManagementAsync(
        Guid paymentId,
        bool passApply = false,
        CancellationToken cancellationToken = default,
        int? statusPollingAttempt = null)
    {
        if (paymentId == Guid.Empty)
        {
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentIdRequired],
                AppHttpStatusCodes.BadRequest);
        }

        string lockName = $"{LockNames.PAYMENT}:{paymentId}";
        await using var locker = await lockProvider.TryAcquireLockAsync(lockName, TimeSpan.FromSeconds(0));
        if (locker is null)
        {
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentLockAcquireFailed],
                AppHttpStatusCodes.Conflict);
        }

        var payment = await unitOfWork.PaymentRepository.GetByIdWithConfirmContextAsync(paymentId);
        if (payment is null)
        {
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentNotFound],
                AppHttpStatusCodes.NotFound);
        }

        if (payment.Status.IsFinal() && !passApply)
        {
            return Result<PaymentStatus>.Success(payment.Status);
        }

        var validationResult = ValidatePaymentForPspSync(payment, !passApply);
        if (!validationResult.IsSuccess)
        {
            return Result<PaymentStatus>.Failure(
                validationResult.Message!,
                validationResult.StatusCode);
        }

        if (!string.Equals(validationResult.Value, AkibaPayProviderCode, StringComparison.OrdinalIgnoreCase))
        {
            return Result<PaymentStatus>.Failure(
                "Payment is not an AkibaPay payment.",
                AppHttpStatusCodes.BadRequest);
        }

        var connector = pspConnectorResolver.Resolve(AkibaPayProviderCode);
        if (connector is null)
        {
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentPspConnectorNotRegisteredForPolling, AkibaPayProviderCode],
                AppHttpStatusCodes.BadRequest);
        }

        var paymentStatusContext = BuildPaymentTransactionStatusContext(payment, statusPollingAttempt);

        var paymentCheckStatusResult = await connector.GetTransactionStatus(paymentStatusContext);
        if (!paymentCheckStatusResult.IsSuccess)
        {
            return Result<PaymentStatus>.Failure(
                paymentCheckStatusResult.Message!,
                paymentCheckStatusResult.StatusCode);
        }

        var paymentCheckStatusModel = paymentCheckStatusResult.Value!;
        if (paymentCheckStatusModel.Status == PaymentStatus.Completed && !passApply)
        {
            return await ApplyAkibaPayWebhookOutcomeAsync(
                paymentId,
                "SUCCESS",
                string.IsNullOrWhiteSpace(paymentCheckStatusModel.PaymentExternalRef)
                    ? null
                    : paymentCheckStatusModel.PaymentExternalRef,
                cancellationToken);
        }
        if (paymentCheckStatusModel.Status == PaymentStatus.Failed && !passApply)
        {
            var applyResult = await ApplyAkibaPayWebhookOutcomeAsync(
                paymentId,
                "FAILED",
                string.IsNullOrWhiteSpace(paymentCheckStatusModel.PaymentExternalRef)
                    ? null
                    : paymentCheckStatusModel.PaymentExternalRef,
                cancellationToken);
            if (applyResult.IsSuccess && !string.IsNullOrWhiteSpace(paymentCheckStatusModel.UserMessage))
            {
                return Result<PaymentStatus>.Success(applyResult.Value, paymentCheckStatusModel.UserMessage);
            }

            return applyResult;
        }

        return ToPaymentStatusResult(paymentCheckStatusModel);
    }
    public async Task<Result<PaymentStatus>> SyncEasypayStatusFromManagementAsync(
        Guid paymentId,
        bool passApply = false,
        CancellationToken cancellationToken = default,
        int? statusPollingAttempt = null)
    {
        if (paymentId == Guid.Empty)
        {
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentIdRequired],
                AppHttpStatusCodes.BadRequest);
        }

        string lockName = $"{LockNames.PAYMENT}:{paymentId}";
        await using var locker = await lockProvider.TryAcquireLockAsync(lockName, TimeSpan.FromSeconds(0));
        if (locker is null)
        {
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentLockAcquireFailed],
                AppHttpStatusCodes.Conflict);
        }

        var payment = await unitOfWork.PaymentRepository.GetByIdWithConfirmContextAsync(paymentId);
        if (payment is null)
        {
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentNotFound],
                AppHttpStatusCodes.NotFound);
        }

        if (payment.Status.IsFinal() && !passApply)
        {
            return Result<PaymentStatus>.Success(payment.Status);
        }

        var validationResult = ValidatePaymentForPspSync(payment, !passApply);
        if (!validationResult.IsSuccess)
        {
            return Result<PaymentStatus>.Failure(
                validationResult.Message!,
                validationResult.StatusCode);
        }

        if (!string.Equals(validationResult.Value, EasypayProviderCode, StringComparison.OrdinalIgnoreCase))
        {
            return Result<PaymentStatus>.Failure(
                "Payment is not an Easypay payment.",
                AppHttpStatusCodes.BadRequest);
        }

        var connector = pspConnectorResolver.Resolve(EasypayProviderCode);
        if (connector is null)
        {
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentPspConnectorNotRegisteredForPolling, EasypayProviderCode],
                AppHttpStatusCodes.BadRequest);
        }

        var paymentStatusContext = BuildPaymentTransactionStatusContext(payment, statusPollingAttempt);

        var paymentCheckStatusResult = await connector.GetTransactionStatus(paymentStatusContext);
        if (!paymentCheckStatusResult.IsSuccess)
        {
            return Result<PaymentStatus>.Failure(
                paymentCheckStatusResult.Message!,
                paymentCheckStatusResult.StatusCode);
        }

        var paymentCheckStatusModel = paymentCheckStatusResult.Value!;
        if (paymentCheckStatusModel.Status == PaymentStatus.Completed && !passApply)
        {
            return await ApplyEasypayWebhookOutcomeAsync(
                paymentId,
                "SUCCESS",
                string.IsNullOrWhiteSpace(paymentCheckStatusModel.PaymentExternalRef)
                    ? null
                    : paymentCheckStatusModel.PaymentExternalRef,
                cancellationToken);
        }

        if (paymentCheckStatusModel.Status == PaymentStatus.Failed && !passApply)
        {
            var applyResult = await ApplyEasypayWebhookOutcomeAsync(
                paymentId,
                "FAILED",
                string.IsNullOrWhiteSpace(paymentCheckStatusModel.PaymentExternalRef)
                    ? null
                    : paymentCheckStatusModel.PaymentExternalRef,
                cancellationToken);
            if (applyResult.IsSuccess && !string.IsNullOrWhiteSpace(paymentCheckStatusModel.UserMessage))
            {
                return Result<PaymentStatus>.Success(applyResult.Value, paymentCheckStatusModel.UserMessage);
            }

            return applyResult;
        }

        return ToPaymentStatusResult(paymentCheckStatusModel);
    }

    public async Task<Result<PaymentStatus>> ApplyAfrimonyWebhookOutcomeAsync(
        Guid paymentId,
        string transactionStatus,
        string? mmTransactionId,
        CancellationToken cancellationToken = default)
    {
        if (paymentId == Guid.Empty)
        {
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentIdRequired],
                AppHttpStatusCodes.BadRequest);
        }

        if (!TryMapAfrimonyTransactionStatusToPaymentStatus(transactionStatus, out var newStatus))
        {
            return Result<PaymentStatus>.Failure(
                $"Unsupported Afrimony transaction status '{transactionStatus}'.",
                AppHttpStatusCodes.BadRequest);
        }

        var payment = await unitOfWork.PaymentRepository.GetByIdWithPspContextAsync(paymentId);
        if (payment is null)
        {
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentNotFound],
                AppHttpStatusCodes.NotFound);
        }

        if (!string.IsNullOrWhiteSpace(mmTransactionId))
        {
            payment.SetExternalRef(mmTransactionId.Trim());
        }

        switch (newStatus)
        {
            case PaymentStatus.Completed:
                switch (payment.Status)
                {
                    case PaymentStatus.Completed:
                        afrimonyStatusPollingScheduler.Cancel(paymentId);
                        return Result<PaymentStatus>.Success(PaymentStatus.Completed);

                    case PaymentStatus.Processing:
                    case PaymentStatus.Pending:
                    {
                        var completeResult = await CompletePaymentArtifactsAsync(payment);
                        if (completeResult.IsSuccess)
                        {
                            afrimonyStatusPollingScheduler.Cancel(paymentId);
                        }

                        return completeResult.IsSuccess
                            ? Result<PaymentStatus>.Success(PaymentStatus.Completed)
                            : Result<PaymentStatus>.Failure(completeResult.Message!, completeResult.StatusCode);
                    }

                    case PaymentStatus.Expired:
                    case PaymentStatus.Cancelled:
                    case PaymentStatus.Failed:
                    {
                        var result = await SyncAfrimonyStatusFromEnquiryAsync(paymentId, true, cancellationToken);
                        if (!result.IsSuccess)
                        {
                            return Result<PaymentStatus>.Failure(
                                localizer[LocalizationKeys.PaymentWebhookCannotCompleteInStatus, paymentId, payment.Status],
                                AppHttpStatusCodes.BadRequest);
                        }

                        if (result.Value != PaymentStatus.Completed)
                        {
                            afrimonyStatusPollingScheduler.Cancel(paymentId);
                            return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                        }

                        var completeResult = await CompletePaymentArtifactsAsync(payment);
                        if (completeResult.IsSuccess)
                        {
                            afrimonyStatusPollingScheduler.Cancel(paymentId);
                        }

                        return completeResult.IsSuccess
                            ? Result<PaymentStatus>.Success(PaymentStatus.Completed)
                            : Result<PaymentStatus>.Failure(completeResult.Message!, completeResult.StatusCode);
                    }
                }
                break;

            case PaymentStatus.Failed:
                switch (payment.Status)
                {
                    case PaymentStatus.Completed:
                    {
                        var result = await SyncAfrimonyStatusFromEnquiryAsync(paymentId, true, cancellationToken);
                        if (!result.IsSuccess)
                        {
                            return Result<PaymentStatus>.Failure(
                                localizer[LocalizationKeys.PaymentWebhookCannotCompleteInStatus, paymentId, payment.Status],
                                AppHttpStatusCodes.BadRequest);
                        }

                        if (result.Value != PaymentStatus.Completed)
                        {
                            await unitOfWork.ExecuteInTransactionAsync(async _ =>
                            {
                                payment.SetNeedsReview(true);
                                await HandlePaymentControlsAsync(payment.Id);
                                await unitOfWork.CompleteAsync();
                            }, cancellationToken);

                            afrimonyStatusPollingScheduler.Cancel(paymentId);
                            return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                        }

                        afrimonyStatusPollingScheduler.Cancel(paymentId);
                        return Result<PaymentStatus>.Success(PaymentStatus.Completed);
                    }

                    case PaymentStatus.Failed:
                    case PaymentStatus.Cancelled:
                    case PaymentStatus.Expired:
                        afrimonyStatusPollingScheduler.Cancel(paymentId);
                        return Result<PaymentStatus>.Success(PaymentStatus.Failed);

                    case PaymentStatus.Processing:
                    case PaymentStatus.Pending:
                        await unitOfWork.ExecuteInTransactionAsync(async _ =>
                        {
                            payment.ChangePaymentArtifactStatus(PaymentStatus.Failed);
                            await HandlePaymentControlsAsync(payment.Id);
                            await unitOfWork.CompleteAsync();
                        }, cancellationToken);

                        afrimonyStatusPollingScheduler.Cancel(paymentId);
                        return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                }
                break;

            case PaymentStatus.Pending:
                switch (payment.Status)
                {
                    case PaymentStatus.Completed:
                    {
                        var result = await SyncAfrimonyStatusFromEnquiryAsync(paymentId, true, cancellationToken);
                        if (!result.IsSuccess)
                        {
                            return Result<PaymentStatus>.Failure(
                                localizer[LocalizationKeys.PaymentWebhookCannotCompleteInStatus, paymentId, payment.Status],
                                AppHttpStatusCodes.BadRequest);
                        }

                        if (result.Value != PaymentStatus.Completed)
                        {
                            await unitOfWork.ExecuteInTransactionAsync(async _ =>
                            {
                                payment.SetNeedsReview(true);
                                await HandlePaymentControlsAsync(payment.Id);
                                await unitOfWork.CompleteAsync();
                            }, cancellationToken);

                            afrimonyStatusPollingScheduler.Cancel(paymentId);
                            return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                        }

                        afrimonyStatusPollingScheduler.Cancel(paymentId);
                        return Result<PaymentStatus>.Success(PaymentStatus.Completed);
                    }

                    case PaymentStatus.Failed:
                    case PaymentStatus.Cancelled:
                    case PaymentStatus.Expired:
                    {
                        var result = await SyncAfrimonyStatusFromEnquiryAsync(paymentId, true, cancellationToken);
                        if (!result.IsSuccess)
                        {
                            return Result<PaymentStatus>.Failure(
                                localizer[LocalizationKeys.PaymentWebhookCannotCompleteInStatus, paymentId, payment.Status],
                                AppHttpStatusCodes.BadRequest);
                        }

                        if (result.Value == PaymentStatus.Completed)
                        {
                            var completeResult = await CompletePaymentArtifactsAsync(payment);
                            if (completeResult.IsSuccess)
                            {
                                afrimonyStatusPollingScheduler.Cancel(paymentId);
                            }

                            return completeResult.IsSuccess
                                ? Result<PaymentStatus>.Success(PaymentStatus.Completed)
                                : Result<PaymentStatus>.Failure(completeResult.Message!, completeResult.StatusCode);
                        }

                        if (result.Value == PaymentStatus.Pending)
                        {
                            return Result<PaymentStatus>.Success(PaymentStatus.Pending);
                        }

                        afrimonyStatusPollingScheduler.Cancel(paymentId);
                        return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                    }

                    case PaymentStatus.Processing:
                    case PaymentStatus.Pending:
                        afrimonyStatusPollingScheduler.Cancel(paymentId);
                        return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                }
                break;
        }

        return Result<PaymentStatus>.Success(newStatus);
    }

    public async Task<Result<PaymentStatus>> SyncAfrimonyStatusFromEnquiryAsync(
        Guid paymentId,
        bool passApply = false,
        CancellationToken cancellationToken = default,
        int? statusPollingAttempt = null)
    {
        if (paymentId == Guid.Empty)
        {
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentIdRequired],
                AppHttpStatusCodes.BadRequest);
        }

        string lockName = $"{LockNames.PAYMENT}:{paymentId}";
        await using var locker = await lockProvider.TryAcquireLockAsync(lockName, TimeSpan.FromSeconds(0));
        if (locker is null)
        {
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentLockAcquireFailed],
                AppHttpStatusCodes.Conflict);
        }

        var payment = await unitOfWork.PaymentRepository.GetByIdWithConfirmContextAsync(paymentId);
        if (payment is null)
        {
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentNotFound],
                AppHttpStatusCodes.NotFound);
        }

        if (payment.Status.IsFinal() && !passApply)
        {
            return Result<PaymentStatus>.Success(payment.Status);
        }

        var validationResult = ValidatePaymentForPspSync(payment, false);
        if (!validationResult.IsSuccess)
        {
            return Result<PaymentStatus>.Failure(
                validationResult.Message!,
                validationResult.StatusCode);
        }

        if (!string.Equals(validationResult.Value, AfrimonyProviderCode, StringComparison.OrdinalIgnoreCase))
        {
            return Result<PaymentStatus>.Failure(
                "Payment is not an Afrimony payment.",
                AppHttpStatusCodes.BadRequest);
        }

        var connector = pspConnectorResolver.Resolve(AfrimonyProviderCode);
        if (connector is null)
        {
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentPspConnectorNotRegisteredForPolling, AfrimonyProviderCode],
                AppHttpStatusCodes.BadRequest);
        }

        var paymentStatusContext = BuildPaymentTransactionStatusContext(payment, statusPollingAttempt);

        var paymentCheckStatusResult = await connector.GetTransactionStatus(paymentStatusContext);
        if (!paymentCheckStatusResult.IsSuccess)
        {
            return Result<PaymentStatus>.Failure(
                paymentCheckStatusResult.Message!,
                paymentCheckStatusResult.StatusCode);
        }

        var paymentCheckStatusModel = paymentCheckStatusResult.Value!;
        if (paymentCheckStatusModel.Status == PaymentStatus.Completed && !passApply)
        {
            return await ApplyAfrimonyWebhookOutcomeAsync(
                paymentId,
                "TS",
                string.IsNullOrWhiteSpace(paymentCheckStatusModel.PaymentExternalRef)
                    ? null
                    : paymentCheckStatusModel.PaymentExternalRef,
                cancellationToken);
        }

        if (paymentCheckStatusModel.Status == PaymentStatus.Failed && !passApply)
        {
            var applyResult = await ApplyAfrimonyWebhookOutcomeAsync(
                paymentId,
                "TF",
                string.IsNullOrWhiteSpace(paymentCheckStatusModel.PaymentExternalRef)
                    ? null
                    : paymentCheckStatusModel.PaymentExternalRef,
                cancellationToken);
            if (applyResult.IsSuccess && !string.IsNullOrWhiteSpace(paymentCheckStatusModel.UserMessage))
            {
                return Result<PaymentStatus>.Success(applyResult.Value, paymentCheckStatusModel.UserMessage);
            }

            return applyResult;
        }

        return ToPaymentStatusResult(paymentCheckStatusModel);
    }

    private static PaymentTransactionStatusContextInternalModel BuildPaymentTransactionStatusContext(
        PaymentEntity payment,
        int? statusPollingAttempt = null)
    {
        int? totalAttempts = null;
        int? notFoundFailAfterAttempts = null;
        if (statusPollingAttempt.HasValue)
        {
            var pollingSchedule = PaymentMethodConfigReader.ResolveStatusPolling(payment.PaymentMethod!.Config);
            totalAttempts = pollingSchedule.TotalAttempts;
            notFoundFailAfterAttempts = pollingSchedule.NotFoundFailAfterAttempts;
        }

        return new PaymentTransactionStatusContextInternalModel(
            PaymentId: payment.Id,
            CurrencyId: payment.CurrencyId,
            PaymentTransactionReference: payment.PaymentRef ?? string.Empty,
            PaymentExternalReference: payment.ExternalRef ?? string.Empty,
            PaymentMethod: payment.PaymentMethod!,
            Amount: payment.Total,
            StatusPollingAttempt: statusPollingAttempt,
            StatusPollingTotalAttempts: totalAttempts,
            NotFoundFailAfterAttempts: notFoundFailAfterAttempts);
    }

    private static Result<PaymentStatus> ToPaymentStatusResult(PaymentCheckStatusInternalModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.UserMessage))
        {
            return Result<PaymentStatus>.Success(model.Status, model.UserMessage);
        }

        return Result<PaymentStatus>.Success(model.Status);
    }

    private static bool TryMapAkibaPayTransactionStatusToPaymentStatus(
        string transactionStatus,
        out PaymentStatus paymentStatus)
    {
        var normalized = (transactionStatus ?? string.Empty).Trim().ToUpperInvariant();
        switch (normalized)
        {
            case "SUCCESS":
            case "SUCCESSFUL":
            case "COMPLETED":
                paymentStatus = PaymentStatus.Completed;
                return true;
            case "FAILED":
            case "FAILURE":
                paymentStatus = PaymentStatus.Failed;
                return true;
            default:
                paymentStatus = PaymentStatus.Pending;
                return false;
        }
    }
    private static bool TryMapEasypayTransactionStatusToPaymentStatus(
        string transactionStatus,
        out PaymentStatus paymentStatus)
    {
        var normalized = (transactionStatus ?? string.Empty).Trim().ToUpperInvariant();
        switch (normalized)
        {
            case "SUCCESS":
                paymentStatus = PaymentStatus.Completed;
                return true;
            case "FAILED":
                paymentStatus = PaymentStatus.Failed;
                return true;
            default:
                paymentStatus = PaymentStatus.Pending;
                return false;
        }
    }

    public async Task<Result<PaymentStatus>> ApplyMpesaWebhookOutcomeAsync(
        Guid paymentId,
        string transactionStatus,
        string? transactionId,
        CancellationToken cancellationToken = default)
    {
        if (paymentId == Guid.Empty)
            return Result<PaymentStatus>.Failure(localizer[LocalizationKeys.PaymentIdRequired], AppHttpStatusCodes.BadRequest);

        if (!TryMapMpesaTransactionStatus(transactionStatus, out var newStatus))
            return Result<PaymentStatus>.Failure($"Unsupported MPesa transaction status '{transactionStatus}'.", AppHttpStatusCodes.BadRequest);

        var payment = await unitOfWork.PaymentRepository.GetByIdWithPspContextAsync(paymentId);
        if (payment is null)
            return Result<PaymentStatus>.Failure(localizer[LocalizationKeys.PaymentNotFound], AppHttpStatusCodes.NotFound);

        if (!string.IsNullOrWhiteSpace(transactionId))
            payment.SetExternalRef(transactionId.Trim());

        switch (newStatus)
        {
            case PaymentStatus.Completed:
                switch (payment.Status)
                {
                    case PaymentStatus.Completed:
                        await StopPollingJobAsync(paymentId);
                        return Result<PaymentStatus>.Success(PaymentStatus.Completed);

                    case PaymentStatus.Processing:
                    case PaymentStatus.Pending:
                    {
                        var completeResult = await CompletePaymentArtifactsAsync(payment);
                        if (completeResult.IsSuccess)
                            await StopPollingJobAsync(paymentId);

                        return completeResult.IsSuccess
                            ? Result<PaymentStatus>.Success(PaymentStatus.Completed)
                            : Result<PaymentStatus>.Failure(completeResult.Message!, completeResult.StatusCode);
                    }

                    case PaymentStatus.Expired:
                    case PaymentStatus.Cancelled:
                    case PaymentStatus.Failed:
                    {
                        var result = await SyncMpesaStatusAsync(paymentId, true, cancellationToken);
                        if (!result.IsSuccess)
                        {
                            return Result<PaymentStatus>.Failure(
                                localizer[LocalizationKeys.PaymentWebhookCannotCompleteInStatus, paymentId, payment.Status],
                                AppHttpStatusCodes.BadRequest);
                        }

                        if (result.Value != PaymentStatus.Completed)
                        {
                            await StopPollingJobAsync(paymentId);
                            return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                        }

                        var completeResult = await CompletePaymentArtifactsAsync(payment);
                        if (completeResult.IsSuccess)
                            await StopPollingJobAsync(paymentId);

                        return completeResult.IsSuccess
                            ? Result<PaymentStatus>.Success(PaymentStatus.Completed)
                            : Result<PaymentStatus>.Failure(completeResult.Message!, completeResult.StatusCode);
                    }
                }
                break;

            case PaymentStatus.Failed:
                switch (payment.Status)
                {
                    case PaymentStatus.Completed:
                    {
                        var result = await SyncMpesaStatusAsync(paymentId, true, cancellationToken);
                        if (!result.IsSuccess)
                        {
                            return Result<PaymentStatus>.Failure(
                                localizer[LocalizationKeys.PaymentWebhookCannotCompleteInStatus, paymentId, payment.Status],
                                AppHttpStatusCodes.BadRequest);
                        }

                        if (result.Value != PaymentStatus.Completed)
                        {
                            await unitOfWork.ExecuteInTransactionAsync(async _ =>
                            {
                                payment.SetNeedsReview(true);
                                await HandlePaymentControlsAsync(payment.Id);
                                await unitOfWork.CompleteAsync();
                            }, cancellationToken);

                            await StopPollingJobAsync(paymentId);
                            return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                        }

                        await StopPollingJobAsync(paymentId);
                        return Result<PaymentStatus>.Success(PaymentStatus.Completed);
                    }

                    case PaymentStatus.Failed:
                    case PaymentStatus.Cancelled:
                    case PaymentStatus.Expired:
                        await StopPollingJobAsync(paymentId);
                        return Result<PaymentStatus>.Success(PaymentStatus.Failed);

                    case PaymentStatus.Processing:
                    case PaymentStatus.Pending:
                        await unitOfWork.ExecuteInTransactionAsync(async _ =>
                        {
                            payment.ChangePaymentArtifactStatus(PaymentStatus.Failed);
                            await HandlePaymentControlsAsync(payment.Id);
                            await unitOfWork.CompleteAsync();
                        }, cancellationToken);

                        await StopPollingJobAsync(paymentId);
                        await SendNotificationForPaymentAsync(payment);
                        return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                }
                break;

            case PaymentStatus.Pending:
                switch (payment.Status)
                {
                    case PaymentStatus.Completed:
                    {
                        var result = await SyncMpesaStatusAsync(paymentId, true, cancellationToken);
                        if (!result.IsSuccess)
                        {
                            return Result<PaymentStatus>.Failure(
                                localizer[LocalizationKeys.PaymentWebhookCannotCompleteInStatus, paymentId, payment.Status],
                                AppHttpStatusCodes.BadRequest);
                        }

                        if (result.Value != PaymentStatus.Completed)
                        {
                            await unitOfWork.ExecuteInTransactionAsync(async _ =>
                            {
                                payment.SetNeedsReview(true);
                                await HandlePaymentControlsAsync(payment.Id);
                                await unitOfWork.CompleteAsync();
                            }, cancellationToken);

                            await StopPollingJobAsync(paymentId);
                            return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                        }

                        await StopPollingJobAsync(paymentId);
                        return Result<PaymentStatus>.Success(PaymentStatus.Completed);
                    }

                    case PaymentStatus.Failed:
                    case PaymentStatus.Cancelled:
                    case PaymentStatus.Expired:
                    {
                        var result = await SyncMpesaStatusAsync(paymentId, true, cancellationToken);
                        if (!result.IsSuccess)
                        {
                            return Result<PaymentStatus>.Failure(
                                localizer[LocalizationKeys.PaymentWebhookCannotCompleteInStatus, paymentId, payment.Status],
                                AppHttpStatusCodes.BadRequest);
                        }

                        if (result.Value == PaymentStatus.Completed)
                        {
                            var completeResult = await CompletePaymentArtifactsAsync(payment);
                            if (completeResult.IsSuccess)
                                await StopPollingJobAsync(paymentId);

                            return completeResult.IsSuccess
                                ? Result<PaymentStatus>.Success(PaymentStatus.Completed)
                                : Result<PaymentStatus>.Failure(completeResult.Message!, completeResult.StatusCode);
                        }

                        if (result.Value == PaymentStatus.Pending)
                            return Result<PaymentStatus>.Success(PaymentStatus.Pending);

                        await StopPollingJobAsync(paymentId);
                        return Result<PaymentStatus>.Success(PaymentStatus.Failed);
                    }

                    case PaymentStatus.Processing:
                    case PaymentStatus.Pending:
                        return Result<PaymentStatus>.Success(PaymentStatus.Pending);
                }
                break;
        }

        return Result<PaymentStatus>.Success(newStatus);
    }

    public async Task<Result<PaymentStatus>> SyncMpesaStatusAsync(
        Guid paymentId,
        bool passApply = false,
        CancellationToken cancellationToken = default,
        int? statusPollingAttempt = null)
    {
        if (paymentId == Guid.Empty)
            return Result<PaymentStatus>.Failure(localizer[LocalizationKeys.PaymentIdRequired], AppHttpStatusCodes.BadRequest);

        var lockName = $"{LockNames.PAYMENT}:{paymentId}";
        await using var locker = await lockProvider.TryAcquireLockAsync(lockName, TimeSpan.Zero);
        if (locker is null)
            return Result<PaymentStatus>.Failure(localizer[LocalizationKeys.PaymentLockAcquireFailed], AppHttpStatusCodes.Conflict);

        var payment = await unitOfWork.PaymentRepository.GetByIdWithConfirmContextAsync(paymentId);
        if (payment is null)
            return Result<PaymentStatus>.Failure(localizer[LocalizationKeys.PaymentNotFound], AppHttpStatusCodes.NotFound);
        if (payment.Status.IsFinal() && !passApply)
            return Result<PaymentStatus>.Success(payment.Status);

        var validation = ValidatePaymentForPspSync(payment, false);
        if (!validation.IsSuccess)
            return Result<PaymentStatus>.Failure(validation.Message!, validation.StatusCode);
        if (!string.Equals(validation.Value, MpesaProviderCode, StringComparison.OrdinalIgnoreCase))
            return Result<PaymentStatus>.Failure("Payment is not an MPesa payment.", AppHttpStatusCodes.BadRequest);

        var connector = pspConnectorResolver.Resolve(MpesaProviderCode);
        if (connector is null)
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentPspConnectorNotRegisteredForPolling, MpesaProviderCode],
                AppHttpStatusCodes.BadRequest);

        var schedule = PaymentMethodConfigReader.ResolveStatusPolling(payment.PaymentMethod!.Config);
        var statusContext = new PaymentTransactionStatusContextInternalModel(
            PaymentId: payment.Id,
            CurrencyId: payment.CurrencyId,
            PaymentTransactionReference: payment.PaymentRef!,
            PaymentExternalReference: payment.ExternalRef,
            PaymentMethod: payment.PaymentMethod,
            Amount: payment.Total,
            StatusPollingAttempt: statusPollingAttempt,
            StatusPollingTotalAttempts: schedule.TotalAttempts,
            NotFoundFailAfterAttempts: schedule.NotFoundFailAfterAttempts);

        var checkedStatus = await connector.GetTransactionStatus(statusContext);
        if (!checkedStatus.IsSuccess)
            return Result<PaymentStatus>.Failure(checkedStatus.Message!, checkedStatus.StatusCode);

        var model = checkedStatus.Value!;
        if (!passApply && model.Status is PaymentStatus.Completed or PaymentStatus.Failed)
            return await ApplyMpesaWebhookOutcomeAsync(
                paymentId,
                model.Status == PaymentStatus.Completed ? "COMPLETED" : "FAILED",
                model.PaymentExternalRef,
                cancellationToken);

        return Result<PaymentStatus>.Success(model.Status);
    }

    public async Task<Result<PaymentStatus>> ApplyOrangeWebhookOutcomeAsync(
        Guid paymentId,
        string transactionStatus,
        string? orangeTransactionId,
        CancellationToken cancellationToken = default)
    {
        if (paymentId == Guid.Empty)
            return Result<PaymentStatus>.Failure(localizer[LocalizationKeys.PaymentIdRequired], AppHttpStatusCodes.BadRequest);
        if (!TryMapOrangeTransactionStatus(transactionStatus, out var newStatus))
            return Result<PaymentStatus>.Failure($"Unsupported Orange transaction status '{transactionStatus}'.", AppHttpStatusCodes.BadRequest);

        var payment = await unitOfWork.PaymentRepository.GetByIdWithPspContextAsync(paymentId);
        if (payment is null)
            return Result<PaymentStatus>.Failure(localizer[LocalizationKeys.PaymentNotFound], AppHttpStatusCodes.NotFound);
        if (!string.IsNullOrWhiteSpace(orangeTransactionId))
            payment.SetExternalRef(orangeTransactionId.Trim());

        if (newStatus == PaymentStatus.Completed)
        {
            if (payment.Status == PaymentStatus.Completed)
            {
                await StopPollingJobAsync(paymentId);
                return Result<PaymentStatus>.Success(PaymentStatus.Completed);
            }

            if (payment.Status is PaymentStatus.Processing or PaymentStatus.Pending)
            {
                var completed = await CompletePaymentArtifactsAsync(payment);
                if (completed.IsSuccess) await StopPollingJobAsync(paymentId);
                return completed.IsSuccess
                    ? Result<PaymentStatus>.Success(PaymentStatus.Completed)
                    : Result<PaymentStatus>.Failure(completed.Message!, completed.StatusCode);
            }

            var verified = await SyncOrangeStatusAsync(paymentId, true, cancellationToken);
            if (!verified.IsSuccess || verified.Value != PaymentStatus.Completed)
                return Result<PaymentStatus>.Failure(
                    localizer[LocalizationKeys.PaymentWebhookCannotCompleteInStatus, paymentId, payment.Status],
                    AppHttpStatusCodes.BadRequest);

            var completion = await CompletePaymentArtifactsAsync(payment);
            if (completion.IsSuccess) await StopPollingJobAsync(paymentId);
            return completion.IsSuccess
                ? Result<PaymentStatus>.Success(PaymentStatus.Completed)
                : Result<PaymentStatus>.Failure(completion.Message!, completion.StatusCode);
        }

        if (newStatus == PaymentStatus.Failed)
        {
            if (payment.Status == PaymentStatus.Completed)
            {
                var verified = await SyncOrangeStatusAsync(paymentId, true, cancellationToken);
                if (!verified.IsSuccess)
                    return Result<PaymentStatus>.Failure(verified.Message!, verified.StatusCode);
                if (verified.Value == PaymentStatus.Completed)
                {
                    await StopPollingJobAsync(paymentId);
                    return Result<PaymentStatus>.Success(PaymentStatus.Completed);
                }

                await unitOfWork.ExecuteInTransactionAsync(async _ =>
                {
                    payment.SetNeedsReview(true);
                    await HandlePaymentControlsAsync(payment.Id);
                    await unitOfWork.CompleteAsync();
                }, cancellationToken);
                await StopPollingJobAsync(paymentId);
                return Result<PaymentStatus>.Success(PaymentStatus.Failed);
            }

            if (payment.Status is PaymentStatus.Processing or PaymentStatus.Pending)
            {
                await unitOfWork.ExecuteInTransactionAsync(async _ =>
                {
                    payment.ChangePaymentArtifactStatus(PaymentStatus.Failed);
                    await HandlePaymentControlsAsync(payment.Id);
                    await unitOfWork.CompleteAsync();
                }, cancellationToken);
                await StopPollingJobAsync(paymentId);
                await SendNotificationForPaymentAsync(payment);
            }
            else
            {
                await StopPollingJobAsync(paymentId);
            }

            return Result<PaymentStatus>.Success(PaymentStatus.Failed);
        }

        return Result<PaymentStatus>.Success(PaymentStatus.Pending);
    }

    public async Task<Result<PaymentStatus>> SyncOrangeStatusAsync(
        Guid paymentId,
        bool passApply = false,
        CancellationToken cancellationToken = default,
        int? statusPollingAttempt = null)
    {
        if (paymentId == Guid.Empty)
            return Result<PaymentStatus>.Failure(localizer[LocalizationKeys.PaymentIdRequired], AppHttpStatusCodes.BadRequest);

        var lockName = $"{LockNames.PAYMENT}:{paymentId}";
        await using var locker = await lockProvider.TryAcquireLockAsync(lockName, TimeSpan.Zero);
        if (locker is null)
            return Result<PaymentStatus>.Failure(localizer[LocalizationKeys.PaymentLockAcquireFailed], AppHttpStatusCodes.Conflict);

        var payment = await unitOfWork.PaymentRepository.GetByIdWithConfirmContextAsync(paymentId);
        if (payment is null)
            return Result<PaymentStatus>.Failure(localizer[LocalizationKeys.PaymentNotFound], AppHttpStatusCodes.NotFound);
        if (payment.Status.IsFinal() && !passApply)
            return Result<PaymentStatus>.Success(payment.Status);

        var validation = ValidatePaymentForPspSync(payment, false);
        if (!validation.IsSuccess)
            return Result<PaymentStatus>.Failure(validation.Message!, validation.StatusCode);
        if (!string.Equals(validation.Value, OrangeProviderCode, StringComparison.OrdinalIgnoreCase))
            return Result<PaymentStatus>.Failure("Payment is not an Orange payment.", AppHttpStatusCodes.BadRequest);

        var connector = pspConnectorResolver.Resolve(OrangeProviderCode);
        if (connector is null)
            return Result<PaymentStatus>.Failure(
                localizer[LocalizationKeys.PaymentPspConnectorNotRegisteredForPolling, OrangeProviderCode],
                AppHttpStatusCodes.BadRequest);

        var statusContext = BuildPaymentTransactionStatusContext(payment, statusPollingAttempt);
        var checkedStatus = await connector.GetTransactionStatus(statusContext);
        if (!checkedStatus.IsSuccess)
            return Result<PaymentStatus>.Failure(checkedStatus.Message!, checkedStatus.StatusCode);

        var model = checkedStatus.Value!;
        if (!passApply && model.Status is PaymentStatus.Completed or PaymentStatus.Failed)
            return await ApplyOrangeWebhookOutcomeAsync(
                paymentId,
                model.Status == PaymentStatus.Completed ? "SUCCESS" : "FAILED",
                model.PaymentExternalRef,
                cancellationToken);

        return ToPaymentStatusResult(model);
    }

    private static bool TryMapOrangeTransactionStatus(string? transactionStatus, out PaymentStatus paymentStatus)
    {
        var status = (transactionStatus ?? string.Empty).Trim().ToUpperInvariant();
        if (status is "SUCCESS" or "SUCCESSFUL" or "COMPLETED")
        {
            paymentStatus = PaymentStatus.Completed;
            return true;
        }
        if (status is "FAILED" or "FAILURE" or "REJECTED" or "CANCELLED" or "EXPIRED")
        {
            paymentStatus = PaymentStatus.Failed;
            return true;
        }
        if (status is "PENDING" or "PROCESSING" or "INITIATED")
        {
            paymentStatus = PaymentStatus.Pending;
            return true;
        }
        paymentStatus = PaymentStatus.Pending;
        return false;
    }

    private static bool TryMapMpesaTransactionStatus(string? transactionStatus, out PaymentStatus paymentStatus)
    {
        var status = (transactionStatus ?? string.Empty).Trim().ToUpperInvariant();
        if (status is "COMPLETED" or "SUCCESS" or "SUCCESSFUL" or "INS-0")
        {
            paymentStatus = PaymentStatus.Completed;
            return true;
        }
        if (status is "FAILED" or "FAILURE" or "CANCELLED" or "CANCELED" or "REJECTED" or "EXPIRED"
            or "INS-1" or "INS-6" or "INS-13" or "INS-20" or "INS-21" or "INS-23" or "INS-28"
            or "INS-996" or "INS-997" or "INS-998")
        {
            paymentStatus = PaymentStatus.Failed;
            return true;
        }
        if (status is "PENDING" or "PROCESSING" or "INS-9")
        {
            paymentStatus = PaymentStatus.Pending;
            return true;
        }
        paymentStatus = PaymentStatus.Pending;
        return false;
    }

    private static bool TryMapAfrimonyTransactionStatusToPaymentStatus(
        string transactionStatus,
        out PaymentStatus paymentStatus)
    {
        var normalized = (transactionStatus ?? string.Empty).Trim().ToUpperInvariant();
        switch (normalized)
        {
            case "TS":
            case "SUCCESS":
            case "SUCCESSFUL":
            case "COMPLETED":
                paymentStatus = PaymentStatus.Completed;
                return true;
            case "TF":
            case "FAILED":
            case "FAILURE":
                paymentStatus = PaymentStatus.Failed;
                return true;
            default:
                paymentStatus = PaymentStatus.Pending;
                return false;
        }
    }
    public async Task<Result<IReadOnlyList<CompletedTopupByCurrencyOutputModel>>> GetCompletedTopupByCurrencyAsync(
        CompletedTopupByCurrencyInputModel model,
        CancellationToken cancellationToken = default)
    {
        var result = await GetCompletedTopupByCurrencyGrouped(model, cancellationToken);
        if (!result.IsSuccess)
            return result.ToFailure<IReadOnlyList<CompletedTopupByCurrencyOutputModel>>();

        return Result<IReadOnlyList<CompletedTopupByCurrencyOutputModel>>.Success(
            result.Value,
            localizer[LocalizationKeys.PaymentCompletedTopupByCurrencyQuerySuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<FileContentResult>> ExportCompletedTopupByCurrencyAsync(
        CompletedTopupByCurrencyInputModel model,
        CancellationToken cancellationToken = default)
    {
        var result = await GetCompletedTopupByCurrencyGroupedExcel(model, cancellationToken);
        if (!result.IsSuccess)
            return result.ToFailure<FileContentResult>();

        var dataObjects = mapper.Map<List<object>>(result.Value);
        var export = await excelFileExportService.ExportExcelFileAsync(
            "completed-topup-by-currency",
            dataObjects,
            typeof(CompletedTopupByCurrencyExcelModel));
        if (!export.IsSuccess)
            return export;

        return Result<FileContentResult>.Success(
            export.Value!,
            localizer[LocalizationKeys.PaymentCompletedTopupByCurrencyExportSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<FileContentResult>> ExportCompletedTopupByCurrencyRowsAsync(
        CompletedTopupByCurrencyInputModel model,
        CancellationToken cancellationToken = default)
    {
        var groups = await GetCompletedTopupByCurrencyGroupedBasic(model, cancellationToken);
        if (!groups.IsSuccess)
            return groups.ToFailure<FileContentResult>();

        var paymentMapper = new PaymentMapper(mapper);
        var dataPayments = paymentMapper.MapExcel(
            OrderCompletedTopupByCurrencyRows(groups.Value, model.GroupByProvider));
        var dataObjects = mapper.Map<List<object>>(dataPayments);
        var export = await excelFileExportService.ExportExcelFileAsync(
            "completed-topup-by-currency-rows",
            dataObjects,
            typeof(PaymentExcelModel));
        if (!export.IsSuccess)
            return export;

        return Result<FileContentResult>.Success(
            export.Value!,
            localizer[LocalizationKeys.PaymentCompletedTopupByCurrencyRowsExportSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<IReadOnlyList<CompletedTopupByDateOutputModel>>> GetCompletedTopupByDateAsync(
        CompletedTopupByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        var result = await GetCompletedTopupByDateGrouped(model, cancellationToken);
        if (!result.IsSuccess)
            return result.ToFailure<IReadOnlyList<CompletedTopupByDateOutputModel>>();

        return Result<IReadOnlyList<CompletedTopupByDateOutputModel>>.Success(
            result.Value,
            localizer[LocalizationKeys.PaymentCompletedTopupByDateQuerySuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<FileContentResult>> ExportCompletedTopupByDateAsync(
        CompletedTopupByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        var dataResult = await GetCompletedTopupByDateDataAsync(model, cancellationToken);
        if (!dataResult.IsSuccess)
            return dataResult.ToFailure<FileContentResult>();

        var currencyDisplayCode = dataResult.Value
            .Select(x => x.Currency?.Abbreviation ?? x.Currency?.Code)
            .FirstOrDefault(code => !string.IsNullOrWhiteSpace(code))
            ?? model.CurrencyCode.Trim();

        var excelRows = BuildCompletedTopupByDateGrouped(dataResult.Value, model.FromDate, model.ToDate)
            .Select(x => new CompletedTopupByDateExcelModel
            {
                Date = x.Date,
                Count = x.Count,
                Sum = x.Sum,
                CurrencyCode = currencyDisplayCode
            })
            .ToList();

        var dataObjects = mapper.Map<List<object>>(excelRows);
        var export = await excelFileExportService.ExportExcelFileAsync(
            "completed-topup-by-date",
            dataObjects,
            typeof(CompletedTopupByDateExcelModel));
        if (!export.IsSuccess)
            return export;

        return Result<FileContentResult>.Success(
            export.Value!,
            localizer[LocalizationKeys.PaymentCompletedTopupByDateExportSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<FileContentResult>> ExportCompletedTopupByDateRowsAsync(
        CompletedTopupByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        var dataResult = await GetCompletedTopupByDateDataAsync(model, cancellationToken);
        if (!dataResult.IsSuccess)
            return dataResult.ToFailure<FileContentResult>();

        var data = dataResult.Value
            .OrderBy(x => DateOnly.FromDateTime(x.CreatedAtUtc.UtcDateTime))
            .ThenBy(x => x.CreatedAtUtc)
            .ToList();

        var paymentMapper = new PaymentMapper(mapper);
        var dataPayments = paymentMapper.MapExcel(data);
        var dataObjects = mapper.Map<List<object>>(dataPayments);
        var export = await excelFileExportService.ExportExcelFileAsync(
            "completed-topup-by-date-rows",
            dataObjects,
            typeof(PaymentExcelModel));
        if (!export.IsSuccess)
            return export;

        return Result<FileContentResult>.Success(
            export.Value!,
            localizer[LocalizationKeys.PaymentCompletedTopupByDateRowsExportSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<IReadOnlyList<CompletedPostpaidPayBillByDateOutputModel>>> GetCompletedPostpaidPayBillByDateAsync(
        CompletedPostpaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        var result = await GetCompletedPostpaidPayBillByDateGrouped(model, cancellationToken);
        if (!result.IsSuccess)
            return result.ToFailure<IReadOnlyList<CompletedPostpaidPayBillByDateOutputModel>>();

        return Result<IReadOnlyList<CompletedPostpaidPayBillByDateOutputModel>>.Success(
            result.Value,
            localizer[LocalizationKeys.PaymentCompletedPostpaidPayBillByDateQuerySuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<FileContentResult>> ExportCompletedPostpaidPayBillByDateAsync(
        CompletedPostpaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        var dataResult = await GetCompletedPostpaidPayBillByDateDataAsync(model, cancellationToken);
        if (!dataResult.IsSuccess)
            return dataResult.ToFailure<FileContentResult>();

        var grouped = BuildCompletedPostpaidPayBillByDateGrouped(
            dataResult.Value,
            model.GroupByBiller,
            model.FromDate,
            model.ToDate);
        var currencyDisplayCode = dataResult.Value
            .Select(x => x.Item.Currency?.Abbreviation ?? x.Item.Currency?.Code)
            .FirstOrDefault(code => !string.IsNullOrWhiteSpace(code))
            ?? model.CurrencyCode.Trim();

        var excelRows = grouped.Select(x => new CompletedPostpaidPayBillByDateExcelModel
        {
            Date = x.Date,
            BillerId = x.BillerId,
            BillerCode = x.BillerCode,
            BillerName = x.BillerName,
            Count = x.Count,
            Sum = x.Sum,
            CurrencyCode = currencyDisplayCode
        }).ToList();

        var dataObjects = mapper.Map<List<object>>(excelRows);
        var export = await excelFileExportService.ExportExcelFileAsync(
            "completed-postpaid-paybill-by-date",
            dataObjects,
            typeof(CompletedPostpaidPayBillByDateExcelModel));
        if (!export.IsSuccess)
            return export;

        return Result<FileContentResult>.Success(
            export.Value!,
            localizer[LocalizationKeys.PaymentCompletedPostpaidPayBillByDateExportSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<FileContentResult>> ExportCompletedPostpaidPayBillByDateRowsAsync(
        CompletedPostpaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        var dataResult = await GetCompletedPostpaidPayBillByDateDataAsync(model, cancellationToken);
        if (!dataResult.IsSuccess)
            return dataResult.ToFailure<FileContentResult>();

        var dataRows = BuildCompletedPostpaidPayBillByDateRows(dataResult.Value, model.GroupByBiller)
            .Select(ToPostpaidPayBillByDateRowExcelModel)
            .ToList();
        var dataObjects = mapper.Map<List<object>>(dataRows);
        var export = await excelFileExportService.ExportExcelFileAsync(
            "completed-postpaid-paybill-by-date-rows",
            dataObjects,
            typeof(CompletedPostpaidPayBillByDateRowExcelModel));
        if (!export.IsSuccess)
            return export;

        return Result<FileContentResult>.Success(
            export.Value!,
            localizer[LocalizationKeys.PaymentCompletedPostpaidPayBillByDateRowsExportSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<IReadOnlyList<CompletedPostpaidPayBillByCenterOutputModel>>> GetCompletedPostpaidPayBillByCenterAsync(
        CompletedPostpaidPayBillByCenterInputModel model,
        CancellationToken cancellationToken = default)
    {
        var result = await GetCompletedPostpaidPayBillByCenterGrouped(model, cancellationToken);
        if (!result.IsSuccess)
            return result.ToFailure<IReadOnlyList<CompletedPostpaidPayBillByCenterOutputModel>>();

        return Result<IReadOnlyList<CompletedPostpaidPayBillByCenterOutputModel>>.Success(
            result.Value,
            localizer[LocalizationKeys.PaymentCompletedPostpaidPayBillByCenterQuerySuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<FileContentResult>> ExportCompletedPostpaidPayBillByCenterAsync(
        CompletedPostpaidPayBillByCenterInputModel model,
        CancellationToken cancellationToken = default)
    {
        var dataResult = await GetCompletedPostpaidPayBillByCenterDataAsync(model, cancellationToken);
        if (!dataResult.IsSuccess)
            return dataResult.ToFailure<FileContentResult>();

        var grouped = BuildCompletedPostpaidPayBillByCenterGrouped(dataResult.Value, model.GroupByBiller);
        var currencyDisplayCode = dataResult.Value
            .Select(x => x.Item.Currency?.Abbreviation ?? x.Item.Currency?.Code)
            .FirstOrDefault(code => !string.IsNullOrWhiteSpace(code))
            ?? model.CurrencyCode.Trim();

        var excelRows = grouped.Select(x => new CompletedPostpaidPayBillByCenterExcelModel
        {
            CenterName = x.CenterName,
            BillerId = x.BillerId,
            BillerCode = x.BillerCode,
            BillerName = x.BillerName,
            Count = x.Count,
            Sum = x.Sum,
            CurrencyCode = currencyDisplayCode
        }).ToList();

        var dataObjects = mapper.Map<List<object>>(excelRows);
        var export = await excelFileExportService.ExportExcelFileAsync(
            "completed-postpaid-paybill-by-center",
            dataObjects,
            typeof(CompletedPostpaidPayBillByCenterExcelModel));
        if (!export.IsSuccess)
            return export;

        return Result<FileContentResult>.Success(
            export.Value!,
            localizer[LocalizationKeys.PaymentCompletedPostpaidPayBillByCenterExportSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<FileContentResult>> ExportCompletedPostpaidPayBillByCenterRowsAsync(
        CompletedPostpaidPayBillByCenterInputModel model,
        CancellationToken cancellationToken = default)
    {
        var dataResult = await GetCompletedPostpaidPayBillByCenterDataAsync(model, cancellationToken);
        if (!dataResult.IsSuccess)
            return dataResult.ToFailure<FileContentResult>();

        var dataRows = BuildCompletedPostpaidPayBillByCenterRows(dataResult.Value, model.GroupByBiller)
            .Select(ToPostpaidPayBillByCenterRowExcelModel)
            .ToList();
        var dataObjects = mapper.Map<List<object>>(dataRows);
        var export = await excelFileExportService.ExportExcelFileAsync(
            "completed-postpaid-paybill-by-center-rows",
            dataObjects,
            typeof(CompletedPostpaidPayBillByCenterRowExcelModel));
        if (!export.IsSuccess)
            return export;

        return Result<FileContentResult>.Success(
            export.Value!,
            localizer[LocalizationKeys.PaymentCompletedPostpaidPayBillByCenterRowsExportSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<IReadOnlyList<CompletedPrepaidPayBillByDateOutputModel>>> GetCompletedPrepaidPayBillByDateAsync(
        CompletedPrepaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        var result = await GetCompletedPrepaidPayBillByDateGrouped(model, cancellationToken);
        if (!result.IsSuccess)
            return result.ToFailure<IReadOnlyList<CompletedPrepaidPayBillByDateOutputModel>>();

        return Result<IReadOnlyList<CompletedPrepaidPayBillByDateOutputModel>>.Success(
            result.Value,
            localizer[LocalizationKeys.PaymentCompletedPrepaidPayBillByDateQuerySuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<FileContentResult>> ExportCompletedPrepaidPayBillByDateAsync(
        CompletedPrepaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        var dataResult = await GetCompletedPrepaidPayBillByDateDataAsync(model, cancellationToken);
        if (!dataResult.IsSuccess)
            return dataResult.ToFailure<FileContentResult>();

        var grouped = BuildCompletedPrepaidPayBillByDateGrouped(
            dataResult.Value,
            model.GroupByBiller,
            model.FromDate,
            model.ToDate);
        var currencyDisplayCode = dataResult.Value
            .Select(x => x.Currency?.Abbreviation ?? x.Currency?.Code)
            .FirstOrDefault(code => !string.IsNullOrWhiteSpace(code))
            ?? model.CurrencyCode.Trim();

        var excelRows = grouped.Select(x => new CompletedPrepaidPayBillByDateExcelModel
        {
            Date = x.Date,
            BillerId = x.BillerId,
            BillerCode = x.BillerCode,
            BillerName = x.BillerName,
            Count = x.Count,
            Sum = x.Sum,
            CurrencyCode = currencyDisplayCode
        }).ToList();

        var dataObjects = mapper.Map<List<object>>(excelRows);
        var export = await excelFileExportService.ExportExcelFileAsync(
            "completed-prepaid-paybill-by-date",
            dataObjects,
            typeof(CompletedPrepaidPayBillByDateExcelModel));
        if (!export.IsSuccess)
            return export;

        return Result<FileContentResult>.Success(
            export.Value!,
            localizer[LocalizationKeys.PaymentCompletedPrepaidPayBillByDateExportSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<FileContentResult>> ExportCompletedPrepaidPayBillByDateRowsAsync(
        CompletedPrepaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        var dataResult = await GetCompletedPrepaidPayBillByDateDataAsync(model, cancellationToken);
        if (!dataResult.IsSuccess)
            return dataResult.ToFailure<FileContentResult>();

        var dataRows = BuildCompletedPrepaidPayBillByDateRows(dataResult.Value, model.GroupByBiller)
            .Select(ToPrepaidPayBillByDateRowExcelModel)
            .ToList();
        var dataObjects = mapper.Map<List<object>>(dataRows);
        var export = await excelFileExportService.ExportExcelFileAsync(
            "completed-prepaid-paybill-by-date-rows",
            dataObjects,
            typeof(CompletedPrepaidPayBillByDateRowExcelModel));
        if (!export.IsSuccess)
            return export;

        return Result<FileContentResult>.Success(
            export.Value!,
            localizer[LocalizationKeys.PaymentCompletedPrepaidPayBillByDateRowsExportSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<PagedResult<PaymentOutputModel>>> GetCompletedTopupByCurrencyRowsListAsync(
        CompletedTopupByCurrencyRowsListInputModel model,
        CancellationToken cancellationToken = default)
    {
        var partner = await paymentProviderAccessPolicy.GetPaymentProvidersReadScopeAsync(cancellationToken);
        if (!partner.IsSuccess)
            return partner.ToFailure<PagedResult<PaymentOutputModel>>();

        long? paymentProviderId = model.PaymentProviderId;
        if (partner.Value is not null)
        {
            var paymentProvider = await paymentProviderService.GetByPartnerIdAsync(partner.Value.Value);
            if (!paymentProvider.IsSuccess)
                return paymentProvider.ToFailure<PagedResult<PaymentOutputModel>>();
            paymentProviderId = paymentProvider.Value.Id;
        }

        NormalizeReportListQuery(model);
        var query = ApplyCompletedTopupPaymentSearch(
            GetCompletedTopupAllData(paymentProviderId, model.CurrencyCode).AsNoTracking(),
            model.Search);

        var totalCount = await unitOfWork.PaymentRepository.CountCompletedTopupReportRowsAsync(
            query,
            cancellationToken);
        var data = await unitOfWork.PaymentRepository.GetCompletedTopupReportRowsPageAsync(
            query,
            model.SortBy,
            model.IsAscending,
            model.PageNumber,
            model.PageSize,
            cancellationToken);

        var paymentMapper = new PaymentMapper(mapper);
        return Result<PagedResult<PaymentOutputModel>>.Success(
            new PagedResult<PaymentOutputModel>(paymentMapper.Map(data), totalCount),
            localizer[LocalizationKeys.PaymentCompletedTopupByCurrencyRowsListSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<PagedResult<PaymentOutputModel>>> GetCompletedTopupByDateRowsListAsync(
        CompletedTopupByDateRowsListInputModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.FromDate > model.ToDate)
        {
            return Result<PagedResult<PaymentOutputModel>>.Failure(
                localizer[LocalizationKeys.PaymentCompletedTopupByDateInvalidDateRange],
                AppHttpStatusCodes.BadRequest);
        }

        var partner = await paymentProviderAccessPolicy.GetPaymentProvidersReadScopeAsync(cancellationToken);
        if (!partner.IsSuccess)
            return partner.ToFailure<PagedResult<PaymentOutputModel>>();

        long? paymentProviderId = model.PaymentProviderId;
        if (partner.Value is not null)
        {
            var paymentProvider = await paymentProviderService.GetByPartnerIdAsync(partner.Value.Value);
            if (!paymentProvider.IsSuccess)
                return paymentProvider.ToFailure<PagedResult<PaymentOutputModel>>();
            paymentProviderId = paymentProvider.Value.Id;
        }

        NormalizeReportListQuery(model);
        var fromUtc = model.FromDate.ToUniversalTime();
        var toUtc = model.ToDate.ToUniversalTime();
        var query = ApplyCompletedTopupPaymentSearch(
            GetCompletedTopupByDateAllData(paymentProviderId, model.CurrencyCode, fromUtc, toUtc).AsNoTracking(),
            model.Search);

        var totalCount = await unitOfWork.PaymentRepository.CountCompletedTopupReportRowsAsync(
            query,
            cancellationToken);
        var data = await unitOfWork.PaymentRepository.GetCompletedTopupReportRowsPageAsync(
            query,
            model.SortBy,
            model.IsAscending,
            model.PageNumber,
            model.PageSize,
            cancellationToken);

        var paymentMapper = new PaymentMapper(mapper);
        return Result<PagedResult<PaymentOutputModel>>.Success(
            new PagedResult<PaymentOutputModel>(paymentMapper.Map(data), totalCount),
            localizer[LocalizationKeys.PaymentCompletedTopupByDateRowsListSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<PagedResult<CompletedPrepaidPayBillByDateRowOutputModel>>> GetCompletedPrepaidPayBillByDateRowsListAsync(
        CompletedPrepaidPayBillByDateRowsListInputModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.FromDate > model.ToDate)
        {
            return Result<PagedResult<CompletedPrepaidPayBillByDateRowOutputModel>>.Failure(
                localizer[LocalizationKeys.PaymentCompletedPrepaidPayBillByDateInvalidDateRange],
                AppHttpStatusCodes.BadRequest);
        }

        var billerScope = await billerAccessPolicy.GetBillersReadScopeAsync(cancellationToken);
        if (!billerScope.IsSuccess)
            return billerScope.ToFailure<PagedResult<CompletedPrepaidPayBillByDateRowOutputModel>>();

        long? billerId = model.BillerId;
        if (billerScope.Value is not null)
        {
            var biller = await billerService.GetByPartnerIdAsync(billerScope.Value.Value);
            if (!biller.IsSuccess)
                return biller.ToFailure<PagedResult<CompletedPrepaidPayBillByDateRowOutputModel>>();
            billerId = biller.Value.Id;
        }

        NormalizeReportListQuery(model);
        var fromUtc = model.FromDate.ToUniversalTime();
        var toUtc = model.ToDate.ToUniversalTime();
        var query = ApplyPayBillItemSearch(
            GetCompletedPayBillAllData(
                billerId,
                PrepaidBillingServiceTypeCode,
                OrderItemType.MeterCharge,
                model.CurrencyCode,
                fromUtc,
                toUtc),
            model.Search);
        query = ApplyPayBillItemRowsOrdering(query, model.SortBy, model.IsAscending);

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .Skip((model.PageNumber - 1) * model.PageSize)
            .Take(model.PageSize)
            .ToListAsync(cancellationToken);

        return Result<PagedResult<CompletedPrepaidPayBillByDateRowOutputModel>>.Success(
            new PagedResult<CompletedPrepaidPayBillByDateRowOutputModel>(
                data.Select(MapPrepaidPayBillByDateRow).ToList(),
                totalCount),
            localizer[LocalizationKeys.PaymentCompletedPrepaidPayBillByDateRowsListSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<PagedResult<CompletedPostpaidPayBillByDateRowOutputModel>>> GetCompletedPostpaidPayBillByDateRowsListAsync(
        CompletedPostpaidPayBillByDateRowsListInputModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.FromDate > model.ToDate)
        {
            return Result<PagedResult<CompletedPostpaidPayBillByDateRowOutputModel>>.Failure(
                localizer[LocalizationKeys.PaymentCompletedPostpaidPayBillByDateInvalidDateRange],
                AppHttpStatusCodes.BadRequest);
        }

        var dataResult = await GetCompletedPostpaidPayBillByDateDataAsync(
            new CompletedPostpaidPayBillByDateInputModel
            {
                GroupByBiller = model.GroupByBiller,
                BillerId = model.BillerId,
                CurrencyCode = model.CurrencyCode,
                FromDate = model.FromDate,
                ToDate = model.ToDate,
                CenterCode = model.CenterCode
            },
            cancellationToken);
        if (!dataResult.IsSuccess)
            return dataResult.ToFailure<PagedResult<CompletedPostpaidPayBillByDateRowOutputModel>>();

        NormalizeReportListQuery(model);
        var rows = BuildCompletedPostpaidPayBillByDateRows(dataResult.Value, model.GroupByBiller);
        rows = ApplyPostpaidPayBillByDateRowSearch(rows, model.Search);
        rows = ApplyPostpaidPayBillByDateRowOrdering(rows, model.SortBy, model.IsAscending);

        var totalCount = rows.Count;
        var page = rows
            .Skip((model.PageNumber - 1) * model.PageSize)
            .Take(model.PageSize)
            .ToList();

        return Result<PagedResult<CompletedPostpaidPayBillByDateRowOutputModel>>.Success(
            new PagedResult<CompletedPostpaidPayBillByDateRowOutputModel>(page, totalCount),
            localizer[LocalizationKeys.PaymentCompletedPostpaidPayBillByDateRowsListSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<PagedResult<CompletedPostpaidPayBillByCenterRowOutputModel>>> GetCompletedPostpaidPayBillByCenterRowsListAsync(
        CompletedPostpaidPayBillByCenterRowsListInputModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.FromDate > model.ToDate)
        {
            return Result<PagedResult<CompletedPostpaidPayBillByCenterRowOutputModel>>.Failure(
                localizer[LocalizationKeys.PaymentCompletedPostpaidPayBillByCenterInvalidDateRange],
                AppHttpStatusCodes.BadRequest);
        }

        var dataResult = await GetCompletedPostpaidPayBillByCenterDataAsync(
            new CompletedPostpaidPayBillByCenterInputModel
            {
                GroupByBiller = model.GroupByBiller,
                BillerId = model.BillerId,
                CurrencyCode = model.CurrencyCode,
                FromDate = model.FromDate,
                ToDate = model.ToDate,
                CenterCode = model.CenterCode
            },
            cancellationToken);
        if (!dataResult.IsSuccess)
            return dataResult.ToFailure<PagedResult<CompletedPostpaidPayBillByCenterRowOutputModel>>();

        NormalizeReportListQuery(model);
        var rows = BuildCompletedPostpaidPayBillByCenterRows(dataResult.Value, model.GroupByBiller);
        rows = ApplyPostpaidPayBillByCenterRowSearch(rows, model.Search);
        rows = ApplyPostpaidPayBillByCenterRowOrdering(rows, model.SortBy, model.IsAscending);

        var totalCount = rows.Count;
        var page = rows
            .Skip((model.PageNumber - 1) * model.PageSize)
            .Take(model.PageSize)
            .ToList();

        return Result<PagedResult<CompletedPostpaidPayBillByCenterRowOutputModel>>.Success(
            new PagedResult<CompletedPostpaidPayBillByCenterRowOutputModel>(page, totalCount),
            localizer[LocalizationKeys.PaymentCompletedPostpaidPayBillByCenterRowsListSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<IReadOnlyList<TopupRateOutputModel>>> GetTopupRate(TopupRateInputModel model)
    {
        var result = await GetTopupRateGrouped(model.GroupByProvider, model.GroupByMethod, model.CurrencyId, model.PaymentProviderId, model.PaymentMethodId);
        if (!result.IsSuccess)
            return result.ToFailure<IReadOnlyList<TopupRateOutputModel>>();
        return Result<IReadOnlyList<TopupRateOutputModel>>.Success(
            result.Value,
            localizer[LocalizationKeys.PaymentTopupRateQuerySuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<PagedResult<PaymentOutputModel>>> GetTopupRateList(TopupRateListInputModel model)
    {
        var partner = await paymentProviderAccessPolicy.GetPaymentProvidersReadScopeAsync();
        if (!partner.IsSuccess)
            return partner.ToFailure<PagedResult<PaymentOutputModel>>();
        if (partner.Value is not null)
        {

            var paymentProvider = await paymentProviderService.GetByPartnerIdAsync(partner.Value.Value);
            if (!paymentProvider.IsSuccess)
                return paymentProvider.ToFailure<PagedResult<PaymentOutputModel>>();
            model.PaymentProviderId = paymentProvider.Value.Id;
        }

        var normalizedQuery = NormalizeTopupRateQuery(model);
        var data = await unitOfWork.PaymentRepository.GetTopupRateListAsync(normalizedQuery);
        var totalCount = await unitOfWork.PaymentRepository.CountTopupRateListAsync(normalizedQuery);
        var paymentMapper = new PaymentMapper(mapper);
        var result = paymentMapper.Map(data); ;
        return Result<PagedResult<PaymentOutputModel>>.Success(
            new PagedResult<PaymentOutputModel>(result, totalCount),
            localizer[LocalizationKeys.PaymentTopupRateListSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<FileContentResult>> ExportTopupRateList(TopupRateExportInputModel model)
    {
        var partner = await paymentProviderAccessPolicy.GetPaymentProvidersReadScopeAsync();
        if (!partner.IsSuccess)
            return partner.ToFailure<FileContentResult>();
        if (partner.Value is not null)
        {

            var paymentProvider = await paymentProviderService.GetByPartnerIdAsync(partner.Value.Value);
            if (!paymentProvider.IsSuccess)
                return paymentProvider.ToFailure<FileContentResult>();
            model.PaymentProviderId = paymentProvider.Value.Id;
        }

        var data = GetTopupRateAllData(model.CurrencyId, model.PaymentProviderId, model.PaymentMethodId).ToList();

        var paymentMapper = new PaymentMapper(mapper);
        var data_payments = paymentMapper.MapExcel(data);
        var data_objects = mapper.Map<List<object>>(data_payments);
        var export = await excelFileExportService.ExportExcelFileAsync("topupRate", data_objects, typeof(PaymentExcelModel));
        if (!export.IsSuccess)
        {
            return export;
        }

        return Result<FileContentResult>.Success(
            export.Value!,
            localizer[LocalizationKeys.PaymentTopupRateExportSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<FileContentResult>> ExportTopupRate(TopupRateInputModel model)
    {
        var result = await GetTopupRateGroupedExcel(model.GroupByProvider, model.GroupByMethod, model.CurrencyId, model.PaymentProviderId, model.PaymentMethodId);
        if (!result.IsSuccess)
            return result.ToFailure<FileContentResult>();
        var data_objects = mapper.Map<List<object>>(result.Value);
        var export = await excelFileExportService.ExportExcelFileAsync("topupRate-grouped", data_objects, typeof(TopupRateExcelModel));
        if (!export.IsSuccess)
        {
            return export;
        }

        return Result<FileContentResult>.Success(
            export.Value!,
            localizer[LocalizationKeys.PaymentTopupRateExportSuccess],
            AppHttpStatusCodes.Ok);        
    }

    public async Task<Result<IReadOnlyList<PaymentRateOutputModel>>> GetPaymentRate(PaymentRateInputModel model)
    {
        var result = await GetPaymentRateGrouped(model.GroupByBiller, model.GroupByBillingService, model.CurrencyId, model.BillerId, model.BillingServiceId);
        if (!result.IsSuccess)
            return result.ToFailure<IReadOnlyList<PaymentRateOutputModel>>();
        return Result<IReadOnlyList<PaymentRateOutputModel>>.Success(result.Value, AppHttpStatusCodes.Ok);
    }
    public async Task<Result<FileContentResult>> ExportPaymentRate(PaymentRateInputModel model)
    {
        var result = await GetPaymentRateGroupedExcel(model.GroupByBiller, model.GroupByBillingService, model.CurrencyId, model.BillerId, model.BillingServiceId);
        if (!result.IsSuccess)
            return result.ToFailure<FileContentResult>();
        var data_objects = mapper.Map<List<object>>(result.Value);
        var export = await excelFileExportService.ExportExcelFileAsync("topupRate-grouped", data_objects, typeof(PaymentRateExcelModel));
        if (!export.IsSuccess)
        {
            return export;
        }

        return Result<FileContentResult>.Success(
            export.Value!,
            localizer[LocalizationKeys.PaymentTopupRateExportSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<PagedResult<PaymentOrderItemOutputModel>>> GetPaymentRateList(PaymentRateListInputModel model)
    {
        var partner = await billerAccessPolicy.GetBillersReadScopeAsync();
        if (!partner.IsSuccess)
            return partner.ToFailure<PagedResult<PaymentOrderItemOutputModel>>();
        if (partner.Value is not null)
        {

            var biller = await billerService.GetByPartnerIdAsync(partner.Value.Value);
            if (!biller.IsSuccess)
                return biller.ToFailure<PagedResult<PaymentOrderItemOutputModel>>();
            model.BillerId = biller.Value.Id;
        }

        var normalizedQuery = NormalizePaymentRateQuery(model);
        var data = await unitOfWork.PaymentOrderItemRepository.GetPaymentRateListAsync(normalizedQuery);
        var totalCount = await unitOfWork.PaymentOrderItemRepository.CountPaymentRateListAsync(normalizedQuery);
        var result = mapper.Map<IReadOnlyList<PaymentOrderItemOutputModel>>(data);
        return Result<PagedResult<PaymentOrderItemOutputModel>>.Success(
            new PagedResult<PaymentOrderItemOutputModel>(result, totalCount));
    }
    public async Task<Result<FileContentResult>> ExportPaymentList(PaymentRateExportInputModel model)
    {
        var partner = await billerAccessPolicy.GetBillersReadScopeAsync();
        if (!partner.IsSuccess)
            return partner.ToFailure<FileContentResult>();
        if (partner.Value is not null)
        {

            var biller = await billerService.GetByPartnerIdAsync(partner.Value.Value);
            if (!biller.IsSuccess)
                return biller.ToFailure<FileContentResult>();
            model.BillerId = biller.Value.Id;
        }

        var data = GetPaymentRateAllData(model.CurrencyId, model.BillerId,model.BillingServiceId).ToList();
        var data_pois = mapper.Map<IReadOnlyList<PaymentOrderItemExcelModel>>(data);
        var data_objects = mapper.Map<List<object>>(data_pois);
        return await excelFileExportService.ExportExcelFileAsync("paymentRate", data_objects, typeof(PaymentOrderItemExcelModel));
    }
    public async Task<Result<PagedResult<PaymentOutputModel>>> GetPaymentsForReviewAsync(
    PaymentReviewQueryInputModel model, 
    CancellationToken cancellationToken = default)
    {
        var partner = await billerAccessPolicy.GetBillersReadScopeAsync();
        if (!partner.IsSuccess)
            return partner.ToFailure<PagedResult<PaymentOutputModel>>();

        var normalizedQuery = NormalizeQuery(model);

        if (partner.Value is not null)
        {
            var biller = await billerService.GetByPartnerIdAsync(partner.Value.Value);
            if (!biller.IsSuccess)
                return biller.ToFailure<PagedResult<PaymentOutputModel>>();

            normalizedQuery.BillerId = biller.Value.Id;
        }

        var payments = await unitOfWork.PaymentRepository.GetPaymentsForReviewAsync(normalizedQuery, cancellationToken);
        var totalCount = await unitOfWork.PaymentRepository.CountPaymentsForReviewAsync(normalizedQuery, cancellationToken);

        var items = mapper.Map<PaymentOutputModel[]>(payments);

        return Result<PagedResult<PaymentOutputModel>>.Success(
            new PagedResult<PaymentOutputModel>(items, totalCount));
    }

    private static PaymentReviewQueryInputModel NormalizeQuery(PaymentReviewQueryInputModel? model)
    {
        if (model == null)
        {
            return new PaymentReviewQueryInputModel
            {
                PageNumber = 1,
                PageSize = 20,
                IsAscending = false
            };
        }

        return new PaymentReviewQueryInputModel
        {
            PageNumber = model.PageNumber <= 0 ? 1 : model.PageNumber,
            PageSize = model.PageSize <= 0 ? 20 : Math.Min(model.PageSize, 100),
            SortBy = string.IsNullOrWhiteSpace(model.SortBy) ? null : model.SortBy.Trim(),
            IsAscending = model.IsAscending,
            Search = string.IsNullOrWhiteSpace(model.Search) ? null : model.Search.Trim(),
            BillerId = model.BillerId // الحفاظ على القيمة إن وجدت
        };
    }

    #endregion

    #region Private Methods

    #region Validation Methods
    //private async Task<Result<PaymentProviderEntity?>> GetPaymentProviderByContext(CancellationToken cancellationToken = default)
    //{
    //    if (currentUserContext.IsInRole(IdentityRoleConstants.Admin))
    //    {
    //        return Result<PaymentProviderEntity?>.Success(null);
    //    }
    //    if (currentUserContext.IsInRole(IdentityRoleConstants.PspAdmin))
    //    {
    //        var user = await unitOfWork.UserRepository.GetByIamRefWithPartnerAsync(currentUserContext.UserId, cancellationToken);
    //        if (user?.PartnerId is null)
    //            return Result<PaymentProviderEntity?>.Failure(localizer[LocalizationKeys.PartnerNotFound], AppHttpStatusCodes.BadRequest);
    //        var provider = await unitOfWork.PaymentProviderRepository.GetByPartnerId(user.PartnerId.Value, cancellationToken);
    //        return Result<PaymentProviderEntity?>.Success(provider);
    //    }
    //    else
    //        throw new UnauthorizedAccessException();                
    //}

    //private async Task<Result<PaymentProviderEntity?>> GetBillerByContext(CancellationToken cancellationToken = default)
    //{
    //    if (currentUserContext.IsInRole(IdentityRoleConstants.Admin))
    //    {
    //        return Result<PaymentProviderEntity?>.Success(null);
    //    }
    //    if (currentUserContext.IsInRole(IdentityRoleConstants.PspAdmin))
    //    {
    //        var user = await unitOfWork.UserRepository.GetByIamRefWithPartnerAsync(currentUserContext.UserId, cancellationToken);
    //        if (user?.PartnerId is null)
    //            return Result<PaymentProviderEntity?>.Failure(localizer[LocalizationKeys.PartnerNotFound], AppHttpStatusCodes.BadRequest);
    //        var provider = await unitOfWork.PaymentProviderRepository.GetByPartnerId(user.PartnerId.Value, cancellationToken);
    //        return Result<PaymentProviderEntity?>.Success(provider);
    //    }
    //    else
    //        throw new UnauthorizedAccessException();


    //}

    private async Task<Result<CustomerEntity>> ValidateCustomerAsync()
    {
        var customer = await GetCurrentCustomerByPartnerAsync();
        if (customer is null)
        {
            return Result<CustomerEntity>.Failure(
                localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.Unauthorized);
        }

        return Result<CustomerEntity>.Success(customer);
    }

    private async Task<CustomerEntity?> GetCurrentCustomerByPartnerAsync(CancellationToken cancellationToken = default)
    {
        var partnerAccess = await GetCurrentUserPartnerAccessAsync(cancellationToken);
        if (partnerAccess is null)
        {
            return null;
        }

        return await unitOfWork.CustomerRepository.GetByPartnerIdAsync(partnerAccess.PartnerId, cancellationToken);
    }

    private async Task<long?> GetCurrentUserPartnerIdAsync(CancellationToken cancellationToken = default)
    {
        var partnerAccess = await GetCurrentUserPartnerAccessAsync(cancellationToken);
        return partnerAccess?.PartnerId;
    }

    private async Task<UserPartnerAccess?> GetCurrentUserPartnerAccessAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currentUserContext.UserId))
        {
            return null;
        }

        var user = await unitOfWork.UserRepository.GetByIamRefWithPartnerAsync(currentUserContext.UserId, cancellationToken);
        if (user?.PartnerId is null)
        {
            return null;
        }

        var partnerTypeCode = user.Partner?.PartnerType?.Code;
        var isPlatform = string.Equals(partnerTypeCode, PlatformPartnerTypeCode, StringComparison.OrdinalIgnoreCase);
        return new UserPartnerAccess(user.PartnerId.Value, isPlatform);
    }

    private async Task<Result<bool>> CompletePaymentArtifactsAsync(PaymentEntity payment)
    {
        bool isTransactional = false;
        if (payment.Direction == PaymentDirectionEnum.OUT)
        {
            isTransactional = payment.PaymentOrder!.Items.Any(x => x.BillingService!.IsTransactional);
            var isOnPay = payment.PaymentOrder!.Items.Any(x => x.BillingService!.Currencies.Any(z => z.CurrencyId == payment.CurrencyId && z.ApplyType == ApplyTypeEnum.ON_PAY));
            isTransactional &= isOnPay;
        }
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            payment.ChangePaymentArtifactStatus(PaymentStatus.Completed);
            payment.SetNeedsReview(false);
            await HandlePaymentControlsAsync(payment.Id);
            var journalsForPaymentResult = await CreateJournalsForPayment(payment);
            var (journals, accounts, platformInvoices) = journalsForPaymentResult.Value;
            accounts = await unitOfWork.AccountRepository.GetByIdsWithLockAsync(accounts.Select(a => a.Id).ToArray());
            foreach (var journal in journals)
            {
                foreach (JournalLineEntity line in journal.Lines)
                {
                    var account = accounts.First(x => x.Id == line.AccountId);
                    line.Balance = account.Balance + line.Credit - line.Debit;
                    account.SetBalance(line.Balance);
                }
            }

            await TrackPlatformInvoicesAndJournalsForPaymentAsync(platformInvoices, journals, ct);
            unitOfWork.AccountRepository.UpdateRange(accounts);
            if (isTransactional)
            {
                var applyResult = await billingServiceService.ApplyPaymentAfterCompleteAsync(payment);
                if (applyResult.IsSuccess && applyResult.Value)
                    await unitOfWork.CompleteAsync();
                else if (!applyResult.IsSuccess)
                    throw new Exception(applyResult.ToFailure<bool>().Message);
                else
                    throw new Exception(localizer[LocalizationKeys.UnexpectedError]);
            }
            await unitOfWork.CompleteAsync();
        });

        await SendNotificationForPaymentAsync(payment);
        if (!isTransactional)
        {
            var applyResult = await billingServiceService.ApplyPaymentAfterCompleteAsync(payment);
            if (applyResult.IsSuccess)
            {
                await unitOfWork.CompleteAsync();
            }
        }        
        return Result<bool>.Success(true);
    }

    private async Task<Result<AccountEntity>> ValidateAccountAsync(long partnerId, int currencyId, int accountType)
    {
        var account =
                await unitOfWork.AccountRepository.GetByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(partnerId, currencyId, accountType);
        if (account is null)
        {
            return Result<AccountEntity>.Failure(
                localizer[LocalizationKeys.AccountNotFoundForCustomer],
                AppHttpStatusCodes.NotFound);
        }

        return Result<AccountEntity>.Success(account);
    }

    private async Task<Result<CurrencyEntity>> ValidateCurrencyByCodeAsync(string currencyCode)
    {
        var allCurrencies = CachingFactory.GetValue<CurrencyEntity>();
        var currency = allCurrencies.FirstOrDefault(x => x.Code == currencyCode);
        logger.LogInformation($"START_SUMMARY_3 {currency?.Code}");
        if (currency is null)
        {
            return Result<CurrencyEntity>.Failure(
                localizer[LocalizationKeys.CommonKeys.CurrencyNotFound, currencyCode],
                AppHttpStatusCodes.NotFound);
        }

        return Result<CurrencyEntity>.Success(currency);
    }

    private async Task<Result<CurrencyEntity>> ValidateCurrencyById(int currencyId)
    {
        var allCurrencies = CachingFactory.GetValue<CurrencyEntity>();
        var currency = allCurrencies.FirstOrDefault(x => x.Id == currencyId);
        if (currency is null)
        {
            return Result<CurrencyEntity>.Failure(
                localizer[LocalizationKeys.CommonKeys.CurrencyNotFound, currencyId],
                AppHttpStatusCodes.NotFound);
        }

        return Result<CurrencyEntity>.Success(currency);
    }

    private async Task<Result<ChannelEntity>> ValidateChannelAsync(string channelCode)
    {
        var normalizedChannelCode = channelCode.Trim().ToUpperInvariant();
        var channel = CachingFactory.GetValue<ChannelEntity>(x => x.Code == normalizedChannelCode).FirstOrDefault();
        if (channel is null)
        {
            return Result<ChannelEntity>.Failure(
                localizer[LocalizationKeys.PaymentInitializeChannelNotConfigured, normalizedChannelCode],
                AppHttpStatusCodes.NotFound);
        }

        if (!channel.Active)
        {
            return Result<ChannelEntity>.Failure(
                localizer[LocalizationKeys.PaymentInitializeChannelNotConfigured, normalizedChannelCode],
                AppHttpStatusCodes.BadRequest);
        }

        return Result<ChannelEntity>.Success(channel);
    }

    private async Task<Result<(string PaymentServiceProviderCode, string PaymentMethodCode, bool requirePhoneNumber)>> ResolveTopupPaymentMethodCodesAsync(
        PaymentInitializeInputModel model)
    {
        var hasPspCode = !string.IsNullOrWhiteSpace(model.PaymentServiceProviderCode);
        var hasMethodCode = !string.IsNullOrWhiteSpace(model.PaymentMethodCode);
        var defaultPaymentPSPInfo = await platformConstantService.GetDefaultPaymentMethodCodesAsync();
        if (defaultPaymentPSPInfo == null || !defaultPaymentPSPInfo.IsSuccess)
        {
            return Result<(string, string, bool)>.Failure(
            localizer[LocalizationKeys.InternalServerErrorDefaultMessage],
            AppHttpStatusCodes.BadRequest);
        }
        if (!hasPspCode && !hasMethodCode)
        {
            if (model.PaymentType == PaymentTypeEnum.BANK_CARD)
            {
                return Result<(string, string, bool)>.Success((
                    EasypayProviderCode,
                    EasypayCardMethodCode,
                    false));
            }

            if (string.IsNullOrEmpty(model.FromNumber) && defaultPaymentPSPInfo.Value.requirePhoneNumber)
            {
                return Result<(string, string, bool)>.Success((defaultPaymentPSPInfo.Value.PaymentServiceProviderCode, defaultPaymentPSPInfo.Value.PaymentMethodCode, defaultPaymentPSPInfo.Value.requirePhoneNumber));
            }
            else 
            {
                var normalizedPhone = NormalizeFromNumber(model.FromNumber);
                var countryCode = normalizedPhone.Substring(0, 3);
                var prefix = normalizedPhone.Substring(3, 2);
                var paymentProviderRouter = await unitOfWork.PaymentProviderRouterRepository.GetByPrefix(countryCode, prefix);
                if(paymentProviderRouter is null)
                {
                    return Result<(string, string, bool)>.Failure(
                        localizer[LocalizationKeys.PaymentPspInitPhoneNumberNotSupported, normalizedPhone],
                        AppHttpStatusCodes.NotFound);
                }
                return Result<(string, string, bool)>.Success((
                    paymentProviderRouter.PaymentProviderCode!.Trim().ToUpperInvariant(),
                    paymentProviderRouter.PaymentMethodCode!.Trim().ToUpperInvariant(),
                    false));
            }
            //return await platformConstantService.GetDefaultPaymentMethodCodesAsync();
        }

        if (!hasPspCode || !hasMethodCode)
        {
            return Result<(string, string, bool)>.Failure(
                localizer[LocalizationKeys.PaymentMethodCodesBothOrNeitherRequired],
                AppHttpStatusCodes.BadRequest);
        }        
        var paymentServiceProviderCode = model.PaymentServiceProviderCode!.Trim().ToUpperInvariant();
        var paymentMethodCode = model.PaymentMethodCode!.Trim().ToUpperInvariant();
        var requirePhoneNumber = !IsEasypayCardMethod(paymentServiceProviderCode, paymentMethodCode) &&
                                 string.IsNullOrEmpty(model.FromNumber) &&
                                 defaultPaymentPSPInfo.Value.requirePhoneNumber;

        return Result<(string, string, bool)>.Success((
            paymentServiceProviderCode,
            paymentMethodCode,
            requirePhoneNumber));
    }

    private async Task<Result<(PaymentMethodEntity, PaymentMethodCurrencyEntity, PspConnector)>> ValidatePaymentMethodAgainstCurrencyAndAmountAsync(
        string paymentMethodCode,
        string paymentServiceProviderCode,
        CurrencyEntity currency,
        decimal amount)
    {
        var paymentMethod = await unitOfWork.PaymentMethodRepository.GetByCodeAndPspWithRelationsAsync(
            paymentMethodCode,
            paymentServiceProviderCode);
        if (paymentMethod is null)
        {
            return Result<(PaymentMethodEntity, PaymentMethodCurrencyEntity, PspConnector)>.Failure(
                localizer[LocalizationKeys.PaymentInitializeMethodNotFound, paymentMethodCode],
                AppHttpStatusCodes.NotFound);
        }

        if (!paymentMethod.Active || paymentMethod.PaymentProvider?.Partner?.Active != true)
        {
            return Result<(PaymentMethodEntity, PaymentMethodCurrencyEntity, PspConnector)>.Failure(
                localizer[LocalizationKeys.PaymentInitializeMethodInactive, paymentMethod.Code],
                AppHttpStatusCodes.BadRequest);
        }


        if (paymentMethod.ValidateCompatibility() < 0)
        {
            return Result<(PaymentMethodEntity, PaymentMethodCurrencyEntity, PspConnector)>.Failure(
                localizer[LocalizationKeys.PaymentMethodIncompatible, paymentMethod.Name],
                AppHttpStatusCodes.BadRequest);
        }

        var methodCurrency = paymentMethod.PaymentMethodCurrencies
            .FirstOrDefault(x => x.CurrencyId == currency.Id && x.Active);

        if (methodCurrency is null)
        {
            return Result<(PaymentMethodEntity, PaymentMethodCurrencyEntity, PspConnector)>.Failure(
                localizer[LocalizationKeys.PaymentMethodCurrencyNotConfigured, paymentMethod.Name],
                AppHttpStatusCodes.BadRequest);
        }

        if (!methodCurrency.CheckLimits(amount))
        {
            return Result<(PaymentMethodEntity, PaymentMethodCurrencyEntity, PspConnector)>.Failure(
                localizer[LocalizationKeys.PaymentAmountOutsideMethodLimits, amount, currency.Abbreviation, paymentMethod.Name],
                AppHttpStatusCodes.BadRequest);
        }


        var connector = pspConnectorResolver.Resolve(paymentMethod.PaymentProvider.Code);
        if (connector is null)
        {
            return Result<(PaymentMethodEntity, PaymentMethodCurrencyEntity, PspConnector)>.Failure(
                localizer[LocalizationKeys.PaymentPspConnectorMissing, paymentMethod.PaymentProvider.Code],
                AppHttpStatusCodes.BadRequest);
        }

        return Result<(PaymentMethodEntity, PaymentMethodCurrencyEntity, PspConnector)>.Success((paymentMethod, methodCurrency, connector));
    }
    private async Task<Result<(BillingServiceEntity, BillingServiceCurrencyEntity, BillerConnector)>> ValidateBillingServiceAgainstCurrencyAndAmountAsync(
        string billingServiceCode,
        string billerCode,
        string billingRef,
        CurrencyEntity currency,
        decimal amount)
    {
        if (string.IsNullOrWhiteSpace(billingRef))
        {
            return Result<(BillingServiceEntity, BillingServiceCurrencyEntity, BillerConnector)>.Failure(
                localizer[LocalizationKeys.BillingInquiryBillingRefRequired],
                AppHttpStatusCodes.NotFound);
        }
        var billingService = await unitOfWork.BillingServiceRepository.GetByBillerCodeAndBillingServiceCodeWithCurrenciesAsync(
            billerCode,
            billingServiceCode);
        if (billingService is null)
        {
            return Result<(BillingServiceEntity, BillingServiceCurrencyEntity, BillerConnector)>.Failure(
                localizer[LocalizationKeys.BillingInquiryServiceNotFound, billerCode,billingServiceCode],
                AppHttpStatusCodes.NotFound);
        }

        if (!billingService.Active || billingService.Biller?.Partner.Active != true)
        {
            return Result<(BillingServiceEntity, BillingServiceCurrencyEntity, BillerConnector)>.Failure(
                localizer[LocalizationKeys.PaymentBillingServiceOrPartnerInactive, billingService.Code],
                AppHttpStatusCodes.BadRequest);
        }

        var billingServiceCurrency = billingService.Currencies
            .FirstOrDefault(x => x.CurrencyId == currency.Id && x.Active);

        if (billingServiceCurrency is null)
        {
            return Result<(BillingServiceEntity, BillingServiceCurrencyEntity, BillerConnector)>.Failure(
                localizer[LocalizationKeys.PaymentBillingServiceCurrencyNotConfigured, billingService.Code],
                AppHttpStatusCodes.BadRequest);
        }

        var connector = billerConnectorResolver.Resolve(billerCode);
        if (connector is null)
        {
            return Result<(BillingServiceEntity, BillingServiceCurrencyEntity, BillerConnector)>.Failure(
                localizer[LocalizationKeys.PaymentBillerConnectorNotConfigured, billerCode],
                AppHttpStatusCodes.BadRequest);
        }
        if (!billingServiceCurrency.CheckLimits(amount))
        {
            return Result<(BillingServiceEntity, BillingServiceCurrencyEntity, BillerConnector)>.Failure(
                localizer[LocalizationKeys.PaymentAmountOutsideBillingServiceLimits, amount, currency.Abbreviation, billingService.Code],
                AppHttpStatusCodes.BadRequest);
        }
        var eligibilityResult = await connector.IsEligible(new BillingServices.Models.Internal.IsEligibleRequestModel(
            ServiceCode: billingService.Code,
            BillingRef: billingRef,
            Amount: amount));
        if (!eligibilityResult.IsSuccess)
        {
            logger.LogInformation($"Failed to validate eligibility for billing service '{billingService.Code}' with biller connector for biller '{billerCode}' with message: {eligibilityResult.Message}", billingService.Code, billerCode, eligibilityResult.Message);
            // Depending on the business decision, you might want to fail the whole operation if eligibility check fails, or you might want to allow it to proceed with a warning. Here we choose to fail.
            return Result<(BillingServiceEntity, BillingServiceCurrencyEntity, BillerConnector)>.Failure(
                               localizer[LocalizationKeys.PaymentBillingEligibilityNotMet, billingService.Code, amount, currency.Abbreviation, eligibilityResult.Message!],
                               AppHttpStatusCodes.BadRequest);
        }



        return Result<(BillingServiceEntity, BillingServiceCurrencyEntity, BillerConnector)>.Success((billingService, billingServiceCurrency, connector));
    }

    private async Task<Result<List<Guid>>> ResolvePayBillInvoiceIdsAsync(PayBillsInputModel model)
    {
        var hasIds = model.InvoicesIds is { Count: > 0 };
        var hasRefs = model.InvoiceRefs is { Count: > 0 };

        if (hasIds && hasRefs)
        {
            return Result<List<Guid>>.Failure(
                localizer[LocalizationKeys.BillingPayBillsAmbiguousInvoiceIdentifiers],
                AppHttpStatusCodes.BadRequest);
        }

        if (!hasIds && !hasRefs)
        {
            return Result<List<Guid>>.Failure(
                localizer[LocalizationKeys.BillingPayBillsInvoiceIdentifierRequired],
                AppHttpStatusCodes.BadRequest);
        }

        if (hasIds)
        {
            return Result<List<Guid>>.Success(model.InvoicesIds!);
        }

        var distinctRefs = model.InvoiceRefs!
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (distinctRefs.Count == 0)
        {
            return Result<List<Guid>>.Failure(
                localizer[LocalizationKeys.BillingPayBillsInvoiceIdentifierRequired],
                AppHttpStatusCodes.BadRequest);
        }

        var invoiceIds = new List<Guid>(distinctRefs.Count);
        foreach (var invoiceRef in distinctRefs)
        {
            var resolveResult = await billingInvoiceRefResolver.ResolveInvoiceByRefAsync(
                invoiceRef,
                requireOutstanding: false);

            if (!resolveResult.IsSuccess)
            {
                return resolveResult.ToFailure<List<Guid>>();
            }

            invoiceIds.Add(resolveResult.Value!.Id);
        }

        return Result<List<Guid>>.Success(invoiceIds);
    }

    private async Task<Result<IReadOnlyList<InvoicePaymentDetailInternalModel>>> ValidateBillsForInitializationAsync(
        List<Guid> inputInvoicesIds,
        decimal amount)
    {
        //TODO: must get invoices from billler-connector (snel will load them from db, but in general this functionality should be on BillerConnector class
        var invoices =
            await unitOfWork.InvoiceRepository.GetByIDsWithBillingServiceWithBillarPartnerAsync(inputInvoicesIds);
        if (invoices is null || inputInvoicesIds.Count <= 0)
        {
            return Result<IReadOnlyList<InvoicePaymentDetailInternalModel>>.Failure(
                localizer[LocalizationKeys.BillingPayBillsInvoicesNotFound],
                AppHttpStatusCodes.BadRequest);
        }

        if (invoices.Count != inputInvoicesIds.Count)
        {
            return Result<IReadOnlyList<InvoicePaymentDetailInternalModel>>.Failure(
                localizer[LocalizationKeys.BillingPayBillsSomeInvoicesNotFound],
                AppHttpStatusCodes.BadRequest);
        }

        if (invoices.Select(i => i.CurrencyId).Distinct().Count() > 1)
        {
            return Result<IReadOnlyList<InvoicePaymentDetailInternalModel>>.Failure(
                localizer[LocalizationKeys.BillingPayBillsInvoicesMustHaveSameCurrency],
                AppHttpStatusCodes.BadRequest);
        }

        if (invoices.Select(i => i.BillingRef).Distinct().Count() > 1)
        {
            return Result<IReadOnlyList<InvoicePaymentDetailInternalModel>>.Failure(
                localizer[LocalizationKeys.BillsMustBeForTheSameSubscription],
                AppHttpStatusCodes.BadRequest);
        }

        var currencyId = invoices.First().CurrencyId;
        var allCurrencies = CachingFactory.GetValue<CurrencyEntity>();
        var currency = allCurrencies.FirstOrDefault(x => x.Id == currencyId);
        if (currency is null)
        {
            return Result<IReadOnlyList<InvoicePaymentDetailInternalModel>>.Failure(
                localizer[LocalizationKeys.PaymentInvoiceCurrencyNotConfigured],
                AppHttpStatusCodes.NotFound);
        }

        var billingserviceId = invoices.First().BillingServiceId;
        var billingService = await unitOfWork.BillingServiceRepository.GetByIdWithRelationsAsync(billingserviceId);
        if (billingService is null)
        {
            return Result<IReadOnlyList<InvoicePaymentDetailInternalModel>>.Failure(
                localizer[LocalizationKeys.PaymentInvoiceBillingServiceMissing],
                AppHttpStatusCodes.NotFound);
        }

        var billingServiceCurrency =
            billingService.Currencies.FirstOrDefault(x => x.CurrencyId == currencyId && x.Active);
        if (billingServiceCurrency is null)
        {
            return Result<IReadOnlyList<InvoicePaymentDetailInternalModel>>.Failure(
                localizer[LocalizationKeys.PaymentBillingServiceCurrencyInactive],
                AppHttpStatusCodes.NotFound);
        }

        var invoicePaymentDetails = new List<InvoicePaymentDetailInternalModel>(invoices.Count);
        if (invoices.Count == 1)
        {
            var invoice = invoices[0];
            if (invoice.AmountOutstanding <= 0)
            {
                return Result<IReadOnlyList<InvoicePaymentDetailInternalModel>>.Failure(
                    localizer[LocalizationKeys.InvoiceHasNoDueAmount, invoice.InvoiceRef],
                    AppHttpStatusCodes.BadRequest);
            }

            if (amount > invoice.AmountOutstanding)
            {
                return Result<IReadOnlyList<InvoicePaymentDetailInternalModel>>.Failure(
                    localizer[LocalizationKeys.PaymentAmountExceedsInvoiceOutstandingAmount, invoice.Id],
                    AppHttpStatusCodes.BadRequest);
            }

            var paidPaymentOrderItems = await unitOfWork.PaymentOrderItemRepository
                .GetByInvoiceIDAndStatusAsync(invoice.Id, PaymentOrderItemStatus.Paid);
            if (paidPaymentOrderItems.Count > 0)
            {
                var totalPaidNotAppliedAmount = paidPaymentOrderItems.Sum(poi => poi.Amount);
                if (amount > (invoice.AmountOutstanding - totalPaidNotAppliedAmount))
                {
                    return Result<IReadOnlyList<InvoicePaymentDetailInternalModel>>.Failure(
                        localizer[LocalizationKeys.PaymentAmountExceedsInvoiceOutstandingAmount, invoice.Id],
                        AppHttpStatusCodes.BadRequest);
                }
            }
            if (billingServiceCurrency.CheckLimits(amount) == false)
            {
                return Result<IReadOnlyList<InvoicePaymentDetailInternalModel>>.Failure(
                    localizer[LocalizationKeys.PaymentAmountOutsideBillingServiceLimits, amount, currency.Abbreviation, billingService.Code],
                    AppHttpStatusCodes.BadRequest);
            }
            var connector = billerConnectorResolver.Resolve(billingService.Biller.Code);
            if (connector is null)
            {
                return Result<IReadOnlyList<InvoicePaymentDetailInternalModel>>.Failure(
                    localizer[LocalizationKeys.PaymentBillerConnectorNotConfigured, billingService.Biller.Code],
                    AppHttpStatusCodes.BadRequest);
            }
            var eligibilityResult = await connector.IsEligible(new BillingServices.Models.Internal.IsEligibleRequestModel(
            ServiceCode: billingService.Code,
            BillingRef: invoice.BillingRef,
            Amount: amount));
            if (!eligibilityResult.IsSuccess)
            {
                logger.LogInformation($"Failed to validate eligibility for billing service '{billingService.Code}' with biller connector for biller '{billingService.Biller.Code}' with message: {eligibilityResult.Message}", billingService.Code, billingService.Biller.Code, eligibilityResult.Message);
                // Depending on the business decision, you might want to fail the whole operation if eligibility check fails, or you might want to allow it to proceed with a warning. Here we choose to fail.
                return eligibilityResult.ToFailure<IReadOnlyList<InvoicePaymentDetailInternalModel>>();
                //Result<IReadOnlyList<InvoicePaymentDetailInternalModel>>.Failure(
                //                   localizer[LocalizationKeys.PaymentBillingEligibilityNotMet, billingService.Code, amount, currency.Abbreviation, eligibilityResult.Message!],
                //                   AppHttpStatusCodes.BadRequest);
            }
            invoicePaymentDetails.Add(new InvoicePaymentDetailInternalModel(invoice, currency, billingServiceCurrency,
                billingServiceCurrency.CalculateCharges(amount), amount));
        }
        else
        {
            decimal totalNetOutstandingAmount = 0m;
            var paidInvoicesPaymentOrderItems = await unitOfWork.PaymentOrderItemRepository
                .GetByInvoicesIDsAndStatusAsync(inputInvoicesIds, PaymentOrderItemStatus.Paid);

            foreach (var invoice in invoices)
            {
                if (invoice.AmountOutstanding == 0)
                {
                    return Result<IReadOnlyList<InvoicePaymentDetailInternalModel>>.Failure(
                        localizer[LocalizationKeys.InvoiceHasNoDueAmount, invoice.InvoiceRef],
                        AppHttpStatusCodes.BadRequest);
                }

                var invoiceNonPaidOutstandingAmount = invoice.AmountOutstanding;
                if (paidInvoicesPaymentOrderItems.Count > 0)
                {
                    var paidAmount = paidInvoicesPaymentOrderItems
                        .Where(poi => poi.InvoiceId == invoice.Id)
                        .Sum(poi => poi.Amount);
                    invoiceNonPaidOutstandingAmount -= paidAmount;
                }

                totalNetOutstandingAmount += invoiceNonPaidOutstandingAmount;
                if (billingServiceCurrency.CheckLimits(invoiceNonPaidOutstandingAmount) == false)
                {
                    return Result<IReadOnlyList<InvoicePaymentDetailInternalModel>>.Failure(
                        localizer[LocalizationKeys.PaymentAmountOutsideBillingServiceLimits, invoiceNonPaidOutstandingAmount, currency.Abbreviation, billingService.Code],
                        AppHttpStatusCodes.BadRequest);
                }
                invoicePaymentDetails.Add(
                    new InvoicePaymentDetailInternalModel(invoice, currency, billingServiceCurrency,
                        billingServiceCurrency.CalculateCharges(invoiceNonPaidOutstandingAmount),
                        invoiceNonPaidOutstandingAmount));
            }

            if (amount != totalNetOutstandingAmount)
            {
                return Result<IReadOnlyList<InvoicePaymentDetailInternalModel>>.Failure(
                    localizer[LocalizationKeys.PaymentAmountMustEqualSumOfInvoicesOutstandingAmount],
                    AppHttpStatusCodes.BadRequest);
            }
        }


        return Result<IReadOnlyList<InvoicePaymentDetailInternalModel>>.Success(invoicePaymentDetails);
    }

    private Result<string> ValidatePaymentForPspSync(PaymentEntity? payment, bool validateStatus = true)
    {
        if (payment is null)
        {
            return Result<string>.Failure(
                localizer[LocalizationKeys.PaymentNotFound],
                AppHttpStatusCodes.NotFound);
        }

        if (validateStatus && payment.Status is not (PaymentStatus.Pending or PaymentStatus.Processing))
        {
            return Result<string>.Failure(
                localizer[LocalizationKeys.PaymentValidationPspSyncNotPollable, payment.Id, payment.Status],
                AppHttpStatusCodes.BadRequest);
        }

        if (string.IsNullOrWhiteSpace(payment.PaymentRef))
        {
            return Result<string>.Failure(
                localizer[LocalizationKeys.PaymentValidationPspSyncMissingRef, payment.Id],
                AppHttpStatusCodes.BadRequest);
        }

        if (payment.PaymentMethod is null)
        {
            return Result<string>.Failure(
                localizer[LocalizationKeys.PaymentValidationPspSyncNoMethod, payment.Id],
                AppHttpStatusCodes.BadRequest);
        }

        var paymentServiceProviderCode = payment.PaymentMethod.PaymentProvider?.Code;
        if (string.IsNullOrWhiteSpace(paymentServiceProviderCode))
        {
            return Result<string>.Failure(
                localizer[LocalizationKeys.PaymentValidationPspSyncNoPsp, payment.Id],
                AppHttpStatusCodes.BadRequest);
        }

        return Result<string>.Success(paymentServiceProviderCode);
    }

    /// <summary>
    /// API accepts <c>+243XXXXXXXXX</c>; AkibaPay expects digits only.
    /// </summary>
    private static string NormalizeFromNumber(string fromNumber) =>
        fromNumber.Trim().TrimStart('+');

    private async Task<Result<(
        CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        bool phoneNumberRequired
        )>> ValidatePaymentTopupModel(PaymentInitializeInputModel model)
    {
        #region Validation and fetching related entities
        logger.LogInformation("START_SUMMARY_2");
        var currencyValidationResult = await ValidateCurrencyByCodeAsync(model.CurrencyCode);
        logger.LogInformation("START_SUMMARY_4");

        if (!currencyValidationResult.IsSuccess)
        {
            return Result<(
                CustomerEntity customer,
                AccountEntity customerWalletAccount,
                ChannelEntity channel,
                PaymentMethodCurrencyEntity paymentMethodCurrency,
                PspConnector pspConnector,
                bool phoneNumberRequired
                )>.Failure(
                currencyValidationResult.Message ?? localizer[LocalizationKeys.CommonKeys.CurrencyValidationError, model.CurrencyCode],
                currencyValidationResult.StatusCode);
        }
        logger.LogInformation("START_SUMMARY_5");
        var currency = currencyValidationResult.Value!;

        var paymentCodesResult = await ResolveTopupPaymentMethodCodesAsync(model);
        logger.LogInformation("START_SUMMARY_6");

        if (!paymentCodesResult.IsSuccess)
        {
            return paymentCodesResult.ToFailure<(
                CustomerEntity customer,
                AccountEntity customerWalletAccount,
                ChannelEntity channel,
                PaymentMethodCurrencyEntity paymentMethodCurrency,
                PspConnector pspConnector,
                bool phoneNumberRequired)>();
        }        
        var (paymentServiceProviderCode, paymentMethodCode, phoneNumberRequired) = paymentCodesResult.Value;
        //if(isFromNumberEmpty)
        //{
        //    return Result<(
        //        CustomerEntity customer,
        //        AccountEntity customerWalletAccount,
        //        ChannelEntity channel,
        //        PaymentMethodCurrencyEntity paymentMethodCurrency,
        //        PspConnector pspConnector)>.Success(
        //        (null, null, null, null, null));
        //}
        if(phoneNumberRequired && string.IsNullOrEmpty(paymentServiceProviderCode))
        {
            return Result<(
                CustomerEntity customer,
                AccountEntity customerWalletAccount,
                ChannelEntity channel,
                PaymentMethodCurrencyEntity paymentMethodCurrency,
                PspConnector pspConnector,
                bool phoneNumberRequired)>.Failure(
                localizer[LocalizationKeys.PaymentPspInitNeedPhoneNumber],
                AppHttpStatusCodes.BadRequest);
        }
        var paymentMethodValidation = await ValidatePaymentMethodAgainstCurrencyAndAmountAsync(
            paymentMethodCode,
            paymentServiceProviderCode,
            currency,
            model.Amount);
        if (!paymentMethodValidation.IsSuccess)
        {
            return paymentMethodValidation.ToFailure<(
                CustomerEntity customer,
                AccountEntity customerWalletAccount,
                ChannelEntity channel,
                PaymentMethodCurrencyEntity paymentMethodCurrency,
                PspConnector pspConnector,
                bool phoneNumberRequired)>();
        }

        var (paymentMethod, paymentMethodCurrency, connector) = paymentMethodValidation.Value;

        var isEasypayCardMethod = IsEasypayCardMethod(paymentServiceProviderCode, paymentMethodCode);
        if (isEasypayCardMethod &&
            model.PaymentType is not null and not PaymentTypeEnum.BANK_CARD)
        {
            return Result<(
                CustomerEntity customer,
                AccountEntity customerWalletAccount,
                ChannelEntity channel,
                PaymentMethodCurrencyEntity paymentMethodCurrency,
                PspConnector pspConnector,
                bool phoneNumberRequired)>.Failure(
                localizer[LocalizationKeys.PaymentMethodIncompatible, paymentMethod.Name],
                AppHttpStatusCodes.BadRequest);
        }

        if (isEasypayCardMethod &&
            model.SendPaymentLinkByEmail &&
            string.IsNullOrWhiteSpace(model.Email))
        {
            return Result<(
                CustomerEntity customer,
                AccountEntity customerWalletAccount,
                ChannelEntity channel,
                PaymentMethodCurrencyEntity paymentMethodCurrency,
                PspConnector pspConnector,
                bool phoneNumberRequired)>.Failure(
                localizer[LocalizationKeys.PaymentEasypayCardEmailRequired],
                AppHttpStatusCodes.BadRequest);
        }

        var customerValidation = await ValidateCustomerAsync();
        if (!customerValidation.IsSuccess)
        {
            return customerValidation.ToFailure<(
                CustomerEntity customer,
                AccountEntity customerWalletAccount,
                ChannelEntity channel,
                PaymentMethodCurrencyEntity paymentMethodCurrency,
                PspConnector pspConnector,
                bool phoneNumberRequired)>();
        }

        var customer = customerValidation.Value;

        var accountValidationResult = await ValidateAccountAsync(customer.PartnerId, currency.Id, (int)AccountTypesEnum.CUSTOMER_LIABILITY);
        if (!accountValidationResult.IsSuccess)
        {
            return accountValidationResult.ToFailure<(CustomerEntity customer,
                AccountEntity customerWalletAccount,
                ChannelEntity channel,
                PaymentMethodCurrencyEntity paymentMethodCurrency,
                PspConnector pspConnector,
                bool phoneNumberRequired
                )>();
        }

        var customerWalletAccount = accountValidationResult.Value;

        var channelResult = await ValidateChannelAsync(WebChannelCode);
        if (!channelResult.IsSuccess)
        {
            return channelResult.ToFailure<(
                CustomerEntity customer,
                AccountEntity customerWalletAccount,
                ChannelEntity channel,
                PaymentMethodCurrencyEntity paymentMethodCurrency,
                PspConnector pspConnector,
                bool phoneNumberRequired    
                )>();
        }

        var channel = channelResult.Value!;

        return Result<(
            CustomerEntity customer,
            AccountEntity customerWallet,
            ChannelEntity channel,
            PaymentMethodCurrencyEntity paymentMethodCurrency,
            PspConnector pspConnector,
            bool phoneNumberRequired
            )>.Success((customer!, customerWalletAccount!, channel!, paymentMethodCurrency!, connector!, phoneNumberRequired));

        #endregion
    }

    private static bool IsEasypayCardMethod(string? providerCode, string? methodCode) =>
        string.Equals(providerCode?.Trim(), EasypayProviderCode, StringComparison.OrdinalIgnoreCase) &&
        string.Equals(methodCode?.Trim(), EasypayCardMethodCode, StringComparison.OrdinalIgnoreCase);

    private async Task<Result<(CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        IReadOnlyList<InvoicePaymentDetailInternalModel> bills)>> ValidatePaymentBillModel(PayBillsInputModel model) {
        var invoiceIdsResult = await ResolvePayBillInvoiceIdsAsync(model);
        if (!invoiceIdsResult.IsSuccess)
        {
            return invoiceIdsResult.ToFailure<(CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        IReadOnlyList<InvoicePaymentDetailInternalModel> bills)>();
        }

        var billValidationResult = await ValidateBillsForInitializationAsync(invoiceIdsResult.Value!, model.Amount);
        if (!billValidationResult.IsSuccess)
        {
            return Result<(CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        IReadOnlyList<InvoicePaymentDetailInternalModel> bills)>.Failure(
                billValidationResult.Message!,
                billValidationResult.StatusCode);
        }
        var bills = billValidationResult.Value!;
        var currency = bills.First().Currency;

        var customerValidation = await ValidateCustomerAsync();
        if (!customerValidation.IsSuccess)
        {
            return customerValidation.ToFailure<(CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        IReadOnlyList<InvoicePaymentDetailInternalModel> bills)>();
        }
        var customer = customerValidation.Value;

        var accountValidationResult = await ValidateAccountAsync(customer.PartnerId, currency.Id, (int)AccountTypesEnum.CUSTOMER_LIABILITY);
        if (!accountValidationResult.IsSuccess)
        {
            return accountValidationResult.ToFailure<(CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        IReadOnlyList<InvoicePaymentDetailInternalModel> bills)>();
        }
        var account = accountValidationResult.Value;


        var channelResult = await ValidateChannelAsync(WebChannelCode);
        if (!channelResult.IsSuccess)
        {
            return channelResult.ToFailure<(CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        IReadOnlyList<InvoicePaymentDetailInternalModel> bills)>();
        }

        var channel = channelResult.Value!;

        var paymentMethodValidation = await ValidatePaymentMethodAgainstCurrencyAndAmountAsync(model.PaymentMethodCode?.Trim().ToUpperInvariant(),
            model.PaymentServiceProviderCode, currency, model.Amount);
        if (!paymentMethodValidation.IsSuccess)
        {
            return paymentMethodValidation.ToFailure<(CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        IReadOnlyList<InvoicePaymentDetailInternalModel> bills)>();
        }

        var (paymentMethod, paymentMethodCurrency, connector) = paymentMethodValidation.Value;

        return Result<(CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        IReadOnlyList<InvoicePaymentDetailInternalModel> bills)>.Success((customer, account, channel, paymentMethodCurrency, connector, bills));
    }

    private async Task<Result<(
        CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        BillingServiceCurrencyEntity billingServiceCurrency)>> ValidatePaymentMeterRechargeModel(InitMeterChargePaymentInputModel model) {
        var currencyValidationResult = await ValidateCurrencyByCodeAsync(MeterChargeCurrencyCode);
        if (!currencyValidationResult.IsSuccess)
        {
            return Result<(
        CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        BillingServiceCurrencyEntity billingServiceCurrency)>.Failure(
                currencyValidationResult.Message ?? localizer[LocalizationKeys.CommonKeys.CurrencyValidationError, MeterChargeCurrencyCode],
                currencyValidationResult.StatusCode);
        }

        var currency = currencyValidationResult.Value!;

        var paymentMethodValidation = await ValidatePaymentMethodAgainstCurrencyAndAmountAsync(model.PaymentMethodCode?.Trim().ToUpperInvariant(),
            model.PaymentProviderCode?.Trim().ToUpperInvariant(), currency, model.Amount);
        if (!paymentMethodValidation.IsSuccess)
        {
            return paymentMethodValidation.ToFailure<(
        CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        BillingServiceCurrencyEntity billingServiceCurrency)>();
        }

        var (paymentMethod, paymentMethodCurrency, pspConnector) = paymentMethodValidation.Value;

        var customerValidation = await ValidateCustomerAsync();
        if (!customerValidation.IsSuccess)
        {
            return customerValidation.ToFailure<(
        CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        BillingServiceCurrencyEntity billingServiceCurrency)>();
        }

        var customer = customerValidation.Value;

        var accountValidationResult = await ValidateAccountAsync(customer.PartnerId, currency.Id, (int)AccountTypesEnum.CUSTOMER_LIABILITY);
        if (!accountValidationResult.IsSuccess)
        {
            return accountValidationResult.ToFailure<(
        CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        BillingServiceCurrencyEntity billingServiceCurrency)>();
        }

        var account = accountValidationResult.Value;

        var channelResult = await ValidateChannelAsync(WebChannelCode);
        if (!channelResult.IsSuccess)
        {
            return channelResult.ToFailure<(
        CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        BillingServiceCurrencyEntity billingServiceCurrency)>();
        }

        var channel = channelResult.Value!;




        var billingServiceValidation = await ValidateBillingServiceAgainstCurrencyAndAmountAsync(model.BillingServiceCode?.Trim().ToUpperInvariant(),
            model.BillerCode?.Trim().ToUpperInvariant(), model.MeterNumber, currency, model.Amount);
        if (!billingServiceValidation.IsSuccess)
        {
            return billingServiceValidation.ToFailure<(
        CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        BillingServiceCurrencyEntity billingServiceCurrency)>();
        }

        var (billingService, billingServiceCurrency, billerConnector) = billingServiceValidation.Value;

        return Result<(
        CustomerEntity customer,
        AccountEntity customerWalletAccount,
        ChannelEntity channel,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        PspConnector pspConnector,
        BillingServiceCurrencyEntity billingServiceCurrency)>.Success((customer,account,channel,paymentMethodCurrency,pspConnector,billingServiceCurrency));
    }
    #endregion

    #region Payment Control Methods
    private async Task<Result<bool>> ReservePaymentControlsForBillsInitiationAsync(
        IReadOnlyList<InvoicePaymentDetailInternalModel> invoicePaymentDetails)
    {
        var existingControls = await unitOfWork.PaymentControlRepository.GetByBillingServiceAndCurrencyAndBillingRefAsync(
                invoicePaymentDetails.First().BillingServiceCurrency.BillingServiceId,
                invoicePaymentDetails.First().BillingServiceCurrency.CurrencyId,
                invoicePaymentDetails.First().Invoice.BillingRef);

        foreach (var detail in invoicePaymentDetails)
        {
            var invoice = detail.Invoice;
            var invoiceRef = invoice.InvoiceRef;

            var existing = existingControls.FirstOrDefault(x => x.BillRef == invoiceRef);
            if (existing is null)
            {
                var available = invoice.AmountTotal - invoice.AmountPaid;
                if (detail.Amount > available)
                {
                    return Result<bool>.Failure(
                        localizer[LocalizationKeys.PaymentAmountExceedsInvoiceAvailableAmount, invoiceRef, available.ToString("0.00")],
                        AppHttpStatusCodes.BadRequest);
                }

                var created = PaymentControlEntity.Create(
                    invoicePaymentDetails.First().BillingServiceCurrency.BillingServiceId,
                    invoicePaymentDetails.First().Invoice.BillingRef,
                    invoiceRef,
                    invoicePaymentDetails.First().BillingServiceCurrency.CurrencyId,
                    invoice.AmountTotal,
                    invoice.AmountPaid,
                    detail.Amount, PaymentControlTypeEnum.UNIQUE);
                await unitOfWork.PaymentControlRepository.AddAsync(created);
            }
            else
            {
                switch (existing.PaymentControlTypeId)
                {
                    case (int)PaymentControlTypeEnum.AGGREGATE:
                        {
                            existing.RefreshAmountsFromInvoice(invoice.AmountTotal, invoice.AmountPaid);
                            // Strict rule: prevent any concurrent payment attempt for the same invoice while a reservation exists,
                            // even if the requested amount would still fit within the available amount.
                            //if (existing.ReservedAmount > 0)
                            //{
                            //    return Result<bool>.Failure(
                            //        localizer[LocalizationKeys.PaymentBillControlConcurrencyConflict],
                            //        AppHttpStatusCodes.Conflict);
                            //}

                            var available = existing.TotalAmount - existing.PaidAmount - existing.ReservedAmount;
                            if (detail.Amount > available)
                            {
                                return Result<bool>.Failure(
                                    localizer[LocalizationKeys.PaymentAmountExceedsInvoiceAvailableAmount, invoiceRef, available.ToString("0.00")],
                                    AppHttpStatusCodes.BadRequest);
                            }
                            existing.ReserveAmount(detail.Amount);
                        }
                        break;
                    // In aggregate type, we expect one control per billing ref, so we refresh the amounts based on the current invoice status.
                    case (int)PaymentControlTypeEnum.UNIQUE:
                    default:
                        return Result<bool>.Failure(
                        localizer[LocalizationKeys.PaymentInvoiceOngoing, invoiceRef],
                        AppHttpStatusCodes.BadRequest);
                }
            }
        }

        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> ReservePaymentControlsForMeterInitiationAsync(
        long billingServiceId, int currencyId, string meterNo, decimal amount)
    {
        var existingControls = await unitOfWork.PaymentControlRepository.GetByBillingServiceAndCurrencyAndBillingRefAsync(
                billingServiceId,
                currencyId,
                meterNo);

        var existing = existingControls.FirstOrDefault();
        if (existing is null)
        {
            var created = PaymentControlEntity.Create(
                billingServiceId,
                meterNo,
                string.Empty,
                currencyId,
                amount,
                0,
                amount,
                PaymentControlTypeEnum.UNIQUE
                );
            await unitOfWork.PaymentControlRepository.AddAsync(created);
        }
        else
        {
            switch (existing.PaymentControlTypeId)
            {
                case (int)PaymentControlTypeEnum.AGGREGATE:
                    {
                        existing.RefreshAmountsFromInvoice(amount, 0m);
                        // Strict rule: prevent any concurrent payment attempt for the same invoice while a reservation exists,
                        // even if the requested amount would still fit within the available amount.
                        //if (existing.ReservedAmount > 0)
                        //{
                        //    return Result<bool>.Failure(
                        //        localizer[LocalizationKeys.PaymentBillControlConcurrencyConflict],
                        //        AppHttpStatusCodes.Conflict);
                        //}

                        var available = existing.TotalAmount - existing.PaidAmount - existing.ReservedAmount;
                        if (amount > available)
                        {
                            return Result<bool>.Failure(
                                localizer[LocalizationKeys.PaymentMeterReserveExceedsAvailable, amount, meterNo],
                                AppHttpStatusCodes.BadRequest);
                        }
                        existing.ReserveAmount(amount);
                    }
                    break;
                // In aggregate type, we expect one control per billing ref, so we refresh the amounts based on the current invoice status.
                case (int)PaymentControlTypeEnum.UNIQUE:
                default:
                    return Result<bool>.Failure(
                    localizer[LocalizationKeys.PaymentMeterOngoing, meterNo],
                    AppHttpStatusCodes.BadRequest);
            }
        }

        return Result<bool>.Success(true);
    }

    private async Task HandlePaymentControlsAsync(Guid paymentId)
    {
        var payment = await unitOfWork.PaymentRepository.GetByIdAsync(paymentId);
        var items = await unitOfWork.PaymentOrderItemRepository.GetByPaymentIdForApplyAsync(paymentId);
        items = items.Where(i => i.ItemType == OrderItemType.PayBill || i.ItemType == OrderItemType.MeterCharge).ToList();

        if (items is null || items.Count == 0)
            return;

        var existingControls = await unitOfWork.PaymentControlRepository.GetByBillingServiceAndCurrencyAndBillingRefAsync(
                items.First().BillingServiceId!.Value,
                items.First().CurrencyId!.Value,
                items.First().BillingRefValue!);

        foreach (var item in items)
        {
            switch (item.ItemType)
            {
                case OrderItemType.PayBill:
                    {
                        var invoice = item.Invoice;

                        var control = existingControls.FirstOrDefault(x => x.BillRef == invoice!.InvoiceRef);
                        if (control is null)
                        {
                            logger.LogWarning($"No payment control found for invoice reference '{invoice.InvoiceRef}' while releasing payment controls for payment '{paymentId}'. This may indicate a data inconsistency issue that should be investigated.");
                            //throw new InvalidOperationException($"Payment control not found for invoice reference '{invoice.InvoiceRef}'");
                            continue;
                        }
                        switch (control.PaymentControlTypeId)
                        {
                            case (int)PaymentControlTypeEnum.AGGREGATE:
                                {
                                    if (payment!.Status == PaymentStatus.Completed)
                                    {
                                        control?.ReleaseAmount(item.Amount);
                                    }
                                    else
                                    {
                                        control?.CancelReservedAmount(item.Amount);
                                    }
                                    unitOfWork.PaymentControlRepository.Update(control!);
                                }
                                break;
                            case (int)PaymentControlTypeEnum.UNIQUE:
                                unitOfWork.PaymentControlRepository.Remove(control!);
                                break;
                        }
                    }
                    break;
                case OrderItemType.MeterCharge:
                    {
                        var control = existingControls.FirstOrDefault(x => x.BillingRef == item.BillingRefValue);
                        if (control is null)
                        {
                            logger.LogWarning($"No payment control found for billing reference '{item.BillingRefValue}' while releasing payment controls for payment '{paymentId}'. This may indicate a data inconsistency issue that should be investigated.");
                            continue;
                            //throw new InvalidOperationException($"Payment control not found for billing reference '{item.BillingRefValue}'");
                        }
                        switch (control.PaymentControlTypeId)
                        {
                            case (int)PaymentControlTypeEnum.AGGREGATE:
                                {
                                    if (payment.Status == PaymentStatus.Completed)
                                    {
                                        control?.ReleaseAmount(item.Amount);
                                    }
                                    else
                                    {
                                        control?.CancelReservedAmount(item.Amount);
                                    }
                                    unitOfWork.PaymentControlRepository.Update(control!);
                                }
                                break;
                            case (int)PaymentControlTypeEnum.UNIQUE:
                                unitOfWork.PaymentControlRepository.Remove(control!);
                                break;
                        }
                    }
                    break;
                case OrderItemType.Topup:
                default:
                    continue; // only bill related items have payment controls to release,
                    break;
            }
        }
    }

    #endregion

    #region Accounting Methods

    private async Task<Result<(List<JournalEntity> journals, List<AccountEntity> accounts, List<PlatformInvoiceEntity> platformInvoices)>> CreateJournalsForPayment(PaymentEntity payment)
    {
        //update payment and POI status


        //generate journals (financial entries)
        var accounts = new List<AccountEntity>();
        var journals = new List<JournalEntity>();
        var platformInvoices = new List<PlatformInvoiceEntity>();
        var pois = payment.PaymentOrder?.Items;
        var paymentMethodCurrency = payment.PaymentMethod.PaymentMethodCurrencies.FirstOrDefault(x => x.CurrencyId == payment.CurrencyId)
            ?? throw new InvalidOperationException(localizer[LocalizationKeys.PaymentPrintCurrencyNotOnMethod, payment.CurrencyId, payment.PaymentMethod.Name].Value);
        foreach (var poi in pois)
        {
            var bundle = (await accountingService.CreateJournalsForPOI(payment.PaymentOrder.Customer.PartnerId, payment.PaymentMethod.PaymentProvider.PartnerId, poi.BillingService?.Biller?.PartnerId, poi, accounts, paymentMethodCurrency.AutoSettleJournal)).Value;
            journals.AddRange(bundle.journals);
            platformInvoices.AddRange(bundle.platformInvoices);
        }

        return Result<(List<JournalEntity>, List<AccountEntity>, List<PlatformInvoiceEntity>)>.Success((journals, accounts, platformInvoices));
        
    }

    /// <summary>Track platform invoices and journals together so EF persists them on the same SaveChanges inside the ambient DB transaction.</summary>
    private async Task TrackPlatformInvoicesAndJournalsForPaymentAsync(
        IReadOnlyList<PlatformInvoiceEntity> platformInvoices,
        IReadOnlyList<JournalEntity> journals,
        CancellationToken cancellationToken = default)
    {
        if (platformInvoices.Count > 0)
        {
            await unitOfWork.PlatformInvoiceRepository.AddRangeAsync(platformInvoices, cancellationToken);
        }

        await unitOfWork.JournalRepository.AddRangeAsync(journals, cancellationToken);
    }

    #endregion

    #region Print Model Building

    private async Task<Result<PaymentPrintModel>> BuildPaymentPrintModelAsync(
        Guid paymentId,
        string? acceptLanguage,
        CancellationToken cancellationToken)
    {
        var payment = await unitOfWork.PaymentRepository.GetByIdWithPrintContextAsync(paymentId);
        if (payment is null)
        {
            return Result<PaymentPrintModel>.Failure(
                localizer[LocalizationKeys.PaymentNotFound],
                AppHttpStatusCodes.NotFound);
        }

        if (payment.PaymentOrder is null)
        {
            return Result<PaymentPrintModel>.Failure(
                localizer[LocalizationKeys.PaymentNotFound],
                AppHttpStatusCodes.NotFound);
        }

        var currency =
            payment.Currency?.Abbreviation
            ?? payment.PaymentOrder.Currency?.Abbreviation
            ?? payment.PaymentOrder.Currency?.Code
            ?? string.Empty;

        var normalizedLanguage = string.IsNullOrWhiteSpace(acceptLanguage)
            ? "en"
            : acceptLanguage.Trim().ToLowerInvariant();

        var items = (payment.PaymentOrder.Items ?? Array.Empty<PaymentOrderItemEntity>())
            .OrderBy(i => i.CreatedAtUtc)
            .Select(i => BuildPrintItemModel(i, normalizedLanguage, payment.Direction))
            .ToArray();

        var (externalReferencePrintLines, externalReferenceIsJsonObject) =
            BuildExternalReferenceForPrint(payment.ExternalRef);

        var createdAtLocal = payment.CreatedAtUtc.UtcDateTime;
        var printOptions = paymentPrintOptions.Value;
        var operatorName = ResolveReceiptOperatorName(payment);
        var orderItems = payment.PaymentOrder.Items ?? Array.Empty<PaymentOrderItemEntity>();
        var tickets = orderItems
            .Where(i => i.ItemType is OrderItemType.PayBill or OrderItemType.MeterCharge)
            .OrderBy(i => i.CreatedAtUtc)
            .Select(i => BuildSnelTicket(i, payment, currency, operatorName, printOptions))
            .ToArray();

        if (tickets.Length == 0)
        {
            tickets =
            [
                BuildFallbackSnelTicket(payment, currency, operatorName, printOptions)
            ];
        }

        var model = new PaymentPrintModel
        {
            PaymentId = payment.Id,
            PaymentRef = payment.PaymentRef ?? payment.Id.ToString(),
            ReceiptDate = createdAtLocal.ToString("dd/MM/yy", CultureInfo.InvariantCulture),
            ReceiptTime = createdAtLocal.ToString("HH:mm", CultureInfo.InvariantCulture),
            Tickets = tickets,
            ShowWalletHeader = tickets.Any(t => !t.IsPrePaid),
            AcceptLanguage = acceptLanguage,
            ExternalReference = payment.ExternalRef,
            ExternalReferencePrintLines = externalReferencePrintLines,
            ExternalReferenceIsJsonObject = externalReferenceIsJsonObject,
            Tax = payment.Tax,
            PspReference = payment.ExternalRef,
            ShowTaxAndFees = paymentPrintOptions.Value.ShowTaxAndFees,
            CreatedAtUtc = payment.CreatedAtUtc.UtcDateTime,
            Status = payment.Status.ToString(),
            Amount = payment.Amount,
            Currency = currency,
            PaymentMethodCode = payment.PaymentMethod?.Code,
            PaymentProviderCode = payment.PaymentMethod?.PaymentProvider?.Code,
            PaymentProviderName = payment.PaymentMethod?.PaymentProvider?.Name,
            Items = items
        };

        return Result<PaymentPrintModel>.Success(model);
    }

    private static string ResolvePaymentPrintTemplate(PaymentPrintModel model)
    {
        if (model.Tickets.Any(t => t.IsPrePaid))
            return "Payment/payment-prepaid";

        if (model.Tickets.Any(t => t.IsPostPaid))
            return "Payment/payment-postpaid";

        return "Payment/payment-topup";
    }

    private static PaymentPrintItemModel BuildPrintItemModel(
        PaymentOrderItemEntity item,
        string languageCode,
        PaymentDirectionEnum? direction)
    {
        var invoiceBillingService = item.Invoice?.BillingService;
        var billingService = invoiceBillingService ?? item.BillingService;

        string? billingServiceCode = billingService?.Code;
        string? billingServiceName = billingService is null
            ? null
            : billingService.Translations?
                .FirstOrDefault(t => t.LanguageCode == languageCode)?.Name
              ?? billingService.Name;

        var (fees, tax, total) = ResolvePaymentPrintItemFeesTaxTotal(item, direction);

        switch (item.ItemType)
        {
            case OrderItemType.PayBill:
                return new PaymentPrintItemModel
                {
                    ItemType = item.ItemType.ToString(),
                    Amount = item.Amount,
                    Fees = fees,
                    Tax = tax,
                    Total = total,
                    ItemDateUtc = (item.Invoice?.IssueDate ?? item.CreatedAtUtc).UtcDateTime,
                    ItemRef = item.Invoice?.InvoiceRef,
                    BillingServiceCode = billingServiceCode,
                    BillingServiceName = billingServiceName,
                    BillingRef = item.BillingRefValue ?? item.Invoice?.BillingRef
                };

            case OrderItemType.MeterCharge:
                return new PaymentPrintItemModel
                {
                    ItemType = item.ItemType.ToString(),
                    Amount = item.Amount,
                    Fees = fees,
                    Tax = tax,
                    Total = total,
                    ItemDateUtc = item.CreatedAtUtc.UtcDateTime,
                    ItemRef = item.BillingRefValue,
                    BillingServiceCode = billingServiceCode,
                    BillingServiceName = billingServiceName,
                    BillingRef = item.BillingRefValue
                };

            case OrderItemType.Topup:
            default:
                return new PaymentPrintItemModel
                {
                    ItemType = item.ItemType.ToString(),
                    Amount = item.Amount,
                    Fees = fees,
                    Tax = tax,
                    Total = total,
                    ItemDateUtc = item.CreatedAtUtc.UtcDateTime,
                    ItemRef = null,
                    BillingServiceCode = null,
                    BillingServiceName = null,
                    BillingRef = null
                };
        }
    }

    private static (decimal Fees, decimal Tax, decimal Total) ResolvePaymentPrintItemFeesTaxTotal(
        PaymentOrderItemEntity item,
        PaymentDirectionEnum? direction)
    {
        if (direction == PaymentDirectionEnum.OUT)
        {
            return (item.TotalCost, item.TotalCostTax, item.TotalAmount);
        }

        return (item.TotalFees, item.TotalFeesTax, item.TotalAmount);
    }

    private static readonly CultureInfo ReceiptAmountCulture = CultureInfo.GetCultureInfo("fr-FR");

    private static string FormatReceiptAmount(decimal? value) =>
        value.HasValue ? value.Value.ToString("N2", ReceiptAmountCulture) : "-";

    private static string? FormatMeterNumber(decimal? meterNumber)
    {
        if (!meterNumber.HasValue)
            return null;

        return meterNumber.Value % 1 == 0
            ? decimal.Truncate(meterNumber.Value).ToString("0", CultureInfo.InvariantCulture)
            : meterNumber.Value.ToString(CultureInfo.InvariantCulture);
    }

    private static string ResolveReceiptOperatorName(PaymentEntity payment)
    {
        if (!string.IsNullOrWhiteSpace(payment.CreatedBy))
            return payment.CreatedBy.Trim();

        var partner = payment.Customer?.Partner ?? payment.PaymentOrder?.Customer?.Partner;
        if (!string.IsNullOrWhiteSpace(partner?.DisplayName))
            return partner.DisplayName.Trim();

        if (!string.IsNullOrWhiteSpace(partner?.LegalName))
            return partner.LegalName.Trim();

        return "-";
    }

    private static string ResolveReceiptNumber(
        PaymentOrderItemEntity? item,
        PaymentEntity payment,
        PaymentPrintOptions options)
    {
        if (item is not null)
        {
            if (TryReadItemExternalRef(item.ExternalRef, out var externalRef) &&
                !string.IsNullOrWhiteSpace(externalRef))
            {
                return externalRef.Trim();
            }

            if (TryReadItemReceiptNo(item.ExternalRef, out var receiptNo) &&
                !string.IsNullOrWhiteSpace(receiptNo))
            {
                return receiptNo.Trim();
            }

            if (ConnectAfricaMeterChargeReceiptData.TryResolve(item.WebResponse, out var webReceipt) &&
                !string.IsNullOrWhiteSpace(webReceipt?.ReceiptNumber))
            {
                return webReceipt.ReceiptNumber.Trim();
            }
        }

        var suffix = payment.PaymentRef?.Trim();
        if (!string.IsNullOrWhiteSpace(suffix) && suffix.Contains('/'))
            return suffix;

        var sequential = !string.IsNullOrWhiteSpace(suffix)
            ? new string(suffix.Where(char.IsDigit).TakeLast(7).ToArray())
            : payment.Id.ToString("N")[..7];

        if (string.IsNullOrWhiteSpace(sequential))
            sequential = payment.Id.ToString("N")[..7];

        return $"{options.VirtualCashRegisterPrefix}{options.VirtualCashRegisterCode}/{sequential}";
    }

    private static bool TryReadItemExternalRef(string? externalRef, out string? value)
    {
        value = null;
        if (!TryParseItemExternalJson(externalRef, out var root))
            return false;

        if (root.TryGetProperty("ExternalRef", out var externalRefElement) &&
            externalRefElement.ValueKind == JsonValueKind.String)
        {
            value = externalRefElement.GetString();
            return true;
        }

        return false;
    }

    private static bool TryReadItemReceiptNo(string? externalRef, out string? value)
    {
        value = null;
        if (!TryParseItemExternalJson(externalRef, out var root))
            return false;

        if (root.TryGetProperty("ReceiptNo", out var receiptElement) &&
            receiptElement.ValueKind == JsonValueKind.String)
        {
            value = receiptElement.GetString();
            return true;
        }

        if (root.TryGetProperty("receiptNumber", out var receiptNumberElement) &&
            receiptNumberElement.ValueKind == JsonValueKind.String)
        {
            value = receiptNumberElement.GetString();
            return true;
        }

        return false;
    }

    private static bool TryParseItemExternalJson(string? externalRef, out JsonElement root)
    {
        root = default;
        if (string.IsNullOrWhiteSpace(externalRef))
            return false;

        var trimmed = externalRef.Trim();
        if (trimmed.Length < 2 || trimmed[0] != '{' || trimmed[^1] != '}')
            return false;

        try
        {
            using var doc = JsonDocument.Parse(trimmed);
            root = doc.RootElement.Clone();
            return root.ValueKind == JsonValueKind.Object;
        }
        catch
        {
            return false;
        }
    }

    private static IReadOnlyList<string> TryReadAllTokens(string? externalRef)
    {
        if (ConnectAfricaMeterChargeReceiptData.TryResolve(externalRef, out var receipt))
        {
            var parsedTokens = NormalizeTokenValues(receipt?.Tokens);
            if (parsedTokens.Count > 0)
                return parsedTokens;
        }

        if (!TryParseItemExternalJson(externalRef, out var root))
            return [];

        if (root.TryGetProperty("tokens", out var tokensLowerElement) &&
            tokensLowerElement.ValueKind == JsonValueKind.Array)
        {
            var tokens = ReadTokenValuesFromJsonArray(tokensLowerElement);
            if (tokens.Count > 0)
                return tokens;
        }

        if (root.TryGetProperty("Tokens", out var tokensElement) &&
            tokensElement.ValueKind == JsonValueKind.Array)
        {
            return ReadTokenValuesFromJsonArray(tokensElement);
        }

        return [];
    }

    private static IReadOnlyList<string> ReadTokenValuesFromJsonArray(JsonElement tokensElement)
    {
        var tokens = new List<string>();
        foreach (var tokenElement in tokensElement.EnumerateArray())
        {
            if (tokenElement.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(tokenElement.GetString()))
            {
                tokens.Add(tokenElement.GetString()!.Trim());
            }
        }

        return tokens;
    }

    private static string? ResolvePaNumber(InvoiceInfoModel? info, PaymentOrderItemEntity item)
    {
        if (!string.IsNullOrWhiteSpace(info?.Pa))
            return info.Pa.Trim();

        if (!string.IsNullOrWhiteSpace(info?.CustomerNum))
            return info.CustomerNum.Trim();

        var billingRef = item.BillingRefValue ?? item.Invoice?.BillingRef;
        return string.IsNullOrWhiteSpace(billingRef) ? null : billingRef.Trim();
    }

    private static string BuildPointOfConnection(InvoiceInfoModel? info, PaymentOrderItemEntity item)
    {
        if (info is null)
            return item.BillingRefValue ?? item.Invoice?.BillingRef ?? "-";

        var parts = new List<string>();
        var poc = info.Pa ?? info.CustomerNum;
        if (!string.IsNullOrWhiteSpace(poc))
            parts.Add(poc.Trim());

        if (!string.IsNullOrWhiteSpace(info.ClientCommune))
            parts.Add(info.ClientCommune.Trim());

        if (!string.IsNullOrWhiteSpace(info.ClientPropertyNumber))
            parts.Add(info.ClientPropertyNumber.Trim());

        if (parts.Count == 0 && !string.IsNullOrWhiteSpace(info.ClientStreet))
            parts.Add(info.ClientStreet.Trim());

        return parts.Count > 0 ? string.Concat(parts) : "-";
    }

    private static decimal? ResolveAmountExcludingTax(
        InvoiceInfoModel? info,
        string currencyCode,
        ConnectAfricaMeterChargeReceiptData? connectAfricaReceipt = null)
    {
        if (connectAfricaReceipt?.TariffAmounts.Count > 0)
        {
            decimal total = 0m;
            var hasValue = false;
            foreach (var amount in connectAfricaReceipt.TariffAmounts)
            {
                if (TryParseConnectAfricaDecimal(amount, out var parsed))
                {
                    total += parsed;
                    hasValue = true;
                }
            }

            if (hasValue)
                return total;
        }

        if (info is null)
            return null;

        return string.Equals(currencyCode, "CDF", StringComparison.OrdinalIgnoreCase)
            ? info.AmountExcludingTaxFc
            : info.AmountExcludingTaxUsd ?? info.AmountExcludingTaxFc;
    }

    private static decimal? ResolveTotalValueTtc(
        PaymentOrderItemEntity item,
        string currencyCode,
        ConnectAfricaMeterChargeReceiptData? connectAfricaReceipt = null)
    {
        if (TryParseConnectAfricaDecimal(connectAfricaReceipt?.PaidAmount, out var paidAmount))
            return paidAmount;

        var invoice = item.Invoice;
        if (invoice is not null)
        {
            var invoiceCurrency = invoice.Currency?.Abbreviation ?? invoice.Currency?.Code;
            if (!string.IsNullOrWhiteSpace(invoiceCurrency)
                && string.Equals(invoiceCurrency, currencyCode, StringComparison.OrdinalIgnoreCase))
            {
                return invoice.AmountTotal;
            }
        }

        return item.Amount;
    }

    private static string? FormatTariffRatePerKwh(decimal? amountExcludingTax, decimal? totalUnitsKwh)
    {
        if (!amountExcludingTax.HasValue || !totalUnitsKwh.HasValue || totalUnitsKwh.Value <= 0)
            return null;

        var rate = amountExcludingTax.Value / totalUnitsKwh.Value;
        return rate.ToString("N2", ReceiptAmountCulture);
    }

    private static PaymentSnelTicketModel BuildFallbackSnelTicket(
        PaymentEntity payment,
        string currency,
        string operatorName,
        PaymentPrintOptions options)
    {
        return new PaymentSnelTicketModel
        {
            IsPostPaid = false,
            IsPrePaid = false,
            ReceiptNumber = ResolveReceiptNumber(null, payment, options),
            CustomerName = operatorName,
            PointOfConnection = "-",
            ServiceTitle = "Paiement",
            TransactionNumber = payment.PaymentRef,
            AmountCollected = FormatReceiptAmount(payment.Amount),
            Currency = currency,
            TotalValueTtc = FormatReceiptAmount(payment.Amount),
            Operator = PaymentPrintOptions.DefaultOperatorDisplayName
        };
    }

    private PaymentSnelTicketModel BuildSnelTicket(
        PaymentOrderItemEntity item,
        PaymentEntity payment,
        string currency,
        string operatorName,
        PaymentPrintOptions options)
    {
        var info = item.Invoice is null ? null : InvoiceInfoModel.Parse(item.Invoice.InvoiceInfo);
        var isPostPaid = item.ItemType == OrderItemType.PayBill;
        var isPrePaid = item.ItemType == OrderItemType.MeterCharge;
        ConnectAfricaMeterChargeReceiptData.TryResolve(
            isPrePaid ? ResolveMeterChargeWebResponse(item) : null,
            out var connectAfricaReceipt);
        var prepaidTokens = ResolvePrepaidTokens(item, connectAfricaReceipt, isPrePaid);
        var amountExcludingTax = ResolveAmountExcludingTax(info, currency, connectAfricaReceipt);
        var totalValueTtc = ResolveTotalValueTtc(item, currency, connectAfricaReceipt);
        var totalUnits = isPrePaid
            ? ResolvePrepaidTotalUnits(info, connectAfricaReceipt)
            : null;
        var tariffCategory = ResolveTariffCategory(info, connectAfricaReceipt);
        var tariffLines = BuildTariffLines(connectAfricaReceipt, info, amountExcludingTax, totalUnits, tariffCategory);
        var meterNumber = !string.IsNullOrWhiteSpace(connectAfricaReceipt?.MeterNumber)
            ? connectAfricaReceipt.MeterNumber.Trim()
            : FormatMeterNumber(info?.MeterNumber)
                ?? (isPrePaid ? item.BillingRefValue : null)
                ?? "-";

        if (isPostPaid && string.Equals(meterNumber, "-", StringComparison.Ordinal) &&
            info?.MeterNumber is null)
        {
            meterNumber = "-";
        }

        var customerName = connectAfricaReceipt?.CustomerName ?? info?.CustomerName;
        if (string.IsNullOrWhiteSpace(customerName))
            customerName = operatorName;

        var hasTechnicalCodes = isPrePaid && (
            !string.IsNullOrWhiteSpace(connectAfricaReceipt?.SupplyGroupCode) ||
            !string.IsNullOrWhiteSpace(connectAfricaReceipt?.TariffIndex) ||
            !string.IsNullOrWhiteSpace(connectAfricaReceipt?.Krn));

        var vatLine = connectAfricaReceipt?.ResolveVatLine();
        var publicLightingLine = connectAfricaReceipt?.ResolvePublicLightingLine();
        var levyLines = BuildLevyLines(connectAfricaReceipt);

        return new PaymentSnelTicketModel
        {
            IsPostPaid = isPostPaid,
            IsPrePaid = isPrePaid,
            ShowPrepaidTokenSection = isPrePaid && prepaidTokens.Count > 0,
            ShowPrepaidTechnicalCodes = hasTechnicalCodes,
            ShowTotalUnits = isPrePaid && !string.IsNullOrWhiteSpace(connectAfricaReceipt?.QuantityUnitsKwh),
            ReceiptNumber = connectAfricaReceipt?.ReceiptNumber ?? ResolveReceiptNumber(item, payment, options),
            ReceiptDate = connectAfricaReceipt?.Date,
            ReceiptTime = connectAfricaReceipt?.Time,
            TaxNumber = connectAfricaReceipt?.TaxNumber,
            Duplicate = FormatConnectAfricaDuplicate(connectAfricaReceipt?.Duplicate),
            MeterNumber = FormatPrepaidMeterDisplay(meterNumber),
            CustomerName = customerName,
            PaNumber = connectAfricaReceipt?.PointOfConnection ?? ResolvePaNumber(info, item),
            PointOfConnection = connectAfricaReceipt?.PointOfConnection ?? BuildPointOfConnection(info, item),
            ServiceTitle = isPrePaid ? "Crédit d'électricité" : "Paiement facture",
            TransactionNumber = connectAfricaReceipt?.TransactionNumber ?? payment.PaymentRef,
            PrepaidTokens = prepaidTokens,
            Sgc = connectAfricaReceipt?.SupplyGroupCode,
            TaxIndex = connectAfricaReceipt?.TariffIndex,
            Krn = connectAfricaReceipt?.Krn,
            AmountCollected = FormatConnectAfricaAmount(connectAfricaReceipt?.TransactionAmount)
                ?? FormatReceiptAmount(item.Amount),
            Currency = currency,
            TotalUnitsKwh = NormalizeKwhDisplay(connectAfricaReceipt?.QuantityUnitsKwh),
            TotalValueTtc = FormatConnectAfricaAmount(connectAfricaReceipt?.PaidAmount)
                ?? FormatReceiptAmount(totalValueTtc),
            TariffLines = tariffLines,
            TariffCategory = tariffCategory,
            TariffRatePerKwh = ResolvePricePerKwhDisplay(connectAfricaReceipt, amountExcludingTax, totalUnits),
            AmountExcludingTax = FormatConnectAfricaTariffAmounts(connectAfricaReceipt)
                ?? FormatReceiptAmount(amountExcludingTax),
            Vat = FormatConnectAfricaTaxLineDisplay(vatLine)
                ?? FormatReceiptAmount(info?.Vat),
            VatAmount = FormatConnectAfricaAmount(vatLine?.Amount)
                ?? FormatReceiptAmount(info?.Vat),
            VatRate = NormalizeTaxRateDisplay(vatLine?.Percentage),
            Tep = FormatConnectAfricaTaxLineDisplay(connectAfricaReceipt?.ResolveTepLine()),
            ProvincialTax = FormatConnectAfricaTaxLineDisplay(publicLightingLine)
                ?? FormatReceiptAmount(info?.ProvincialTax),
            ProvincialTaxAmount = FormatConnectAfricaAmount(publicLightingLine?.Amount)
                ?? FormatReceiptAmount(info?.ProvincialTax),
            ProvincialTaxRate = NormalizeTaxRateDisplay(publicLightingLine?.Percentage),
            LevyLines = levyLines,
            PreTaxDebts1 = ResolvePreTaxDebts1(connectAfricaReceipt),
            Cashpower = connectAfricaReceipt?.Cashpower,
            Arrears = FormatConnectAfricaAmount(connectAfricaReceipt?.OutstandingArrears),
            AdvanceOnWorks = FormatConnectAfricaAmount(connectAfricaReceipt?.AdvanceOnWorks)
                ?? FormatConnectAfricaFixedCharges(connectAfricaReceipt)
                ?? FormatReceiptAmount(0m),
            Debts1 = FormatConnectAfricaAmount(connectAfricaReceipt?.Debts1)
                ?? FormatReceiptAmount(0m),
            TotalDeductions = FormatConnectAfricaAmount(connectAfricaReceipt?.ArrearsTotal)
                ?? FormatConnectAfricaTotalDeductions(connectAfricaReceipt)
                ?? FormatReceiptAmount(0m),
            Operator = connectAfricaReceipt?.Operator ?? PaymentPrintOptions.DefaultOperatorDisplayName,
            InvoiceRef = item.Invoice?.InvoiceRef
        };
    }

    private static IReadOnlyList<PrepaidTokenPrintModel> ResolvePrepaidTokens(
        PaymentOrderItemEntity item,
        ConnectAfricaMeterChargeReceiptData? connectAfricaReceipt,
        bool isPrePaid)
    {
        if (!isPrePaid)
            return [];

        var tokenValues = ReadPrepaidTokenValues(connectAfricaReceipt, item);
        if (tokenValues.Count == 0)
            return [];

        var tokenNumbers = connectAfricaReceipt?.TokenNumbers ?? [];
        var prepaidTokens = new List<PrepaidTokenPrintModel>(tokenValues.Count);
        for (var i = 0; i < tokenValues.Count; i++)
        {
            prepaidTokens.Add(new PrepaidTokenPrintModel
            {
                Index = ResolvePrepaidTokenIndex(tokenNumbers, i),
                Value = tokenValues[i],
                DisplayLines = FormatPrepaidTokenDisplayLines(tokenValues[i])
            });
        }

        return prepaidTokens;
    }

    private static IReadOnlyList<string> ReadPrepaidTokenValues(
        ConnectAfricaMeterChargeReceiptData? connectAfricaReceipt,
        PaymentOrderItemEntity item)
    {
        var fromReceipt = NormalizeTokenValues(connectAfricaReceipt?.Tokens);
        if (fromReceipt.Count > 0)
            return fromReceipt;

        var fromWebResponse = TryReadAllTokens(item.WebResponse);
        if (fromWebResponse.Count > 0)
            return fromWebResponse;

        return TryReadAllTokens(item.ExternalRef);
    }

    private static IReadOnlyList<string> NormalizeTokenValues(IReadOnlyList<string>? tokens)
    {
        if (tokens is null || tokens.Count == 0)
            return [];

        return tokens
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .Select(token => token.Trim())
            .ToArray();
    }

    private static int ResolvePrepaidTokenIndex(IReadOnlyList<string> tokenNumbers, int tokenPosition)
    {
        if (tokenPosition < tokenNumbers.Count &&
            int.TryParse(tokenNumbers[tokenPosition], out var parsedIndex))
        {
            return parsedIndex;
        }

        return tokenPosition + 1;
    }

    private static string? ResolveTariffCategory(
        InvoiceInfoModel? info,
        ConnectAfricaMeterChargeReceiptData? receipt)
    {
        if (receipt?.TariffNames.Count > 0)
            return string.Join(" / ", receipt.TariffNames);

        if (!string.IsNullOrWhiteSpace(receipt?.TariffName))
            return receipt.TariffName.Trim();

        return string.IsNullOrWhiteSpace(info?.CustomerTypeName)
            ? null
            : info.CustomerTypeName.Trim();
    }

    private static decimal? ResolvePrepaidTotalUnits(
        InvoiceInfoModel? info,
        ConnectAfricaMeterChargeReceiptData? receipt)
    {
        if (!string.IsNullOrWhiteSpace(receipt?.QuantityUnitsKwh) &&
            TryParseConnectAfricaDecimal(receipt.QuantityUnitsKwh, out var parsedUnits))
        {
            return parsedUnits;
        }

        return info?.BilledConsumption ?? info?.Consumption;
    }

    private static string? ResolvePricePerKwhDisplay(
        ConnectAfricaMeterChargeReceiptData? receipt,
        decimal? amountExcludingTax,
        decimal? totalUnitsKwh)
    {
        if (receipt?.PricePerKwh.Count > 0)
            return string.Join(" / ", receipt.PricePerKwh);

        return FormatTariffRatePerKwh(amountExcludingTax, totalUnitsKwh);
    }

    private static IReadOnlyList<PaymentTariffLinePrintModel> BuildTariffLines(
        ConnectAfricaMeterChargeReceiptData? receipt,
        InvoiceInfoModel? info,
        decimal? amountExcludingTax,
        decimal? totalUnitsKwh,
        string? tariffCategory)
    {
        if (receipt?.TariffAmounts.Count > 0)
        {
            var lines = new List<PaymentTariffLinePrintModel>(receipt.TariffAmounts.Count);
            for (var i = 0; i < receipt.TariffAmounts.Count; i++)
            {
                var amount = receipt.TariffAmounts[i];
                if (string.IsNullOrWhiteSpace(amount))
                    continue;

                var category = i < receipt.TariffNames.Count
                    ? receipt.TariffNames[i]
                    : null;
                var rate = i < receipt.PricePerKwh.Count
                    ? receipt.PricePerKwh[i]
                    : null;

                lines.Add(new PaymentTariffLinePrintModel
                {
                    AmountExcludingTax = amount.Trim(),
                    RatePerKwh = FormatConnectAfricaTariffRateDisplay(rate),
                    Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim()
                });
            }

            return lines;
        }

        var formattedAmount = FormatReceiptAmount(amountExcludingTax);
        if (formattedAmount is null)
            return [];

        var computedRate = FormatTariffRatePerKwh(amountExcludingTax, totalUnitsKwh);
        return
        [
            new PaymentTariffLinePrintModel
            {
                AmountExcludingTax = formattedAmount,
                RatePerKwh = computedRate is null ? null : $"@ {computedRate}/kWh",
                Category = tariffCategory
            }
        ];
    }

    private static string? FormatConnectAfricaTariffRateDisplay(string? rawRate)
    {
        if (string.IsNullOrWhiteSpace(rawRate))
            return null;

        var trimmed = rawRate.Trim();
        if (trimmed.StartsWith('@') || trimmed.Contains("/kWh", StringComparison.OrdinalIgnoreCase))
            return trimmed;

        return $"@ {trimmed}/kWh";
    }

    private static string? FormatConnectAfricaTariffAmounts(ConnectAfricaMeterChargeReceiptData? receipt)
    {
        if (receipt is null || receipt.TariffAmounts.Count == 0)
            return null;

        return string.Join(" / ", receipt.TariffAmounts);
    }

    private static string? FormatConnectAfricaDuplicate(string? duplicate)
    {
        if (string.IsNullOrWhiteSpace(duplicate))
            return null;

        var trimmed = duplicate.Trim();
        if (trimmed == "1" ||
            trimmed.Equals("DUPLICATA", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("DUPLICATE", StringComparison.OrdinalIgnoreCase))
        {
            return "DUPLICATA";
        }

        return trimmed;
    }

    private static string? FormatConnectAfricaTaxLineDisplay(ConnectAfricaReceiptTaxLine? line)
    {
        if (line is null)
            return null;

        var amount = FormatConnectAfricaAmount(line.Amount);
        if (string.IsNullOrWhiteSpace(amount))
            return null;

        if (!string.IsNullOrWhiteSpace(line.Percentage))
            return $"{amount} ({line.Percentage.Trim()})";

        return amount;
    }

    private static string? FormatConnectAfricaFixedCharges(ConnectAfricaMeterChargeReceiptData? receipt)
    {
        if (receipt is null || receipt.FixedChargeAmounts.Count == 0)
            return null;

        return FormatConnectAfricaNamedAmounts(receipt.FixedChargeAmounts, receipt.FixedChargeNames);
    }

    private static IReadOnlyList<PaymentLevyLinePrintModel> BuildLevyLines(
        ConnectAfricaMeterChargeReceiptData? receipt)
    {
        if (receipt is null || receipt.LevyAmounts.Count == 0)
            return [];

        var lines = new List<PaymentLevyLinePrintModel>(receipt.LevyAmounts.Count);
        AppendConnectAfricaLevyLines(lines, receipt.LevyAmounts, receipt.LevyNames);
        return lines;
    }

    private static string? ResolvePreTaxDebts1(ConnectAfricaMeterChargeReceiptData? receipt)
    {
        if (receipt is null || receipt.PreTaxLevyAmounts.Count == 0)
            return null;

        for (var i = 0; i < receipt.PreTaxLevyAmounts.Count; i++)
        {
            var label = i < receipt.PreTaxLevyNames.Count
                ? receipt.PreTaxLevyNames[i]
                : null;
            if (label is not null && label.Contains("Dettes", StringComparison.OrdinalIgnoreCase))
                return FormatConnectAfricaAmount(receipt.PreTaxLevyAmounts[i]);
        }

        return null;
    }

    private static void AppendConnectAfricaLevyLines(
        List<PaymentLevyLinePrintModel> lines,
        IReadOnlyList<string> amounts,
        IReadOnlyList<string> names)
    {
        for (var i = 0; i < amounts.Count; i++)
        {
            var amount = FormatConnectAfricaAmount(amounts[i]);
            if (string.IsNullOrWhiteSpace(amount))
                continue;

            var label = i < names.Count && !string.IsNullOrWhiteSpace(names[i])
                ? names[i].Trim()
                : "Dettes 1";

            lines.Add(new PaymentLevyLinePrintModel
            {
                Label = label,
                Amount = amount
            });
        }
    }

    private static string FormatPrepaidMeterDisplay(string? meterNumber)
    {
        if (string.IsNullOrWhiteSpace(meterNumber))
            return "-";

        var trimmed = meterNumber.Trim();
        var lastSlash = trimmed.LastIndexOf('/');
        if (lastSlash >= 0 && lastSlash < trimmed.Length - 1)
            return trimmed[(lastSlash + 1)..];

        return trimmed;
    }

    private static IReadOnlyList<string> FormatPrepaidTokenDisplayLines(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return [];

        var digits = new string(token.Where(char.IsDigit).ToArray());
        if (digits.Length == 0)
            return [token.Trim()];

        var groups = new List<string>((digits.Length + 3) / 4);
        for (var i = 0; i < digits.Length; i += 4)
        {
            groups.Add(digits.Substring(i, Math.Min(4, digits.Length - i)));
        }

        if (groups.Count <= 3)
            return [string.Join(' ', groups)];

        var mid = (groups.Count + 1) / 2;
        return
        [
            string.Join(' ', groups.Take(mid)),
            string.Join(' ', groups.Skip(mid))
        ];
    }

    private static string? NormalizeTaxRateDisplay(string? rate)
    {
        if (string.IsNullOrWhiteSpace(rate))
            return null;

        var trimmed = rate.Trim();
        return trimmed.StartsWith('@') ? trimmed : $"@ {trimmed}";
    }

    private static string? FormatConnectAfricaLevyAmount(ConnectAfricaMeterChargeReceiptData? receipt)
    {
        if (receipt is null || receipt.LevyAmounts.Count == 0)
            return null;

        return FormatConnectAfricaNamedAmounts(receipt.LevyAmounts, receipt.LevyNames);
    }

    private static string? FormatConnectAfricaNamedAmounts(
        IReadOnlyList<string> amounts,
        IReadOnlyList<string> names)
    {
        var parts = new List<string>(amounts.Count);
        for (var i = 0; i < amounts.Count; i++)
        {
            var amount = FormatConnectAfricaAmount(amounts[i]);
            if (string.IsNullOrWhiteSpace(amount))
                continue;

            if (i < names.Count && !string.IsNullOrWhiteSpace(names[i]))
                parts.Add($"{amount} ({names[i].Trim()})");
            else
                parts.Add(amount);
        }

        return parts.Count > 0 ? string.Join(" / ", parts) : null;
    }

    private static string? FormatConnectAfricaTotalDeductions(ConnectAfricaMeterChargeReceiptData? receipt)
    {
        if (receipt is null)
            return null;

        decimal total = 0m;
        var hasValue = false;

        if (TryParseConnectAfricaDecimal(receipt.OutstandingArrears, out var arrears))
        {
            total += arrears;
            hasValue = true;
        }

        foreach (var amount in receipt.FixedChargeAmounts)
        {
            if (TryParseConnectAfricaDecimal(amount, out var fixedCharge))
            {
                total += fixedCharge;
                hasValue = true;
            }
        }

        foreach (var levy in receipt.LevyAmounts)
        {
            if (TryParseConnectAfricaDecimal(levy, out var levyAmount))
            {
                total += levyAmount;
                hasValue = true;
            }
        }

        return hasValue ? FormatReceiptAmount(total) : null;
    }

    private static string? NormalizeKwhDisplay(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value
            .Replace("KWh", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("kWh", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();
    }

    private static string? FormatConnectAfricaAmount(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return TryParseConnectAfricaDecimal(value, out var parsed)
            ? FormatReceiptAmount(parsed)
            : value.Trim();
    }

    private static bool TryParseConnectAfricaDecimal(string? value, out decimal parsed)
    {
        parsed = 0m;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalized = value
            .Replace("CDF", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("FCFA", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("KWh", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("kWh", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();

        return decimal.TryParse(normalized, NumberStyles.Number, ReceiptAmountCulture, out parsed)
            || decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out parsed);
    }

    private static string? ResolveMeterChargeWebResponse(PaymentOrderItemEntity item) =>
        !string.IsNullOrWhiteSpace(item.WebResponse) ? item.WebResponse : item.ExternalRef;

    private static readonly Regex JsonPropertyCamelBoundary = new(
        @"(?<=[a-z])(?=[A-Z])|(?<=[A-Za-z])(?=[0-9])",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static (IReadOnlyList<string> Lines, bool IsJsonObject) BuildExternalReferenceForPrint(string? externalRef)
    {
        if (string.IsNullOrWhiteSpace(externalRef))
        {
            return ([], false);
        }

        var trimmed = externalRef.Trim();
        if (trimmed.Length < 2 || trimmed[0] != '{' || trimmed[^1] != '}')
        {
            return ([trimmed], false);
        }

        try
        {
            using var doc = JsonDocument.Parse(trimmed);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return ([trimmed], false);
            }

            var lines = new List<string>();
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                var line = FormatExternalReferenceJsonProperty(prop.Name, prop.Value);
                if (line is not null)
                {
                    lines.Add(line);
                }
            }

            var displayLines = lines.Count > 0 ? lines : (IReadOnlyList<string>)[trimmed];
            return (displayLines, true);
        }
        catch (JsonException)
        {
            return ([trimmed], false);
        }
    }

    private static string? FormatExternalReferenceJsonProperty(string propertyName, JsonElement value)
    {
        return propertyName switch
        {
            "RetrievalReferenceNumber" => FormatExternalReferenceLabelValueLine("Retrieval Reference Number", value),
            "PaymentReference" => FormatExternalReferenceLabelValueLine("Payment Reference", value),
            "TerminalId" => FormatExternalReferenceLabelValueLine("Terminal Id", value),
            "PaymentId" => FormatExternalReferencePaymentIdLine(value),
            _ => FormatExternalReferenceLabelValueLine(JsonPropertyKeyToPrintLabel(propertyName), value),
        };
    }

    private static string JsonPropertyKeyToPrintLabel(string propertyName)
    {
        var spaced = JsonPropertyCamelBoundary.Replace(propertyName, " ");
        if (string.IsNullOrEmpty(spaced))
        {
            return propertyName;
        }

        spaced = $"{char.ToUpperInvariant(spaced[0])}{spaced[1..]}";
        return spaced;
    }

    private static string FormatExternalReferenceLabelValueLine(string label, JsonElement value)
    {
        if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return label;
        }

        var text = ExternalReferenceJsonPrimitiveToString(value);
        return string.IsNullOrWhiteSpace(text) ? label : $"{label} {text}";
    }

    private static string? FormatExternalReferencePaymentIdLine(JsonElement value)
    {
        if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        var text = ExternalReferenceJsonPrimitiveToString(value);
        return string.IsNullOrWhiteSpace(text) ? null : $"Payment {text}";
    }

    private static string? ExternalReferenceJsonPrimitiveToString(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.String:
                return value.GetString();
            case JsonValueKind.Number:
                return value.GetRawText().Trim('"');
            case JsonValueKind.True:
                return "true";
            case JsonValueKind.False:
                return "false";
            default:
                return null;
        }
    }

    #endregion

    private static PaymentHistoryQueryInternalModel NormalizePaymentHistoryQuery(
        PaymentHistoryQueryInputModel model,
        Guid customerId)
    {
        return new PaymentHistoryQueryInternalModel
        {
            CustomerId = customerId,
            StatusId = model.StatusId,
            OnlyActive = model.OnlyActive ?? true,
            PageNumber = model.PageNumber <= 0 ? 1 : model.PageNumber,
            PageSize = model.PageSize <= 0 ? 20 : Math.Min(model.PageSize, 100),
            Search = string.IsNullOrWhiteSpace(model.Search) ? null : model.Search.Trim(),
            SortBy = string.IsNullOrWhiteSpace(model.SortBy) ? "createdAtUtc" : model.SortBy.Trim(),
            IsAscending = model.SortBy is not null && model.IsAscending,
            BillerCode = string.IsNullOrWhiteSpace(model.BillerCode) ? null : model.BillerCode.Trim(),
            BillingServiceCode = string.IsNullOrWhiteSpace(model.BillingServiceCode) ? null : model.BillingServiceCode.Trim(),
            BillingRefValue = string.IsNullOrWhiteSpace(model.BillingRefValue) ? null : model.BillingRefValue.Trim()
        };
    }

    private static TopupRateListInputModel NormalizeTopupRateQuery(
        TopupRateListInputModel model)
    {
        model.PageNumber = model.PageNumber <= 0 ? 1 : model.PageNumber;
        model.PageSize = model.PageSize <= 0 ? 20 : Math.Min(model.PageSize, 100);
        model.Search = string.IsNullOrWhiteSpace(model.Search) ? null : model.Search.Trim();
        model.SortBy = string.IsNullOrWhiteSpace(model.SortBy) ? "createdAtUtc" : model.SortBy.Trim();
        model.IsAscending = model.SortBy is not null && model.IsAscending;
        return model;
    }

    private static PaymentRateListInputModel NormalizePaymentRateQuery(
       PaymentRateListInputModel model)
    {
        model.PageNumber = model.PageNumber <= 0 ? 1 : model.PageNumber;
        model.PageSize = model.PageSize <= 0 ? 20 : Math.Min(model.PageSize, 100);
        model.Search = string.IsNullOrWhiteSpace(model.Search) ? null : model.Search.Trim();
        model.SortBy = string.IsNullOrWhiteSpace(model.SortBy) ? "createdAtUtc" : model.SortBy.Trim();
        model.IsAscending = model.SortBy is not null && model.IsAscending;
        return model;
    }

    private static void NormalizeReportListQuery(BaseQueryInputModel model)
    {
        model.PageNumber = model.PageNumber <= 0 ? 1 : model.PageNumber;
        model.PageSize = model.PageSize <= 0 ? 20 : Math.Min(model.PageSize, 100);
        model.Search = string.IsNullOrWhiteSpace(model.Search) ? null : model.Search.Trim();
        model.SortBy = string.IsNullOrWhiteSpace(model.SortBy) ? "createdAtUtc" : model.SortBy.Trim();
    }

    private static IQueryable<PaymentEntity> ApplyCompletedTopupPaymentSearch(
        IQueryable<PaymentEntity> query,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var normalizedSearch = search.Trim().ToLowerInvariant();
        return query.Where(x =>
            (x.PaymentRef != null && x.PaymentRef.ToLower().Contains(normalizedSearch))
            || (x.ExternalRef != null && x.ExternalRef.ToLower().Contains(normalizedSearch))
            || (x.PaymentMethod != null
                && x.PaymentMethod.PaymentProvider != null
                && ((x.PaymentMethod.PaymentProvider.Code != null
                     && x.PaymentMethod.PaymentProvider.Code.ToLower().Contains(normalizedSearch))
                    || (x.PaymentMethod.PaymentProvider.Name != null
                        && x.PaymentMethod.PaymentProvider.Name.ToLower().Contains(normalizedSearch))))
            || (x.PaymentMethod != null
                && ((x.PaymentMethod.Code != null && x.PaymentMethod.Code.ToLower().Contains(normalizedSearch))
                    || (x.PaymentMethod.Name != null && x.PaymentMethod.Name.ToLower().Contains(normalizedSearch))))
            || (x.Currency != null
                && ((x.Currency.Code != null && x.Currency.Code.ToLower().Contains(normalizedSearch))
                    || (x.Currency.Abbreviation != null && x.Currency.Abbreviation.ToLower().Contains(normalizedSearch)))));
    }

    private static IQueryable<PaymentOrderItemEntity> ApplyPayBillItemSearch(
        IQueryable<PaymentOrderItemEntity> query,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var normalizedSearch = search.Trim().ToLowerInvariant();
        return query.Where(x =>
            (x.BillingRefValue != null && x.BillingRefValue.ToLower().Contains(normalizedSearch))
            || (x.PaymentOrder != null
                && x.PaymentOrder.Description != null
                && x.PaymentOrder.Description.ToLower().Contains(normalizedSearch))
            || (x.BillingService != null
                && x.BillingService.Biller != null
                && (x.BillingService.Biller.Code.ToLower().Contains(normalizedSearch)
                    || x.BillingService.Biller.Name.ToLower().Contains(normalizedSearch))));
    }

    private static IQueryable<PaymentOrderItemEntity> ApplyPayBillItemRowsOrdering(
        IQueryable<PaymentOrderItemEntity> query,
        string? sortBy,
        bool isAscending)
    {
        var normalizedSortBy = NormalizeReportSortBy(sortBy);
        return normalizedSortBy switch
        {
            "amount" => isAscending
                ? query.OrderBy(x => x.Amount).ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.Amount).ThenByDescending(x => x.Id),
            "billingrefvalue" => isAscending
                ? query.OrderBy(x => x.BillingRefValue).ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.BillingRefValue).ThenByDescending(x => x.Id),
            "paymentdate" => OrderPayBillItemsByPaymentDate(query, isAscending),
            "createdatutc" => isAscending
                ? query.OrderBy(x => x.CreatedAtUtc).ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id),
            _ => isAscending
                ? query.OrderBy(x => x.CreatedAtUtc).ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id)
        };
    }

    private static IQueryable<PaymentOrderItemEntity> OrderPayBillItemsByPaymentDate(
        IQueryable<PaymentOrderItemEntity> query,
        bool isAscending)
    {
        if (isAscending)
        {
            return query
                .OrderBy(x => x.PaymentOrder!.Payments
                    .Where(p => p.Direction == PaymentDirectionEnum.OUT && p.Status == PaymentStatus.Completed)
                    .Max(p => p.CreatedAtUtc))
                .ThenBy(x => x.Id);
        }

        return query
            .OrderByDescending(x => x.PaymentOrder!.Payments
                .Where(p => p.Direction == PaymentDirectionEnum.OUT && p.Status == PaymentStatus.Completed)
                .Max(p => p.CreatedAtUtc))
            .ThenByDescending(x => x.Id);
    }

    private static IReadOnlyList<CompletedPostpaidPayBillByDateRowOutputModel> ApplyPostpaidPayBillByDateRowSearch(
        IReadOnlyList<CompletedPostpaidPayBillByDateRowOutputModel> rows,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return rows;

        var normalizedSearch = search.Trim().ToLowerInvariant();
        return rows
            .Where(x =>
                (x.BillingRefValue != null && x.BillingRefValue.ToLower().Contains(normalizedSearch))
                || (x.BillingRefForDisplay != null && x.BillingRefForDisplay.ToLower().Contains(normalizedSearch))
                || (x.BillerCode != null && x.BillerCode.ToLower().Contains(normalizedSearch))
                || (x.BillerName != null && x.BillerName.ToLower().Contains(normalizedSearch))
                || x.CenterCode.ToLower().Contains(normalizedSearch)
                || x.CenterName.ToLower().Contains(normalizedSearch)
                || (x.Description != null && x.Description.ToLower().Contains(normalizedSearch)))
            .ToList();
    }

    private static IReadOnlyList<CompletedPostpaidPayBillByDateRowOutputModel> ApplyPostpaidPayBillByDateRowOrdering(
        IReadOnlyList<CompletedPostpaidPayBillByDateRowOutputModel> rows,
        string? sortBy,
        bool isAscending)
    {
        var normalizedSortBy = NormalizeReportSortBy(sortBy);
        return normalizedSortBy switch
        {
            "amount" => isAscending
                ? rows.OrderBy(x => x.Amount).ThenBy(x => x.CreatedAtUtc).ToList()
                : rows.OrderByDescending(x => x.Amount).ThenByDescending(x => x.CreatedAtUtc).ToList(),
            "centername" => isAscending
                ? rows.OrderBy(x => x.CenterName, StringComparer.OrdinalIgnoreCase).ThenBy(x => x.CreatedAtUtc).ToList()
                : rows.OrderByDescending(x => x.CenterName, StringComparer.OrdinalIgnoreCase).ThenByDescending(x => x.CreatedAtUtc).ToList(),
            "paymentdate" => isAscending
                ? rows.OrderBy(x => x.PaymentDate).ThenBy(x => x.CreatedAtUtc).ToList()
                : rows.OrderByDescending(x => x.PaymentDate).ThenByDescending(x => x.CreatedAtUtc).ToList(),
            "createdatutc" => isAscending
                ? rows.OrderBy(x => x.CreatedAtUtc).ToList()
                : rows.OrderByDescending(x => x.CreatedAtUtc).ToList(),
            _ => isAscending
                ? rows.OrderBy(x => x.CreatedAtUtc).ToList()
                : rows.OrderByDescending(x => x.CreatedAtUtc).ToList()
        };
    }

    private static IReadOnlyList<CompletedPostpaidPayBillByCenterRowOutputModel> ApplyPostpaidPayBillByCenterRowSearch(
        IReadOnlyList<CompletedPostpaidPayBillByCenterRowOutputModel> rows,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return rows;

        var normalizedSearch = search.Trim().ToLowerInvariant();
        return rows
            .Where(x =>
                (x.BillingRefValue != null && x.BillingRefValue.ToLower().Contains(normalizedSearch))
                || (x.BillingRefForDisplay != null && x.BillingRefForDisplay.ToLower().Contains(normalizedSearch))
                || (x.BillerCode != null && x.BillerCode.ToLower().Contains(normalizedSearch))
                || (x.BillerName != null && x.BillerName.ToLower().Contains(normalizedSearch))
                || x.CenterCode.ToLower().Contains(normalizedSearch)
                || x.CenterName.ToLower().Contains(normalizedSearch)
                || (x.Description != null && x.Description.ToLower().Contains(normalizedSearch)))
            .ToList();
    }

    private static IReadOnlyList<CompletedPostpaidPayBillByCenterRowOutputModel> ApplyPostpaidPayBillByCenterRowOrdering(
        IReadOnlyList<CompletedPostpaidPayBillByCenterRowOutputModel> rows,
        string? sortBy,
        bool isAscending)
    {
        var normalizedSortBy = NormalizeReportSortBy(sortBy);
        return normalizedSortBy switch
        {
            "amount" => isAscending
                ? rows.OrderBy(x => x.Amount).ThenBy(x => x.CreatedAtUtc).ToList()
                : rows.OrderByDescending(x => x.Amount).ThenByDescending(x => x.CreatedAtUtc).ToList(),
            "centername" => isAscending
                ? rows.OrderBy(x => x.CenterName, StringComparer.OrdinalIgnoreCase).ThenBy(x => x.CreatedAtUtc).ToList()
                : rows.OrderByDescending(x => x.CenterName, StringComparer.OrdinalIgnoreCase).ThenByDescending(x => x.CreatedAtUtc).ToList(),
            "paymentdate" => isAscending
                ? rows.OrderBy(x => x.PaymentDate).ThenBy(x => x.CreatedAtUtc).ToList()
                : rows.OrderByDescending(x => x.PaymentDate).ThenByDescending(x => x.CreatedAtUtc).ToList(),
            "createdatutc" => isAscending
                ? rows.OrderBy(x => x.CreatedAtUtc).ToList()
                : rows.OrderByDescending(x => x.CreatedAtUtc).ToList(),
            _ => isAscending
                ? rows.OrderBy(x => x.CreatedAtUtc).ToList()
                : rows.OrderByDescending(x => x.CreatedAtUtc).ToList()
        };
    }

    private static string NormalizeReportSortBy(string? sortBy) =>
        string.IsNullOrWhiteSpace(sortBy) ? "createdatutc" : sortBy.Trim().ToLowerInvariant();

    private static IEnumerable<DateOnly> EnumerateDateRange(DateOnly fromDate, DateOnly toDate)
    {
        for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            yield return date;
    }

    private static (DateOnly Start, DateOnly End) GetReportDateRange(DateTimeOffset fromDate, DateTimeOffset toDate)
    {
        var fromUtc = fromDate.ToUniversalTime();
        var toUtc = toDate.ToUniversalTime();
        return (DateOnly.FromDateTime(fromUtc.UtcDateTime), DateOnly.FromDateTime(toUtc.UtcDateTime));
    }

    private static IReadOnlyList<CompletedTopupByDateOutputModel> BuildCompletedTopupByDateGrouped(
        IReadOnlyList<PaymentEntity> items,
        DateTimeOffset fromDate,
        DateTimeOffset toDate)
    {
        var (rangeStart, rangeEnd) = GetReportDateRange(fromDate, toDate);
        var groupedByDate = items
            .GroupBy(x => DateOnly.FromDateTime(x.CreatedAtUtc.UtcDateTime))
            .ToDictionary(g => g.Key, g => new { Count = g.Count(), Sum = g.Sum(x => x.Amount) });

        return EnumerateDateRange(rangeStart, rangeEnd)
            .Select(date => new CompletedTopupByDateOutputModel
            {
                Date = date,
                Count = groupedByDate.TryGetValue(date, out var totals) ? totals.Count : 0,
                Sum = groupedByDate.TryGetValue(date, out totals) ? totals.Sum : 0m
            })
            .ToList();
    }

    private static TopUpPaymentHistoryQueryInternalModel NormalizeTopUpPaymentHistoryQuery(
        TopUpPaymentHistoryQueryInputModel model,
        Guid customerId)
    {
        return new TopUpPaymentHistoryQueryInternalModel
        {
            CustomerId = customerId,
            StatusId = model.StatusId,
            OnlyActive = model.OnlyActive ?? true,
            PaymentProviderCode = model.PaymentProviderCode,
            PaymentMethodCode = model.PaymentMethodCode,
            PageNumber = model.PageNumber <= 0 ? 1 : model.PageNumber,
            PageSize = model.PageSize <= 0 ? 20 : Math.Min(model.PageSize, 100),
            Search = string.IsNullOrWhiteSpace(model.Search) ? null : model.Search.Trim(),
            SortBy = string.IsNullOrWhiteSpace(model.SortBy) ? "createdAtUtc" : model.SortBy.Trim(),
            IsAscending = model.SortBy is not null && model.IsAscending
        };
    }

    private static PaymentTxnQueryInputModel NormalizePaymentTxnQuery(PaymentTxnQueryInputModel model)
    {
        return new PaymentTxnQueryInputModel
        {
            PageNumber = model.PageNumber <= 0 ? 1 : model.PageNumber,
            PageSize = model.PageSize <= 0 ? 20 : Math.Min(model.PageSize, 100),
            SortBy = string.IsNullOrWhiteSpace(model.SortBy) ? "createdAtUtc" : model.SortBy.Trim(),
            IsAscending = model.SortBy is not null && model.IsAscending
        };
    }

    #region Payment Artifacts
    private async Task<PaymentEntity>
        CreateTopupPaymentArtifactsAsync(
            CustomerEntity customer,
            ChannelEntity channel,
            AccountEntity accountEntity,
            PaymentMethodCurrencyEntity paymentMethodCurrency,
            PaymentTypeEnum? paymentType,
            decimal amount)
    {
        var payment = await CreatePaymentArtifactsAsync(
            customer,
            channel,
            accountEntity,
            paymentMethodCurrency.PaymentMethod!,
            amount,
            isTopup: true,
            paymentType: paymentType,
            createOrderItems: async paymentOrder =>
            {
                var chargesRecord = paymentMethodCurrency.CalculateCharges(amount);

                var paymentOrderItem = PaymentOrderItemEntity.Create(
                    paymentOrderId: paymentOrder.Id,
                    channelId: channel.Id,
                    itemType: OrderItemType.Topup,
                    billingServiceId: null,
                    customerSubscriptionId: null,
                    billingRefValue: null,
                    invoiceId: null,
                    walletAccountId: accountEntity.Id,
                    currencyId: paymentMethodCurrency.CurrencyId,
                    amount: amount,
                    pspFees: chargesRecord.Fees,
                    pspFeesTax: chargesRecord.FeesTax,
                    billerFees: 0m,
                    billerFeesTax: 0m,
                    pspCost: chargesRecord.Cost,
                    pspCostTax: chargesRecord.CostTax,
                    billerCost: 0m,
                    billerCostTax: 0m,
                    pspCommission: 0m,
                    pspCommissionTax: 0m,
                    billerCommission: 0m,
                    billerCommissionTax: 0m);
                await unitOfWork.PaymentOrderItemRepository.AddAsync(paymentOrderItem);
                return [paymentOrderItem];
            });

        return payment;
    }

    private async Task<PaymentEntity> CreateBillPaymentArtifactsAsync(
        CustomerEntity customer,
        ChannelEntity channel,
        AccountEntity accountEntity,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        IReadOnlyList<InvoicePaymentDetailInternalModel> invoicePaymentDetails,
        decimal amount)
    {
        return await CreatePaymentArtifactsAsync(
            customer,
            channel,
            accountEntity,
            paymentMethodCurrency.PaymentMethod!,
            amount,
            isTopup: false,
            paymentType: null,
            createOrderItems: async paymentOrder =>
            {
                var paymentOrderItems = new List<PaymentOrderItemEntity>(invoicePaymentDetails.Count);
                foreach (var detail in invoicePaymentDetails)
                {
                    var invoice = detail.Invoice;
                    var chargeRecord = detail.BillingServiceCurrency.CalculateCharges(detail.Amount);
                    var paymentOrderItem = PaymentOrderItemEntity.Create(
                        paymentOrderId: paymentOrder.Id,
                        channelId: channel.Id,
                        itemType: OrderItemType.PayBill,
                        billingServiceId: invoice.BillingServiceId,
                        customerSubscriptionId: null,
                        billingRefValue: invoice.BillingRef,
                        invoiceId: invoice.Id,
                        walletAccountId: accountEntity.Id,
                        currencyId: accountEntity.CurrencyId,
                        amount: detail.Amount,
                        pspFees: 0m,
                        pspFeesTax: 0m,
                        billerFees: chargeRecord.Fees,
                        billerFeesTax: chargeRecord.FeesTax,
                        pspCost: 0m,
                        pspCostTax: 0m,
                        billerCost: chargeRecord.Cost,
                        billerCostTax: chargeRecord.CostTax,
                        pspCommission: 0m,
                        pspCommissionTax: 0m,
                        billerCommission: chargeRecord.Commission,
                        billerCommissionTax: chargeRecord.CommissionTax
                    );

                    await unitOfWork.PaymentOrderItemRepository.AddAsync(paymentOrderItem);
                    paymentOrderItems.Add(paymentOrderItem);
                }

                return paymentOrderItems;
            },
            paymentControlCheck: async () => await ReservePaymentControlsForBillsInitiationAsync(invoicePaymentDetails));
    }

    private async Task<PaymentEntity>
        CreateMeterChargePaymentArtifactsAsync(
        CustomerEntity customer,
        ChannelEntity channel,
        AccountEntity accountEntity,
        PaymentMethodCurrencyEntity paymentMethodCurrency,
        BillingServiceCurrencyEntity billingServiceCurrency,
        string meterNumber,
        decimal amount)
    {
        return await CreatePaymentArtifactsAsync(
            customer,
            channel,
            accountEntity,
            paymentMethodCurrency.PaymentMethod,
            amount,
            isTopup: false,
            paymentType: null,
            createOrderItems: async paymentOrder =>
            {
                var chargeRecord = billingServiceCurrency.CalculateCharges(amount);
                var paymentOrderItem = PaymentOrderItemEntity.Create(
                    paymentOrderId: paymentOrder.Id,
                    channelId: channel.Id,
                    itemType: OrderItemType.MeterCharge,
                    billingServiceId: billingServiceCurrency.BillingServiceId,
                    customerSubscriptionId: null,
                    billingRefValue: meterNumber,
                    invoiceId: null,
                    walletAccountId: accountEntity.Id,
                    currencyId: accountEntity.CurrencyId,
                    amount: amount,
                    pspFees: 0m,
                    pspFeesTax: 0m,
                    billerFees: chargeRecord.Fees,
                    billerFeesTax: chargeRecord.FeesTax,
                    pspCost: 0m,
                    pspCostTax: 0m,
                    billerCost: chargeRecord.Cost,
                    billerCostTax: chargeRecord.CostTax,
                    pspCommission: 0m,
                    pspCommissionTax: 0m,
                    billerCommission: chargeRecord.Commission,
                    billerCommissionTax: chargeRecord.CommissionTax);

                await unitOfWork.PaymentOrderItemRepository.AddAsync(paymentOrderItem);
                return [paymentOrderItem];
            },
            paymentControlCheck: async () => await ReservePaymentControlsForMeterInitiationAsync(
                billingServiceCurrency.BillingServiceId,
                billingServiceCurrency.CurrencyId,
                meterNumber, amount));
    }

    private async Task<PaymentEntity>
        CreatePaymentArtifactsAsync(
            CustomerEntity customer,
            ChannelEntity channel,
            AccountEntity accountEntity,
            PaymentMethodEntity paymentMethod,
            decimal amount,
            bool isTopup,
            PaymentTypeEnum? paymentType,
            Func<PaymentOrderEntity, Task<List<PaymentOrderItemEntity>>> createOrderItems,
            Func<Task<Result<bool>>> paymentControlCheck = null)
    {
        logger.LogInformation(
            "CreatePaymentArtifacts started for customer {CustomerId}, paymentMethod {PaymentMethodCode}, amount {Amount}, isTopup {IsTopup}",
            customer.Id,
            paymentMethod.Code,
            amount,
            isTopup);

        if (paymentControlCheck is not null)
        {
            var preArtifactsResult = await paymentControlCheck();
            if (!preArtifactsResult.IsSuccess)
            {
                logger.LogError(
                    "CreatePaymentArtifacts payment control reservation failed for customer {CustomerId}, paymentMethod {PaymentMethodCode}. Reason: {Reason}",
                    customer.Id,
                    paymentMethod.Code,
                    preArtifactsResult.Message);
                throw new InvalidOperationException(
                    localizer[LocalizationKeys.PaymentCreateArtifactsControlReservationFailed, preArtifactsResult.Message!].Value);
            }
        }


        var paymentOrder = PaymentOrderEntity.Create(
            customerId: customer.Id,
            channelId: channel.Id,
            description: localizer[LocalizationKeys.PaymentOrderDescriptionVia, paymentMethod.Code].Value,
            status: PaymentOrderStatus.Pending,
            currencyId: accountEntity.CurrencyId);
        await unitOfWork.PaymentOrderRepository.AddAsync(paymentOrder);
        logger.LogInformation(
            "CreatePaymentArtifacts created payment order {PaymentOrderId} for customer {CustomerId}",
            paymentOrder.Id,
            customer.Id);

        var paymentOrderItems = await createOrderItems(paymentOrder);
        logger.LogInformation(
            "CreatePaymentArtifacts created {ItemsCount} payment order items for order {PaymentOrderId}",
            paymentOrderItems.Count,
            paymentOrder.Id);
        paymentOrder.UpdateAmount(
            amount: paymentOrderItems.Sum(i => i.Amount),
            totalFees: paymentOrderItems.Sum(i => i.PaymentProviderFees + i.BillerFees),
            totalFeesTax: paymentOrderItems.Sum(i => i.PaymentProviderFeesTax + i.BillerFeesTax),
            totalCost: paymentOrderItems.Sum(i => i.PaymentProviderCost + i.BillerCost),
            totalCostTax: paymentOrderItems.Sum(i => i.PaymentProviderCostTax + i.BillerCostTax),
            totalCommission: paymentOrderItems.Sum(i => i.PaymentProviderCommission + i.BillerCommission),
            totalCommissionTax: paymentOrderItems.Sum(i => i.PaymentProviderCommissionTax + i.BillerCommissionTax));


        var paymentRef = paymentRefGenerator.GeneratePaymentRef(isTopup);
        var payment = PaymentEntity.Create(
            customerId: customer.Id,
            channelId: channel.Id,
            direction: isTopup ? PaymentDirectionEnum.IN : PaymentDirectionEnum.OUT,
            paymentOrderId: paymentOrder.Id,
            paymentMethodId: paymentMethod.Id,
            currencyId: accountEntity.CurrencyId,
            amount: paymentOrder.TotalAmount,
            tax: null,
            externalRef: null,
            paymentRef: paymentRef,
            paymentType: paymentType,
            status: PaymentStatus.Pending);
        await unitOfWork.PaymentRepository.AddAsync(payment);
        logger.LogInformation(
            "CreatePaymentArtifacts created payment {PaymentId} with reference {PaymentRef} for order {PaymentOrderId}",
            payment.Id,
            payment.PaymentRef,
            paymentOrder.Id);

        return payment;
    }

    #endregion

    #region CronJob Methods

    private async Task TriggerPollingJob(Guid paymentId, string? paymentMethodConfig)
    {
        var pollingSettings = PaymentMethodConfigReader.ResolveStatusPolling(paymentMethodConfig);

        var scheduler = await schedulerFactory.GetScheduler();
        // throws if job was not registered on startup
        if (!await scheduler.CheckExists(PaymentStatusPollingJobConstants.JobKey))
        {
            throw new InvalidOperationException(
                localizer[LocalizationKeys.PaymentQuartzJobNotRegistered, PaymentStatusPollingJobConstants.JobGroup, PaymentStatusPollingJobConstants.JobName].Value);
        }

        var triggerKey = PaymentStatusPollingJobConstants.CreateTriggerKey(paymentId);
        // Removes old registered job if exists
        if (await scheduler.CheckExists(triggerKey))
        {
            await scheduler.UnscheduleJob(triggerKey);
        }

        var trigger = TriggerBuilder.Create()
            .WithIdentity(triggerKey)
            .ForJob(PaymentStatusPollingJobConstants.JobKey)
            .UsingJobData(PaymentStatusPollingJobConstants.PaymentIdKey, paymentId.ToString())
            .StartAt(DateBuilder.FutureDate(
                pollingSettings.InitialDelaySeconds,
                IntervalUnit.Second))
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(pollingSettings.IntervalSeconds)
                .WithRepeatCount(pollingSettings.TotalAttempts - 1))
            .Build();

        await scheduler.ScheduleJob(trigger);
    }

    private async Task StopPollingJobAsync(Guid paymentId)
    {
        logger.LogInformation($"Stop Status polling job for payment {paymentId}");
        var scheduler = await schedulerFactory.GetScheduler();
        var triggerKey = PaymentStatusPollingJobConstants.CreateTriggerKey(paymentId);
        if (await scheduler.CheckExists(triggerKey))
        {
            await scheduler.UnscheduleJob(triggerKey);
        }
    }

    #endregion

    #region Notification Methods
    private async Task SendNotificationForPaymentAsync(PaymentEntity payment)
    {
        if (payment.Status is not (PaymentStatus.Completed or PaymentStatus.Failed))
        {
            return;
        }
        var currencies = CachingFactory.GetValue<CurrencyEntity>();
        //var currency = allCurrencies.FirstOrDefault(x => x.Id == currencyId);
        //var pois = await unitOfWork.PaymentOrderItemRepository.GetByPaymentIdForApplyAsync(payment.Id);
        if (payment.PaymentOrder.Items.Count == 0)
        {
            logger.LogWarning("Payment order items not found for payment {PaymentId} while sending notification. Notification will be skipped.", payment.Id);
            return;
        }

        //var poiGroups = payment.PaymentOrder.Items.GroupBy(x=> x.ItemType).ToList();
        var poiTopupByCustomerGroups = payment.PaymentOrder.Items
            .Where(i => i.ItemType == OrderItemType.Topup)
            .GroupBy(i => i.WalletAccountId)
            .Select(x => new
            {
                WalletAccountId = x.Key,
                TotalAmount = x.Sum(y => y.Amount),
                Currency = currencies.FirstOrDefault(c => c.Id == x.FirstOrDefault()?.CurrencyId)?.Abbreviation
            }
            ).ToList();
        var poiPaymentGroups = payment.PaymentOrder.Items
            .Where(i => i.ItemType == OrderItemType.PayBill || i.ItemType == OrderItemType.MeterCharge)
            .GroupBy(i => i.WalletAccountId)
            .Select(x => new
            {
                WalletAccountId = x.Key,
                TotalAmount = x.Sum(y => y.Amount),
                Currency = currencies.FirstOrDefault(c => c.Id == x.FirstOrDefault()?.CurrencyId)?.Abbreviation
            }
            ).ToList();        
        var accounts = await unitOfWork.AccountRepository.GetByIdsAsync(payment.PaymentOrder.Items.Select(x => x.WalletAccountId).Distinct().ToList());

        foreach (var group in poiTopupByCustomerGroups)
        {
            try
            {
                var account = accounts.First(x => x.Id == group.WalletAccountId);
                if (payment.Status == PaymentStatus.Completed)
                {
                    await SendPaymentNotificationAsync(
                        NotificationCodeEnum.PAYMENT_TOPUP_SUCSESS,
                        NotificationTypeEnum.TOPUP,
                        [
                            group.TotalAmount.ToString("0.##") + $" {group.Currency}",
                            account.Balance.ToString("0.##") + $" {group.Currency}"
                        ], [account.PartnerId], payment.Id);

                    await SendTopupDataEventPushAsync(
                        payment,
                        group.WalletAccountId,
                        group.TotalAmount,
                        group.Currency,
                        account);
                }
                else
                {
                    await SendPaymentNotificationAsync(
                        NotificationCodeEnum.PAYMENT_TOPUP_FAILED,
                    NotificationTypeEnum.TOPUP,
                    [], [account.PartnerId], payment.Id);
                }
            }
            catch (Exception notifyEx)
            {
                logger.LogError(
                    notifyEx,
                    "Failed to send topup notification for payment {PaymentId} and wallet account {WalletAccountId}",
                    payment.Id, group.WalletAccountId);
            }
        }

        foreach (var group in poiPaymentGroups)
        {
            try
            {
                var account = accounts.First(x => x.Id == group.WalletAccountId);
                if (payment.Status == PaymentStatus.Completed)
                {

                    await SendPaymentNotificationAsync(
                        NotificationCodeEnum.PAYMENT_INVOICE_SUCCESS,
                    NotificationTypeEnum.BILL,
                    [
                        group.TotalAmount.ToString("0.##") + $" {group.Currency}",
                        account.Balance.ToString("0.##") + $" {group.Currency}"
                    ], [account.PartnerId], payment.Id);
                }
                else
                {

                }
            }
            catch (Exception notifyEx)
            {
                logger.LogError(
                    notifyEx,
                    "Failed to send payment success notification for payment {PaymentId} and wallet account {WalletAccountId}",
                    payment.Id, group.WalletAccountId);
            }
        }

    }

    private async Task<List<UserEntity>?> GetNotificationUsersOrNullAsync(
        List<long> userPartnerIds,
        CancellationToken cancellationToken = default)
    {
        var users = await unitOfWork.UserRepository.GetByPartnerIdsWithNotificationsDependenciesAsync(userPartnerIds);
        if (users.Count == 0)
        {
            logger.LogWarning("Notification skipped: users not found for user Ids {UserPartnerIds}", string.Join(",", userPartnerIds));
            return null;
        }

        return users;
    }

    private async Task SendPaymentNotificationAsync(
        NotificationCodeEnum notificationCode,
        NotificationTypeEnum notificationType,
        List<string> parameters,
        List<long> userPartnerIds,
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var users = await GetNotificationUsersOrNullAsync(userPartnerIds, cancellationToken);
        if (users == null)
            return;

        var requestedChannels = new List<NotificationChannel> { NotificationChannel.Push, NotificationChannel.SMS };
        var channels = notificationService.GetTargettedChannels(requestedChannels);
        if (channels.Count == 0)
        {
            logger.LogInformation("Notification skipped: no enabled channels for channels {Channles}", string.Join(",", requestedChannels.Select(x => x.ToString())));
            return;
        }

        var model = new NotificationDetailsModel
        {
            IsTransactional = false,
            NotificationCode = notificationCode,
            NotificationType = notificationType,
            Params =
            [
                new ParameterDataModel
                {
                    Language = currentUserContext.PreferredLanguage,
                    Params = parameters
                }
            ],
            SenderId = null,
            ReferenceType = ReferenceTypeEnum.PAYMENT,
            ReferenceId = paymentId.ToString(),
            OverrideReceiverChannelsPreferences = false,
            RequestedChannels = channels,
            Receivers = users
        };

        await notificationService.SendNotificationsAsync([model]);
    }

    private async Task SendTopupDataEventPushAsync(
        PaymentEntity payment,
        Guid walletAccountId,
        decimal totalAmount,
        string? currency,
        AccountEntity account,
        CancellationToken cancellationToken = default)
    {
        var users = await GetNotificationUsersOrNullAsync([account.PartnerId], cancellationToken);
        if (users == null)
            return;

        var pushChannels = notificationService.GetTargettedChannels([NotificationChannel.Push]);
        if (pushChannels.Count == 0)
        {
            logger.LogInformation("Top-up data event skipped: push channel disabled for payment {PaymentId}", payment.Id);
            return;
        }

        await notificationService.SendNotificationsAsync(
        [
            new NotificationDetailsModel
            {
                IsTransactional = false,
                NotificationCode = NotificationCodeEnum.PAYMENT_TOPUP_SUCSESS,
                NotificationType = NotificationTypeEnum.TOPUP,
                SenderId = null,
                OverrideReceiverChannelsPreferences = false,
                RequestedChannels = pushChannels,
                Receivers = users,
                SaveInAppNotification = false,
                PushMessageMode = PushMessageMode.Data,
                PushData = DataPushPayloadBuilder.BuildTopupSuccessPayload(
                    payment.Id.ToString(),
                    walletAccountId.ToString(),
                    totalAmount.ToString("0.##"),
                    currency ?? string.Empty,
                    account.Balance.ToString("0.##"))
            }
        ]);
    }

    #endregion

    #region Reports

    public sealed record GroupResult<T>(int Key, List<T> Data);

    private async Task<Result<List<GroupResult<PaymentEntity>>>> GetCompletedTopupByCurrencyGroupedBasic(
        CompletedTopupByCurrencyInputModel model,
        CancellationToken cancellationToken = default)
    {
        var partner = await paymentProviderAccessPolicy.GetPaymentProvidersReadScopeAsync(cancellationToken);
        if (!partner.IsSuccess)
            return partner.ToFailure<List<GroupResult<PaymentEntity>>>();

        long? paymentProviderId = model.PaymentProviderId;
        if (partner.Value is not null)
        {
            var paymentProvider = await paymentProviderService.GetByPartnerIdAsync(partner.Value.Value);
            if (!paymentProvider.IsSuccess)
                return paymentProvider.ToFailure<List<GroupResult<PaymentEntity>>>();
            paymentProviderId = paymentProvider.Value.Id;
        }

        var data = GetCompletedTopupAllData(paymentProviderId, model.CurrencyCode);
        List<GroupResult<PaymentEntity>> groups;
        if (model.GroupByProvider)
        {
            groups = data
                .GroupBy(x => x.PaymentMethod!.PaymentProviderId)
                .Select(x => new GroupResult<PaymentEntity>(1, x.ToList()))
                .ToList();
        }
        else
        {
            groups = [new GroupResult<PaymentEntity>(0, data.ToList())];
        }

        return Result<List<GroupResult<PaymentEntity>>>.Success(groups);
    }

    private async Task<Result<IReadOnlyList<CompletedTopupByCurrencyOutputModel>>> GetCompletedTopupByCurrencyGrouped(
        CompletedTopupByCurrencyInputModel model,
        CancellationToken cancellationToken = default)
    {
        var groups = await GetCompletedTopupByCurrencyGroupedBasic(model, cancellationToken);
        if (!groups.IsSuccess)
            return groups.ToFailure<IReadOnlyList<CompletedTopupByCurrencyOutputModel>>();

        var result = groups.Value.Select(g => new CompletedTopupByCurrencyOutputModel
        {
            Count = g.Data.Count,
            Sum = g.Data.Sum(x => x.Amount),
            Min = g.Data.Min(x => x.Amount),
            Max = g.Data.Max(x => x.Amount),
            Avg = Math.Round(g.Data.Average(x => x.Amount), 2),
            Data = GetCompletedTopupRecordData(g.Data.First(), g.Key)
        })
            .OrderBy(x => x.Data.PaymentProviderId)
            .ToList();

        return Result<IReadOnlyList<CompletedTopupByCurrencyOutputModel>>.Success(result);
    }

    private async Task<Result<IReadOnlyList<CompletedTopupByCurrencyExcelModel>>> GetCompletedTopupByCurrencyGroupedExcel(
        CompletedTopupByCurrencyInputModel model,
        CancellationToken cancellationToken = default)
    {
        var groups = await GetCompletedTopupByCurrencyGroupedBasic(model, cancellationToken);
        if (!groups.IsSuccess)
            return groups.ToFailure<IReadOnlyList<CompletedTopupByCurrencyExcelModel>>();

        var result = groups.Value.Select(g =>
        {
            var item = GetCompletedTopupRecordExcelData(g.Data.First(), g.Key);
            return new CompletedTopupByCurrencyExcelModel
            {
                Count = g.Data.Count,
                Sum = g.Data.Sum(x => x.Amount),
                Min = g.Data.Min(x => x.Amount),
                Max = g.Data.Max(x => x.Amount),
                Avg = Math.Round(g.Data.Average(x => x.Amount), 2),
                CurrencyCode = item.CurrencyCode,
                CurrencyName = item.CurrencyName,
                PaymentProviderCode = item.PaymentProviderCode,
                PaymentProviderDescription = item.PaymentProviderDescription
            };
        })
            .OrderBy(x => x.PaymentProviderCode)
            .ToList();

        return Result<IReadOnlyList<CompletedTopupByCurrencyExcelModel>>.Success(result);
    }

    private CompletedTopupBasic GetCompletedTopupRecordData(PaymentEntity entity, int key)
    {
        if (key == 1)
        {
            return new CompletedTopupBasic
            {
                CurrencyId = entity.CurrencyId,
                CurrencyCode = entity.Currency?.Code,
                CurrencyName = entity.Currency?.Name,
                PaymentProviderId = entity.PaymentMethod!.PaymentProviderId,
                PaymentProviderCode = entity.PaymentMethod.PaymentProvider.Code,
                PaymentProviderDescription = entity.PaymentMethod.PaymentProvider.Name
            };
        }

        return new CompletedTopupBasic
        {
            CurrencyId = entity.CurrencyId,
            CurrencyCode = entity.Currency?.Code,
            CurrencyName = entity.Currency?.Name
        };
    }

    private CompletedTopupByCurrencyExcel GetCompletedTopupRecordExcelData(PaymentEntity entity, int key)
    {
        if (key == 1)
        {
            return new CompletedTopupByCurrencyExcel
            {
                CurrencyCode = entity.Currency!.Code,
                CurrencyName = entity.Currency.Name,
                PaymentProviderCode = entity.PaymentMethod!.PaymentProvider.Code,
                PaymentProviderDescription = entity.PaymentMethod.PaymentProvider.Name
            };
        }

        return new CompletedTopupByCurrencyExcel
        {
            CurrencyCode = entity.Currency!.Code,
            CurrencyName = entity.Currency.Name
        };
    }

    private sealed class CompletedTopupByCurrencyExcel
    {
        public string CurrencyCode { get; init; } = string.Empty;
        public string? CurrencyName { get; init; }
        public string? PaymentProviderCode { get; init; }
        public string? PaymentProviderDescription { get; init; }
    }

    private async Task<Result<List<PaymentEntity>>> GetCompletedTopupByDateDataAsync(
        CompletedTopupByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.FromDate > model.ToDate)
        {
            return Result<List<PaymentEntity>>.Failure(
                localizer[LocalizationKeys.PaymentCompletedTopupByDateInvalidDateRange],
                AppHttpStatusCodes.BadRequest);
        }

        var partner = await paymentProviderAccessPolicy.GetPaymentProvidersReadScopeAsync(cancellationToken);
        if (!partner.IsSuccess)
            return partner.ToFailure<List<PaymentEntity>>();

        long? paymentProviderId = model.PaymentProviderId;
        if (partner.Value is not null)
        {
            var paymentProvider = await paymentProviderService.GetByPartnerIdAsync(partner.Value.Value);
            if (!paymentProvider.IsSuccess)
                return paymentProvider.ToFailure<List<PaymentEntity>>();
            paymentProviderId = paymentProvider.Value.Id;
        }

        var fromUtc = model.FromDate.ToUniversalTime();
        var toUtc = model.ToDate.ToUniversalTime();

        var data = GetCompletedTopupByDateAllData(
            paymentProviderId,
            model.CurrencyCode,
            fromUtc,
            toUtc).ToList();

        return Result<List<PaymentEntity>>.Success(data);
    }

    private async Task<Result<IReadOnlyList<CompletedTopupByDateOutputModel>>> GetCompletedTopupByDateGrouped(
        CompletedTopupByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        var dataResult = await GetCompletedTopupByDateDataAsync(model, cancellationToken);
        if (!dataResult.IsSuccess)
            return dataResult.ToFailure<IReadOnlyList<CompletedTopupByDateOutputModel>>();

        var result = BuildCompletedTopupByDateGrouped(dataResult.Value, model.FromDate, model.ToDate);

        return Result<IReadOnlyList<CompletedTopupByDateOutputModel>>.Success(result);
    }

    private sealed record CompletedPostpaidPayBillByCenterRecord(
        PaymentOrderItemEntity Item,
        string CenterCode,
        string CenterName);

    private async Task<Result<List<CompletedPostpaidPayBillByCenterRecord>>> GetCompletedPostpaidPayBillByCenterDataAsync(
        CompletedPostpaidPayBillByCenterInputModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.FromDate > model.ToDate)
        {
            return Result<List<CompletedPostpaidPayBillByCenterRecord>>.Failure(
                localizer[LocalizationKeys.PaymentCompletedPostpaidPayBillByCenterInvalidDateRange],
                AppHttpStatusCodes.BadRequest);
        }

        var billerScope = await billerAccessPolicy.GetBillersReadScopeAsync(cancellationToken);
        if (!billerScope.IsSuccess)
            return billerScope.ToFailure<List<CompletedPostpaidPayBillByCenterRecord>>();

        long? billerId = model.BillerId;
        if (billerScope.Value is not null)
        {
            var biller = await billerService.GetByPartnerIdAsync(billerScope.Value.Value);
            if (!biller.IsSuccess)
                return biller.ToFailure<List<CompletedPostpaidPayBillByCenterRecord>>();
            billerId = biller.Value.Id;
        }

        var fromUtc = model.FromDate.ToUniversalTime();
        var toUtc = model.ToDate.ToUniversalTime();
        var normalizedCenterCode = string.IsNullOrWhiteSpace(model.CenterCode)
            ? null
            : model.CenterCode.Trim();
        var centers = CachingFactory.GetValue<CenterEntity>(c => c.Active)            
            .Select(c => (c.Code, c.Name))
            .ToList();
        //var centers = (await unitOfWork.CenterRepository.GetActiveAsync(cancellationToken))
        //    .Select(c => (c.Code, c.Name))
        //    .ToList();

        var items = await GetCompletedPayBillAllData(
                billerId,
                PostpaidBillingServiceTypeCode,
                OrderItemType.PayBill,
                model.CurrencyCode,
                fromUtc,
                toUtc)
            .ToListAsync(cancellationToken);

        var records = items
            .Select(item =>
            {
                var billingRef = ResolvePayBillBillingRef(item);
                return new
                {
                    Item = item,
                    CenterCode = BillingRefCenterCodeParser.ResolveCenterCode(billingRef, centers),
                    CenterName = BillingRefCenterCodeParser.ResolveCenterName(billingRef, centers)
                };
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.CenterCode)
                        && !string.IsNullOrWhiteSpace(x.CenterName)
                        && (normalizedCenterCode == null
                            || string.Equals(x.CenterCode, normalizedCenterCode, StringComparison.OrdinalIgnoreCase)))
            .Select(x => new CompletedPostpaidPayBillByCenterRecord(x.Item, x.CenterCode!, x.CenterName!))
            .ToList();

        return Result<List<CompletedPostpaidPayBillByCenterRecord>>.Success(records);
    }

    private async Task<Result<List<CompletedPostpaidPayBillByCenterRecord>>> GetCompletedPostpaidPayBillByDateDataAsync(
        CompletedPostpaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default) =>
        await GetCompletedPostpaidPayBillByCenterDataAsync(
            new CompletedPostpaidPayBillByCenterInputModel
            {
                GroupByBiller = model.GroupByBiller,
                BillerId = model.BillerId,
                CurrencyCode = model.CurrencyCode,
                FromDate = model.FromDate,
                ToDate = model.ToDate,
                CenterCode = model.CenterCode
            },
            cancellationToken);

    private async Task<Result<IReadOnlyList<CompletedPostpaidPayBillByDateOutputModel>>> GetCompletedPostpaidPayBillByDateGrouped(
        CompletedPostpaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        var dataResult = await GetCompletedPostpaidPayBillByDateDataAsync(model, cancellationToken);
        if (!dataResult.IsSuccess)
            return dataResult.ToFailure<IReadOnlyList<CompletedPostpaidPayBillByDateOutputModel>>();

        var result = BuildCompletedPostpaidPayBillByDateGrouped(
            dataResult.Value,
            model.GroupByBiller,
            model.FromDate,
            model.ToDate);
        return Result<IReadOnlyList<CompletedPostpaidPayBillByDateOutputModel>>.Success(result);
    }

    private static IReadOnlyList<CompletedPostpaidPayBillByDateOutputModel> BuildCompletedPostpaidPayBillByDateGrouped(
        IReadOnlyList<CompletedPostpaidPayBillByCenterRecord> records,
        bool groupByBiller,
        DateTimeOffset fromDate,
        DateTimeOffset toDate)
    {
        if (groupByBiller)
        {
            return records
                .GroupBy(x => x.Item.BillingService!.BillerId)
                .Select(g => new CompletedPostpaidPayBillByDateOutputModel
                {
                    BillerId = g.Key,
                    BillerCode = g.First().Item.BillingService!.Biller.Code,
                    BillerName = g.First().Item.BillingService!.Biller.Name,
                    Count = g.Count(),
                    Sum = g.Sum(x => x.Item.Amount)
                })
                .OrderBy(x => x.BillerCode)
                .ToList();
        }

        var billers = records
            .Select(x => x.Item.BillingService!.Biller)
            .GroupBy(b => b.Id)
            .Select(g => g.First())
            .ToList();
        var singleBiller = billers.Count == 1 ? billers[0] : null;
        var (rangeStart, rangeEnd) = GetReportDateRange(fromDate, toDate);
        var groupedByDate = records
            .GroupBy(x => GetCompletedPayBillPaymentDate(x.Item))
            .ToDictionary(g => g.Key, g => new { Count = g.Count(), Sum = g.Sum(x => x.Item.Amount) });

        return EnumerateDateRange(rangeStart, rangeEnd)
            .Select(date => new CompletedPostpaidPayBillByDateOutputModel
            {
                Date = date,
                BillerId = singleBiller?.Id,
                BillerCode = singleBiller?.Code,
                BillerName = singleBiller?.Name,
                Count = groupedByDate.TryGetValue(date, out var totals) ? totals.Count : 0,
                Sum = groupedByDate.TryGetValue(date, out totals) ? totals.Sum : 0m
            })
            .ToList();
    }

    private async Task<Result<IReadOnlyList<CompletedPostpaidPayBillByCenterOutputModel>>> GetCompletedPostpaidPayBillByCenterGrouped(
        CompletedPostpaidPayBillByCenterInputModel model,
        CancellationToken cancellationToken = default)
    {
        var dataResult = await GetCompletedPostpaidPayBillByCenterDataAsync(model, cancellationToken);
        if (!dataResult.IsSuccess)
            return dataResult.ToFailure<IReadOnlyList<CompletedPostpaidPayBillByCenterOutputModel>>();

        var result = BuildCompletedPostpaidPayBillByCenterGrouped(dataResult.Value, model.GroupByBiller);
        return Result<IReadOnlyList<CompletedPostpaidPayBillByCenterOutputModel>>.Success(result);
    }

    private static IReadOnlyList<CompletedPostpaidPayBillByCenterOutputModel> BuildCompletedPostpaidPayBillByCenterGrouped(
        IReadOnlyList<CompletedPostpaidPayBillByCenterRecord> records,
        bool groupByBiller)
    {
        if (groupByBiller)
        {
            return records
                .GroupBy(x => x.Item.BillingService!.BillerId)
                .Select(g => new CompletedPostpaidPayBillByCenterOutputModel
                {
                    BillerId = g.Key,
                    BillerCode = g.First().Item.BillingService!.Biller.Code,
                    BillerName = g.First().Item.BillingService!.Biller.Name,
                    Count = g.Count(),
                    Sum = g.Sum(x => x.Item.Amount)
                })
                .OrderBy(x => x.BillerCode)
                .ToList();
        }

        var billers = records
            .Select(x => x.Item.BillingService!.Biller)
            .GroupBy(b => b.Id)
            .Select(g => g.First())
            .ToList();
        var singleBiller = billers.Count == 1 ? billers[0] : null;

        return records
            .GroupBy(x => x.CenterName, StringComparer.OrdinalIgnoreCase)
            .Select(g => new CompletedPostpaidPayBillByCenterOutputModel
            {
                CenterName = g.Key,
                BillerId = singleBiller?.Id,
                BillerCode = singleBiller?.Code,
                BillerName = singleBiller?.Name,
                Count = g.Count(),
                Sum = g.Sum(x => x.Item.Amount)
            })
            .OrderBy(x => x.CenterName)
            .ToList();
    }

    private async Task<Result<List<PaymentOrderItemEntity>>> GetCompletedPrepaidPayBillByDateDataAsync(
        CompletedPrepaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.FromDate > model.ToDate)
        {
            return Result<List<PaymentOrderItemEntity>>.Failure(
                localizer[LocalizationKeys.PaymentCompletedPrepaidPayBillByDateInvalidDateRange],
                AppHttpStatusCodes.BadRequest);
        }

        var billerScope = await billerAccessPolicy.GetBillersReadScopeAsync(cancellationToken);
        if (!billerScope.IsSuccess)
            return billerScope.ToFailure<List<PaymentOrderItemEntity>>();

        long? billerId = model.BillerId;
        if (billerScope.Value is not null)
        {
            var biller = await billerService.GetByPartnerIdAsync(billerScope.Value.Value);
            if (!biller.IsSuccess)
                return biller.ToFailure<List<PaymentOrderItemEntity>>();
            billerId = biller.Value.Id;
        }

        var fromUtc = model.FromDate.ToUniversalTime();
        var toUtc = model.ToDate.ToUniversalTime();

        var items = await GetCompletedPayBillAllData(
                billerId,
                PrepaidBillingServiceTypeCode,
                OrderItemType.MeterCharge,
                model.CurrencyCode,
                fromUtc,
                toUtc)
            .ToListAsync(cancellationToken);

        return Result<List<PaymentOrderItemEntity>>.Success(items);
    }

    private async Task<Result<IReadOnlyList<CompletedPrepaidPayBillByDateOutputModel>>> GetCompletedPrepaidPayBillByDateGrouped(
        CompletedPrepaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        var dataResult = await GetCompletedPrepaidPayBillByDateDataAsync(model, cancellationToken);
        if (!dataResult.IsSuccess)
            return dataResult.ToFailure<IReadOnlyList<CompletedPrepaidPayBillByDateOutputModel>>();

        var result = BuildCompletedPrepaidPayBillByDateGrouped(
            dataResult.Value,
            model.GroupByBiller,
            model.FromDate,
            model.ToDate);
        return Result<IReadOnlyList<CompletedPrepaidPayBillByDateOutputModel>>.Success(result);
    }

    private static IReadOnlyList<CompletedPrepaidPayBillByDateOutputModel> BuildCompletedPrepaidPayBillByDateGrouped(
        IReadOnlyList<PaymentOrderItemEntity> items,
        bool groupByBiller,
        DateTimeOffset fromDate,
        DateTimeOffset toDate)
    {
        if (groupByBiller)
        {
            return items
                .GroupBy(x => x.BillingService!.BillerId)
                .Select(g => new CompletedPrepaidPayBillByDateOutputModel
                {
                    BillerId = g.Key,
                    BillerCode = g.First().BillingService!.Biller.Code,
                    BillerName = g.First().BillingService!.Biller.Name,
                    Count = g.Count(),
                    Sum = g.Sum(x => x.Amount)
                })
                .OrderBy(x => x.BillerCode)
                .ToList();
        }

        var billers = items
            .Select(x => x.BillingService!.Biller)
            .GroupBy(b => b.Id)
            .Select(g => g.First())
            .ToList();
        var singleBiller = billers.Count == 1 ? billers[0] : null;
        var (rangeStart, rangeEnd) = GetReportDateRange(fromDate, toDate);
        var groupedByDate = items
            .GroupBy(GetCompletedPayBillPaymentDate)
            .ToDictionary(g => g.Key, g => new { Count = g.Count(), Sum = g.Sum(x => x.Amount) });

        return EnumerateDateRange(rangeStart, rangeEnd)
            .Select(date => new CompletedPrepaidPayBillByDateOutputModel
            {
                Date = date,
                BillerId = singleBiller?.Id,
                BillerCode = singleBiller?.Code,
                BillerName = singleBiller?.Name,
                Count = groupedByDate.TryGetValue(date, out var totals) ? totals.Count : 0,
                Sum = groupedByDate.TryGetValue(date, out totals) ? totals.Sum : 0m
            })
            .ToList();
    }

    private static IReadOnlyList<PaymentEntity> OrderCompletedTopupByCurrencyRows(
        IReadOnlyList<GroupResult<PaymentEntity>> groups,
        bool groupByProvider) =>
        groupByProvider
            ? groups
                .OrderBy(g => g.Data.First().PaymentMethod!.PaymentProvider.Code)
                .SelectMany(g => g.Data.OrderBy(x => x.CreatedAtUtc))
                .ToList()
            : groups
                .SelectMany(g => g.Data)
                .OrderBy(x => x.CreatedAtUtc)
                .ToList();

    private static IReadOnlyList<CompletedPrepaidPayBillByDateRowOutputModel> BuildCompletedPrepaidPayBillByDateRows(
        IReadOnlyList<PaymentOrderItemEntity> items,
        bool groupByBiller)
    {
        var ordered = groupByBiller
            ? items
                .OrderBy(x => x.BillingService!.Biller.Code)
                .ThenBy(GetCompletedPayBillPaymentDate)
                .ThenBy(x => x.CreatedAtUtc)
                .ToList()
            : items
                .OrderBy(GetCompletedPayBillPaymentDate)
                .ThenBy(x => x.CreatedAtUtc)
                .ToList();

        return ordered.Select(MapPrepaidPayBillByDateRow).ToList();
    }

    private static CompletedPrepaidPayBillByDateRowOutputModel MapPrepaidPayBillByDateRow(PaymentOrderItemEntity x) =>
        new()
        {
            BillerId = x.BillingService!.BillerId,
            BillerCode = x.BillingService.Biller.Code,
            BillerName = x.BillingService.Biller.Name,
            PaymentDate = GetCompletedPayBillPaymentDate(x),
            CurrencyCode = x.Currency?.Abbreviation ?? x.Currency?.Code,
            BillingRefValue = x.BillingRefValue,
            BillingRefForDisplay = ResolvePayBillBillingRefForDisplay(x.BillingRefValue),
            Amount = x.Amount,
            Fees = x.TotalFees,
            Tax = x.TotalFeesTax,
            Total = x.TotalAmount,
            Status = x.Status,
            CreatedAtUtc = x.CreatedAtUtc,
            Description = x.PaymentOrder?.Description
        };

    private static CompletedPrepaidPayBillByDateRowExcelModel ToPrepaidPayBillByDateRowExcelModel(
        CompletedPrepaidPayBillByDateRowOutputModel row) =>
        new()
        {
            BillerId = row.BillerId,
            BillerCode = row.BillerCode,
            BillerName = row.BillerName,
            PaymentDate = row.PaymentDate,
            CurrencyCode = row.CurrencyCode,
            BillingRefValue = row.BillingRefValue,
            BillingRefForDisplay = row.BillingRefForDisplay,
            Amount = row.Amount,
            Fees = row.Fees,
            Tax = row.Tax,
            Total = row.Total,
            Status = row.Status,
            CreatedAtUtc = row.CreatedAtUtc,
            Description = row.Description
        };

    private static IReadOnlyList<CompletedPostpaidPayBillByDateRowOutputModel> BuildCompletedPostpaidPayBillByDateRows(
        IReadOnlyList<CompletedPostpaidPayBillByCenterRecord> records,
        bool groupByBiller)
    {
        var ordered = groupByBiller
            ? records
                .OrderBy(x => x.Item.BillingService!.Biller.Code)
                .ThenBy(x => GetCompletedPayBillPaymentDate(x.Item))
                .ThenBy(x => x.CenterName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Item.CreatedAtUtc)
                .ToList()
            : records
                .OrderBy(x => GetCompletedPayBillPaymentDate(x.Item))
                .ThenBy(x => x.CenterName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Item.CreatedAtUtc)
                .ToList();

        return ordered.Select(MapPostpaidPayBillByDateRow).ToList();
    }

    private static CompletedPostpaidPayBillByDateRowOutputModel MapPostpaidPayBillByDateRow(
        CompletedPostpaidPayBillByCenterRecord x) =>
        new()
        {
            BillerId = x.Item.BillingService!.BillerId,
            BillerCode = x.Item.BillingService.Biller.Code,
            BillerName = x.Item.BillingService.Biller.Name,
            CenterCode = x.CenterCode,
            CenterName = x.CenterName,
            PaymentDate = GetCompletedPayBillPaymentDate(x.Item),
            CurrencyCode = x.Item.Currency?.Abbreviation ?? x.Item.Currency?.Code,
            BillingRefValue = x.Item.BillingRefValue,
            BillingRefForDisplay = ResolvePayBillBillingRefForDisplay(x.Item.BillingRefValue),
            Amount = x.Item.Amount,
            Fees = x.Item.TotalFees,
            Tax = x.Item.TotalFeesTax,
            Total = x.Item.TotalAmount,
            Status = x.Item.Status,
            CreatedAtUtc = x.Item.CreatedAtUtc,
            Description = x.Item.PaymentOrder?.Description
        };

    private static CompletedPostpaidPayBillByDateRowExcelModel ToPostpaidPayBillByDateRowExcelModel(
        CompletedPostpaidPayBillByDateRowOutputModel row) =>
        new()
        {
            BillerId = row.BillerId,
            BillerCode = row.BillerCode,
            BillerName = row.BillerName,
            CenterCode = row.CenterCode,
            CenterName = row.CenterName,
            PaymentDate = row.PaymentDate,
            CurrencyCode = row.CurrencyCode,
            BillingRefValue = row.BillingRefValue,
            BillingRefForDisplay = row.BillingRefForDisplay,
            Amount = row.Amount,
            Fees = row.Fees,
            Tax = row.Tax,
            Total = row.Total,
            Status = row.Status,
            CreatedAtUtc = row.CreatedAtUtc,
            Description = row.Description
        };

    private static IReadOnlyList<CompletedPostpaidPayBillByCenterRowOutputModel> BuildCompletedPostpaidPayBillByCenterRows(
        IReadOnlyList<CompletedPostpaidPayBillByCenterRecord> records,
        bool groupByBiller)
    {
        var ordered = groupByBiller
            ? records
                .OrderBy(x => x.Item.BillingService!.Biller.Code)
                .ThenBy(x => x.CenterName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => GetCompletedPayBillPaymentDate(x.Item))
                .ThenBy(x => x.Item.CreatedAtUtc)
                .ToList()
            : records
                .OrderBy(x => x.CenterName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => GetCompletedPayBillPaymentDate(x.Item))
                .ThenBy(x => x.Item.CreatedAtUtc)
                .ToList();

        return ordered.Select(MapPostpaidPayBillByCenterRow).ToList();
    }

    private static CompletedPostpaidPayBillByCenterRowOutputModel MapPostpaidPayBillByCenterRow(
        CompletedPostpaidPayBillByCenterRecord x) =>
        new()
        {
            BillerId = x.Item.BillingService!.BillerId,
            BillerCode = x.Item.BillingService.Biller.Code,
            BillerName = x.Item.BillingService.Biller.Name,
            CenterCode = x.CenterCode,
            CenterName = x.CenterName,
            PaymentDate = GetCompletedPayBillPaymentDate(x.Item),
            CurrencyCode = x.Item.Currency?.Abbreviation ?? x.Item.Currency?.Code,
            BillingRefValue = x.Item.BillingRefValue,
            BillingRefForDisplay = ResolvePayBillBillingRefForDisplay(x.Item.BillingRefValue),
            Amount = x.Item.Amount,
            Fees = x.Item.TotalFees,
            Tax = x.Item.TotalFeesTax,
            Total = x.Item.TotalAmount,
            Status = x.Item.Status,
            CreatedAtUtc = x.Item.CreatedAtUtc,
            Description = x.Item.PaymentOrder?.Description
        };

    private static CompletedPostpaidPayBillByCenterRowExcelModel ToPostpaidPayBillByCenterRowExcelModel(
        CompletedPostpaidPayBillByCenterRowOutputModel row) =>
        new()
        {
            BillerId = row.BillerId,
            BillerCode = row.BillerCode,
            BillerName = row.BillerName,
            CenterCode = row.CenterCode,
            CenterName = row.CenterName,
            PaymentDate = row.PaymentDate,
            CurrencyCode = row.CurrencyCode,
            BillingRefValue = row.BillingRefValue,
            BillingRefForDisplay = row.BillingRefForDisplay,
            Amount = row.Amount,
            Fees = row.Fees,
            Tax = row.Tax,
            Total = row.Total,
            Status = row.Status,
            CreatedAtUtc = row.CreatedAtUtc,
            Description = row.Description
        };

    private static string? ResolvePayBillBillingRef(PaymentOrderItemEntity item) =>
        item.Invoice?.BillingRef ?? item.BillingRefValue;

    private static string? ResolvePayBillBillingRefForDisplay(string? billingRefValue) =>
        billingRefValue != null && billingRefValue.Contains('-')
            ? billingRefValue.Substring(billingRefValue.LastIndexOf('-') + 1)
            : billingRefValue;

    private static DateOnly GetCompletedPayBillPaymentDate(PaymentOrderItemEntity item)
    {
        var payment = item.PaymentOrder!.Payments
            .Where(p => p.Direction == PaymentDirectionEnum.OUT
                        && p.Status == PaymentStatus.Completed)
            .OrderByDescending(p => p.CreatedAtUtc)
            .First();
        return DateOnly.FromDateTime(payment.CreatedAtUtc.UtcDateTime);
    }

    private async Task<Result<IReadOnlyList<TopupRateOutputModel>>> GetTopupRateGrouped(bool groupByProvider, bool groupByMethod, int currencyId, long? paymentProviderId, long? paymentMethodId)
    {
        var partner = await paymentProviderAccessPolicy.GetPaymentProvidersReadScopeAsync();
        if (!partner.IsSuccess)
            return partner.ToFailure<IReadOnlyList<TopupRateOutputModel>>();
        if (partner.Value is not null)
        {

            var paymentProvider = await paymentProviderService.GetByPartnerIdAsync(partner.Value.Value);
            if (!paymentProvider.IsSuccess)
                return paymentProvider.ToFailure<IReadOnlyList<TopupRateOutputModel>>();
            paymentProviderId = paymentProvider.Value.Id;
        }

        var data = GetTopupRateAllData(currencyId, paymentProviderId, paymentMethodId);
        var groups = new List<GroupResult<PaymentEntity>>();
        if (groupByProvider && groupByMethod)
        {
            var g = data.GroupBy(x => new
            {
                x.Status,
                ProviderId = x.PaymentMethod.PaymentProviderId,
                x.PaymentMethodId
            });
            groups = g.Select(x => new GroupResult<PaymentEntity>(3, x.ToList())).ToList();
        }
        else if (groupByProvider)
        {
            var g = data.GroupBy(x => new
            {
                x.Status,
                ProviderId = x.PaymentMethod.PaymentProviderId
            });
            groups = g.Select(x => new GroupResult<PaymentEntity>(2, x.ToList())).ToList();
        }
        else if (groupByMethod)
        {
            var g = data.GroupBy(x => new
            {
                x.Status,
                x.PaymentMethodId
            });
            groups = g.Select(x => new GroupResult<PaymentEntity>(1, x.ToList())).ToList();
        }
        else
        {
            var g = data.GroupBy(x => new
            {
                x.Status
            });
            groups = g.Select(x => new GroupResult<PaymentEntity>(0, x.ToList())).ToList();
        }

        var result = groups.Select(g => new TopupRateOutputModel
        {
            Count = g.Data.Count(),
            Sum = g.Data.Sum(x => x.Amount),
            Min = g.Data.Min(x => x.Amount),
            Max = g.Data.Max(x => x.Amount),
            Avg = Math.Round(g.Data.Average(x => x.Amount), 2),
            Data = GetTopupRecordData(g.Data.First(), g.Key)
        }).ToList();
        return Result<IReadOnlyList<TopupRateOutputModel>>.Success(result);
    }

    private async Task<Result<IReadOnlyList<TopupRateExcelModel>>> GetTopupRateGroupedExcel(bool groupByProvider, bool groupByMethod, int currencyId, long? paymentProviderId, long? paymentMethodId)
    {
        var groups = await GetTopupRateGroupedBasic(groupByProvider, groupByMethod, currencyId, paymentProviderId, paymentMethodId);
        if (!groups.IsSuccess)
            return groups.ToFailure<IReadOnlyList<TopupRateExcelModel>>();
        var result = groups.Value.Select(g =>
        {
            var item = GetTopupRecordExcelData(g.Data.First(), g.Key);
            return new TopupRateExcelModel
            {
                Count = g.Data.Count(),
                Sum = g.Data.Sum(x => x.Amount),
                Min = g.Data.Min(x => x.Amount),
                Max = g.Data.Max(x => x.Amount),
                Avg = Math.Round(g.Data.Average(x => x.Amount), 2),
                CurrencyCode = item.CurrencyCode,
                Status = item.Status,
                PaymentMethodCode = item.PaymentMethodCode,
                PaymentMethodDescription = item.PaymentMethodDescription,
                PaymentProviderCode = item.PaymentProviderCode,
                PaymentProviderDescription = item.PaymentProviderDescription
            };
        }).ToList();

        return Result<IReadOnlyList<TopupRateExcelModel>>.Success(result);
       
    }
    private async Task<Result<List<GroupResult<PaymentEntity>>>> GetTopupRateGroupedBasic(bool groupByProvider, bool groupByMethod, int currencyId, long? paymentProviderId, long? paymentMethodId)
    {
        var partner = await paymentProviderAccessPolicy.GetPaymentProvidersReadScopeAsync();
        if (!partner.IsSuccess)
            return partner.ToFailure<List<GroupResult<PaymentEntity>>>();
        if (partner.Value is not null)
        {

            var paymentProvider = await paymentProviderService.GetByPartnerIdAsync(partner.Value.Value);
            if (!paymentProvider.IsSuccess)
                return paymentProvider.ToFailure<List<GroupResult<PaymentEntity>>>();
            paymentProviderId = paymentProvider.Value.Id;
        }

        var data = GetTopupRateAllData(currencyId, paymentProviderId, paymentMethodId);
        var groups = new List<GroupResult<PaymentEntity>>();
        if (groupByProvider && groupByMethod)
        {
            var g = data.GroupBy(x => new
            {
                x.Status,
                ProviderId = x.PaymentMethod.PaymentProviderId,
                x.PaymentMethodId
            });
            groups = g.Select(x => new GroupResult<PaymentEntity>(3, x.ToList())).ToList();
        }
        else if (groupByProvider)
        {
            var g = data.GroupBy(x => new
            {
                x.Status,
                ProviderId = x.PaymentMethod.PaymentProviderId
            });
            groups = g.Select(x => new GroupResult<PaymentEntity>(2, x.ToList())).ToList();
        }
        else if (groupByMethod)
        {
            var g = data.GroupBy(x => new
            {
                x.Status,
                x.PaymentMethodId
            });
            groups = g.Select(x => new GroupResult<PaymentEntity>(1, x.ToList())).ToList();
        }
        else
        {
            var g = data.GroupBy(x => new
            {
                x.Status
            });
            groups = g.Select(x => new GroupResult<PaymentEntity>(0, x.ToList())).ToList();
        }
        return Result<List<GroupResult<PaymentEntity>>>.Success(groups);
    }
    private async Task<Result<List<GroupResult<PaymentOrderItemEntity>>>> GetPaymentRateGroupedBasic(bool groupByBiller, bool groupByBillingService, int currencyId, long? billerId, long? billingServiceId)
    {
        var partner = await billerAccessPolicy.GetBillersReadScopeAsync();
        if (!partner.IsSuccess)
            return partner.ToFailure<List<GroupResult<PaymentOrderItemEntity>>>();
        if (partner.Value is not null)
        {              
            
            var biller = await billerService.GetByPartnerIdAsync(partner.Value.Value);
            if(!biller.IsSuccess)
                return biller.ToFailure<List<GroupResult<PaymentOrderItemEntity>>>();
            billerId = biller.Value.Id;
        }

        var data = GetPaymentRateAllData(currencyId, billerId, billingServiceId);

        var groups = new List<GroupResult<PaymentOrderItemEntity>>();

        if (groupByBiller && groupByBillingService)
        {
            var g = data.GroupBy(x => new
            {
                x.Status,
                BillerId = x.BillingService.BillerId,
                x.BillingServiceId
            });

            groups = g.Select(x =>
                new GroupResult<PaymentOrderItemEntity>(3, x.ToList())
            ).ToList();
        }
        else if (groupByBiller)
        {
            var g = data.GroupBy(x => new
            {
                x.Status,
                BillerId = x.BillingService.BillerId
            });

            groups = g.Select(x =>
                new GroupResult<PaymentOrderItemEntity>(2, x.ToList())
            ).ToList();
        }
        else if (groupByBillingService)
        {
            var g = data.GroupBy(x => new
            {
                x.Status,
                x.BillingServiceId
            });

            groups = g.Select(x =>
                new GroupResult<PaymentOrderItemEntity>(1, x.ToList())
            ).ToList();
        }
        else
        {
            var g = data.GroupBy(x => new
            {
                x.Status
            });

            groups = g.Select(x =>
                new GroupResult<PaymentOrderItemEntity>(0, x.ToList())
            ).ToList();
        }
        return Result<List<GroupResult<PaymentOrderItemEntity>>>.Success(groups);
    }
    private async Task<Result<IReadOnlyList<PaymentRateExcelModel>>> GetPaymentRateGroupedExcel(bool groupByBiller,bool groupByBillingService, int currencyId, long? billerId, long? billingServiceId)
    {
        var groups = await GetPaymentRateGroupedBasic(groupByBiller, groupByBillingService, currencyId, billerId, billingServiceId);
        if (!groups.IsSuccess)
            return groups.ToFailure<IReadOnlyList<PaymentRateExcelModel>>();
        var result = groups.Value.Select(g => { 
            var item = GetPaymentRecordExcelData(g.Data.First(), g.Key); 
            return new PaymentRateExcelModel
            {
                Count = g.Data.Count(),
                Sum = g.Data.Sum(x => x.Amount),
                Min = g.Data.Min(x => x.Amount),
                Max = g.Data.Max(x => x.Amount),
                Avg = Math.Round(g.Data.Average(x => x.Amount), 2),
                CurrencyCode = item.CurrencyCode,
                Status = item.Status,
                BillerCode = item.BillerCode,
                BillerDescription = item.BillerDescription,
                BillingServiceCode = item.BillingServiceCode,
                BillingServiceDescription = item.BillingServiceDescription
            }; 
        }).ToList();

        return Result<IReadOnlyList<PaymentRateExcelModel>>.Success(result);
    }
    private async Task<Result<IReadOnlyList<PaymentRateOutputModel>>> GetPaymentRateGrouped(bool groupByBiller, bool groupByBillingService, int currencyId, long? billerId, long? billingServiceId)
    {
        var groups = await GetPaymentRateGroupedBasic(groupByBiller, groupByBillingService, currencyId, billerId, billingServiceId);
        if (!groups.IsSuccess)
            return groups.ToFailure<IReadOnlyList<PaymentRateOutputModel>>();
        var result = groups.Value.Select(g => new PaymentRateOutputModel
        {
            Count = g.Data.Count(),
            Sum = g.Data.Sum(x => x.Amount),
            Min = g.Data.Min(x => x.Amount),
            Max = g.Data.Max(x => x.Amount),
            Avg = Math.Round(g.Data.Average(x => x.Amount), 2),
            Data = GetPaymentRecordData(g.Data.First(), g.Key)
        }).ToList();

        return Result<IReadOnlyList<PaymentRateOutputModel>>.Success(result);
    }
    private TopupRateBasic GetTopupRecordData(PaymentEntity entity, int key)
    {
        if(key == 3)
            return new TopupRateBasic { 
                Status = entity.Status, 
                CurrencyId = entity.CurrencyId,
                PaymentMethodId = entity.PaymentMethodId, 
                PaymentMethodDescription = entity.PaymentMethod.Name, 
                PaymentProviderId = entity.PaymentMethod.PaymentProviderId,
                PaymentProviderDescription = entity.PaymentMethod.PaymentProvider.Name
            };
        if (key == 2)
            return new TopupRateBasic
            {
                Status = entity.Status,
                CurrencyId = entity.CurrencyId,
                PaymentProviderId = entity.PaymentMethod.PaymentProviderId,
                PaymentProviderDescription = entity.PaymentMethod.PaymentProvider.Name
            };
        if (key == 1)
            return new TopupRateBasic
            {
                Status = entity.Status,
                CurrencyId = entity.CurrencyId,
                PaymentMethodId = entity.PaymentMethodId,
                PaymentMethodDescription = entity.PaymentMethod.Name
            };
        return new TopupRateBasic
        {
            Status = entity.Status,
            CurrencyId = entity.CurrencyId
        };
    }

    private TopupRateExcel GetTopupRecordExcelData(PaymentEntity entity, int key)
    {
        if (key == 3)
            return new TopupRateExcel
            {
                Status = entity.Status.ToString(),
                CurrencyCode = entity.Currency.Code,
                PaymentMethodCode = entity.PaymentMethod.Code,
                PaymentMethodDescription = entity.PaymentMethod.Name,
                PaymentProviderCode = entity.PaymentMethod.PaymentProvider.Code,
                PaymentProviderDescription = entity.PaymentMethod.PaymentProvider.Name
            };
        if (key == 2)
            return new TopupRateExcel
            {
                Status = entity.Status.ToString(),
                CurrencyCode = entity.Currency.Code,
                PaymentProviderCode = entity.PaymentMethod.PaymentProvider.Code,
                PaymentProviderDescription = entity.PaymentMethod.PaymentProvider.Name
            };
        if (key == 1)
            return new TopupRateExcel
            {
                Status = entity.Status.ToString(),
                CurrencyCode = entity.Currency.Code,
                PaymentMethodCode = entity.PaymentMethod.Code,
                PaymentMethodDescription = entity.PaymentMethod.Name
            };
        return new TopupRateExcel
        {
            Status = entity.Status.ToString(),
            CurrencyCode = entity.Currency.Code
        };
    }

    private PaymentRateBasic GetPaymentRecordData(PaymentOrderItemEntity entity, int key)
    {
        if (key == 3)
            return new PaymentRateBasic
            {
                Status = entity.Status,
                CurrencyId = entity.CurrencyId,
                BillerDescription = entity.BillingService.Biller.Name,
                BillerId = entity.BillingService.BillerId,
                BillingServiceDescription = entity.BillingService.Name,
                BillingServiceId = entity.BillingServiceId,
            };
        if (key == 2)
            return new PaymentRateBasic
            {
                Status = entity.Status,
                CurrencyId = entity.CurrencyId,
                BillerDescription = entity.BillingService.Biller.Name,
                BillerId = entity.BillingService.BillerId,                
            };
        if (key == 1)
            return new PaymentRateBasic
            {
                Status = entity.Status,
                CurrencyId = entity.CurrencyId,
                BillingServiceDescription = entity.BillingService.Name,
                BillingServiceId = entity.BillingServiceId,
            };
        return new PaymentRateBasic
        {
            Status = entity.Status,
            CurrencyId = entity.CurrencyId
        };
    }

    private PaymentRateExcel GetPaymentRecordExcelData(PaymentOrderItemEntity entity, int key)
    {
        if (key == 3)
            return new PaymentRateExcel
            {
                Status = entity.Status.ToString(),
                CurrencyCode = entity.Currency.Code,
                BillerDescription = entity.BillingService.Biller.Name,
                BillerCode = entity.BillingService.Biller.Code,
                BillingServiceDescription = entity.BillingService.Name,
                BillingServiceCode = entity.BillingService.Code,
            };
        if (key == 2)
            return new PaymentRateExcel
            {
                Status = entity.Status.ToString(),
                CurrencyCode = entity.Currency.Code,
                BillerDescription = entity.BillingService.Biller.Name,
                BillerCode = entity.BillingService.Biller.Code,                
            };
        if (key == 1)
            return new PaymentRateExcel
            {
                Status = entity.Status.ToString(),
                CurrencyCode = entity.Currency.Code,
                BillingServiceDescription = entity.BillingService.Name,
                BillingServiceCode = entity.BillingService.Code,
            };
        return new PaymentRateExcel
        {
            Status = entity.Status.ToString(),
            CurrencyCode = entity.Currency.Code,            
        };
    }
    private IQueryable<PaymentEntity> GetTopupRateAllData(int? currencyId, long? paymentProviderId, long? paymentMethodId)
    {
        var query = unitOfWork.PaymentRepository.GetAllQueryable()
            .Include(x => x.Customer)
            .Include(x => x.Currency)
            .Include(x => x.PaymentMethod).ThenInclude(x => x.PaymentProvider)
            .Where(x => (x.Direction == PaymentDirectionEnum.IN) &&
                        x.CurrencyId == currencyId &&
                        (paymentProviderId == null ? true : x.PaymentMethod!.PaymentProviderId == paymentProviderId) &&
                        (paymentMethodId == null ? true : x.PaymentMethodId == paymentMethodId));
        return query;
    }

    private IQueryable<PaymentEntity> GetCompletedTopupAllData(long? paymentProviderId, string? currencyCode)
    {
        var normalizedCurrencyCode = string.IsNullOrWhiteSpace(currencyCode)
            ? null
            : currencyCode.Trim().ToLowerInvariant();

        return unitOfWork.PaymentRepository.GetAllQueryable()
            .Include(x => x.Customer)
            .Include(x => x.Currency)
            .Include(x => x.PaymentMethod).ThenInclude(x => x.PaymentProvider)
            .Where(x => x.Direction == PaymentDirectionEnum.IN
                        && x.Status == PaymentStatus.Completed
                        && (paymentProviderId == null
                            || x.PaymentMethod!.PaymentProviderId == paymentProviderId)
                        && (normalizedCurrencyCode == null
                            || (x.Currency != null
                                && ((x.Currency.Code != null
                                     && x.Currency.Code.ToLower() == normalizedCurrencyCode)
                                    || (x.Currency.Abbreviation != null
                                        && x.Currency.Abbreviation.ToLower() == normalizedCurrencyCode)))));
    }

    private IQueryable<PaymentEntity> GetCompletedTopupByDateAllData(
        long? paymentProviderId,
        string currencyCode,
        DateTimeOffset fromDateUtc,
        DateTimeOffset toDateUtc)
    {
        var normalizedCurrencyCode = currencyCode.Trim().ToLowerInvariant();
        var toExclusive = toDateUtc.AddDays(1);

        return unitOfWork.PaymentRepository.GetAllQueryable()
            .Include(x => x.Currency)
            .Include(x => x.PaymentMethod).ThenInclude(x => x.PaymentProvider)
            .Where(x => x.Direction == PaymentDirectionEnum.IN
                        && x.Status == PaymentStatus.Completed
                        && x.CreatedAtUtc >= fromDateUtc
                        && x.CreatedAtUtc < toExclusive
                        && (paymentProviderId == null
                            || x.PaymentMethod!.PaymentProviderId == paymentProviderId)
                        && x.Currency != null
                        && ((x.Currency.Code != null
                             && x.Currency.Code.ToLower() == normalizedCurrencyCode)
                            || (x.Currency.Abbreviation != null
                                && x.Currency.Abbreviation.ToLower() == normalizedCurrencyCode)));
    }
    private IQueryable<PaymentOrderItemEntity> GetCompletedPayBillAllData(
        long? billerId,
        string billingServiceTypeCode,
        OrderItemType itemType,
        string currencyCode,
        DateTimeOffset fromDateUtc,
        DateTimeOffset toDateUtc)
    {
        var normalizedCurrencyCode = currencyCode.Trim().ToLowerInvariant();
        var toExclusive = toDateUtc.AddDays(1);

        return unitOfWork.PaymentOrderItemRepository.GetAllQueryable()
            .AsNoTracking()
            .Include(x => x.Invoice)
            .Include(x => x.Currency)
            .Include(x => x.BillingService).ThenInclude(x => x!.ServiceType)
            .Include(x => x.BillingService).ThenInclude(x => x!.Biller)
            .Include(x => x.PaymentOrder).ThenInclude(x => x!.Payments)
            .Where(x => x.ItemType == itemType
                        && (x.Status == PaymentOrderItemStatus.Paid
                            || x.Status == PaymentOrderItemStatus.Applied)
                        && x.BillingService != null
                        && x.BillingService.ServiceType.Code == billingServiceTypeCode
                        && (billerId == null || x.BillingService.BillerId == billerId)
                        && x.PaymentOrder != null
                        && x.PaymentOrder.Payments.Any(p =>
                            p.Direction == PaymentDirectionEnum.OUT
                            && p.Status == PaymentStatus.Completed
                            && p.CreatedAtUtc >= fromDateUtc
                            && p.CreatedAtUtc < toExclusive)
                        && x.Currency != null
                        && ((x.Currency.Code != null
                             && x.Currency.Code.ToLower() == normalizedCurrencyCode)
                            || (x.Currency.Abbreviation != null
                                && x.Currency.Abbreviation.ToLower() == normalizedCurrencyCode)));
    }

    private IQueryable<PaymentOrderItemEntity> GetPaymentRateAllData(int? currencyId, long? billerId, long? billingServiceId)
    {
        var query = unitOfWork.PaymentOrderItemRepository.GetAllQueryable()
            .Include(x => x.Currency)
            .Include(x => x.BillingService).ThenInclude(x => x.Biller)
            .Include(x => x.PaymentOrder).ThenInclude(x => x.Payments)
            .Where(x => (x.PaymentOrder!.Payments!.FirstOrDefault()!.Direction == PaymentDirectionEnum.OUT) &&
                        x.CurrencyId == currencyId &&
                        (billerId == null ? true : x.BillingService.BillerId == billerId) &&
                        (billingServiceId == null ? true : x.BillingServiceId == billingServiceId));
        return query;        
    }


    #endregion

    #endregion


}
