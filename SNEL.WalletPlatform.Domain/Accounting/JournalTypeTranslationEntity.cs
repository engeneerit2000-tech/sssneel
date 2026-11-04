using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Accounting;

public class JournalTypeTranslationEntity:TranslationEntity<int>
{
    
        public JournalTypeEntity JournalType { get; set; } = default!;

}