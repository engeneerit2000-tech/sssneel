using SNEL.WalletPlatform.Application.Accounting.Models.Input;
using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Accounting;

namespace SNEL.WalletPlatform.Application.Accounting.Interfaces;

public interface IJournalLineRepository : IEntityRepository<JournalLineEntity, Guid>
{
    Task<int> CountAsync(JournalLinesQueryInputModel normalizedQuery);
    Task<int> CountAsync(JournalLinesStatementInternalModel normalizedQuery);
    Task<IReadOnlyCollection<JournalLineEntity>> GetPagedAsync(JournalLinesQueryInputModel normalizedQuery);
    Task<IReadOnlyCollection<JournalLineEntity>> GetJournalLineStatementAsync(JournalLinesStatementInternalModel normalizedQuery);
}

