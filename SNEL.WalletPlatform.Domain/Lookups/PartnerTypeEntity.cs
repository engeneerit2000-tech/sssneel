using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;
using SNEL.WalletPlatform.Domain.Partners;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class PartnerTypeEntity : LookupEntity<int>, ITranslatableEntity<PartnerTypeTranslationEntity, int>
{
    public ICollection<PartnerEntity> Partners { get; set; } = new List<PartnerEntity>();
    public ICollection<PartnerTypeTranslationEntity> Translations { get; set; } =
        new List<PartnerTypeTranslationEntity>();
}
