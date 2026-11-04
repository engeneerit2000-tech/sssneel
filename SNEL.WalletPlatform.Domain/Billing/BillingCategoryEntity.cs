using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;

namespace SNEL.WalletPlatform.Domain.Billing;

public sealed class BillingCategoryEntity : LookupEntity<int>, ITranslatableEntity<BillingCategoryTranslationEntity, int>
{
    public ICollection<BillingServiceEntity> BillingServices { get; set; } = new List<BillingServiceEntity>();
    public ICollection<BillingCategoryTranslationEntity> Translations { get; set; } =
        new List<BillingCategoryTranslationEntity>();
}
