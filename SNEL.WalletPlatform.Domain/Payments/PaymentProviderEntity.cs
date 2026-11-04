using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Partners;

namespace SNEL.WalletPlatform.Domain.Payments;

public sealed class PaymentProviderEntity : TrackedEntity<long>
{
    private PaymentProviderEntity(
        long partnerId,
        string code,
        string name)
    {
        PartnerId = partnerId;
        Code = code.Trim().ToUpper();
        Name = name.Trim();
        PaymentMethods = new List<PaymentMethodEntity>();
    }

    public long PartnerId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }

    public PartnerEntity Partner { get; private set; } = null!;
    public ICollection<PaymentMethodEntity> PaymentMethods { get; private set; }

    public static PaymentProviderEntity Create(
        long partnerId,
        string code,
        string name)
    {
        if (partnerId == 0)
            throw new ArgumentException("PartnerId cannot be empty", nameof(partnerId));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        return new PaymentProviderEntity(partnerId, code, name);
    }

    public static PaymentProviderEntity Create(
        PartnerEntity partner,
        string code,
        string name)
    {
        if (partner is null)
            throw new ArgumentNullException(nameof(partner));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        return new PaymentProviderEntity(partner.Id, code, name)
        {
            Partner = partner
        };
    }

    public void Update(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Code = code.Trim().ToUpper();
        Name = name.Trim();
    }
}
