using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class CurrencyTranslationEntity : TranslationEntity<int>
{
    public CurrencyEntity Currency { get; set; } = default!;
}
