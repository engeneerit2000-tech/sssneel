using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Invoicing;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Domain.Billing;

public sealed class InvoiceEntity : TrackedEntity<Guid>
{
    private InvoiceEntity() { }

    public long BillingServiceId { get; private set; }
    public BillingServiceEntity BillingService { get; private set; } = null!;
    public string BillingRef { get; private set; } = string.Empty;
    public long? OriginInvoiceId { get; private set; }
    public string InvoiceRef { get; private set; } = string.Empty;
    public decimal AmountOutstanding { get; private set; }
    public decimal AmountPaid { get; private set; }
    public decimal AmountTotal { get; private set; }
    public int CurrencyId { get; private set; }
    public CurrencyEntity Currency { get; private set; } = null!;
    public DateTimeOffset IssueDate { get; private set; }
    public DateTimeOffset? DueDate { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public string? InvoiceInfo { get; private set; }
    /// <summary>Customer id from origin system (Oracle), stored when syncing from staging.</summary>
    public string? OriginCustomerId { get; private set; }
    /// <summary>True when staging total paid &gt; (POIs Applied + POIs Paid) on the platform (possible external payment).</summary>
    public bool PaymentMismatchFlag { get; private set; }
    public DateTime? InvoiceUpdateDate { get; private set; }
    public ICollection<PaymentOrderItemEntity>? PaymentOrderItems { get; private set; } = null;

    public static InvoiceEntity Create(
        long billingServiceId,
        string billingRef,
        long? originInvoiceId,
        string invoiceRef,
        decimal amountOutstanding,
        decimal amountPaid,
        decimal amountTotal,
        int currencyId,
        DateTimeOffset issueDate,
        DateTimeOffset? dueDate,
        string status,
        string? invoiceInfo,
        DateTimeOffset? invoiceUpdateDate,
        string? originCustomerId = null)
    {
        var now = DateTimeOffset.Now;
        return new InvoiceEntity
        {
            Id = Guid.NewGuid(),
            BillingServiceId = billingServiceId,
            BillingRef = billingRef.Trim(),
            OriginInvoiceId = originInvoiceId,
            InvoiceRef = invoiceRef.Trim(),
            AmountOutstanding = amountOutstanding,
            AmountPaid = amountPaid,
            AmountTotal = amountTotal,
            CurrencyId = currencyId,
            IssueDate = issueDate,
            DueDate = dueDate,
            Status = status.Trim(),
            InvoiceInfo = string.IsNullOrWhiteSpace(invoiceInfo) ? null : invoiceInfo.Trim(),
            InvoiceUpdateDate = invoiceUpdateDate?.UtcDateTime,
            OriginCustomerId = string.IsNullOrWhiteSpace(originCustomerId) ? null : originCustomerId.Trim()
        };
    }

    /// <summary>
    /// Updates invoice amounts and metadata from staging (when invoice_update_date_stg > invoice_update_date_db).
    /// </summary>
    public void UpdateFromStaging(decimal amountTotal, decimal amountPaid, DateTimeOffset issueDate, string? invoiceInfo, DateTimeOffset? invoiceUpdateDate, bool overPaid,string billingRef, string? originCustomerId = null)
    {
        if (amountPaid < 0) amountPaid = 0;
        var amountOutstanding = amountTotal - amountPaid;
        if (amountOutstanding < 0) amountOutstanding = 0;

        AmountTotal = amountTotal;
        AmountPaid = amountPaid;
        AmountOutstanding = amountOutstanding;
        IssueDate = issueDate;
        InvoiceInfo = string.IsNullOrWhiteSpace(invoiceInfo) ? null : invoiceInfo.Trim();
        Status = amountPaid >= amountTotal
            ? InvoiceStatus.Paid.ToString()
            : amountPaid > 0
                ? InvoiceStatus.PartiallyPaid.ToString()
                : InvoiceStatus.Issued.ToString();
        InvoiceUpdateDate = invoiceUpdateDate?.UtcDateTime;
        PaymentMismatchFlag = overPaid;
        BillingRef = billingRef;
        if (originCustomerId != null)
            OriginCustomerId = string.IsNullOrWhiteSpace(originCustomerId) ? null : originCustomerId.Trim();
    }

    public void ApplyPayment(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        AmountPaid += amount;

        AmountOutstanding = AmountTotal - AmountPaid;
        //if (amountOutstanding < 0)
        //    amountOutstanding = 0;

        Status = AmountPaid >= AmountTotal
            ? InvoiceStatus.Paid.ToString()
            : AmountPaid > 0
                ? InvoiceStatus.PartiallyPaid.ToString()
                : InvoiceStatus.Issued.ToString();
    }
}
