using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;

namespace SNEL.WalletPlatform.Domain.Accounting;

public sealed class AccountTypeEntity : LookupEntity<int>, ITranslatableEntity<AccountTypeTranslationEntity,int>
{
    public ICollection<AccountTypeTranslationEntity> Translations { get; set; }
}

