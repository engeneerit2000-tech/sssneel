using SNEL.WalletPlatform.Application.BillingServices.Models.Internal;
using System.Data;

namespace SNEL.WalletPlatform.Application.BillingServices.Interfaces;

public interface ISnelOracleJournalService
{
    Task<SnelOracleJournalModel> ClosePreviousAndOpenDailyJournalAsync(
        string areaCode,
        DateTime dateUtc,
        string posCode,
        CancellationToken ct = default);

    Task<SnelOracleJournalAllocationModel> GetCurrentAllocationAsync(
        string areaCode, 
        DateTime dateUtc,
        string posCode,
        CancellationToken ct = default);
    /// <summary>
    /// Returns false when any journal is not active
    /// </summary>
    /// <param name="configs"></param>
    /// <param name="journalDate"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<bool> CheckJournalValidity(
        IDictionary<string, SnelConfigModel> configs,               
        CancellationToken ct);
}
