using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class CountryEntity : LookupEntity<int>, ITranslatableEntity<CountryTranslationEntity, int>
{
    public ICollection<CountryTranslationEntity> Translations { get; set; } = new List<CountryTranslationEntity>();
    public ICollection<RegionEntity> Regions { get; set; } = new List<RegionEntity>();
}
