using SNEL.WalletPlatform.Application.Customers.Models.Output;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Users;

namespace SNEL.WalletPlatform.Application.Customers.Mapper;

internal static class CustomerSubscriptionOutputMapping
{
    public static async Task EnrichCustomerFromSubscriptionsAsync(
        IReadOnlyList<CustomerSubscriptionOutputModel> outputs,
        IReadOnlyList<CustomerSubscriptionEntity> sources,
        IUnitOfWork unitOfWork)
    {
        if (outputs.Count != sources.Count)
        {
            throw new ArgumentException("outputs and sources must have the same length.", nameof(sources));
        }

        var uniquePartnerIds = new HashSet<long>();
        for (int i = 0; i < sources.Count; i++)
        {
            if (sources[i].Customer is not null)
            {
                uniquePartnerIds.Add(sources[i].Customer!.PartnerId);
            }
        }

        if (uniquePartnerIds.Count == 0)
        {
            return;
        }

        var users = await unitOfWork.UserRepository
            .GetByPartnerIdsWithNotificationsDependenciesAsync([.. uniquePartnerIds]);
        var userByPartnerId = users
            .Where(u => u.PartnerId.HasValue)
            .GroupBy(u => u.PartnerId!.Value)
            .ToDictionary(g => g.Key, g => g.First());

        for (int i = 0; i < outputs.Count; i++)
        {
            var customer = sources[i].Customer;
            if (customer is null)
            {
                continue;
            }

            if (userByPartnerId.TryGetValue(customer.PartnerId, out UserEntity? user))
            {
                outputs[i].Customer = CustomerOutputModelFactory.FromCustomerAndUser(customer, user);
            }
        }
    }
}
