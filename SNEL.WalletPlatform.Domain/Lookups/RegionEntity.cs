using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class RegionEntity : LookupEntity<int>, ITranslatableEntity<RegionTranslationEntity, int>
{
    public ICollection<RegionTranslationEntity> Translations { get; set; } = new List<RegionTranslationEntity>();
    public int CountryId { get; set; }
    public CountryEntity Country { get; set; } = null!;
    public ICollection<CityEntity> Cities { get; set; } = new List<CityEntity>();
}
