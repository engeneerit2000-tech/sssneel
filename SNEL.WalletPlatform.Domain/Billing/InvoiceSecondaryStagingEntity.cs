using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Invoicing;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Domain.Billing;

/// <summary>
/// Second-stage table: projected <see cref="InvoiceEntity"/> payloads from Oracle staging (same scalar columns as invoices)
/// plus <see cref="JobKey"/> and <see cref="Updated"/> (false = new invoice, true = change vs existing invoice).
/// </summary>
[PrimaryKey(nameof(Id))]
[Table("InvoiceSecondaryStagings")]
[Index(nameof(JobKey), nameof(InvoiceRef))]
[Index(nameof(JobKey), nameof(Updated), nameof(Id))]
public sealed class InvoiceSecondaryStagingEntity : TrackedEntity<Guid>
{
    [Required]
    public long BillingServiceId { get; set; }

    /// <summary>Navigation for FK only.</summary>
    [ForeignKey(nameof(BillingServiceId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public BillingServiceEntity BillingService { get; set; } = null!;

    [Required]
    [MaxLength(150)]
    public string BillingRef { get; set; } = string.Empty;

    public long? OriginInvoiceId { get; set; }

    [Required]
    [MaxLength(150)]
    public string InvoiceRef { get; set; } = string.Empty;

    [Required]
    [Precision(18, 2)]
    public decimal AmountOutstanding { get; set; }

    [Required]
    [Precision(18, 2)]
    public decimal AmountPaid { get; set; }

    [Required]
    [Precision(18, 2)]
    public decimal AmountTotal { get; set; }

    [Required]
    public int CurrencyId { get; set; }

    /// <summary>Navigation for FK only.</summary>
    [ForeignKey(nameof(CurrencyId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public CurrencyEntity Currency { get; set; } = null!;

    [Required]
    public DateTimeOffset IssueDate { get; set; }

    public DateTimeOffset? DueDate { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? InvoiceInfo { get; set; }

    [MaxLength(50)]
    public string? OriginCustomerId { get; set; }

    [Required]
    public bool PaymentMismatchFlag { get; set; } = false;

    public DateTime? InvoiceUpdateDate { get; set; }

    /// <summary>Job run key (same as <see cref="InvoiceStagingEntity.JobKey"/>).</summary>
    [Required]
    [MaxLength(128)]
    public string JobKey { get; set; } = string.Empty;

    /// <summary>True when this row reflects an update to an existing invoice; false when it is a new invoice projection.</summary>
    [Required]
    public bool Updated { get; set; }

    /// <summary>
    /// Builds a row from <see cref="InvoiceStagingEntity"/> plus resolved FKs. Pass <paramref name="invoiceInfo"/> from the application layer (e.g. JSON of <c>OracleInvoiceRowModel</c>).
    /// </summary>
    public static InvoiceSecondaryStagingEntity FromInvoiceStaging(
        InvoiceStagingEntity staging,
        long billingServiceId,
        int currencyId,
        string jobKey,
        bool updated,
        bool paymentMismatchFlag,
        string invoiceInfo)
    {
        var invoiceRef = (staging.InvoiceNum ?? "").Trim();
        var billingRef = $"{staging.RegionCode}-{staging.DistrictCode}-{staging.CustomerNum?.Trim()}";

        var isUsd = string.Equals(staging.InvoiceCurrencyCode, "USD", StringComparison.OrdinalIgnoreCase);
        var amountTotal = isUsd
            ? (staging.AmountUsd ?? 0)
            : (staging.AmountFc ?? 0);
        var amountPaid = isUsd
            ? (staging.PaidUsd ?? 0)
            : (staging.PaidFc ?? 0);
        var amountOutstanding = amountTotal - amountPaid;
        if (amountOutstanding < 0)
            amountOutstanding = 0;

        var issueDate = staging.InvoiceDate.HasValue
            ? new DateTimeOffset(staging.InvoiceDate.Value, TimeSpan.Zero)
            : DateTimeOffset.UtcNow;
        var dueDate = issueDate.AddDays(30);

        var statusEnum = amountPaid >= amountTotal
            ? InvoiceStatus.Paid
            : amountPaid > 0
                ? InvoiceStatus.PartiallyPaid
                : InvoiceStatus.Issued;

        var status = statusEnum.ToString();

        DateTime? invoiceUpdateDate = null;
        if (staging.EffectiveInvoiceUpdateDate.HasValue)
            invoiceUpdateDate = new DateTimeOffset(staging.EffectiveInvoiceUpdateDate.Value, TimeSpan.Zero).UtcDateTime;

        var originCustomerId = staging.CustomerId?.ToString(CultureInfo.InvariantCulture);

        var now = DateTimeOffset.UtcNow;
        return new InvoiceSecondaryStagingEntity
        {
            Id = Guid.NewGuid(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            BillingServiceId = billingServiceId,
            BillingRef = billingRef.Trim(),
            OriginInvoiceId = staging.OriginInvoiceId,
            InvoiceRef = invoiceRef,
            AmountOutstanding = amountOutstanding,
            AmountPaid = amountPaid,
            AmountTotal = amountTotal,
            CurrencyId = currencyId,
            IssueDate = issueDate,
            DueDate = dueDate,
            Status = status,
            InvoiceInfo = string.IsNullOrWhiteSpace(invoiceInfo) ? null : invoiceInfo.Trim(),
            OriginCustomerId = string.IsNullOrWhiteSpace(originCustomerId) ? null : originCustomerId.Trim(),
            PaymentMismatchFlag = paymentMismatchFlag,
            InvoiceUpdateDate = invoiceUpdateDate,
            JobKey = jobKey,
            Updated = updated
        };
    }
}
