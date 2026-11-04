using SNEL.WalletPlatform.Application.Common;

namespace SNEL.WalletPlatform.Application.BillingServices.Models.Output;

public sealed record PagedInquireOutputModel(
    string BillerCode,
    string ServiceCode,
    string BillingRef,
    string BillingRefForDisplay,
    IReadOnlyCollection<InquireBalanceOutputModel>? Balances,
    PagedResult<InquireBillOutputModel> Bills,
    IReadOnlyCollection<InquireTotalOutstandingByCurrencyOutputModel> TotalOutstanding,
    DateTimeOffset RetrievedAtUtc);
