namespace SNEL.WalletPlatform.Application.BillingServices.Models.Internal;

public sealed record BillerSubscriptionExistsRequestModel(
    long BillingServiceId,
    string AreaCode,
    string SubscriptionNumber);
