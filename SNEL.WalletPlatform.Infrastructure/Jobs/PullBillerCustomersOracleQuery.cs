using System.Data;
using EFCore.BulkExtensions;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Infrastructure.Configuration;
using SNEL.WalletPlatform.Infrastructure.Oracle;
using SNEL.WalletPlatform.Infrastructure.Persistence;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

/// <summary>
/// Oracle pull for GCO_RACCORDEMENTS (IDE_RACCORDEMENT, NUM_RACCORDEMENT, NAM_RACCORDEMENT).
/// Uses keyset pagination on <c>IDE_RACCORDEMENT</c> (not OFFSET) so each page stays cheap at multi-million row scale.
/// Each page is written to PostgreSQL with EFCore.BulkExtensions bulk insert (same approach as invoice secondary staging).
/// </summary>
public sealed class PullBillerCustomersOracleQuery : IBillerCustomerOraclePull
{
    /// <summary>
    /// Fetches the next chunk of rows strictly after <c>:lastIdeExclusive</c>, ordered by IDE (same pattern as cursor/keyset paging).
    /// </summary>
    private const string PullSql = """
                                 SELECT
                                 	gr.IDE_RACCORDEMENT,
                                 	gr.NUM_RACCORDEMENT,
                                 	gr.NAM_RACCORDEMENT
                                 FROM
                                 	gco.GCO_RACCORDEMENTS gr
                                 WHERE
                                 	gr.IDE_RACCORDEMENT > :lastIdeExclusive
                                 ORDER BY
                                 	gr.IDE_RACCORDEMENT
                                 FETCH NEXT :pageSize ROWS ONLY
                                 """;

    private readonly IOracleSnelConnectionFactory _oracleFactory;
    private readonly ILogger<PullBillerCustomersOracleQuery> _logger;
    private readonly SnelOracleLoggingOptions _loggingOptions;

    public PullBillerCustomersOracleQuery(
        IOracleSnelConnectionFactory oracleFactory,
        ILogger<PullBillerCustomersOracleQuery> logger,
        IOptions<SnelOracleLoggingOptions> loggingOptions)
    {
        _oracleFactory = oracleFactory;
        _logger = logger;
        _loggingOptions = loggingOptions.Value;
    }

    public async Task<long> PullAllToStagingAsync(
        string areaCode,
        long billerId,
        string jobKey,
        int pageSize,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be greater than zero.");
        if (string.IsNullOrWhiteSpace(jobKey))
            throw new ArgumentException("jobKey must be non-empty.", nameof(jobKey));

        await Task.Yield();
        // First page: rows with IDE > -1 (includes all non-negative IDs; invalid <=0 still skipped when mapping).
        var lastIdeExclusive = -1L;
        long totalStaged = 0L;
        var pageNumber = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            pageNumber++;

            var stagedThisPage = 0;
            var oracleRowsThisPage = 0;
            long lastIdeOnPage = lastIdeExclusive;

            using var connection = _oracleFactory.CreateConnection(areaCode);
            connection.Open();

            using var cmd = (OracleCommand)connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = PullSql;
            cmd.CommandTimeout = 1200;
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("lastIdeExclusive", OracleDbType.Int64, lastIdeExclusive,
                ParameterDirection.Input));
            cmd.Parameters.Add(new OracleParameter("pageSize", OracleDbType.Int32, pageSize, ParameterDirection.Input));

            SnelOracleCommandLogger.LogCommand(_logger, _loggingOptions.Enabled, "ExecuteReader", cmd);
            _logger.LogInformation(
                "PullBillerCustomersOracleQuery: page {Page}, pageSize={PageSize}, afterIdeExclusive={AfterIde}.",
                pageNumber, pageSize, lastIdeExclusive);

            var pageRows = new List<BillerCustomerStagingEntity>(capacity: pageSize);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    oracleRowsThisPage++;
                    var ide = ReadInt64(reader, "IDE_RACCORDEMENT");
                    lastIdeOnPage = ide;
                    if (ide <= 0)
                        continue;

                    stagedThisPage++;
                    var num = ReadString(reader, "NUM_RACCORDEMENT");
                    var nam = ReadString(reader, "NAM_RACCORDEMENT");

                    pageRows.Add(new BillerCustomerStagingEntity
                    {
                        Id = Guid.NewGuid(),
                        JobKey = jobKey.Trim(),
                        BillerId = billerId,
                        CustomerId = ide,
                        Number = $"{areaCode}-{num}",
                        Name = nam,
                        Updated = false
                    });
                }
            }

            totalStaged += stagedThisPage;
            if (oracleRowsThisPage == 0)
                break;

            if (pageRows.Count > 0)
            {
                await dbContext.BulkInsertAsync(pageRows, cancellationToken: cancellationToken);
                dbContext.ChangeTracker.Clear();
            }

            _logger.LogInformation(
                "PullBillerCustomersOracleQuery: saved page {Page}, staged={Staged}, oracleRows={OracleRows}, totalStaged={Total}, lastIdeOnPage={LastIde}.",
                pageNumber, stagedThisPage, oracleRowsThisPage, totalStaged, lastIdeOnPage);

            if (oracleRowsThisPage < pageSize)
                break;

            // Next chunk: strictly after the largest IDE returned (ordering is by IDE ASC).
            lastIdeExclusive = lastIdeOnPage;
        }

        return totalStaged;
    }

    private static long ReadInt64(System.Data.Common.DbDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        if (reader.IsDBNull(ordinal))
            return 0L;
        var v = reader.GetValue(ordinal);
        return v switch
        {
            long l => l,
            int i => i,
            decimal d => (long)d,
            _ => Convert.ToInt64(v, System.Globalization.CultureInfo.InvariantCulture)
        };
    }

    private static string ReadString(System.Data.Common.DbDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        if (reader.IsDBNull(ordinal))
            return string.Empty;
        return reader.GetString(ordinal).Trim();
    }
}
