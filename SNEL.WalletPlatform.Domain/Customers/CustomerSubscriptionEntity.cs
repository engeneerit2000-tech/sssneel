using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Customers;

public sealed class CustomerSubscriptionEntity : TrackedEntity<Guid>
{
    private CustomerSubscriptionEntity() { }

    public Guid CustomerId { get; private set; }
    public CustomerEntity Customer { get; private set; } = null!;
    public long BillingServiceId { get; private set; }
    public BillingServiceEntity BillingService { get; private set; } = null!;
    public string CustomerRef { get; private set; } = string.Empty;
    public string BillingRefValue { get; private set; } = string.Empty;
    public string NickName { get; private set; } = string.Empty;

    public static CustomerSubscriptionEntity Create(
        Guid customerId,
        long billingServiceId,
        string customerRef,
        string billingRefValue,
        string nickName)
    {
        return new CustomerSubscriptionEntity
        {
            CustomerId = customerId,
            BillingServiceId = billingServiceId,
            CustomerRef = customerRef.Trim(),
            BillingRefValue = billingRefValue.Trim(),
            NickName = nickName.Trim()
        };
    }

    public void Update(
        long billingServiceId,
        string customerRef,
        string billingRefValue,
        string nickName)
    {
        BillingServiceId = billingServiceId;
        CustomerRef = customerRef.Trim();
        BillingRefValue = billingRefValue.Trim();
        NickName = nickName.Trim();
    }
}
