namespace SNEL.WalletPlatform.Domain.Common;

public interface IBaseEntity<TKey>
     where TKey:notnull
{
    public TKey Id { get; set; }

}