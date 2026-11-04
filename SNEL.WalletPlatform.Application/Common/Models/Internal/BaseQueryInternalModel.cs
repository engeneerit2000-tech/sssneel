namespace SNEL.WalletPlatform.Application.Common.Models.Internal;

public abstract record BaseQueryInternalModel(
    int PageNumber,
    int PageSize,
    string? Search,
    string? SortBy,
    bool IsAscending);
