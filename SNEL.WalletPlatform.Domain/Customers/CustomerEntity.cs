using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Partners;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Domain.Customers;

public sealed class CustomerEntity : TrackedEntity<Guid>
{
    private CustomerEntity() { }

    private CustomerEntity(
        PartnerEntity partner,
        int? areaId,
        string? address)
    {
        Partner = partner;
        AreaId = areaId;
        Address = address;
        Status = CustomerStatus.Active;
    }

    public long PartnerId { get; private set; }
    public PartnerEntity? Partner { get; private set; }
    public int? AreaId { get; private set; }
    public AreaEntity? Area { get; private set; }
    public string? Address { get; private set; }
    public ICollection<CustomerRegistrationHistoryEntity> RegistrationHistories { get; private set; }
        = new List<CustomerRegistrationHistoryEntity>();
    public ICollection<CustomerSubscriptionEntity> Subscriptions { get; private set; }
        = new List<CustomerSubscriptionEntity>();
    public ICollection<PaymentEntity> Payments { get; set; } = new List<PaymentEntity>();
    public CustomerStatus Status { get; private set; } = CustomerStatus.Active;

    public static CustomerEntity Create(
        PartnerEntity partner,
        int? areaId = null,
        string? address = null)
    {

        return new CustomerEntity(
            partner,
            areaId,
            string.IsNullOrWhiteSpace(address) ? null : address.Trim());
    }

    public void UpdateProfile(int? areaId, string? address)
    {
        AreaId = areaId;
        Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
    }

    public void ChangeStatus(CustomerStatus status)
    {
        Status = status;
    }
}
