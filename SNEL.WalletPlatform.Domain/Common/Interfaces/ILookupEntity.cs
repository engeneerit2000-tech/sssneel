namespace SNEL.WalletPlatform.Domain.Common;

public interface ILookupEntity
{
    public string Code { get; set; } 
    public string Name { get; set; } 
    public bool Active { get; set; }
    public int Order { get; set; }
}