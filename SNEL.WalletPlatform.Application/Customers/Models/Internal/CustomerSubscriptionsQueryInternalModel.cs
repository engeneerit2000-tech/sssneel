using SNEL.WalletPlatform.Application.Common.Models.Internal;

namespace SNEL.WalletPlatform.Application.Customers.Models.Internal;

public sealed record CustomerSubscriptionsQueryInternalModel(
    Guid? CustomerId,
    long? BillingServiceId,
    int PageNumber,
    int PageSize,
    string? Search,
    string? SortBy,
    bool IsAscending)
    : BaseQueryInternalModel(PageNumber, PageSize, Search, SortBy, IsAscending);