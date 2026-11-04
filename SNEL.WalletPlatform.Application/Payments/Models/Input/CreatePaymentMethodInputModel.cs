using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public class CreatePaymentMethodInputModel
{
    [Required]
    public int PaymentCategoryId { get; init; }

    [Required]
    public long PaymentProviderId { get; init; }

    [Required, MaxLength(50)]
    public string Code { get; init; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [Required]
    public int PaymentInitMode { get; init; }

    [Required]
    public int ConfirmationMode { get; init; }

    [Required]
    public int CompletionMode { get; init; }

    [Required]
    public int PaymentMethodPurpose { get; init; }

    [Required]
    public int ConfirmationType { get; init; }

    public bool Active { get; init; } = true;

    /// <summary>
    /// Payment method inline JSON configuration. It will be persisted as JSONB.
    /// Known shapes:
    /// - Interswitch: {"fields": {...}}
    /// - SnelWallet: {"confirmation":{"message":{"en":"...","fr":"..."}}}
    /// </summary>
    public PaymentMethodConfigInputModel? Config { get; init; }

    [Required, MinLength(1)]
    public List<PaymentMethodCurrencyInputModel> Currencies { get; init; } = [];
}

public sealed class UpdatePaymentMethodInputModel : CreatePaymentMethodInputModel
{
}

public sealed class PaymentMethodCurrencyInputModel
{
    [Required]
    public int CurrencyId { get; init; }

    public decimal Min { get; init; }
    public decimal Max { get; init; }

    [Required]
    public string FeesType { get; init; }
    public decimal FeesValue { get; init; }

    [Required]
    public string CostType { get; init; }
    public decimal CostValue { get; init; }

    public bool AutoSettleJournal { get; init; }
    public Guid? BankAccountId { get; init; }
    public decimal VAT { get; init; }
    public int Scale { get; init; }
    public bool Active { get; init; } = true;
    public int? MinimumAgeMinutes { get; init; } = null;
}
