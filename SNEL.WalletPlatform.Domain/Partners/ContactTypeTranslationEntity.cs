using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Partners;

public sealed class ContactTypeTranslationEntity : TranslationEntity<int>
{
    public ContactTypeEntity ContactType { get; set; } = default!;
}
