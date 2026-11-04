using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.BillingServices.Models.Input;

public class CreateBillingServiceInputModel
{
    [Required]
    public int BillingCategoryId { get; init; }

    [Required]
    public long BillerId { get; init; }

    public bool IsActive { get; init; } = true;

    [Required, MaxLength(50)]
    public string Code { get; init; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [Required]
    public int ServiceTypeId { get; init; }

    public bool SupportPartial { get; init; }
    public bool SupportInquiry { get; init; }
    public bool SupportCustomActions { get; init; }
    public bool IsTransactional { get; init; }

    [MaxLength(200)]
    public string? BillingRefFormat { get; init; }

    [Required, MinLength(1)]
    public List<BillingServiceCurrencyInputModel> Currencies { get; init; } = [];

    public List<BillerConfigInputModel> BillerConfigs { get; init; } = [];
}

public sealed class BillingServiceCurrencyInputModel
{
    [Required]
    public int CurrencyId { get; init; }

    public decimal Min { get; init; }
    public decimal Max { get; init; }
    public string CommissionType { get; init; }
    public decimal CommissionValue { get; init; }
    public string FeesType { get; init; }
    public decimal FeesValue { get; init; }
    public string CostType { get; init; }
    public decimal CostValue { get; init; }
    public decimal VAT { get; init; }
    public bool AutoSettleJournal { get; init; }
    public Guid BankAccountId { get; init; }
    public int Scale { get; init; }
    public bool Active { get; init; } = true;
    public int ApplyType { get; init; } = 3;
}

public sealed class BillerConfigInputModel
{
    public long? Id { get; init; }

    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string BillerName { get; init; } = string.Empty;

    [Required, MaxLength(500)]
    public string Value { get; init; } = string.Empty;
}
