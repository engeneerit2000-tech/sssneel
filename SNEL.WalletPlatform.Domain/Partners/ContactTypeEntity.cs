using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;

namespace SNEL.WalletPlatform.Domain.Partners;

public sealed class ContactTypeEntity : LookupEntity<int>, ITranslatableEntity<ContactTypeTranslationEntity, int>
{
    public ICollection<PartnerContactEntity> PartnerContacts { get; set; } = new List<PartnerContactEntity>();
    public ICollection<ContactTypeTranslationEntity> Translations { get; set; } =
        new List<ContactTypeTranslationEntity>();
}
