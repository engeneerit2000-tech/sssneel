using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;
using SNEL.WalletPlatform.Domain.Customers;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class PlatformConstantEntity : LookupEntity<int>, ITranslatableEntity<PlatformConstantTranslationEntity, int>
{
    public ICollection<PlatformConstantTranslationEntity> Translations { get; set; } = new List<PlatformConstantTranslationEntity>();
    public required string Group { get; set; } = default;
    public required string Value { get; set; } = default;
}
