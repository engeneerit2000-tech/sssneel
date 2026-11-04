using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Lookups;

public sealed class PartnerTypeTranslationEntity : TranslationEntity<int>
{
    public PartnerTypeEntity PartnerType { get; set; } = default!;
}
