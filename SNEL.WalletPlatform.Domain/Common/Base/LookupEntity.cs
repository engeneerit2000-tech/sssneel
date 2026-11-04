namespace SNEL.WalletPlatform.Domain.Common.Base;

public abstract class LookupEntity<TKey>: BaseEntity<TKey>, ILookupEntity 
    where TKey:notnull
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; }= true;
    public int Order { get; set; }

}
