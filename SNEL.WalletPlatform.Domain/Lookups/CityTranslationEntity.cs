using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class CityTranslationEntity : TranslationEntity<int>
{
    public CityEntity City { get; set; } = default!;
}
