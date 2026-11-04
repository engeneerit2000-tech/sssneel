using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.BillingServices.Models.Input;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Models.Input;
using SNEL.WalletPlatform.Application.Payments.Models.Input;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

public interface IPaymentService
{
    Task<Result<PagedResult<PaymentWithOrderAndItemsOutputModel>>> GetPaymentHistoryAsync(
        string languageCode,
        PaymentHistoryQueryInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<PagedResult<PaymentWithOrderAndItemsOutputModel>>> GetScopePaymentHistoryAsync(
        string languageCode,
        PaymentHistoryQueryInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<FileContentResult>> ExportScopePaymentHistoryAsync(
        string languageCode,
        PaymentHistoryQueryInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<PagedResult<PaymentWithOrderAndItemsOutputModel>>> GetTopUpPaymentHistoryAsync(
        string languageCode,
        TopUpPaymentHistoryQueryInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<PagedResult<PaymentWithOrderAndItemsOutputModel>>> GetScopeTopUpPaymentHistoryAsync(
        string languageCode,
        TopUpPaymentHistoryQueryInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<FileContentResult>> ExportScopeTopUpPaymentHistoryAsync(
        string languageCode,
        TopUpPaymentHistoryQueryInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<PaymentWithOrderAndItemsOutputModel>> GetPaymentDetailsAsync(
        string languageCode,
        Guid paymentId,
        CancellationToken cancellationToken = default);
    Task<Result<PagedResult<PaymentTxnOutputModel>>> GetPaymentTransactionsAsync(
        Guid paymentId,
        PaymentTxnQueryInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<byte[]>> GetPaymentPdfAsync(Guid paymentId, string? acceptLanguage, CancellationToken cancellationToken = default);
    Task<Result<PaymentInitializationOutputModel>> InitTopupPayment(PaymentInitializeInputModel model);
    Task<Result<PaymentInitializationOutputModel>> InitMeterChargePayment(InitMeterChargePaymentInputModel model);

    Task<Result<PaymentSummaryOutputModel>> MeterRechargePaymentSummary(InitMeterChargePaymentInputModel model);
    Task<Result<bool>> ConfirmTopupPayment(Guid paymentId);
    Task<Result<PaymentInitializationOutputModel>> InitBillPayment(PayBillsInputModel model);
    Task<Result<PaymentSummaryOutputModel>> BillPaymentSummary(PayBillsInputModel model);
    Task<Result<PaymentSummaryOutputModel>> TopupSummary(PaymentInitializeInputModel model);
    Task<Result<PaymentConfirmationOutputModel>> ConfirmBillPayment(Guid paymentId);
    Task<Result<PaymentConfirmationOutputModel>> TerminatePaymentAfterInitAsync(
        Guid paymentId,
        PaymentStatus terminalStatus = PaymentStatus.Cancelled);
    Task<Result<PaymentStatus>> ManualCompletePaymentAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);
    Task<Result<bool>> ManualApplyPaymentAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);
    Task<Result<bool>> ManualReconcilePoiAsync(
        Guid paymentId,
        string externalRef,
        CancellationToken cancellationToken = default);
    Task<Result<PaymentStatus>> SyncPaymentStatusWithPsp(Guid paymentId, int? statusPollingAttempt = null);
    /// <summary>
    /// Syncs status with the PSP once, then marks the payment expired if it is still pending or processing.
    /// </summary>
    Task<Result<PaymentStatus>> ExpireStalePendingPaymentAsync(Guid paymentId);
    Task<Result<int>> SyncInPendingPaymentsStatusWithPspAsync(
        int pageSize,
        int minimumAgeMinutes,
        CancellationToken cancellationToken = default);
    Task<Result<PaymentStatus>> CompletePaymentFromWebhook(Guid paymentId);

    /// <summary>
    /// Applies AkibaPay callback outcome (SUCCESS / FAILED) to the wallet payment.
    /// </summary>
    Task<Result<PaymentStatus>> ApplyAkibaPayWebhookOutcomeAsync(
        Guid paymentId,
        string transactionStatus,
        string? akibaReference,
        CancellationToken cancellationToken = default);
    /// <summary>
    /// Applies Easypay callback outcome (SUCCESS / FAILED) to the wallet payment.
    /// </summary>
    Task<Result<PaymentStatus>> ApplyEasypayWebhookOutcomeAsync(
        Guid paymentId,
        string transactionStatus,
        string? easypayReference,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries AkibaPay management API for transaction status and applies the outcome when final.
    /// </summary>
    Task<Result<PaymentStatus>> SyncAkibaPayStatusFromManagementAsync(
        Guid paymentId,
        bool passApply = false,
        CancellationToken cancellationToken = default,
        int? statusPollingAttempt = null);
    
    /// <summary>
    /// Queries Easypay management API for transaction status and applies the outcome when final.
    /// </summary>
    Task<Result<PaymentStatus>> SyncEasypayStatusFromManagementAsync(
        Guid paymentId,
        bool passApply = false,
        CancellationToken cancellationToken = default,
        int? statusPollingAttempt = null);

    /// <summary>
    /// Applies Afrimony callback outcome (TS / TF) to the wallet payment.
    /// </summary>
    Task<Result<PaymentStatus>> ApplyAfrimonyWebhookOutcomeAsync(
        Guid paymentId,
        string transactionStatus,
        string? mmTransactionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries Afrimony TransactionEnquiry API for status and applies the outcome when final.
    /// </summary>
    Task<Result<PaymentStatus>> SyncAfrimonyStatusFromEnquiryAsync(
        Guid paymentId,
        bool passApply = false,
        CancellationToken cancellationToken = default,
        int? statusPollingAttempt = null);

    Task<Result<PaymentStatus>> ApplyMpesaWebhookOutcomeAsync(
        Guid paymentId,
        string transactionStatus,
        string? transactionId,
        CancellationToken cancellationToken = default);

    Task<Result<PaymentStatus>> SyncMpesaStatusAsync(
        Guid paymentId,
        bool passApply = false,
        CancellationToken cancellationToken = default,
        int? statusPollingAttempt = null);

    Task<Result<PaymentStatus>> ApplyOrangeWebhookOutcomeAsync(
        Guid paymentId,
        string transactionStatus,
        string? orangeTransactionId,
        CancellationToken cancellationToken = default);

    Task<Result<PaymentStatus>> SyncOrangeStatusAsync(
        Guid paymentId,
        bool passApply = false,
        CancellationToken cancellationToken = default,
        int? statusPollingAttempt = null);

    Task<Result<bool>> TestLock(string lockKey, TimeSpan timeout);
    Task<Result<IReadOnlyList<TopupRateOutputModel>>> GetTopupRate(TopupRateInputModel model);
    Task<Result<PagedResult<PaymentOutputModel>>> GetTopupRateList(TopupRateListInputModel model);
    Task<Result<FileContentResult>> ExportTopupRateList(TopupRateExportInputModel model);
    Task<Result<FileContentResult>> ExportTopupRate(TopupRateInputModel model);
    Task<Result<IReadOnlyList<PaymentRateOutputModel>>> GetPaymentRate(PaymentRateInputModel model);
    Task<Result<PagedResult<PaymentOrderItemOutputModel>>> GetPaymentRateList(PaymentRateListInputModel model);
    Task<Result<FileContentResult>> ExportPaymentList(PaymentRateExportInputModel model);
    Task<Result<FileContentResult>> ExportPaymentRate(PaymentRateInputModel model);
    Task<Result<IReadOnlyList<CompletedTopupByCurrencyOutputModel>>> GetCompletedTopupByCurrencyAsync(
        CompletedTopupByCurrencyInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<FileContentResult>> ExportCompletedTopupByCurrencyAsync(
        CompletedTopupByCurrencyInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<FileContentResult>> ExportCompletedTopupByCurrencyRowsAsync(
        CompletedTopupByCurrencyInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<PagedResult<PaymentOutputModel>>> GetCompletedTopupByCurrencyRowsListAsync(
        CompletedTopupByCurrencyRowsListInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CompletedTopupByDateOutputModel>>> GetCompletedTopupByDateAsync(
        CompletedTopupByDateInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<FileContentResult>> ExportCompletedTopupByDateAsync(
        CompletedTopupByDateInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<FileContentResult>> ExportCompletedTopupByDateRowsAsync(
        CompletedTopupByDateInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<PagedResult<PaymentOutputModel>>> GetCompletedTopupByDateRowsListAsync(
        CompletedTopupByDateRowsListInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CompletedPostpaidPayBillByDateOutputModel>>> GetCompletedPostpaidPayBillByDateAsync(
        CompletedPostpaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<FileContentResult>> ExportCompletedPostpaidPayBillByDateAsync(
        CompletedPostpaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<FileContentResult>> ExportCompletedPostpaidPayBillByDateRowsAsync(
        CompletedPostpaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<PagedResult<CompletedPostpaidPayBillByDateRowOutputModel>>> GetCompletedPostpaidPayBillByDateRowsListAsync(
        CompletedPostpaidPayBillByDateRowsListInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CompletedPostpaidPayBillByCenterOutputModel>>> GetCompletedPostpaidPayBillByCenterAsync(
        CompletedPostpaidPayBillByCenterInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<FileContentResult>> ExportCompletedPostpaidPayBillByCenterAsync(
        CompletedPostpaidPayBillByCenterInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<FileContentResult>> ExportCompletedPostpaidPayBillByCenterRowsAsync(
        CompletedPostpaidPayBillByCenterInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<PagedResult<CompletedPostpaidPayBillByCenterRowOutputModel>>> GetCompletedPostpaidPayBillByCenterRowsListAsync(
        CompletedPostpaidPayBillByCenterRowsListInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CompletedPrepaidPayBillByDateOutputModel>>> GetCompletedPrepaidPayBillByDateAsync(
        CompletedPrepaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<FileContentResult>> ExportCompletedPrepaidPayBillByDateAsync(
        CompletedPrepaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<FileContentResult>> ExportCompletedPrepaidPayBillByDateRowsAsync(
        CompletedPrepaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<PagedResult<CompletedPrepaidPayBillByDateRowOutputModel>>> GetCompletedPrepaidPayBillByDateRowsListAsync(
        CompletedPrepaidPayBillByDateRowsListInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<PagedResult<PaymentOutputModel>>> GetPaymentsForReviewAsync(
        PaymentReviewQueryInputModel model,
        CancellationToken cancellationToken = default);
}
