using SNEL.WalletPlatform.Application.Common.Models.Input;
using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public sealed class TopupRateListInputModel : BaseQueryInputModel
{
    [Required]
    [Range(1, 2, ErrorMessage = "CurrencyId must be either 1 or 2")]
    public int CurrencyId { get; init; }
    public long? CustomerPartnerId { get; init; } = null;
    public long? PaymentProviderId { get; set; } = null;
    public long? PaymentMethodId { get; init; } = null;


}

public sealed class TopupRateExportInputModel
{
    [Required]
    [Range(1, 2, ErrorMessage = "CurrencyId must be either 1 or 2")]
    public int CurrencyId { get; init; }
    public long? CustomerPartnerId { get; init; } = null;
    public long? PaymentProviderId { get; set; } = null;
    public long? PaymentMethodId { get; init; } = null;


}
