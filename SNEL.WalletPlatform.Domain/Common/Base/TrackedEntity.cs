namespace SNEL.WalletPlatform.Domain.Common;

public abstract class TrackedEntity<TKey>: BaseEntity<TKey> ,ITrackedEntity 
    where TKey:notnull
{
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
