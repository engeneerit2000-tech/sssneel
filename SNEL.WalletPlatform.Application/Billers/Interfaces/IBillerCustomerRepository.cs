using SNEL.WalletPlatform.Domain.Billing;

namespace SNEL.WalletPlatform.Application.Billers.Interfaces;

public interface IBillerCustomerRepository
{
    Task<bool> ExistsByBillerIdAndNumberAsync(long billerId, string areaCode, string number, CancellationToken cancellationToken = default);

    Task<BillerCustomerEntity> GetByBillerIdAndNumberAsync(long billerId, string normalizedNumber, CancellationToken cancellationToken = default);    
}
