using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Accounting;

public sealed class AccountTypeTranslationEntity : TranslationEntity<int>
{
    public AccountTypeEntity AccountType { get; set; } = default!;
}

