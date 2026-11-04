using System.Globalization;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.BillingServices.Models.Internal;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Domain.Billing;

namespace SNEL.WalletPlatform.Infrastructure.BillingServices;

public sealed class SnelConfigService(IUnitOfWork unitOfWork) : ISnelConfigService
{
    private const string BillerCode = "SNEL";
    private const string PaymentModeCodeKey = "PaymentModeCode";
    private const string PaymentModeBillerName = "MODR_COD_MODE_REGL";
    private const string PosCodeKey = "PosCode";
    private const string PosIdKey = "PosId";
    private const string JournalNumberKey = "JournalNumber";
    private const string JournalItemNumberKey = "JournalItemNumber";

    public async Task<string> GetConnectionStringAsync(string areaCode, CancellationToken ct = default)
    {
        var biller = await unitOfWork.BillerRepository.GetByCodeAsync(BillerCode, ct);
        if (biller is null)
            throw new InvalidOperationException($"Biller '{BillerCode}' was not found.");

        var configs = await unitOfWork.BillerConfigRepository.GetByBillerCodeAsync(BillerCode, ct);
        var connStr_obj = configs.FirstOrDefault(x => x.BillerName == "ConnectionString" && x.Name.StartsWith(areaCode));
        if (connStr_obj is null)
            return null;
        return connStr_obj.Value;
    }
    public async Task<SnelConfigModel?> GetConfigAsync(string areaCode, CancellationToken ct = default)
    {
        var biller = await unitOfWork.BillerRepository.GetByCodeAsync(BillerCode, ct);
        if (biller is null)
            throw new InvalidOperationException($"Biller '{BillerCode}' was not found.");

        var configs = await unitOfWork.BillerConfigRepository.GetByBillerCodeAsync(BillerCode, ct);
        return TryBuildConfig(configs, areaCode);
    }    
    public async Task<SnelConfigModel> GetRequiredConfigAsync(string areaCode, CancellationToken ct = default)
    {
        var normalizedAreaCode = NormalizeAreaCode(areaCode);
        var config = await GetConfigAsync(normalizedAreaCode, ct);
        if (config is null)
            throw new InvalidOperationException(
                $"SNEL local state is missing required area-scoped '{PosCodeKey}', '{PosIdKey}', '{JournalNumberKey}', '{JournalItemNumberKey}', or '{PaymentModeCodeKey}' values for area '{normalizedAreaCode}'.");

        return config;
    }

    public async Task<Dictionary<string,SnelConfigModel>> GetAllRequiredConfigAsync(string[] areaCodes, CancellationToken ct = default)
    {
        //var config = await GetAllConfigAsync(ct);
        var biller = await unitOfWork.BillerRepository.GetByCodeAsync(BillerCode, ct);
        if (biller is null)
            throw new InvalidOperationException($"Biller '{BillerCode}' was not found.");

        var configs = await unitOfWork.BillerConfigRepository.GetByBillerCodeAsync(BillerCode, ct);
        if (areaCodes == null || areaCodes.Length == 0)
        {
            areaCodes = configs.Select(x => string.Join('-', x.Name.Split('-').Take(2))).Distinct().Reverse().ToArray();
        }        
        Dictionary<string, SnelConfigModel> grouped = new Dictionary<string, SnelConfigModel>();
        foreach (var area in areaCodes)
        {
            var config = TryBuildConfig(configs.Where(x => x.Name.StartsWith(area)).ToList(), area);
            if(config is null)
                throw new InvalidOperationException(
                $"SNEL local state is missing required area-scoped '{PosCodeKey}', '{PosIdKey}', '{JournalNumberKey}', '{JournalItemNumberKey}', or '{PaymentModeCodeKey}' values for area '{area}'.");
            grouped.Add(area, config);
        }
        return grouped;        
    }

