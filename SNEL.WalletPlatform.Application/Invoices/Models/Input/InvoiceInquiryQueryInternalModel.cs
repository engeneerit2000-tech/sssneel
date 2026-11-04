using SNEL.WalletPlatform.Application.Common.Models.Internal;

namespace SNEL.WalletPlatform.Application.Invoices.Models.Input;

public sealed record InvoiceInquiryQueryInternalModel(
    long BillingServiceId,
    string BillingRef,
    bool UnpaidOnly,
    int PageNumber,
    int PageSize,
    string? Search,
    string? SortBy,
    bool IsAscending)
    : BaseQueryInternalModel(PageNumber, PageSize, Search, SortBy, IsAscending);
