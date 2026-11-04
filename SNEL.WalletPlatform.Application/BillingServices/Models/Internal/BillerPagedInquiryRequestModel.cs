namespace SNEL.WalletPlatform.Application.BillingServices.Models.Internal;

public sealed record BillerPagedInquiryRequestModel(
    long BillingServiceId,
    string ServiceCode,
    string BillingRef,
    bool UnpaidOnly,
    int PageNumber,
    int PageSize,
    string? Search,
    string? SortBy,
    bool IsAscending);
