using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class CompletionModeTranslationEntity : TranslationEntity<int>
{
    public CompletionModeEntity CompletionMode { get; set; } = default!;
}
