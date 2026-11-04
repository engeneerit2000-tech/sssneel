using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Partners;

public sealed class PartnerContactEntity : TrackedEntity<long>
{
    private PartnerContactEntity() { }

    public long PartnerId { get; private set; }
    public int ContactTypeId { get; private set; }
    public string ContactInfo { get; private set; } = string.Empty;

    public PartnerEntity Partner { get; private set; } = null!;
    public ContactTypeEntity ContactType { get; private set; } = null!;

    public static PartnerContactEntity Create(long partnerId, int contactTypeId, string contactInfo)
    {
        return new PartnerContactEntity
        {
            PartnerId = partnerId,
            ContactTypeId = contactTypeId,
            ContactInfo = contactInfo.Trim()
        };
    }
}
