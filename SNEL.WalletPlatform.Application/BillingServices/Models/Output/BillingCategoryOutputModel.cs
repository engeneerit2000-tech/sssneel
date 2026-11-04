namespace SNEL.WalletPlatform.Application.BillingServices.Models.Output;

public sealed class BillingCategoryOutputModel
{
    public BillingCategoryOutputModel() { }

    public BillingCategoryOutputModel(
        int id,
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

    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}