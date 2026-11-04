using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Accounting;

public class JournalLineTypeTranslationEntity:TranslationEntity<int>
{
    
        public JournalLineTypeEntity JournalLineType { get; set; } = default!;

}