using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Accounting.Interfaces;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class JournalRepository(ApplicationDbContext context)
    : EntityRepository<JournalEntity,Guid>(context), IJournalRepository
{
    public Task<bool> ExistsBySourceAsync(int sourceType, string sourceRef, CancellationToken cancellationToken = default)
    {
        return context.Journals.AnyAsync(x => x.SourceTypeId == sourceType
            && x.SourceRef == sourceRef, cancellationToken);
    }
}

