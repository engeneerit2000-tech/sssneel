namespace SNEL.WalletPlatform.Application.BillingServices.Models.Output;

public sealed record InquireOutputModel(
    string BillerCode,
    string ServiceCode,
    string BillingRef,
    string BillingRefForDisplay,
    IReadOnlyCollection<InquireBalanceOutputModel>? Balances,
    IReadOnlyCollection<InquireBillOutputModel>? Bills,
    IReadOnlyCollection<InquireTotalOutstandingByCurrencyOutputModel> TotalOutstanding,
    DateTimeOffset RetrievedAtUtc);

public sealed record InquireTotalOutstandingByCurrencyOutputModel(
    string CurrencyCode,
    decimal Amount);

public static class InquireOutputTotals
{
    public static IReadOnlyCollection<InquireTotalOutstandingByCurrencyOutputModel> CalculateTotalOutstandingByCurrency(
        IReadOnlyCollection<InquireBillOutputModel>? bills) =>
        bills?
            .GroupBy(bill => bill.CurrencyCode, StringComparer.OrdinalIgnoreCase)
            .Select(group => new InquireTotalOutstandingByCurrencyOutputModel(
                group.Key,
                group.Sum(bill => bill.AmountOutstanding)))
            .OrderBy(total => total.CurrencyCode, StringComparer.OrdinalIgnoreCase)
            .ToArray()
            ?? Array.Empty<InquireTotalOutstandingByCurrencyOutputModel>();
}

public sealed class InquireBillOutputModel
{
    public InquireBillOutputModel(Guid invoiceId,
    string invoiceRef,
    decimal amountTotal,
    decimal amountPaid,
    decimal amountOutstanding,
    string currencyCode,
    string status,
    int statusId,
    DateTimeOffset issueDate,
    DateTimeOffset? dueDate,
    string? customerName,
    string? invoiceInfo)
    {
        InvoiceId = invoiceId;
        InvoiceRef = invoiceRef;
        AmountTotal = amountTotal;
        AmountPaid = amountPaid;
        AmountOutstanding = amountOutstanding;
        CurrencyCode = currencyCode;
        Status = status;
        StatusId = statusId;
        IssueDate = issueDate;
        DueDate = dueDate;
        CustomerName = customerName;
        InvoiceInfo = invoiceInfo;
    }

    public Guid InvoiceId { get; set; }
    public string InvoiceRef { get; set; }
    public decimal AmountTotal { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountOutstanding { get; set; }
    public string CurrencyCode { get; set; }
    public string Status { get; set; }
    public int StatusId { get; set; }
    public DateTimeOffset IssueDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public string? CustomerName { get; set; }
    public string? InvoiceInfo { get; set; }
}
public sealed record InquireBalanceOutputModel(
    string CurrencyCode,
    decimal Amount,
    string? BalanceInfo);
