using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Accounting;

namespace SNEL.WalletPlatform.Application.Accounting.Interfaces;

public interface IJournalRepository : IEntityRepository<JournalEntity,Guid>
{
    Task<bool> ExistsBySourceAsync(int sourceType, string sourceRef, CancellationToken cancellationToken = default);

}

