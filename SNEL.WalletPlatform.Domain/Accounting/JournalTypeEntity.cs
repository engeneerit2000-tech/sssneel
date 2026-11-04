using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;

namespace SNEL.WalletPlatform.Domain.Accounting;

public sealed class JournalTypeEntity : LookupEntity<int>, ITranslatableEntity<JournalTypeTranslationEntity, int>
{
    public ICollection<JournalTypeTranslationEntity> Translations { get; set; }
}

