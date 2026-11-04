using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;

namespace SNEL.WalletPlatform.Domain.Accounting;

public sealed class JournalLineTypeEntity : LookupEntity<int>,ITranslatableEntity<JournalLineTypeTranslationEntity,int>
{
    public ICollection<JournalLineTypeTranslationEntity> Translations { get; set; }
}

