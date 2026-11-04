using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Payments;

public sealed class PaymentCategoryTranslationEntity : TranslationEntity<int>
{
    public PaymentCategoryEntity PaymentCategory { get; set; } = default!;
}
