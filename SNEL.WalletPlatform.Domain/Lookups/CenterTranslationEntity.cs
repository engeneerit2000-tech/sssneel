using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class CenterTranslationEntity : TranslationEntity<int>
{
    public CenterEntity Center { get; set; } = default!;

}
