using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class CityEntity : LookupEntity<int>, ITranslatableEntity<CityTranslationEntity, int>
{
    public ICollection<CityTranslationEntity> Translations { get; set; } = new List<CityTranslationEntity>();
    public int RegionId { get; set; }
    public RegionEntity Region { get; set; } = null!;
    public ICollection<AreaEntity> Areas { get; set; } = new List<AreaEntity>();
}
