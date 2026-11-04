using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Billing;

/// <summary>
/// Staging for raw rows from the biller customer pull (GCO_RACCORDEMENTS).
/// </summary>
[PrimaryKey(nameof(Id))]
[Table("BillerCustomerStagings")]
[Index(nameof(BillerId))]
[Index(nameof(JobKey))]
public sealed class BillerCustomerStagingEntity : BaseEntity<Guid>
{
    /// <summary>Quartz job key for this pull/merge run (same idea as <c>InvoiceStagingEntity.JobKey</c>).</summary>
    [Required]
    [MaxLength(128)]
    public string JobKey { get; set; } = string.Empty;

    public long BillerId { get; set; }

    [ForeignKey(nameof(BillerId))]
    public BillerEntity Biller { get; set; } = null!;

    /// <summary>Biller customer id (e.g. IDE_RACCORDEMENT).</summary>
    public long CustomerId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Number { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Set during merge: true when a biller customer row already existed for this biller and customer id; false when the merge inserts a new row.
    /// </summary>
    [Required]
    public bool Updated { get; set; }
}
