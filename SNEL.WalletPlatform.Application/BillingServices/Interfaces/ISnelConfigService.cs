using SNEL.WalletPlatform.Application.BillingServices.Models.Internal;

namespace SNEL.WalletPlatform.Application.BillingServices.Interfaces;

public interface ISnelConfigService
{
    Task<string> GetConnectionStringAsync(string areaCode, CancellationToken ct = default); 
    Task<SnelConfigModel?> GetConfigAsync(string areaCode, CancellationToken ct = default);
    Task<SnelConfigModel> GetRequiredConfigAsync(string areaCode, CancellationToken ct = default);
    Task<Dictionary<string, SnelConfigModel>> GetAllRequiredConfigAsync(string[] areaCodes, CancellationToken ct = default);
    Task<SnelConfigModel> ReserveApplyConfigAsync(int itemCount, string areaCode, CancellationToken ct = default);
    Task SetPaymentModeAsync(string paymentModeCode, string areaCode, CancellationToken ct = default);
    Task SetPosAsync(string posCode, decimal posId, string areaCode, CancellationToken ct = default);
    Task SetJournalAsync(decimal journalNumber, decimal journalItemNumber, string areaCode, CancellationToken ct = default);
    Task SetJournalItemNumberAsync(decimal journalItemNumber, string areaCode, CancellationToken ct = default);    
}
