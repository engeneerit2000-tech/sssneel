using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Users;

namespace SNEL.WalletPlatform.Application.Customers.Models.Output;

public static class CustomerOutputModelFactory
{
    public static CustomerOutputModel FromCustomerAndUser(CustomerEntity customer, UserEntity user)
    {
        return new CustomerOutputModel(
            Id: customer.Id,
            UserId: user.Id,
            PartnerId: customer.PartnerId,
            Username: user.Username,
            FirstName: user.FirstName,
            SecondName: user.SecondName,
            ThirdName: user.ThirdName,
            LastName: user.LastName,
            GenderId: user.GenderId,
            MobileNo: user.MobileNo,
            NationalNo: user.NationalNo,
            Email: user.Email,
            AreaId: customer.AreaId,
            Address: customer.Address,
            Language: user.Language,
            NotificationChannels: user.NotificationPreferences.Where(x => x.Active).Select(x => (int)x.NotificationChannel).ToArray(),
            UserStatus: user.Status,
            Status: customer.Status,
            CreatedAtUtc: customer.CreatedAtUtc,
            UpdatedAtUtc: customer.UpdatedAtUtc,
            CreatedBy: customer.CreatedBy ?? string.Empty,
            UpdatedBy: customer.UpdatedBy ?? string.Empty);
    }
}
