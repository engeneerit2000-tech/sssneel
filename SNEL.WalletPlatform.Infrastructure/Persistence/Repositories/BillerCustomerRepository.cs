using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Billers.Interfaces;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Infrastructure.Persistence;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class BillerCustomerRepository(ApplicationDbContext context) : IBillerCustomerRepository
{
    public Task<bool> ExistsByBillerIdAndNumberAsync(long billerId, string areaCode, string number, CancellationToken cancellationToken = default)
    {
        var normalized = number.Trim();
        if(!string.IsNullOrEmpty(areaCode))
            normalized = $"{areaCode}-{normalized}";
        return context.BillerCustomers.AsNoTracking()
            .AnyAsync(bc => bc.BillerId == billerId && bc.Number == normalized, cancellationToken);
    }

    public Task<BillerCustomerEntity?> GetByBillerIdAndNumberAsync(long billerId, string normalizedNumber, CancellationToken cancellationToken = default)
    {
        return context.BillerCustomers.AsNoTracking()
            .FirstOrDefaultAsync(bc => bc.BillerId == billerId && bc.Number == normalizedNumber, cancellationToken);
    }
}
