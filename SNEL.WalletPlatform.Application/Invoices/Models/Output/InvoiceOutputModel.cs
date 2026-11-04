using SNEL.WalletPlatform.Application.Lookups.Models.Output;

namespace SNEL.WalletPlatform.Application.Invoices.Models.Output;

public sealed class InvoiceOutputModel
{
    public InvoiceOutputModel() { }

    public InvoiceOutputModel(
        long billingServiceId,
        string billingRef,
        string invoiceRef,
        decimal amountOutstanding,
        decimal amountPaid,
        decimal amountTotal,
        int currencyId,
        CurrencyOutputModel currency,
        DateTimeOffset issueDate,
        DateTimeOffset? dueDate,
        string status,
        string? invoiceInfo,
        bool paymentMismatchFlag,
        DateTime? invoiceUpdateDate,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        string? createdBy,
        string? updatedBy)
    {
        BillingServiceId = billingServiceId;
        BillingRef = billingRef;
        InvoiceRef = invoiceRef;
        AmountOutstanding = amountOutstanding;
        AmountPaid = amountPaid;
        AmountTotal = amountTotal;
        CurrencyId = currencyId;
        Currency = currency;
        IssueDate = issueDate;
        DueDate = dueDate;
        Status = status;
        InvoiceInfo = invoiceInfo;
        PaymentMismatchFlag = paymentMismatchFlag;
        InvoiceUpdateDate = invoiceUpdateDate;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        CreatedBy = createdBy;
        UpdatedBy = updatedBy;
    }

    public long BillingServiceId { get; private set; }
    public string BillingRef { get; private set; } = string.Empty;
    public string InvoiceRef { get; private set; } = string.Empty;
    public decimal AmountOutstanding { get; private set; }
    public decimal AmountPaid { get; private set; }
    public decimal AmountTotal { get; private set; }
    public int CurrencyId { get; private set; }
    public CurrencyOutputModel Currency { get; private set; } = null!;
    public DateTimeOffset IssueDate { get; private set; }
    public DateTimeOffset? DueDate { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public string? InvoiceInfo { get; private set; }
    public bool PaymentMismatchFlag { get; private set; }
    public DateTime? InvoiceUpdateDate { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
