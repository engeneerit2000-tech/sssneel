using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class RegionTranslationEntity : TranslationEntity<int>
{
    public RegionEntity Region { get; set; } = default!;
}
