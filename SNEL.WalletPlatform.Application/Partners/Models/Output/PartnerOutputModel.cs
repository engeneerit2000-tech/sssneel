using SNEL.WalletPlatform.Application.Partners.Models.Output;

namespace SNEL.WalletPlatform.Application.Partners.Models.Output;

public sealed class PartnerOutputModel
{
    public PartnerOutputModel() { }    
    public PartnerOutputModel(
        long id,
        int partnerTypeId,
        PartnerTypeOutputModel? partnerType,
        string legalName,
        string displayName,
        bool active,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        string? createdBy,
        string? updatedBy)
    {
        Id = id;
        PartnerTypeId = partnerTypeId;
        PartnerType = partnerType;
        LegalName = legalName;
        Active = active;
        DisplayName = displayName;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        CreatedBy = createdBy;
        UpdatedBy = updatedBy;
    }
    public long Id { get; set; }
    public int PartnerTypeId { get; private set; }
    public PartnerTypeOutputModel PartnerType { get; private set; } = null!;
    public string LegalName { get; private set; } = string.Empty;
    public string? DisplayName { get; private set; }
    public bool Active { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

}
