using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Partners;

namespace SNEL.WalletPlatform.Domain.Billing;

public sealed class BillerEntity : TrackedEntity<long>
{
    private BillerEntity() {}

    public long PartnerId { get; private set; }
    public PartnerEntity Partner { get; private set; } = null!;
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;

    public bool ApplyAfterPay { get; set; }

    public ICollection<BillingServiceEntity> BillingServices { get; private set; } = new List<BillingServiceEntity>();

    public static BillerEntity Create(
        long partnerId,
        string code,
        string name)
    {
        return new BillerEntity
        {
            PartnerId = partnerId,
            Code = code.Trim(),
            Name = name.Trim()
        };
    }

    /// <summary>Creates a biller linked to a partner by reference; PartnerId is set by EF on save.</summary>
    public static BillerEntity Create(
        PartnerEntity partner,
        string code,
        string name)
    {
        return new BillerEntity
        {
            Partner = partner,
            Code = code.Trim(),
            Name = name.Trim()
        };
    }

    public void Update(string code, string name, bool applyAfterPay)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Code = code.Trim();
        Name = name.Trim();
        ApplyAfterPay = applyAfterPay;
    }
}
