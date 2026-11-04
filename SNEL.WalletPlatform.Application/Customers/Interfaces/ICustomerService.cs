using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Customers.Models.Input;
using SNEL.WalletPlatform.Application.Customers.Models.Output;

namespace SNEL.WalletPlatform.Application.Customers.Interfaces;

public interface ICustomerService
{
    Task<Result<PagedResult<CustomerOutputModel>>> GetCustomersAsync(CustomersQueryInputModel model);
    Task<Result<CustomerOutputModel>> GetMyProfileAsync();
    Task<Result<CustomerOutputModel>> UpdateMyProfileAsync(UpdateMyCustomerProfileInputModel model);
    Task<Result<CustomerOutputModel>> GetCustomerByIdAsync(Guid id);
    Task<Result<CustomerOutputModel>> CreateCustomerAsync(CreateCustomerInputModel model);
    Task<Result<CustomerOutputModel>> UpdateCustomerAsync(Guid id, UpdateCustomerInputModel model);
    Task<Result<bool>> UpdateCustomerStatusAsync(Guid id, UpdateCustomerStatusInputModel model);
    Task<Result<bool>> DeleteCustomerAsync(Guid id, CancellationToken cancellationToken = default);
}