    public async Task<SnelConfigModel> ReserveApplyConfigAsync(int itemCount, string areaCode, CancellationToken ct = default)
    {
        if (itemCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(itemCount), itemCount, "Item count must be greater than zero.");

        var normalizedAreaCode = NormalizeAreaCode(areaCode);
        

        try
        {
            await EnsureBillerExistsAsync(ct);

            var configs = await unitOfWork.BillerConfigRepository.GetByBillerCodeForUpdateAsync(BillerCode, ct);
            var config = TryBuildConfig(configs, normalizedAreaCode)
                ?? throw new InvalidOperationException(
                    $"SNEL local state is missing required area-scoped '{PosCodeKey}', '{PosIdKey}', '{JournalNumberKey}', '{JournalItemNumberKey}', or '{PaymentModeCodeKey}' values for area '{normalizedAreaCode}'.");

            var journalItemConfig = configs.FirstOrDefault(x =>
                string.Equals(x.Name, BuildConfigName(normalizedAreaCode, JournalItemNumberKey), StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"SNEL local state is missing required area-scoped '{JournalItemNumberKey}' value for area '{normalizedAreaCode}'.");

            journalItemConfig.SetValue(ToStringValue(config.JournalItemNumber + itemCount));
            unitOfWork.BillerConfigRepository.Update(journalItemConfig);

            await unitOfWork.CompleteAsync();            

            return config;
        }
        catch
        {
            throw;
        }
    }

    public async Task SetPosAsync(string posCode, decimal posId, string areaCode, CancellationToken ct = default)
    {
        var normalizedAreaCode = NormalizeAreaCode(areaCode);
        await EnsureBillerExistsAsync(ct);

        await UpsertConfigAsync(normalizedAreaCode, PosCodeKey, "COD_PPR", posCode.Trim(), ct);
        await UpsertConfigAsync(normalizedAreaCode, PosIdKey, "IDE_PPR", ToStringValue(posId), ct);
        await unitOfWork.CompleteAsync();
    }   
    public async Task SetJournalAsync(decimal journalNumber, decimal journalItemNumber, string areaCode, CancellationToken ct = default)
    {
        var normalizedAreaCode = NormalizeAreaCode(areaCode);
        await using var tx = await unitOfWork.BeginTransactionAsync();

        try
        {
            await EnsureBillerExistsAsync(ct);

            await UpsertConfigAsync(normalizedAreaCode, JournalNumberKey, "NUM_JOURNAL", ToStringValue(journalNumber), ct);
            await UpsertConfigAsync(normalizedAreaCode, JournalItemNumberKey, "NUM_ITEM", ToStringValue(journalItemNumber), ct);
            await unitOfWork.CompleteAsync();
            await unitOfWork.CommitTransactionAsync();// tx.CommitAsync();
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();// tx.RollbackAsync();
            throw;
        }
    }

    public async Task SetPaymentModeAsync(string paymentModeCode, string areaCode, CancellationToken ct = default)
    {
        var normalizedAreaCode = NormalizeAreaCode(areaCode);
        await EnsureBillerExistsAsync(ct);

        await UpsertConfigAsync(normalizedAreaCode, PaymentModeCodeKey, PaymentModeBillerName, paymentModeCode.Trim(), ct);
        await unitOfWork.CompleteAsync();
    }

    public async Task SetJournalItemNumberAsync(decimal journalItemNumber, string areaCode, CancellationToken ct = default)
    {
        var normalizedAreaCode = NormalizeAreaCode(areaCode);
        await EnsureBillerExistsAsync(ct);

        await UpsertConfigAsync(normalizedAreaCode, JournalItemNumberKey, "NUM_ITEM", ToStringValue(journalItemNumber), ct);
        await unitOfWork.CompleteAsync();
    }

    private async Task EnsureBillerExistsAsync(CancellationToken ct)
    {
        var biller = await unitOfWork.BillerRepository.GetByCodeAsync(BillerCode, ct);
        if (biller is null)
            throw new InvalidOperationException($"Biller '{BillerCode}' was not found.");
    }

    private async Task UpsertConfigAsync(string areaCode, string name, string billerName, string value, CancellationToken ct)
    {
        await unitOfWork.BillerConfigRepository.UpsertAsync(BillerCode, BuildConfigName(areaCode, name), billerName, value, ct);
    }

    private static decimal ParseDecimal(string value, string name)
    {
        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
            throw new InvalidOperationException($"SNEL local state value '{name}' is invalid: '{value}'.");

        return parsed;
    }

    private static int ParseInt(string value, string name)
    {
        if (!int.TryParse(value, out var parsed))
            throw new InvalidOperationException($"SNEL local state value '{name}' is invalid: '{value}'.");

        return parsed;
    }

    private static SnelConfigModel? TryBuildConfig(IReadOnlyCollection<BillerConfigEntity> configs, string areaCode)
    {
        var normalizedAreaCode = NormalizeAreaCode(areaCode);
        var configMap = configs.ToDictionary(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);
        var posCodeName = BuildConfigName(normalizedAreaCode, PosCodeKey);
        var posIdName = BuildConfigName(normalizedAreaCode, PosIdKey);
        var journalNumberName = BuildConfigName(normalizedAreaCode, JournalNumberKey);
        var journalItemNumberName = BuildConfigName(normalizedAreaCode, JournalItemNumberKey);
        var paymentModeName = BuildConfigName(normalizedAreaCode, PaymentModeCodeKey);        

        if (!configMap.TryGetValue(posCodeName, out var posCode) ||
            !configMap.TryGetValue(posIdName, out var posIdValue) ||
            !configMap.TryGetValue(journalNumberName, out var journalNumberValue) ||
            !configMap.TryGetValue(journalItemNumberName, out var journalItemNumberValue) ||
            !configMap.TryGetValue(paymentModeName, out var paymentModeCode))
        {
            return null;
        }

        return new SnelConfigModel(
            posCode,
            ParseDecimal(posIdValue, PosIdKey),
            ParseDecimal(journalNumberValue, JournalNumberKey),
            ParseDecimal(journalItemNumberValue, JournalItemNumberKey),
            paymentModeCode);
    }

    private static string ToStringValue(decimal value) =>
        value.ToString(CultureInfo.InvariantCulture);

    private static string NormalizeAreaCode(string areaCode)
    {
        if (string.IsNullOrWhiteSpace(areaCode))
            throw new ArgumentException("Area code is required.", nameof(areaCode));

        return areaCode.Trim();
    }

    private static string BuildConfigName(string areaCode, string name) =>
        $"{NormalizeAreaCode(areaCode)}-{name}";
}
