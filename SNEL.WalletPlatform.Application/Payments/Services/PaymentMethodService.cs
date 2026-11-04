using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.Caching;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Models.Input;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Accounting.Enums;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Payments;
using System.Text.Json;

namespace SNEL.WalletPlatform.Application.Payments.Services;

public sealed class PaymentMethodService(
    IUnitOfWork unitOfWork,
    IStringLocalizer<SharedResource> localizer,
    IPaymentMethodAccessPolicy paymentMethodAccessPolicy) : IPaymentMethodService
{
    private static readonly JsonSerializerOptions ConfigSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task<Result<IReadOnlyCollection<PaymentMethodOutputModel>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var scopeResult = await paymentMethodAccessPolicy.GetPaymentMethodsScopeAsync(cancellationToken);
        if (!scopeResult.IsSuccess)
        {
            return Result<IReadOnlyCollection<PaymentMethodOutputModel>>.Failure(scopeResult.Message!, scopeResult.StatusCode);
        }
        var scopedProviderPartnerId = scopeResult.Value;

        var entities = await unitOfWork.PaymentMethodRepository
            .GetAllWithRelationsScopedByProviderPartnerAsync(scopedProviderPartnerId, cancellationToken);
        var output = entities.Select(ToOutputModel).ToList();

        return Result<IReadOnlyCollection<PaymentMethodOutputModel>>.Success(output);
    }

    public async Task<Result<PaymentMethodOutputModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.PaymentMethodRepository.GetByIdWithRelationsAsync(id);
        if (entity is null)
        {
            return Result<PaymentMethodOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentMethodNotFound, id],
                AppHttpStatusCodes.NotFound);
        }

        return Result<PaymentMethodOutputModel>.Success(ToOutputModel(entity));
    }

    public async Task<Result<PaymentMethodOutputModel>> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.PaymentMethodRepository.GetByCodeAsync(code);
        if (entity is null)
        {
            return Result<PaymentMethodOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentMethodNotFound, code],
                AppHttpStatusCodes.NotFound);
        }

        return Result<PaymentMethodOutputModel>.Success(ToOutputModel(entity));
    }
    public async Task<Result<List<PaymentMethodOutputModel>>> GetByProviderIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.PaymentMethodRepository.GetByProviderIdWithRelationsAsync(id);
        if (entity is null)
        {
            return Result<List<PaymentMethodOutputModel>>.Failure(
                localizer[LocalizationKeys.PaymentMethodNotFound, id],
                AppHttpStatusCodes.NotFound);
        }
        var result = entity.Select(x => ToOutputModel(x)).ToList();
        return Result<List<PaymentMethodOutputModel>>.Success(result);
    }
    public async Task<Result<PaymentMethodOutputModel>> CreateAsync(
        CreatePaymentMethodInputModel model,
        CancellationToken cancellationToken = default)
    {
        var managementScopeResult = await paymentMethodAccessPolicy.GetPaymentMethodManagementScopeAsync(cancellationToken);
        if (!managementScopeResult.IsSuccess)
        {
            return Result<PaymentMethodOutputModel>.Failure(managementScopeResult.Message!, managementScopeResult.StatusCode);
        }
        var scopedProviderPartnerId = managementScopeResult.Value;

        var code = model.Code?.Trim() ?? string.Empty;
        var name = model.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name) || model.Currencies.Count == 0)
        {
            return Result<PaymentMethodOutputModel>.Failure(
                localizer[LocalizationKeys.BillerAddInvalidRequest],
                AppHttpStatusCodes.BadRequest);
        }

        if (!Enum.IsDefined(typeof(PaymentInitModeEnum), model.PaymentInitMode) ||
            !Enum.IsDefined(typeof(ConfirmationModeEnum), model.ConfirmationMode) ||
            !Enum.IsDefined(typeof(CompletionModeEnum), model.CompletionMode) ||
            !Enum.IsDefined(typeof(PaymentMethodPurposeEnum), model.PaymentMethodPurpose) ||
            !Enum.IsDefined(typeof(ConfirmationTypeEnum), model.ConfirmationType))
        {
            return Result<PaymentMethodOutputModel>.Failure(
                localizer[LocalizationKeys.BillerAddInvalidRequest],
                AppHttpStatusCodes.BadRequest);
        }

        var provider = await unitOfWork.PaymentProviderRepository.GetByIdAsync(model.PaymentProviderId, cancellationToken);
        if (provider is null)
        {
            return Result<PaymentMethodOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentProviderNotFound, model.PaymentProviderId],
                AppHttpStatusCodes.NotFound);
        }

        if (scopedProviderPartnerId.HasValue && provider.PartnerId != scopedProviderPartnerId.Value)
        {
            return Result<PaymentMethodOutputModel>.Failure(
                localizer[LocalizationKeys.UserManagementForbidden],
                AppHttpStatusCodes.Forbidden);
        }

        if (await unitOfWork.PaymentMethodRepository.ExistsByCodeAsync(code, cancellationToken))
        {
            return Result<PaymentMethodOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentMethodCodeAlreadyExists, code],
                AppHttpStatusCodes.Conflict);
        }

        var duplicatedCurrencyIds = model.Currencies
            .GroupBy(x => x.CurrencyId)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToArray();
        if (duplicatedCurrencyIds.Length > 0)
        {
            return Result<PaymentMethodOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentMethodCurrencyDuplicateCurrencies, string.Join(", ", duplicatedCurrencyIds)],
                AppHttpStatusCodes.Conflict);
        }

        // validate referenced bank accounts + currencies
        foreach (var currency in model.Currencies)
        {
            if (!Enum.IsDefined(typeof(PaymentFeesTypeEnum), currency.FeesType) ||
                !Enum.IsDefined(typeof(PaymentCostTypeEnum), currency.CostType))
            {
                return Result<PaymentMethodOutputModel>.Failure(
                    localizer[LocalizationKeys.BillerAddInvalidRequest],
                    AppHttpStatusCodes.BadRequest);
            }
            var currencyEntity = CachingFactory.GetValue<CurrencyEntity>(x => x.Id == currency.CurrencyId).FirstOrDefault();
            if (currencyEntity is null)
            {
                return Result<PaymentMethodOutputModel>.Failure(
                    localizer[LocalizationKeys.CommonKeys.CurrencyNotFound, currency.CurrencyId],
                    AppHttpStatusCodes.NotFound);
            }

            if (currency.BankAccountId.HasValue)
            {
                var bankAccount = await unitOfWork.AccountRepository.GetByIdAsync(currency.BankAccountId.Value, cancellationToken);
                if (bankAccount is null)
                {
                    return Result<PaymentMethodOutputModel>.Failure(
                        localizer[LocalizationKeys.BillingServiceBankAccountNotFound, currency.BankAccountId.Value],
                        AppHttpStatusCodes.NotFound);
                }

                if (bankAccount.AccountTypeId != (int)AccountTypesEnum.BANK)
                {
                    return Result<PaymentMethodOutputModel>.Failure(
                        localizer[LocalizationKeys.BillingServiceBankAccountInvalidType, currency.BankAccountId.Value],
                        AppHttpStatusCodes.BadRequest);
                }
            }
        }

        var configJson = model.Config is null ? null : JsonSerializer.Serialize(model.Config, ConfigSerializerOptions);
        var compatibilityProbe = PaymentMethodEntity.Create(
            model.PaymentCategoryId,
            model.PaymentProviderId,
            code,
            name,
            PaymentMethodEnum.UNKNOWN,
            (PaymentInitModeEnum)model.PaymentInitMode,
            (ConfirmationModeEnum)model.ConfirmationMode,
            (CompletionModeEnum)model.CompletionMode,
            (PaymentMethodPurposeEnum)model.PaymentMethodPurpose,
            model.Active);
        compatibilityProbe.SetConfirmationType((ConfirmationTypeEnum)model.ConfirmationType);
        compatibilityProbe.SetConfig(configJson);
        if (compatibilityProbe.ValidateCompatibility() < 0)
        {
            return Result<PaymentMethodOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentMethodIncompatible, name],
                AppHttpStatusCodes.BadRequest);
        }

        var createdId = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var entity = PaymentMethodEntity.Create(
                model.PaymentCategoryId,
                model.PaymentProviderId,
                code,
                name,
                PaymentMethodEnum.UNKNOWN,
                (PaymentInitModeEnum)model.PaymentInitMode,
                (ConfirmationModeEnum)model.ConfirmationMode,
                (CompletionModeEnum)model.CompletionMode,
                (PaymentMethodPurposeEnum)model.PaymentMethodPurpose,
                model.Active);

            entity.SetConfirmationType((ConfirmationTypeEnum)model.ConfirmationType);
            entity.SetConfig(configJson);

            unitOfWork.PaymentMethodRepository.Add(entity);

            var currencies = model.Currencies.Select(c =>
                PaymentMethodCurrencyEntity.CreateForPaymentMethod(
                    c.CurrencyId,
                    entity,
                    c.Min,
                    c.Max,
                    Enum.Parse<PaymentFeesTypeEnum>(c.FeesType),
                    c.FeesValue,
                    Enum.Parse<PaymentCostTypeEnum>(c.CostType),
                    c.CostValue,
                    c.AutoSettleJournal,
                    c.BankAccountId,
                    c.VAT,
                    c.Scale,
                    c.Active)).ToList();

            unitOfWork.PaymentMethodCurrencyRepository.AddRange(currencies);

            await unitOfWork.CompleteAsync();
            return entity.Id;
        }, cancellationToken);

        var created = await unitOfWork.PaymentMethodRepository.GetByIdWithRelationsAsync(createdId);
        return Result<PaymentMethodOutputModel>.Success(ToOutputModel(created!), AppHttpStatusCodes.Created);
    }

    public async Task<Result<PaymentMethodOutputModel>> UpdateAsync(
        long id,
        UpdatePaymentMethodInputModel model,
        CancellationToken cancellationToken = default)
    {
        var managementScopeResult = await paymentMethodAccessPolicy.GetPaymentMethodManagementScopeAsync(cancellationToken);
        if (!managementScopeResult.IsSuccess)
        {
            return Result<PaymentMethodOutputModel>.Failure(managementScopeResult.Message!, managementScopeResult.StatusCode);
        }
        var scopedProviderPartnerId = managementScopeResult.Value;

        var entity = await unitOfWork.PaymentMethodRepository.GetByIdWithRelationsAsync(id);
        if (entity is null)
        {
            return Result<PaymentMethodOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentMethodNotFound, id],
                AppHttpStatusCodes.NotFound);
        }

        if (scopedProviderPartnerId.HasValue && entity.PaymentProvider?.PartnerId != scopedProviderPartnerId.Value)
        {
            return Result<PaymentMethodOutputModel>.Failure(
                localizer[LocalizationKeys.UserManagementForbidden],
                AppHttpStatusCodes.Forbidden);
        }

        var code = model.Code?.Trim() ?? string.Empty;
        var name = model.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name) || model.Currencies.Count == 0)
        {
            return Result<PaymentMethodOutputModel>.Failure(
                localizer[LocalizationKeys.BillerAddInvalidRequest],
                AppHttpStatusCodes.BadRequest);
        }

        if (await unitOfWork.PaymentMethodRepository.ExistsByCodeExceptIdAsync(code, id, cancellationToken))
        {
            return Result<PaymentMethodOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentMethodCodeAlreadyExists, code],
                AppHttpStatusCodes.Conflict);
        }

        if (!Enum.IsDefined(typeof(PaymentInitModeEnum), model.PaymentInitMode) ||
            !Enum.IsDefined(typeof(ConfirmationModeEnum), model.ConfirmationMode) ||
            !Enum.IsDefined(typeof(CompletionModeEnum), model.CompletionMode) ||
            !Enum.IsDefined(typeof(PaymentMethodPurposeEnum), model.PaymentMethodPurpose) ||
            !Enum.IsDefined(typeof(ConfirmationTypeEnum), model.ConfirmationType))
        {
            return Result<PaymentMethodOutputModel>.Failure(
                localizer[LocalizationKeys.BillerAddInvalidRequest],
                AppHttpStatusCodes.BadRequest);
        }

        var duplicatedCurrencyIds = model.Currencies
            .GroupBy(x => x.CurrencyId)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToArray();
        if (duplicatedCurrencyIds.Length > 0)
        {
            return Result<PaymentMethodOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentMethodCurrencyDuplicateCurrencies, string.Join(", ", duplicatedCurrencyIds)],
                AppHttpStatusCodes.Conflict);
        }

        foreach (var currency in model.Currencies)
        {
            if (!Enum.IsDefined(typeof(PaymentFeesTypeEnum), currency.FeesType) ||
                !Enum.IsDefined(typeof(PaymentCostTypeEnum), currency.CostType))
            {
                return Result<PaymentMethodOutputModel>.Failure(
                    localizer[LocalizationKeys.BillerAddInvalidRequest],
                    AppHttpStatusCodes.BadRequest);
            }
            
            var currencyEntity = CachingFactory.GetValue<CurrencyEntity>(x => x.Id == currency.CurrencyId).FirstOrDefault();
            if (currencyEntity is null)
            {
                return Result<PaymentMethodOutputModel>.Failure(
                    localizer[LocalizationKeys.CommonKeys.CurrencyNotFound, currency.CurrencyId],
                    AppHttpStatusCodes.NotFound);
            }

            if (currency.BankAccountId.HasValue)
            {
                var bankAccount = await unitOfWork.AccountRepository.GetByIdAsync(currency.BankAccountId.Value, cancellationToken);
                if (bankAccount is null)
                {
                    return Result<PaymentMethodOutputModel>.Failure(
                        localizer[LocalizationKeys.BillingServiceBankAccountNotFound, currency.BankAccountId.Value],
                        AppHttpStatusCodes.NotFound);
                }

                if (bankAccount.AccountTypeId != (int)AccountTypesEnum.BANK)
                {
                    return Result<PaymentMethodOutputModel>.Failure(
                        localizer[LocalizationKeys.BillingServiceBankAccountInvalidType, currency.BankAccountId.Value],
                        AppHttpStatusCodes.BadRequest);
                }
            }
        }

        var configJson = model.Config is null ? null : JsonSerializer.Serialize(model.Config, ConfigSerializerOptions);
        var targetProvider = await unitOfWork.PaymentProviderRepository.GetByIdAsync(model.PaymentProviderId, cancellationToken);
        if (targetProvider is null)
        {
            return Result<PaymentMethodOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentProviderNotFound, model.PaymentProviderId],
                AppHttpStatusCodes.NotFound);
        }

        if (scopedProviderPartnerId.HasValue && targetProvider.PartnerId != scopedProviderPartnerId.Value)
        {
            return Result<PaymentMethodOutputModel>.Failure(
                localizer[LocalizationKeys.UserManagementForbidden],
                AppHttpStatusCodes.Forbidden);
        }

        entity.Update(
            model.PaymentCategoryId,
            model.PaymentProviderId,
            code,
            name,
            (PaymentInitModeEnum)model.PaymentInitMode,
            (ConfirmationModeEnum)model.ConfirmationMode,
            (CompletionModeEnum)model.CompletionMode,
            (PaymentMethodPurposeEnum)model.PaymentMethodPurpose,
            (ConfirmationTypeEnum)model.ConfirmationType,
            model.Active,
            configJson);
        if (entity.ValidateCompatibility() < 0)
        {
            return Result<PaymentMethodOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentMethodIncompatible, name],
                AppHttpStatusCodes.BadRequest);
        }

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            unitOfWork.PaymentMethodRepository.Update(entity);

            var existingByCurrency = entity.PaymentMethodCurrencies.ToDictionary(x => x.CurrencyId, x => x);
            foreach (var currencyModel in model.Currencies)
            {
                if (existingByCurrency.TryGetValue(currencyModel.CurrencyId, out var existingCurrency))
                {
                    existingCurrency.Update(
                        currencyModel.Min,
                        currencyModel.Max,
                        Enum.Parse<PaymentFeesTypeEnum>(currencyModel.FeesType),
                        currencyModel.FeesValue,
                        Enum.Parse<PaymentCostTypeEnum>(currencyModel.CostType),
                        currencyModel.CostValue,
                        currencyModel.AutoSettleJournal,
                        currencyModel.BankAccountId,
                        currencyModel.VAT,
                        currencyModel.Scale,
                        currencyModel.Active,
                        currencyModel.MinimumAgeMinutes);
                    unitOfWork.PaymentMethodCurrencyRepository.Update(existingCurrency);
                    continue;
                }

                var newCurrency = PaymentMethodCurrencyEntity.CreateForPaymentMethod(
                    currencyModel.CurrencyId,
                    entity,
                    currencyModel.Min,
                    currencyModel.Max,
                    Enum.Parse<PaymentFeesTypeEnum>(currencyModel.FeesType),
                    currencyModel.FeesValue,
                    Enum.Parse<PaymentCostTypeEnum>(currencyModel.CostType),
                    currencyModel.CostValue,
                    currencyModel.AutoSettleJournal,
                    currencyModel.BankAccountId,
                    currencyModel.VAT,
                    currencyModel.Scale,
                    currencyModel.Active);
                unitOfWork.PaymentMethodCurrencyRepository.Add(newCurrency);
            }

            var currencyIdsInRequest = model.Currencies.Select(x => x.CurrencyId).ToHashSet();
            var currenciesToRemove = entity.PaymentMethodCurrencies.Where(x => !currencyIdsInRequest.Contains(x.CurrencyId)).ToList();
            if (currenciesToRemove.Count > 0)
            {
                unitOfWork.PaymentMethodCurrencyRepository.RemoveRange(currenciesToRemove);
            }

            await unitOfWork.CompleteAsync();
        }, cancellationToken);

        var updated = await unitOfWork.PaymentMethodRepository.GetByIdWithRelationsAsync(id);
        return Result<PaymentMethodOutputModel>.Success(ToOutputModel(updated!));
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var managementScopeResult = await paymentMethodAccessPolicy.GetPaymentMethodManagementScopeAsync(cancellationToken);
        if (!managementScopeResult.IsSuccess)
        {
            return Result<bool>.Failure(managementScopeResult.Message!, managementScopeResult.StatusCode);
        }
        var scopedProviderPartnerId = managementScopeResult.Value;

        var entity = await unitOfWork.PaymentMethodRepository.GetByIdWithRelationsAsync(id);
        if (entity is null)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.PaymentMethodNotFound, id],
                AppHttpStatusCodes.NotFound);
        }

        if (scopedProviderPartnerId.HasValue && entity.PaymentProvider?.PartnerId != scopedProviderPartnerId.Value)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.UserManagementForbidden],
                AppHttpStatusCodes.Forbidden);
        }

        if (entity.PaymentMethodCurrencies.Count > 0)
        {
            unitOfWork.PaymentMethodCurrencyRepository.RemoveRange(entity.PaymentMethodCurrencies);
        }

        unitOfWork.PaymentMethodRepository.Remove(entity);
        await unitOfWork.CompleteAsync();
        return Result<bool>.Success(true);
    }

    private static PaymentMethodOutputModel ToOutputModel(PaymentMethodEntity entity)
    {
        JsonElement? config = null;
        if (!string.IsNullOrWhiteSpace(entity.Config))
        {
            config = JsonDocument.Parse(entity.Config).RootElement.Clone();
        }

        var currencies = entity.PaymentMethodCurrencies
            .Select(c => new PaymentMethodCurrencyOutputModel(
                c.Id,
                c.CurrencyId,
                c.Min,
                c.Max,
                (int)c.FeesType,
                c.FeesValue,
                (int)c.CostType,
                c.CostValue,
                c.AutoSettleJournal,
                c.BankAccountId,
                c.VAT,
                c.Scale,
                c.Active))
            .ToList();

        return new PaymentMethodOutputModel(
            entity.Id,
            entity.PaymentCategoryId,
            entity.PaymentProviderId,
            entity.Code,
            entity.Name,
            entity.PaymentInitModeId,
            entity.ConfirmationModeId,
            entity.CompletionModeId,
            entity.PaymentMethodPurposeId,
            entity.ConfirmationTypeId,
            entity.Active,
            config,
            currencies,
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc,
            entity.CreatedBy ?? string.Empty,
            entity.UpdatedBy ?? string.Empty);
    }
}
