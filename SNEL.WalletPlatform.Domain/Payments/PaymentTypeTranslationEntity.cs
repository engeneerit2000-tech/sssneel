using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Payments;

public sealed class PaymentTypeTranslationEntity : TranslationEntity<int>
{
    public PaymentTypeEntity PaymentType { get; set; } = default!;
}
