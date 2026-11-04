using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Accounting.Interfaces;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Infrastructure.Persistence;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class PlatformInvoiceRepository(ApplicationDbContext context)
    : EntityRepository<PlatformInvoiceEntity, long>(context), IPlatformInvoiceRepository
{
    public Task<PlatformInvoiceEntity?> GetByIdWithPrintContextAsync(long id, CancellationToken cancellationToken = default)
    {
        return context.PlatformInvoices
            .AsNoTracking()
            .Include(x => x.Currency)
            .Include(x => x.Partner)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
