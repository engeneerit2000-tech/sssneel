using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class ConfirmationTypeTranslationEntity : TranslationEntity<int>
{
    public ConfirmationTypeEntity ConfirmationType { get; set; } = default!;
}
