namespace SNEL.WalletPlatform.Domain.Common;

public abstract class TranslationEntity<TEntityKey>: BaseEntity<int>, ITranslationEntity<TEntityKey> 
{
    public TEntityKey EntityId { get; set; }
    public string LanguageCode { get; set; }
    public string Name { get; set; } = string.Empty;

}
