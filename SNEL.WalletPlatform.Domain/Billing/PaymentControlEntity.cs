using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Domain.Billing;

/// <summary>
/// Platform-owned control row for bill (Pay_BILL) payments, keyed by bill identity
/// (<see cref="BillerCode"/>, <see cref="InvoiceReference"/>) as returned from inquiry.
/// Amounts mirror the <see cref="InvoiceEntity"/> at payment initiation; <see cref="ReservedAmount"/>
/// tracks platform funds in flight until apply completes.
/// </summary>
[PrimaryKey(nameof(Id))]
[Table("PaymentControls")]
[Index(nameof(BillingServiceId),nameof(BillingRef),nameof(BillRef), IsUnique = true)]
public sealed class PaymentControlEntity : TrackedEntity<Guid>
{
    [Required]

    public long BillingServiceId { get; set; }

    [Required]
    [MaxLength(150)]
    public string BillingRef { get; set; } = string.Empty;

    public string BillRef { get; set; }

    [Required]
    public int CurrencyId { get; set; }

    [Required]
    [Precision(18, 6)]
    public decimal TotalAmount { get; set; }

    [Required]
    [Precision(18, 6)]
    public decimal PaidAmount { get; set; }

    [Required]
    [Precision(18, 6)]
    public decimal ReservedAmount { get; set; }
    
    [Required]
    public int PaymentControlTypeId { get; set; }

    /// <summary>Maps to PostgreSQL <c>xmin</c> (optimistic concurrency); updated by the database.</summary>
    [ConcurrencyCheck]
    public int Version { get; set; }

    [ForeignKey(nameof(BillingServiceId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public BillingServiceEntity BillingService { get; set; } = null!;

    [ForeignKey(nameof(CurrencyId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public CurrencyEntity Currency { get; set; } = null!;

    [NotMapped]
    public decimal OutstandingAmount => TotalAmount - PaidAmount - ReservedAmount;


    public static PaymentControlEntity Create(
        long billingServiceId,
        string billingRef,
        string billRef,
        int currencyId,
        decimal totalAmount,
        decimal paidAmount,
        decimal reservedAmount,
        PaymentControlTypeEnum controlType)
    {
        if (reservedAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(reservedAmount));

        return new PaymentControlEntity
        {
            Id = Guid.NewGuid(),
            BillingServiceId = billingServiceId,
            BillingRef = billingRef.Trim(),
            BillRef = billRef.Trim(),
            CurrencyId = currencyId,
            TotalAmount = totalAmount,
            PaidAmount = paidAmount,
            ReservedAmount = reservedAmount,
            PaymentControlTypeId = (int)controlType
        };
    }
    public void RefreshAmountsFromInvoice(decimal totalAmount, decimal paidAmount)
    {
        TotalAmount = totalAmount;
        // Keep platform-tracked paid amount if source invoice is behind.
        PaidAmount = Math.Max(PaidAmount, paidAmount);
    }

    public void ReserveAmount(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        ReservedAmount += amount;
    }

    public void ReleaseAmount(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        ReservedAmount = ReservedAmount - amount;
        PaidAmount = PaidAmount + amount;
    }

    public void CancelReservedAmount(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        ReservedAmount = ReservedAmount - amount;
    }
}
