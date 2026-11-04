using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Billing;

/// <summary>
/// Biller-side customer (raccordement) synced from the biller system. <see cref="CustomerId"/> is the biller-system
/// identifier (e.g. Oracle IDE_RACCORDEMENT), not the wallet customer user id.
/// </summary>
[PrimaryKey(nameof(Id))]
[Table("BillerCustomers")]
[Index(nameof(BillerId), nameof(CustomerId), nameof(Number), IsUnique = true)]
public sealed class BillerCustomerEntity : BaseEntity<Guid>
{
    public long BillerId { get; set; }

    [ForeignKey(nameof(BillerId))]
    public BillerEntity Biller { get; set; } = null!;

    /// <summary>Biller customer id (e.g. GCO_RACCORDEMENTS.IDE_RACCORDEMENT).</summary>
    public long CustomerId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Number { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;
}
