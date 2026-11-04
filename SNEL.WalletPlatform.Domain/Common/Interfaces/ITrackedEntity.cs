namespace SNEL.WalletPlatform.Domain.Common;

public interface ITrackedEntity
{
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public string? CreatedBy { get; set; } 
    public string? UpdatedBy { get; set; }
}