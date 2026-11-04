using Microsoft.Extensions.Logging;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.Caching;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Infrastructure.BillingServices;

public sealed class SnelBootstrapService(
    ISnelOracleJournalService journalService,
    ISnelConfigService configService,
    IUnitOfWork unitOfWork,
    ILogger<SnelBootstrapService> logger) : ISnelBootstrapService
{
    //private string[] AreaCodes = { "06-05", "06-03" };
    private string PosCode = "9999";
    private const decimal PosId = 441;
    private const string RuntimePaymentModeCode = "8";

    public async Task EnsureInitializedAsync(CancellationToken ct = default)
    {
        string[] AreaCodes = CachingFactory.GetValue<CenterEntity>().Select(x => x.Code).ToArray();
        //string[] AreaCodes = unitOfWork.CenterRepository.GetAll().Select(x => x.Code).ToArray();
        // Center 33 is the only runtime POS for now. Multi-center behavior needs stakeholder confirmation.
        foreach (var code in AreaCodes)
        {
            var journal = await journalService.ClosePreviousAndOpenDailyJournalAsync(code, DateTime.UtcNow, PosCode, ct);
            var allocation = await journalService.GetCurrentAllocationAsync(code, DateTime.UtcNow, PosCode, ct);

            await configService.SetPosAsync(PosCode, PosId, code, ct);
            await configService.SetJournalAsync(journal.NumJournal, allocation.NextItemNumber, code, ct);
            await configService.SetPaymentModeAsync(RuntimePaymentModeCode, code, ct);

            logger.LogInformation(
                "SNEL bootstrap completed for area {AreaCode}. POS {PosCode}/{PosId}, journal {NumJournal}, next item {NextItemNumber}, payment mode {PaymentModeCode}.",
                code,
                PosCode,
                PosId,
                journal.NumJournal,
                allocation.NextItemNumber,
                RuntimePaymentModeCode);
        }
    }
}
