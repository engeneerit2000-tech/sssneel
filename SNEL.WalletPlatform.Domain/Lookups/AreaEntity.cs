using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;
using SNEL.WalletPlatform.Domain.Customers;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class AreaEntity : LookupEntity<int>, ITranslatableEntity<AreaTranslationEntity, int>
{
    public ICollection<AreaTranslationEntity> Translations { get; set; } = new List<AreaTranslationEntity>();
    public int CityId { get; set; }
    
    public CityEntity City { get; set; } = null!;
    public ICollection<CustomerEntity> Customers { get; set; } = new List<CustomerEntity>();
}
