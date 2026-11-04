using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class GenderEntity : LookupEntity<int>, ITranslatableEntity<GenderTranslationEntity, int>
{
    public ICollection<GenderTranslationEntity> Translations { get; set; } = new List<GenderTranslationEntity>();
}
