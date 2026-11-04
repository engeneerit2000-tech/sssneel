using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class BillingServiceTypeTranslationEntity : TranslationEntity<int>
{
    public BillingServiceTypeEntity BillingServiceType { get; set; } = default!;
}
