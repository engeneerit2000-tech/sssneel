using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Billing;

public sealed class BillingServiceTranslationEntity : TranslationEntity<long>
{
    public BillingServiceEntity BillingService { get; set; } = default!;
}
