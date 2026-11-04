using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class PlatformConstantTranslationEntity : TranslationEntity<int>
{
    public PlatformConstantEntity PlatformConstant { get; set; } = default!;
}
