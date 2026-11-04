using SNEL.WalletPlatform.Application.Common.Models.Input;

namespace SNEL.WalletPlatform.Application.Customers.Models.Input;

public sealed class CustomerSubscriptionsQueryInputModel : BaseQueryInputModel
{
    public long? BillingServiceId { get; init; }
}