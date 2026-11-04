using System.Text.Json;
using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Lookups.Interfaces;
using SNEL.WalletPlatform.Application.Lookups.Models;
using SNEL.WalletPlatform.Application.Lookups.Models.Output;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Application.Lookups.Services;

public sealed class PlatformConstantService(
    IUnitOfWork unitOfWork,
    IStringLocalizer<SharedResource> localizer) : IPlatformConstantService
{
    private static readonly JsonSerializerOptions DefaultPaymentMethodJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<Result<string>> GetLatestMobileVersionAsync()
    {
        string version = await unitOfWork.PlatformConstantRepository.GetValueByGroupAndCode(
            Domain.Lookups.PlatformConstantGroupsEnum.SYSTEM,
            Domain.Lookups.PlatformConstantCodesEnum.MOBILE_APP_VERSION);
        return Result<string>.Success(version);
    }

    public async Task<Result<IReadOnlyList<PlatformConstantOutputModel>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var constants = await unitOfWork.PlatformConstantRepository.GetAllWithTranslationsAsync(cancellationToken);
        var output = constants
            .Select(MapToOutput)
            .ToList();

        return Result<IReadOnlyList<PlatformConstantOutputModel>>.Success(output);
    }

    public async Task<Result<(string PaymentServiceProviderCode, string PaymentMethodCode, bool requirePhoneNumber)>> GetDefaultPaymentMethodCodesAsync(
        CancellationToken cancellationToken = default)
    {
        var constant = await unitOfWork.PlatformConstantRepository.GetByGroupAndCode(
            PlatformConstantGroupsEnum.PAYMENT,
            PlatformConstantCodesEnum.DEFAULT_PAYMENT_METHOD,
            cancellationToken);

        if (constant is null || !constant.Active)
        {
            return Result<(string, string, bool)>.Failure(
                localizer[LocalizationKeys.PaymentDefaultPaymentMethodNotConfigured],
                AppHttpStatusCodes.NotFound);
        }

        DefaultPaymentMethodCodesModel? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<DefaultPaymentMethodCodesModel>(
                constant.Value,
                DefaultPaymentMethodJsonOptions);
        }
        catch (JsonException)
        {
            parsed = null;
        }

        if (parsed is null)
            //|| string.IsNullOrWhiteSpace(parsed.PaymentServiceProviderCode)
            //|| string.IsNullOrWhiteSpace(parsed.PaymentMethodCode))
        {
            return Result<(string, string, bool)>.Failure(
                localizer[LocalizationKeys.PaymentDefaultPaymentMethodInvalidConfig],
                AppHttpStatusCodes.Failed);
        }

        return Result<(string, string, bool)>.Success((
            parsed.PaymentServiceProviderCode?.Trim()?.ToUpperInvariant() ?? string.Empty,
            parsed.PaymentMethodCode?.Trim()?.ToUpperInvariant() ?? string.Empty,
            bool.Parse(parsed.RequirePhoneNumber ?? "false")));
    }

    private static PlatformConstantOutputModel MapToOutput(PlatformConstantEntity entity) =>
        new(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.Group,
            entity.Value,
            entity.Order,
            entity.Active,
            entity.Translations
                .OrderBy(t => t.LanguageCode)
                .Select(t => new PlatformConstantTranslationOutputModel(t.Id, t.LanguageCode, t.Name))
                .ToList());
}
