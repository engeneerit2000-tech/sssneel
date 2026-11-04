using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class PaymentInitModeTranslationEntity : TranslationEntity<int>
{
    public PaymentInitModeEntity PaymentInitMode { get; set; } = default!;
}
