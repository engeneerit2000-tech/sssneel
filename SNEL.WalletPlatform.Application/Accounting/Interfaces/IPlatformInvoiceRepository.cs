using System.Threading;
using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Accounting;

namespace SNEL.WalletPlatform.Application.Accounting.Interfaces;

public interface IPlatformInvoiceRepository : IEntityRepository<PlatformInvoiceEntity, long>
{
    Task<PlatformInvoiceEntity?> GetByIdWithPrintContextAsync(long id, CancellationToken cancellationToken = default);
}
