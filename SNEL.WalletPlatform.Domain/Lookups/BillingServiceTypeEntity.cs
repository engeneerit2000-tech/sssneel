using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class BillingServiceTypeEntity : LookupEntity<int>,
    ITranslatableEntity<BillingServiceTypeTranslationEntity, int>
{
    public ICollection<BillingServiceEntity> BillingServices { get; set; } =
        new List<BillingServiceEntity>();
    public ICollection<BillingServiceTypeTranslationEntity> Translations { get; set; } =
        new List<BillingServiceTypeTranslationEntity>();
}
