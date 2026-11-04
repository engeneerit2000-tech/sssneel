using SNEL.WalletPlatform.Application.Common.Models.Input;
using SNEL.WalletPlatform.Domain.Customers;

namespace SNEL.WalletPlatform.Application.Customers.Models.Input;

public sealed class CustomersQueryInputModel : BaseQueryInputModel
{
    public CustomerStatus? Status { get; set; }
}
