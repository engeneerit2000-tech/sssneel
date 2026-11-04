namespace SNEL.WalletPlatform.Application.Payments.Models.Internal;

/// <summary>
/// Authorization context for a single payment, pre-computed in a single query.
/// Built by traversing from POI upward so biller scoping is sourced from the order items.
/// </summary>
public sealed record PaymentAccessContextInternalModel(
    Guid PaymentId,
    Guid? CustomerId,
    long? CustomerPartnerId,
    long? ProviderPartnerId,
    IReadOnlyCollection<long> BillerPartnerIds);
