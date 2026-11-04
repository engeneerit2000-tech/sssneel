using SNEL.WalletPlatform.Application.BillingServices.Models.Input;
using SNEL.WalletPlatform.Application.BillingServices.Models.Output;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.BillingServices.Interfaces;

public interface IBillingServiceService
{
    Task<Result<BillingServiceOutputModel>> CreateAsync(CreateBillingServiceInputModel model, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyCollection<BillingServiceOutputModel>>> GetBillingServicesAsync(
        string languageCode = "en",
        bool? isActive = true,
        CancellationToken cancellationToken = default);
    Task<Result<BillingServiceOutputModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Result<BillingServiceOutputModel>> GetByCodeAsync(string BillerCode, string BillingSeriveCode, CancellationToken cancellationToken = default);
    Task<Result<BillingServiceOutputModel>> UpdateAsync(long id, UpdateBillingServiceInputModel model, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<Result<InquireOutputModel>> Inquire(Guid customerSubscriptionId, bool unpaidOnly = true, CancellationToken cancellationToken = default);
    Task<Result<PagedInquireOutputModel>> InquirePaged(
        PagedBillingInquiryInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<InquireOutputModel>> Inquire(long billingServiceId, string billingRef, bool unpaidOnly = true, CancellationToken cancellationToken = default);
    Task<Result<InquireOutputModel>> Inquire(string billerCode, string serviceCode, string billingRef, bool unpaidOnly = true, CancellationToken cancellationToken = default);
    Task<Result<InquireOutputModel>> InquireByInvoiceRef(string invoiceRef, bool unpaidOnly = true, CancellationToken cancellationToken = default);
    Task<Result<PrepaidMeterInquiryOutputModel>> InquirePrepaidMeter(string meterNumber);
    Task<Result<List<CustomActionsOutputModel>>> GetCustomActions(Guid customerSubscriptionId);
    Task<Result<List<CustomActionsOutputModel>>> GetCustomActions(long billingServiceId);
    Task<Result<List<CustomActionsOutputModel>>> GetCustomActions(string billerCode,string serviceCode);
    Task<Result<object>> ExecuteCustomActions(ExecuteCustomActionsInputModel model);
    Task<Result<bool>> ApplyPayment(PaymentEntity? pmnt=null, Guid? paymentId = null, string? internalRef = null, string? externalRef = null);

    //ApplyType = CRON_JOB
    Task<Result<bool>> ApplyCompletedPaidPaymentsAsync();
    //ApplyType = CRON_JOB FOR BILLER
    Task<Result<bool>> ApplyCompletedPaidPaymentsForBillingServiceAsync(string billingServiceCode);
    //Apply type=AFTER_PAY
    Task<Result> ApplyPOIsAfterPay(Guid paymentId);

    //ApplyType = ON_PAY
    Task<Result<bool>> ApplyPaymentAfterCompleteAsync(PaymentEntity? payment = null);
}
