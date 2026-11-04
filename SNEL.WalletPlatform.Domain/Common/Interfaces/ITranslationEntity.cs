namespace SNEL.WalletPlatform.Domain.Common;

public interface ITranslationEntity<TEntityKey>:IBaseEntity<int>
     where TEntityKey:notnull 
{
    public TEntityKey EntityId { get; set; }
    public string LanguageCode { get; set; } 
    public string Name { get; set; } 
}