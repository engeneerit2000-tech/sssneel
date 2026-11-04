using AutoMapper;
using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Notifications;
using SNEL.WalletPlatform.Application.Notifications.Interfaces;
using SNEL.WalletPlatform.Application.Notifications.Models.Input;
using SNEL.WalletPlatform.Application.Notifications.Models.Output;
using SNEL.WalletPlatform.Domain.Notifications;

namespace SNEL.WalletPlatform.Application.Notifications.Services;

public sealed class CampaignService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IStringLocalizer<SharedResource> localizer,
    ICampaignNotificationScheduler campaignNotificationScheduler,
    ICampaignAudienceResolver campaignAudienceResolver) : ICampaignService
{
    public async Task<Result<IReadOnlyCollection<CampaignOutputModel>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await unitOfWork.CampaignRepository.GetAllWithTranslationsAsync(cancellationToken);
        var output = mapper.Map<IReadOnlyCollection<CampaignOutputModel>>(entities);
        return Result<IReadOnlyCollection<CampaignOutputModel>>.Success(output);
    }

    public async Task<Result<CampaignOutputModel>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.CampaignRepository.GetByIdWithTranslationsAsync(id, cancellationToken);
        if (entity is null)
        {
            return Result<CampaignOutputModel>.Failure(
                localizer[LocalizationKeys.CampaignNotFound, id],
                AppHttpStatusCodes.NotFound);
        }

        return Result<CampaignOutputModel>.Success(mapper.Map<CampaignOutputModel>(entity));
    }

    public async Task<Result<CampaignOutputModel>> CreateAsync(
        CreateCampaignInputModel model,
        CancellationToken cancellationToken = default)
    {
        var audienceGroupCode = NormalizeAudienceGroupCode(model.AudienceGroupCode);
        if (campaignAudienceResolver.Resolve(audienceGroupCode) is null)
        {
            return Result<CampaignOutputModel>.Failure(
                localizer[LocalizationKeys.CampaignAudienceGroupNotSupported, audienceGroupCode],
                AppHttpStatusCodes.BadRequest);
        }

        var entity = CampaignEntity.Create(
            model.Description,
            audienceGroupCode,
            model.StartAtUtc);

        ApplyTranslations(entity, NormalizeTranslations(model.Translations));
        await unitOfWork.CampaignRepository.AddAsync(entity, cancellationToken);
        await unitOfWork.CompleteAsync();

        var scheduled = await campaignNotificationScheduler.ScheduleAsync(
            entity.Id,
            entity.StartAtUtc,
            cancellationToken);

        if (!scheduled)
        {
            return Result<CampaignOutputModel>.Failure(
                localizer[LocalizationKeys.CampaignScheduleNotAllowedWhenCompleted],
                AppHttpStatusCodes.Conflict);
        }

        var created = await unitOfWork.CampaignRepository.GetByIdWithTranslationsAsync(entity.Id, cancellationToken);
        return Result<CampaignOutputModel>.Success(
            mapper.Map<CampaignOutputModel>(created!),
            localizer[LocalizationKeys.CampaignCreateSuccess],
            AppHttpStatusCodes.Created);
    }

    public async Task<Result<CampaignOutputModel>> UpdateAsync(
        Guid id,
        UpdateCampaignInputModel model,
        CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.CampaignRepository.GetByIdWithTranslationsAsync(id, cancellationToken);
        if (entity is null)
        {
            return Result<CampaignOutputModel>.Failure(
                localizer[LocalizationKeys.CampaignNotFound, id],
                AppHttpStatusCodes.NotFound);
        }

        if (!entity.CanBeModified())
        {
            return Result<CampaignOutputModel>.Failure(
                localizer[LocalizationKeys.CampaignUpdateNotAllowedWhenCompleted],
                AppHttpStatusCodes.Conflict);
        }

        await campaignNotificationScheduler.UnscheduleAsync(id, cancellationToken);

        var audienceGroupCode = NormalizeAudienceGroupCode(model.AudienceGroupCode);
        if (campaignAudienceResolver.Resolve(audienceGroupCode) is null)
        {
            return Result<CampaignOutputModel>.Failure(
                localizer[LocalizationKeys.CampaignAudienceGroupNotSupported, audienceGroupCode],
                AppHttpStatusCodes.BadRequest);
        }

        entity.Update(model.Description, audienceGroupCode, model.StartAtUtc);
        SyncTranslations(entity, NormalizeTranslations(model.Translations));
        unitOfWork.CampaignRepository.Update(entity);
        await unitOfWork.CompleteAsync();

        if (entity.IsDispatchActive())
        {
            var scheduled = await campaignNotificationScheduler.ScheduleAsync(
                entity.Id,
                entity.StartAtUtc,
                cancellationToken);

            if (!scheduled)
            {
                return Result<CampaignOutputModel>.Failure(
                    localizer[LocalizationKeys.CampaignScheduleNotAllowedWhenCompleted],
                    AppHttpStatusCodes.Conflict);
            }
        }

        var updated = await unitOfWork.CampaignRepository.GetByIdWithTranslationsAsync(id, cancellationToken);
        return Result<CampaignOutputModel>.Success(
            mapper.Map<CampaignOutputModel>(updated!),
            localizer[LocalizationKeys.CampaignUpdateSuccess]);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.CampaignRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.CampaignNotFound, id],
                AppHttpStatusCodes.NotFound);
        }

        await campaignNotificationScheduler.UnscheduleAsync(id, cancellationToken);

        entity.MarkCancelled();
        unitOfWork.CampaignRepository.Remove(entity);
        await unitOfWork.CompleteAsync();

        return Result<bool>.Success(true, localizer[LocalizationKeys.CampaignDeleteSuccess]);
    }

    private sealed record CampaignTranslationData(string Title, string Message);

    private static string NormalizeAudienceGroupCode(string? audienceGroupCode) =>
        string.IsNullOrWhiteSpace(audienceGroupCode)
            ? CampaignAudienceGroupCodes.AllCustomers
            : audienceGroupCode.Trim().ToUpperInvariant();

    private static Dictionary<string, CampaignTranslationData> NormalizeTranslations(
        IEnumerable<CampaignTranslationInputModel>? translations)
    {
        var normalized = new Dictionary<string, CampaignTranslationData>(StringComparer.OrdinalIgnoreCase);
        if (translations is null)
        {
            return normalized;
        }

        foreach (var translation in translations)
        {
            var languageCode = translation.LanguageCode.Trim().ToLowerInvariant();
            normalized[languageCode] = new CampaignTranslationData(
                translation.Title.Trim(),
                translation.Message.Trim());
        }

        return normalized;
    }

    private static void ApplyTranslations(CampaignEntity entity, IReadOnlyDictionary<string, CampaignTranslationData> translations)
    {
        foreach (var (languageCode, translation) in translations)
        {
            entity.Translations.Add(new CampaignTranslationEntity
            {
                EntityId = entity.Id,
                LanguageCode = languageCode,
                Name = translation.Title,
                Message = translation.Message
            });
        }
    }

    private static void SyncTranslations(CampaignEntity entity, IReadOnlyDictionary<string, CampaignTranslationData> translations)
    {
        foreach (var (languageCode, translation) in translations)
        {
            var existing = entity.Translations
                .FirstOrDefault(t => string.Equals(t.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                entity.Translations.Add(new CampaignTranslationEntity
                {
                    EntityId = entity.Id,
                    LanguageCode = languageCode,
                    Name = translation.Title,
                    Message = translation.Message
                });
                continue;
            }

            existing.LanguageCode = languageCode;
            existing.Name = translation.Title;
            existing.Message = translation.Message;
        }
    }
}
