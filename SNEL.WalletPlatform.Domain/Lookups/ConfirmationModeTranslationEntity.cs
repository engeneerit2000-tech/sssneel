using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class ConfirmationModeTranslationEntity : TranslationEntity<int>
{
    public ConfirmationModeEntity ConfirmationMode { get; set; } = default!;
}
