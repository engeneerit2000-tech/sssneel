using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Customers.Models.Input;
using SNEL.WalletPlatform.Application.Customers.Models.Output;

namespace SNEL.WalletPlatform.Application.Customers.Interfaces;

public interface ICustomerSubscriptionService
{
    Task<Result<PagedResult<CustomerSubscriptionOutputModel>>> GetCustomerSubscriptionsAsync(CustomerSubscriptionsQueryInputModel model);
    Task<Result<CustomerSubscriptionOutputModel>> GetCustomerSubscriptionByIdAsync(Guid id);
    Task<Result<CustomerSubscriptionOutputModel>> CreateCustomerSubscriptionAsync(CreateCustomerSubscriptionInputModel model);
    Task<Result<CustomerSubscriptionOutputModel>> UpdateCustomerSubscriptionAsync(Guid id, UpdateCustomerSubscriptionInputModel model);
    Task<Result<bool>> DeleteAsync(Guid id);
    Task<Result<CustomerSubscriptionInfoOutputModel>> GetCustomerSubscriptionInfoAsync(CreateCustomerSubscriptionInputModel model);
}
