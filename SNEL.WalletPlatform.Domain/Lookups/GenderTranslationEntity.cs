using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class GenderTranslationEntity : TranslationEntity<int>
{
    public GenderEntity Gender { get; set; } = default!;
}
