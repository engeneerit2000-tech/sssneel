using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.Accounting.Models.Input;
using SNEL.WalletPlatform.Application.Accounting.Models.Output;
using SNEL.WalletPlatform.Application.Common;

namespace SNEL.WalletPlatform.Application.Accounting.Interfaces;

public interface IJournalLineService
{
    Task<Result<PagedResult<JournalLineOutputModel>>> GetJournalLinesAsync(
        JournalLinesQueryInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<PagedResultWithSummary<JournalLineStatementOutputModel, JournalLineStatementSummary>>> GetJournalLineStatementAsync(JournalLinesStatementInputModel model);
    Task<Result<FileContentResult>> ExportJournalLineStatementAsync(JournalLinesStatementInputModel model);
}
