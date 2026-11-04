namespace SNEL.WalletPlatform.Application.BillingServices.Models.Output;

public sealed class BillingServiceTypeOutputModel
{
    public BillingServiceTypeOutputModel() { }

    public BillingServiceTypeOutputModel(
        long id,
        string code,
        string name,
        bool isActive,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        string createdBy,
        string updatedBy)
    {
        Id = id;
        Code = code;
        Name = name;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        CreatedBy = createdBy;
        UpdatedBy = updatedBy;
    }

    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}