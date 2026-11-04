using SNEL.WalletPlatform.Application.Common.Models.Internal;
using SNEL.WalletPlatform.Domain.Customers;

namespace SNEL.WalletPlatform.Application.Customers.Models.Internal;

public sealed record CustomerQueryInternalModel(
    CustomerStatus? Status,
    int PageNumber,
    int PageSize,
    string? Search,
    string? SortBy,
    bool IsAscending)
    : BaseQueryInternalModel(PageNumber, PageSize, Search, SortBy, IsAscending);
