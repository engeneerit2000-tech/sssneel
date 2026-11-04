using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class PaymentMethodPurposeTranslationEntity : TranslationEntity<int>
{
    public PaymentMethodPurposeEntity PaymentMethodPurpose { get; set; } = default!;
}
