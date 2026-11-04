using SNEL.WalletPlatform.Domain.Common.Base;
using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class CenterEntity : LookupEntity<int>, ITranslatableEntity<CenterTranslationEntity, int>
{
    public ICollection<CenterTranslationEntity> Translations { get; set; } = new List<CenterTranslationEntity>();

}
