using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class CountryTranslationEntity : TranslationEntity<int>
{
    public CountryEntity Country { get; set; } = default!;
}
