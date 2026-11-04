namespace SNEL.WalletPlatform.Domain.Common;

public interface ITranslatableEntity<TTranslationEntity,TEntityKey>
    where TTranslationEntity : ITranslationEntity<TEntityKey> 
    where TEntityKey:notnull 
{
    ICollection<TTranslationEntity> Translations { get; }
}