using SNEL.WalletPlatform.Application.Invoicing;
using SNEL.WalletPlatform.Application.Invoicing.Models;
using SNEL.WalletPlatform.Infrastructure.Persistence;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

/// <summary>
/// Abstraction for pulling invoice rows (e.g. from Oracle GCO_ENCOURS_FACTURES). Allows tests to supply rows without a real DB.
/// </summary>
public interface IInvoiceRetrieval
{
    /// <summary>
    /// Fetches invoice rows for the given date range with pagination, invoking <paramref name="onPageAsync"/> for each page.
    /// Use this for large datasets to avoid loading all rows into memory. When <paramref name="takeRows"/> is set, at most that many rows are returned across all pages.
    /// </summary>
    Task GetPageAsync(
        string areaCode,
        DateTime startTs,
        DateTime endTs,
        int pageIndex,//strating from 1
        int pageSize,
        Func<IReadOnlyList<OracleInvoiceRowModel>, CancellationToken, Task> onPageAsync,
        CancellationToken cancellationToken = default);

    Task<long> GetAllAsync(
        string areaCode,
        DateTime startTs,
        DateTime endTs,
        int pageSize,
        string jobKey,
        string sourceSystem,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default);

    Task<bool> TestConnectionAsync(
        string areaCode,
        CancellationToken cancellationToken = default);
}
