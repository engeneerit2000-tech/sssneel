using SNEL.WalletPlatform.Domain.Billing;

namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

public record InvoiceSummaryOutputModel(
    Guid Id,
    string InvoiceRef,
    decimal AmountOutstanding,
    decimal Amount,
    decimal AmountPaid,
    int CurrencyId,
    DateTimeOffset IssueDate,
    DateTimeOffset? DueDate,
    string Status
    );