using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class ChannelTranslationEntity : TranslationEntity<int>
{
    public ChannelEntity Channel { get; set; } = default!;
}
