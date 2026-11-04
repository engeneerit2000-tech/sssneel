using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Domain.Partners;

public sealed class PartnerEntity : TrackedEntity<long>
{
    private PartnerEntity() { }

    public int PartnerTypeId { get; private set; }
    public PartnerTypeEntity PartnerType { get; private set; } = null!;
    public string LegalName { get; private set; } = string.Empty;
    public string? DisplayName { get; private set; }
    public bool Active { get; private set; }


    public CustomerEntity? Customer { get; private set; }
    public ICollection<Billing.BillerEntity> Billers { get; private set; } = new List<Billing.BillerEntity>();
    public ICollection<PartnerContactEntity> Contacts { get; private set; } = new List<PartnerContactEntity>();

    public static PartnerEntity Create(
        int partnerTypeId,
        string legalName,
        string? displayName,
        bool active)
    {
        return new PartnerEntity
        {
            PartnerTypeId = partnerTypeId,
            LegalName = legalName.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim(),
            Active = active
        };
    }

    public void Deactivate()
    {
        Active = false;
    }

    public void Activate()
    {
        Active = true;
    }

    public void UpdateProfile(string legalName, string? displayName, bool active)
    {
        if (string.IsNullOrWhiteSpace(legalName))
            throw new ArgumentException("LegalName cannot be empty", nameof(legalName));

        LegalName = legalName.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        Active = active;
    }
}
