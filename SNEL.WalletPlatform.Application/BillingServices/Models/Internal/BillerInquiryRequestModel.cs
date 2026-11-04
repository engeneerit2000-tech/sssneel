namespace SNEL.WalletPlatform.Application.BillingServices.Models.Internal;

public sealed record BillerInquiryRequestModel(
    long BillingServiceId,
    string ServiceCode,
    string BillingRef,
    bool UnpaidOnly = true);
