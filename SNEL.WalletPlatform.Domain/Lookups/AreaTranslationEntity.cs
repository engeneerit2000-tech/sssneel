using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class AreaTranslationEntity : TranslationEntity<int>
{
    public AreaEntity Area { get; set; } = default!;
}
