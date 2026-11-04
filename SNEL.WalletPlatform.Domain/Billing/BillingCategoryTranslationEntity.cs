using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Billing;

public sealed class BillingCategoryTranslationEntity : TranslationEntity<int>
{
    public BillingCategoryEntity BillingCategory { get; set; } = default!;
}
