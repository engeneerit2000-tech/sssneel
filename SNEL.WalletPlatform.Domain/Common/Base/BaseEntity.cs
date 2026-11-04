namespace SNEL.WalletPlatform.Domain.Common;

public abstract class BaseEntity<TKey> :IBaseEntity<TKey>
    where TKey:notnull
{
    public TKey Id { get; set; }=default!;
}
