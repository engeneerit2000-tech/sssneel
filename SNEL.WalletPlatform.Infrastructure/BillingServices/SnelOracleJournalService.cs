using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.BillingServices.Models.Internal;
using SNEL.WalletPlatform.Infrastructure.Configuration;
using SNEL.WalletPlatform.Infrastructure.Oracle;

namespace SNEL.WalletPlatform.Infrastructure.BillingServices;

public sealed class SnelOracleJournalService(
    IOracleSnelConnectionFactory oracleFactory,
    ILogger<SnelOracleJournalService> logger,
    IOptions<SnelOracleLoggingOptions> loggingOptions) : ISnelOracleJournalService
{
    private const string ActiveFlag = "Y";
    private const string OperatorName = "SNEL-SYSTEM";

    public async Task<SnelOracleJournalModel> OpenDailyJournalAsync(
        IDbConnection connection,
        DateTime dateUtc,
        string posCode,        
        CancellationToken ct = default)
    {
        var journalDate = NormalizeUtcDate(dateUtc);
        var normalizedPosCode = NormalizePosCode(posCode);

        //using var connection = oracleFactory.CreateConnection();
        //await OpenAsync(connection, ct);
        using var transaction = connection.BeginTransaction();

        var existingJournal = await GetOpenJournalByDateInternalAsync(connection, transaction, journalDate, normalizedPosCode, ct);
        if (existingJournal is not null)
        {
            transaction.Commit();
            return existingJournal;
        }

        var nextJournalNumber = await GetNextJournalNumberAsync(connection, transaction, normalizedPosCode, ct);
        await InsertJournalAsync(connection, transaction, journalDate, normalizedPosCode, nextJournalNumber, ct);

        var createdJournal = await GetOpenJournalByDateInternalAsync(connection, transaction, journalDate, normalizedPosCode, ct);
        if (createdJournal is null)
            throw new InvalidOperationException("SNEL journal was inserted but could not be reloaded.");

        transaction.Commit();
        return createdJournal;
    }

    public async Task<SnelOracleJournalModel> ClosePreviousAndOpenDailyJournalAsync(
        string areaCode,
        DateTime dateUtc,
        string posCode,
        CancellationToken ct = default)
    {
        var journalDate = NormalizeUtcDate(dateUtc);
        var normalizedPosCode = NormalizePosCode(posCode);
        var closeTimestamp = journalDate.AddSeconds(-1);

        using var connection = oracleFactory.CreateConnection(areaCode);
        await OpenAsync(connection, ct);
        using var transaction = connection.BeginTransaction();

        await ClosePreviousOpenJournalsAsync(connection, transaction, journalDate, closeTimestamp, normalizedPosCode, ct);

        var existingJournal = await GetOpenJournalByDateInternalAsync(connection, transaction, journalDate, normalizedPosCode, ct);
        if (existingJournal is not null)
        {
            transaction.Commit();
            return existingJournal;
        }

        var nextJournalNumber = await GetNextJournalNumberAsync(connection, transaction, normalizedPosCode, ct);
        await InsertJournalAsync(connection, transaction, journalDate, normalizedPosCode, nextJournalNumber, ct);

        var createdJournal = await GetOpenJournalByDateInternalAsync(connection, transaction, journalDate, normalizedPosCode, ct);
        if (createdJournal is null)
            throw new InvalidOperationException("SNEL journal was inserted but could not be reloaded.");

        transaction.Commit();
        return createdJournal;
    }

    public async Task<SnelOracleJournalAllocationModel> GetCurrentAllocationAsync(
        string areaCode,
        DateTime dateUtc,
        string posCode,
        CancellationToken ct = default)
    {
        var journalDate = NormalizeUtcDate(dateUtc);
        var normalizedPosCode = NormalizePosCode(posCode);

        using var connection = oracleFactory.CreateConnection(areaCode);
        await OpenAsync(connection, ct);

        // Open today's journal on demand if the scheduler has not created it yet.
        var journal = await GetOpenJournalByDateInternalAsync(connection, null, journalDate, normalizedPosCode, ct)
            ?? await OpenDailyJournalAsync(connection, journalDate, normalizedPosCode, ct);

        // Get the next item number
        var nextItemNumber = await GetNextItemNumberAsync(connection, journal.NumJournal, journal.PosCode, ct);

        return new SnelOracleJournalAllocationModel(
            journal.IdeCaisse,
            journal.PosCode,
            journal.NumJournal,
            nextItemNumber,
            journal.DatJournal);
    }

    public async Task<bool> CheckJournalValidity(
        IDictionary<string, SnelConfigModel> configs,        
        CancellationToken ct)
    {
        var journalDate = NormalizeUtcDate(DateTime.UtcNow);
        foreach (var config in configs)
        {
            using var connection = oracleFactory.CreateConnection(config.Key);
            await OpenAsync(connection, ct);
            using var command = connection.CreateCommand();
            command.CommandText =
                """
            SELECT
                1
            FROM gco.GCO_CAISSES
            WHERE PPRS_COD_PPR = :pprsCodPpr
              AND TRUNC(DAT_JOURNAL) = :journalDate
              AND DAT_CLOTURE IS NULL
            ORDER BY IDE_CAISSE DESC
            FETCH FIRST 1 ROWS ONLY
            """;

            AddParameter(command, "pprsCodPpr", config.Value.PosCode);
            AddParameter(command, "journalDate", journalDate);

            using var reader = await ExecuteReaderAsync(command, ct);
            var res = await ReadAsync(reader, ct);
            if (!res)
                return false;
        }
        return true;
    }
    private static string NormalizePosCode(string posCode)
    {
        if (string.IsNullOrWhiteSpace(posCode))
            throw new ArgumentException("POS code is required.", nameof(posCode));

        return posCode.Trim();
    }

    private static DateTime NormalizeUtcDate(DateTime dateUtc)
    {
        var normalized = dateUtc.Kind switch
        {
            DateTimeKind.Utc => dateUtc,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateUtc, DateTimeKind.Utc),
            _ => dateUtc.ToUniversalTime()
        };

        return normalized.Date;
    }

    private static async Task OpenAsync(IDbConnection connection, CancellationToken ct)
    {
        if (connection is DbConnection dbConnection)
        {
            await dbConnection.OpenAsync(ct);
            return;
        }

        connection.Open();
    }
    
    private async Task<SnelOracleJournalModel?> GetOpenJournalByDateInternalAsync(
        IDbConnection connection,
        IDbTransaction? transaction,
        DateTime journalDate,
        string posCode,
        CancellationToken ct)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            SELECT
                IDE_CAISSE,
                PPRS_COD_PPR,
                NUM_JOURNAL,
                DAT_JOURNAL,
                DAT_OUVERTURE,
                DAT_CLOTURE
            FROM gco.GCO_CAISSES
            WHERE PPRS_COD_PPR = :pprsCodPpr
              AND TRUNC(DAT_JOURNAL) = :journalDate
              AND DAT_CLOTURE IS NULL
            ORDER BY IDE_CAISSE DESC
            FETCH FIRST 1 ROWS ONLY
            """;

        AddParameter(command, "pprsCodPpr", posCode);
        AddParameter(command, "journalDate", journalDate);

        using var reader = await ExecuteReaderAsync(command, ct);
        if (!await ReadAsync(reader, ct))
            return null;

        return MapJournal(reader);
    }

    private async Task<decimal> GetNextJournalNumberAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        string posCode,
        CancellationToken ct)
    {
        // TODO add cashing
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            SELECT NVL(MAX(NUM_JOURNAL), 0) + 1
            FROM gco.GCO_CAISSES
            WHERE PPRS_COD_PPR = :pprsCodPpr
            """;

        AddParameter(command, "pprsCodPpr", posCode);

        var result = await ExecuteScalarAsync(command, ct);
        return result is null or DBNull ? 1m : Convert.ToDecimal(result);
    }

    //TODO: Nedal: Shaker, why you use posId?
    private async Task<decimal> GetNextItemNumberAsync(
        IDbConnection connection,
        decimal numJournal,
        string posCode,
        CancellationToken ct)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT NVL(MAX(NUM_ITEM), 0) + 1
            FROM gco.GCO_REGLEMENTS
            WHERE NUM_JOURNAL = :numJournal
            AND PPRS_COD_PPR = :posCode
            """;

        AddParameter(command, "numJournal", numJournal);
        AddParameter(command, "posCode", posCode);

        var result = await ExecuteScalarAsync(command, ct);
        return result is null or DBNull ? 1m : Convert.ToDecimal(result);
    }

    private async Task ClosePreviousOpenJournalsAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        DateTime journalDate,
        DateTime closeTimestamp,
        string posCode,
        CancellationToken ct)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            UPDATE gco.GCO_CAISSES
            SET DAT_CLOTURE = :closeTimestamp
            WHERE PPRS_COD_PPR = :pprsCodPpr
              AND DAT_CLOTURE IS NULL
              AND TRUNC(DAT_JOURNAL) < :journalDate
            """;

        AddParameter(command, "closeTimestamp", closeTimestamp);
        AddParameter(command, "pprsCodPpr", posCode);
        AddParameter(command, "journalDate", journalDate);

        await ExecuteNonQueryAsync(command, ct);
    }

    private async Task InsertJournalAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        DateTime journalDate,
        string posCode,
        decimal nextJournalNumber,
        CancellationToken ct)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            INSERT INTO gco.GCO_CAISSES
            (
                IDE_CAISSE,
                PPRS_COD_PPR,
                NUM_JOURNAL,
                USER_IDE_USER,
                FAG_ACTIF,
                DAT_OUVERTURE,
                DAT_CLOTURE,
                DAT_JOURNAL,
                NAM_OPERATEUR,
                FAG_CLOTURE
            )
            VALUES
            (
                :ideCaisse,
                :pprsCodPpr,
                :numJournal,
                :userIdeUser,
                :fagActif,
                :datOuverture,
                :datCloture,
                :datJournal,
                :namOperateur,
                :fagCloture
            )
            """;

        AddParameter(command, "ideCaisse", 0m);
        AddParameter(command, "pprsCodPpr", posCode);
        AddParameter(command, "numJournal", nextJournalNumber);
        AddParameter(command, "userIdeUser", DBNull.Value);
        AddParameter(command, "fagActif", ActiveFlag);
        AddParameter(command, "datOuverture", journalDate);
        AddParameter(command, "datCloture", DBNull.Value);
        AddParameter(command, "datJournal", journalDate);
        AddParameter(command, "namOperateur", OperatorName);
        AddParameter(command, "fagCloture", DBNull.Value);

        await ExecuteNonQueryAsync(command, ct);
    }

    private static SnelOracleJournalModel MapJournal(IDataRecord record)
    {
        return new SnelOracleJournalModel(
            IdeCaisse: Convert.ToDecimal(record["IDE_CAISSE"]),
            PosCode: Convert.ToString(record["PPRS_COD_PPR"]) ?? string.Empty,
            NumJournal: Convert.ToDecimal(record["NUM_JOURNAL"]),
            DatJournal: Convert.ToDateTime(record["DAT_JOURNAL"]),
            DatOuverture: Convert.ToDateTime(record["DAT_OUVERTURE"]),
            DatCloture: record["DAT_CLOTURE"] is DBNull ? null : Convert.ToDateTime(record["DAT_CLOTURE"]));
    }

    private static void AddParameter(IDbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    private async Task<IDataReader> ExecuteReaderAsync(IDbCommand command, CancellationToken ct)
    {
        SnelOracleCommandLogger.LogCommand(logger, loggingOptions.Value.Enabled, "ExecuteReader", command);

        if (command is DbCommand dbCommand)
            return await dbCommand.ExecuteReaderAsync(ct);

        return command.ExecuteReader();
    }

    private async Task<object?> ExecuteScalarAsync(IDbCommand command, CancellationToken ct)
    {
        SnelOracleCommandLogger.LogCommand(logger, loggingOptions.Value.Enabled, "ExecuteScalar", command);

        if (command is DbCommand dbCommand)
            return await dbCommand.ExecuteScalarAsync(ct);

        return command.ExecuteScalar();
    }

    private async Task<int> ExecuteNonQueryAsync(IDbCommand command, CancellationToken ct)
    {
        SnelOracleCommandLogger.LogCommand(logger, loggingOptions.Value.Enabled, "ExecuteNonQuery", command);

        if (command is DbCommand dbCommand)
        {
            var res = await dbCommand.ExecuteNonQueryAsync(ct);
            return res;
        }

        var result =  command.ExecuteNonQuery();
        return result;
    }

    private static async Task<bool> ReadAsync(IDataReader reader, CancellationToken ct)
    {
        if (reader is DbDataReader dbReader)
            return await dbReader.ReadAsync(ct);

        return reader.Read();
    }
}
