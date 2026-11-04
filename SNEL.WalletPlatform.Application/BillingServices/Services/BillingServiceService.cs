using AutoMapper;
using Medallion.Threading;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Logging;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.BillingServices.Models.Input;
using SNEL.WalletPlatform.Application.BillingServices.Models.Internal;
using SNEL.WalletPlatform.Application.BillingServices.Models.Output;
using SNEL.WalletPlatform.Application.Caching;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Enums;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Jobs;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Accounting.Enums;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Invoicing;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.BillingServices.Services;

public sealed class BillingServiceService(
    IUnitOfWork unitOfWork,
    IBillerConnectorResolver connectorResolver,
    IBillingInvoiceRefResolver billingInvoiceRefResolver,
    IBillingServiceAccessPolicy billingServiceAccessPolicy,
    IStringLocalizer<SharedResource> localizer,
    ILogger<BillingServiceService> logger,
    IMapper mapper,
    ISchedulerFactory schedulerFactory,
    IDistributedLockProvider distributedLockProvider) : IBillingServiceService
{
    public async Task<Result<BillingServiceOutputModel>> CreateAsync(
        CreateBillingServiceInputModel model,
        CancellationToken cancellationToken = default)
    {
        var managementScopeResult = await billingServiceAccessPolicy.GetBillingServiceManagementScopeAsync(cancellationToken);
        if (!managementScopeResult.IsSuccess)
        {
            return Result<BillingServiceOutputModel>.Failure(managementScopeResult.Message!, managementScopeResult.StatusCode);
        }
        var scopedPartnerId = managementScopeResult.Value;

        var code = model.Code?.Trim() ?? string.Empty;
        var name = model.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name) || model.Currencies.Count == 0)
        {
            return Result<BillingServiceOutputModel>.Failure(
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
            return Result<BillingServiceOutputModel>.Failure(
                localizer[LocalizationKeys.BillingServiceDuplicateCurrencies, string.Join(", ", duplicatedCurrencyIds)],
                AppHttpStatusCodes.Conflict);
        }

        var normalizedBillerConfigs = model.BillerConfigs
            .Select(config => new
            {
                config.Id,
                Name = config.Name?.Trim() ?? string.Empty,
                BillerName = config.BillerName?.Trim() ?? string.Empty,
                Value = config.Value?.Trim() ?? string.Empty
            })
            .ToList();
        if (normalizedBillerConfigs.Any(config => config.Id.HasValue))
        {
            return Result<BillingServiceOutputModel>.Failure(
                localizer[LocalizationKeys.BillerAddInvalidRequest],
                AppHttpStatusCodes.BadRequest);
        }
        if (normalizedBillerConfigs.Any(config =>
                string.IsNullOrWhiteSpace(config.Name) ||
                string.IsNullOrWhiteSpace(config.BillerName) ||
                string.IsNullOrWhiteSpace(config.Value)))
        {
            return Result<BillingServiceOutputModel>.Failure(
                localizer[LocalizationKeys.BillerAddInvalidRequest],
                AppHttpStatusCodes.BadRequest);
        }

        var duplicatedConfigNames = normalizedBillerConfigs
            .GroupBy(config => config.Name, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        if (duplicatedConfigNames.Length > 0)
        {
            return Result<BillingServiceOutputModel>.Failure(
                localizer[LocalizationKeys.BillerAddInvalidRequest],
                AppHttpStatusCodes.Conflict);
        }

        var biller = await unitOfWork.BillerRepository.GetByIdAsync(model.BillerId, cancellationToken);
        if (biller is null)
        {
            return Result<BillingServiceOutputModel>.Failure(
                localizer[LocalizationKeys.BillerNotFound, model.BillerId],
                AppHttpStatusCodes.NotFound);
        }

        if (scopedPartnerId.HasValue && biller.PartnerId != scopedPartnerId.Value)
        {
            return Result<BillingServiceOutputModel>.Failure(
                localizer[LocalizationKeys.UserManagementForbidden],
                AppHttpStatusCodes.Forbidden);
        }

        if (await unitOfWork.BillingServiceRepository.ExistsByCodeAsync(model.BillerId, code))
        {
            return Result<BillingServiceOutputModel>.Failure(
                localizer[LocalizationKeys.BillingServiceCodeAlreadyExists, code],
                AppHttpStatusCodes.Conflict);
        }

        foreach (var currency in model.Currencies)
        {
            if (!Enum.IsDefined(typeof(BillingCommissionTypeEnum), currency.CommissionType) ||
                !Enum.IsDefined(typeof(BillingFeesTypeEnum), currency.FeesType) ||
                !Enum.IsDefined(typeof(BillingCostTypeEnum), currency.CostType) ||
                !Enum.IsDefined(typeof(ApplyTypeEnum), currency.ApplyType))
            {
                return Result<BillingServiceOutputModel>.Failure(
                    localizer[LocalizationKeys.BillerAddInvalidRequest],
                    AppHttpStatusCodes.BadRequest);
            }            
            var currencyEntity = CachingFactory.GetValue<CurrencyEntity>(x => x.Id == currency.CurrencyId).FirstOrDefault();
            if (currencyEntity is null)
            {
                return Result<BillingServiceOutputModel>.Failure(
                    localizer[LocalizationKeys.CommonKeys.CurrencyNotFound, currency.CurrencyId],
                    AppHttpStatusCodes.NotFound);
            }

            if (currency.BankAccountId == Guid.Empty)
            {
                return Result<BillingServiceOutputModel>.Failure(
                    localizer[LocalizationKeys.BillingServiceBankAccountRequired],
                    AppHttpStatusCodes.BadRequest);
            }

            var bankAccount = await unitOfWork.AccountRepository.GetByIdAsync(currency.BankAccountId, cancellationToken);
            if (bankAccount is null)
            {
                return Result<BillingServiceOutputModel>.Failure(
                    localizer[LocalizationKeys.BillingServiceBankAccountNotFound, currency.BankAccountId],
                    AppHttpStatusCodes.NotFound);
            }

            if (bankAccount.AccountTypeId != (int)AccountTypesEnum.BANK)
            {
                return Result<BillingServiceOutputModel>.Failure(
                    localizer[LocalizationKeys.BillingServiceBankAccountInvalidType, currency.BankAccountId],
                    AppHttpStatusCodes.BadRequest);
            }
        }

        var createdId = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var entity = BillingServiceEntity.Create(
                model.BillingCategoryId,
                model.BillerId,
                model.IsActive,
                code,
                name,
                model.ServiceTypeId,
                model.SupportPartial,
                model.SupportInquiry,
                model.SupportCustomActions,
                model.IsTransactional,
                model.BillingRefFormat);

            unitOfWork.BillingServiceRepository.Add(entity);

            var currencyById = model.Currencies.ToDictionary(x => x.CurrencyId, x => x);
            var billingServiceCurrencies = currencyById.Values.Select(currency =>
            {
                var currencyEntity = BillingServiceCurrencyEntity.CreateForBillingService(
                    currency.CurrencyId,
                    entity,
                    currency.Min,
                    currency.Max,
                    Enum.Parse<BillingCommissionTypeEnum>(currency.CommissionType),
                    currency.CommissionValue,
                    Enum.Parse<BillingFeesTypeEnum>(currency.FeesType),
                    currency.FeesValue,
                    Enum.Parse<BillingCostTypeEnum>(currency.CostType),
                    currency.CostValue,
                    currency.VAT,
                    currency.AutoSettleJournal,
                    currency.BankAccountId,
                    currency.Scale,
                    currency.Active);
                currencyEntity.ApplyType = (ApplyTypeEnum)currency.ApplyType;
                return currencyEntity;
            }).ToList();

            unitOfWork.BillingServiceCurrencyRepository.AddRange(billingServiceCurrencies);

            foreach (var config in normalizedBillerConfigs)
            {
                await unitOfWork.BillerConfigRepository.UpsertAsync(
                    code,
                    config.Name,
                    config.BillerName,
                    config.Value,
                    ct);
            }

            await unitOfWork.CompleteAsync();

            return entity.Id;
        }, cancellationToken);

        var created = await unitOfWork.BillingServiceRepository.GetByIdWithRelationsAsync(createdId);
        var createdModel = await EnrichWithBillerConfigsAsync(
            mapper.Map<BillingServiceOutputModel>(created!),
            cancellationToken);
        return Result<BillingServiceOutputModel>.Success(
            createdModel,
            localizer[LocalizationKeys.BillingServiceCreateSuccess],
            AppHttpStatusCodes.Created);
    }

    public async Task<Result<IReadOnlyCollection<BillingServiceOutputModel>>> GetBillingServicesAsync(
        string languageCode = "en",
        bool? isActive = true,
        CancellationToken cancellationToken = default)
    {
        var readScopeResult = await billingServiceAccessPolicy.GetBillingServicesReadScopeAsync(cancellationToken);
        if (!readScopeResult.IsSuccess)
        {
            return Result<IReadOnlyCollection<BillingServiceOutputModel>>.Failure(readScopeResult.Message!, readScopeResult.StatusCode);
        }

        var entities = await unitOfWork.BillingServiceRepository
            .GetAllWithRelationsScopedByBillerPartnerAsync(readScopeResult.Value, isActive, cancellationToken);
        var models = mapper.Map<IReadOnlyCollection<BillingServiceOutputModel>>(entities);//,opt=>opt.Items["LanguageCode"] = languageCode);
        var enrichedModels = await EnrichWithBillerConfigsAsync(models, cancellationToken);
        return Result<IReadOnlyCollection<BillingServiceOutputModel>>.Success(enrichedModels);
    }

    public async Task<Result<BillingServiceOutputModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.BillingServiceRepository.GetByIdWithRelationsAsync(id);
        if (entity is null)
        {
            return Result<BillingServiceOutputModel>.Failure(
                localizer[LocalizationKeys.BillingInquiryServiceIdNotFound, id],
                AppHttpStatusCodes.NotFound);
        }

        var model = await EnrichWithBillerConfigsAsync(
            mapper.Map<BillingServiceOutputModel>(entity),
            cancellationToken);
        return Result<BillingServiceOutputModel>.Success(model);
    }
    public async Task<Result<BillingServiceOutputModel>> GetByCodeAsync(string BillerCode, string BillingSeriveCode, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.BillingServiceRepository.GetByBillerCodeAndBillingServiceCodeWithCurrenciesAsync(BillerCode, BillingSeriveCode);
        if (entity is null)
        {
            return Result<BillingServiceOutputModel>.Failure(
                localizer[LocalizationKeys.BillingInquiryServiceCodeNotFound, BillerCode, BillingSeriveCode],
                AppHttpStatusCodes.NotFound);
        }

        var model = await EnrichWithBillerConfigsAsync(
            mapper.Map<BillingServiceOutputModel>(entity),
            cancellationToken);
        return Result<BillingServiceOutputModel>.Success(model);
    }
    public async Task<Result<BillingServiceOutputModel>> UpdateAsync(
        long id,
        UpdateBillingServiceInputModel model,
        CancellationToken cancellationToken = default)
    {
        var managementScopeResult = await billingServiceAccessPolicy.GetBillingServiceManagementScopeAsync(cancellationToken);
        if (!managementScopeResult.IsSuccess)
        {
            return Result<BillingServiceOutputModel>.Failure(managementScopeResult.Message!, managementScopeResult.StatusCode);
        }
        var scopedPartnerId = managementScopeResult.Value;

        var entity = await unitOfWork.BillingServiceRepository.GetByIdWithRelationsAsync(id);
        if (entity is null)
        {
            return Result<BillingServiceOutputModel>.Failure(
                localizer[LocalizationKeys.BillingInquiryServiceIdNotFound, id],
                AppHttpStatusCodes.NotFound);
        }

        if (scopedPartnerId.HasValue && entity.Biller.PartnerId != scopedPartnerId.Value)
        {
            return Result<BillingServiceOutputModel>.Failure(
                localizer[LocalizationKeys.UserManagementForbidden],
                AppHttpStatusCodes.Forbidden);
        }

        var code = model.Code?.Trim() ?? string.Empty;
        var name = model.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name) || model.Currencies.Count == 0)
        {
            return Result<BillingServiceOutputModel>.Failure(
                localizer[LocalizationKeys.BillerAddInvalidRequest],
                AppHttpStatusCodes.BadRequest);
        }

        if (await unitOfWork.BillingServiceRepository.ExistsByCodeExceptIdAsync(model.BillerId, code, id))
        {
            return Result<BillingServiceOutputModel>.Failure(
                localizer[LocalizationKeys.BillingServiceCodeAlreadyExists, code],
                AppHttpStatusCodes.Conflict);
        }

        var targetBiller = await unitOfWork.BillerRepository.GetByIdAsync(model.BillerId, cancellationToken);
        if (targetBiller is null)
        {
            return Result<BillingServiceOutputModel>.Failure(
                localizer[LocalizationKeys.BillerNotFound, model.BillerId],
                AppHttpStatusCodes.NotFound);
        }

        if (scopedPartnerId.HasValue && targetBiller.PartnerId != scopedPartnerId.Value)
        {
            return Result<BillingServiceOutputModel>.Failure(
                localizer[LocalizationKeys.UserManagementForbidden],
                AppHttpStatusCodes.Forbidden);
        }

        var duplicatedCurrencyIds = model.Currencies
            .GroupBy(x => x.CurrencyId)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToArray();
        if (duplicatedCurrencyIds.Length > 0)
        {
            return Result<BillingServiceOutputModel>.Failure(
                localizer[LocalizationKeys.BillingServiceDuplicateCurrencies, string.Join(", ", duplicatedCurrencyIds)],
                AppHttpStatusCodes.Conflict);
        }

        var normalizedBillerConfigs = model.BillerConfigs
            .Select(config => new
            {
                config.Id,
                Name = config.Name?.Trim() ?? string.Empty,
                BillerName = config.BillerName?.Trim() ?? string.Empty,
                Value = config.Value?.Trim() ?? string.Empty
            })
            .ToList();
        if (normalizedBillerConfigs.Any(config =>
                string.IsNullOrWhiteSpace(config.Name) ||
                string.IsNullOrWhiteSpace(config.BillerName) ||
                string.IsNullOrWhiteSpace(config.Value)))
        {
            return Result<BillingServiceOutputModel>.Failure(
                localizer[LocalizationKeys.BillerAddInvalidRequest],
                AppHttpStatusCodes.BadRequest);
        }

        var duplicatedConfigNames = normalizedBillerConfigs
            .GroupBy(config => config.Name, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        if (duplicatedConfigNames.Length > 0)
        {
            return Result<BillingServiceOutputModel>.Failure(
                localizer[LocalizationKeys.BillerAddInvalidRequest],
                AppHttpStatusCodes.Conflict);
        }

        var duplicatedConfigIds = normalizedBillerConfigs
            .Where(config => config.Id.HasValue)
            .GroupBy(config => config.Id!.Value)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        if (duplicatedConfigIds.Length > 0)
        {
            return Result<BillingServiceOutputModel>.Failure(
                localizer[LocalizationKeys.BillerAddInvalidRequest],
                AppHttpStatusCodes.Conflict);
        }

        if (model.Currencies.Any(currency =>
                !Enum.IsDefined(typeof(BillingCommissionTypeEnum), currency.CommissionType) ||
                !Enum.IsDefined(typeof(BillingFeesTypeEnum), currency.FeesType) ||
                !Enum.IsDefined(typeof(BillingCostTypeEnum), currency.CostType) ||
                !Enum.IsDefined(typeof(ApplyTypeEnum), currency.ApplyType)))
        {
            return Result<BillingServiceOutputModel>.Failure(
                localizer[LocalizationKeys.BillerAddInvalidRequest],
                AppHttpStatusCodes.BadRequest);
        }

        var previousCode = entity.Code;
        entity.Update(
            model.BillingCategoryId,
            model.IsActive,
            code,
            name,
            model.ServiceTypeId,
            model.SupportPartial,
            model.SupportInquiry,
            model.SupportCustomActions,
            model.IsTransactional,
            model.BillingRefFormat);
        unitOfWork.BillingServiceRepository.Update(entity);

        var existingByCurrency = entity.Currencies.ToDictionary(x => x.CurrencyId, x => x);
        foreach (var currencyModel in model.Currencies)
        {
            var currencyEntity = CachingFactory.GetValue<CurrencyEntity>(x => x.Id == currencyModel.CurrencyId).FirstOrDefault();
            if (currencyEntity is null)
            {
                return Result<BillingServiceOutputModel>.Failure(
                    localizer[LocalizationKeys.CommonKeys.CurrencyNotFound, currencyModel.CurrencyId],
                    AppHttpStatusCodes.NotFound);
            }

            if (currencyModel.BankAccountId == Guid.Empty)
            {
                return Result<BillingServiceOutputModel>.Failure(
                    localizer[LocalizationKeys.BillingServiceBankAccountRequired],
                    AppHttpStatusCodes.BadRequest);
            }

            var bankAccount = await unitOfWork.AccountRepository.GetByIdAsync(currencyModel.BankAccountId, cancellationToken);
            if (bankAccount is null)
            {
                return Result<BillingServiceOutputModel>.Failure(
                    localizer[LocalizationKeys.BillingServiceBankAccountNotFound, currencyModel.BankAccountId],
                    AppHttpStatusCodes.NotFound);
            }

            if (bankAccount.AccountTypeId != (int)AccountTypesEnum.BANK)
            {
                return Result<BillingServiceOutputModel>.Failure(
                    localizer[LocalizationKeys.BillingServiceBankAccountInvalidType, currencyModel.BankAccountId],
                    AppHttpStatusCodes.BadRequest);
            }

            if (existingByCurrency.TryGetValue(currencyModel.CurrencyId, out var existingCurrency))
            {
                existingCurrency.Update(
                    currencyModel.Min,
                    currencyModel.Max,
                    Enum.Parse<BillingCommissionTypeEnum>(currencyModel.CommissionType),
                    currencyModel.CommissionValue,
                    Enum.Parse<BillingFeesTypeEnum>(currencyModel.FeesType),
                    currencyModel.FeesValue,
                    Enum.Parse<BillingCostTypeEnum>(currencyModel.CostType) ,
                    currencyModel.CostValue,
                    currencyModel.VAT,
                    currencyModel.AutoSettleJournal,
                    currencyModel.BankAccountId,
                    currencyModel.Scale,
                    currencyModel.Active,
                    (ApplyTypeEnum)currencyModel.ApplyType);
                unitOfWork.BillingServiceCurrencyRepository.Update(existingCurrency);
                continue;
            }

            var newCurrency = BillingServiceCurrencyEntity.Create(
                currencyModel.CurrencyId,
                id,
                currencyModel.Min,
                currencyModel.Max,
                Enum.Parse<BillingCommissionTypeEnum>(currencyModel.CommissionType),
                currencyModel.CommissionValue,
                Enum.Parse<BillingFeesTypeEnum>(currencyModel.FeesType),
                currencyModel.FeesValue,
                Enum.Parse<BillingCostTypeEnum>(currencyModel.CostType),
                currencyModel.CostValue,
                currencyModel.VAT,
                currencyModel.AutoSettleJournal,
                currencyModel.BankAccountId,
                currencyModel.Scale,
                currencyModel.Active);
            newCurrency.ApplyType = (ApplyTypeEnum)currencyModel.ApplyType;
            unitOfWork.BillingServiceCurrencyRepository.Add(newCurrency);
        }

        var currencyIdsInRequest = model.Currencies.Select(x => x.CurrencyId).ToHashSet();
        var currenciesToRemove = entity.Currencies.Where(x => !currencyIdsInRequest.Contains(x.CurrencyId)).ToList();
        if (currenciesToRemove.Count > 0)
        {
            unitOfWork.BillingServiceCurrencyRepository.RemoveRange(currenciesToRemove);
        }

        var existingConfigs = await unitOfWork.BillerConfigRepository
            .GetByBillerCodeForUpdateAsync(previousCode, cancellationToken);
        var existingConfigById = existingConfigs.ToDictionary(config => config.Id, config => config);
        foreach (var config in normalizedBillerConfigs.Where(config => config.Id.HasValue))
        {
            if (!existingConfigById.ContainsKey(config.Id!.Value))
            {
                return Result<BillingServiceOutputModel>.Failure(
                    localizer[LocalizationKeys.BillerAddInvalidRequest],
                    AppHttpStatusCodes.BadRequest);
            }
        }

        foreach (var config in normalizedBillerConfigs)
        {
            await unitOfWork.BillerConfigRepository.UpsertAsync(
                code,
                config.Name,
                config.BillerName,
                config.Value,
                cancellationToken);
        }

        var requestConfigNames = normalizedBillerConfigs
            .Select(config => config.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var codeChanged = !string.Equals(previousCode, code, StringComparison.OrdinalIgnoreCase);
        var configsToRemove = existingConfigs
            .Where(config => codeChanged || !requestConfigNames.Contains(config.Name))
            .ToList();
        if (configsToRemove.Count > 0)
        {
            unitOfWork.BillerConfigRepository.RemoveRange(configsToRemove);
        }

        await unitOfWork.CompleteAsync();
        var updated = await unitOfWork.BillingServiceRepository.GetByIdWithRelationsAsync(id);
        var updatedModel = await EnrichWithBillerConfigsAsync(
            mapper.Map<BillingServiceOutputModel>(updated!),
            cancellationToken);
        return Result<BillingServiceOutputModel>.Success(
            updatedModel,
            localizer[LocalizationKeys.BillingServiceUpdateSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var managementScopeResult = await billingServiceAccessPolicy.GetBillingServiceManagementScopeAsync(cancellationToken);
        if (!managementScopeResult.IsSuccess)
        {
            return Result<bool>.Failure(managementScopeResult.Message!, managementScopeResult.StatusCode);
        }
        var scopedPartnerId = managementScopeResult.Value;

        var entity = await unitOfWork.BillingServiceRepository.GetByIdWithRelationsAsync(id);
        if (entity is null)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.BillingInquiryServiceIdNotFound, id],
                AppHttpStatusCodes.NotFound);
        }

        if (scopedPartnerId.HasValue && entity.Biller.PartnerId != scopedPartnerId.Value)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.UserManagementForbidden],
                AppHttpStatusCodes.Forbidden);
        }

        var hasInvoices = await unitOfWork.BillingServiceRepository.HasInvoicesAsync(id, cancellationToken);
        var hasSubscriptions = await unitOfWork.BillingServiceRepository.HasCustomerSubscriptionsAsync(id, cancellationToken);
        if (hasInvoices || hasSubscriptions)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.BillingServiceDeleteBlockedByReferences],
                AppHttpStatusCodes.Conflict);
        }

        if (entity.Currencies.Count > 0)
        {
            unitOfWork.BillingServiceCurrencyRepository.RemoveRange(entity.Currencies);
        }

        unitOfWork.BillingServiceRepository.Remove(entity);
        await unitOfWork.CompleteAsync();
        return Result<bool>.Success(true);
    }

    private async Task<BillingServiceOutputModel> EnrichWithBillerConfigsAsync(
        BillingServiceOutputModel model,
        CancellationToken cancellationToken)
    {
        var billingServiceCode = model.Code?.Trim();
        if (string.IsNullOrWhiteSpace(billingServiceCode))
        {
            return model with { BillerConfigs = [] };
        }

        var configs = await unitOfWork.BillerConfigRepository.GetByBillerCodeAsync(billingServiceCode, cancellationToken);
        var configModels = configs
            .Select(config => new BillerConfigOutputModel(config.Id, config.Name, config.BillerName, config.Value))
            .ToArray();

        return model with { BillerConfigs = configModels };
    }

    private async Task<IReadOnlyCollection<BillingServiceOutputModel>> EnrichWithBillerConfigsAsync(
        IReadOnlyCollection<BillingServiceOutputModel> models,
        CancellationToken cancellationToken)
    {
        var billingServiceCodes = models
            .Select(model => model.Code?.Trim())
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var configsByBillingServiceCode = new Dictionary<string, IReadOnlyCollection<BillerConfigOutputModel>>(StringComparer.OrdinalIgnoreCase);
        foreach (var billingServiceCode in billingServiceCodes)
        {
            var configs = await unitOfWork.BillerConfigRepository.GetByBillerCodeAsync(billingServiceCode, cancellationToken);
            configsByBillingServiceCode[billingServiceCode] = configs
                .Select(config => new BillerConfigOutputModel(config.Id, config.Name, config.BillerName, config.Value))
                .ToArray();
        }

        return models
            .Select(model =>
            {
                var code = model.Code?.Trim();
                return !string.IsNullOrWhiteSpace(code) && configsByBillingServiceCode.TryGetValue(code, out var billerConfigs)
                    ? model with { BillerConfigs = billerConfigs }
                    : model with { BillerConfigs = [] };
            })
            .ToArray();
    }

    public async Task<Result<bool>> ApplyPayment(PaymentEntity? pmnt = null, Guid? paymentId = null, string? internalRef = null, string? externalRef = null)
    {
        var providedCount =
            (paymentId.HasValue ? 1 : 0) +
            (!string.IsNullOrWhiteSpace(internalRef) ? 1 : 0) +
            (!string.IsNullOrWhiteSpace(externalRef) ? 1 : 0) +
            (pmnt != null ? 1 : 0);

        if (providedCount != 1)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.BillingApplyIdentifierRequired],
                AppHttpStatusCodes.BadRequest);
        }

        // Getting the payment entity
        var payment = pmnt != null
            ? pmnt
            : paymentId.HasValue
                ? await unitOfWork.PaymentRepository.GetByIdWithApplyContextAsync(paymentId.Value)
                : !string.IsNullOrWhiteSpace(internalRef)
                    ? await unitOfWork.PaymentRepository.GetByPaymentRefWithApplyContextAsync(internalRef.Trim())
                    : await unitOfWork.PaymentRepository.GetByExternalRefWithApplyContextAsync(externalRef!.Trim());

        if (payment is null)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.BillingApplyPaymentNotFound, paymentId ?? Guid.Empty],
                AppHttpStatusCodes.NotFound);
        }

        if (payment.Status != PaymentStatus.Completed)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.BillingApplyPaymentNotCompleted, payment.Id],
                AppHttpStatusCodes.BadRequest);
        }
        if (payment.Direction != PaymentDirectionEnum.OUT)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.PaymentKeys.NotApplicable, payment.Id],
                AppHttpStatusCodes.BadRequest);
        }

        await ApplyLoadedPaymentsInternalAsync(payment.PaymentOrder!.Items.ToList());

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// This to be used in payment flow if completion is immediate, as saving data (POI status and external ref) is on caller.
    /// </summary>
    /// <param name="payment"></param>
    /// <returns></returns>
    public async Task<Result<bool>> ApplyPaymentAfterCompleteAsync(PaymentEntity? payment = null)
    {
        if (payment is null)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.BillingApplyPaymentNull],
                AppHttpStatusCodes.NotFound);
        }

        if (payment.Status != PaymentStatus.Completed)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.BillingApplyPaymentNotCompleted, payment.Id],
                AppHttpStatusCodes.BadRequest);
        }
        if (payment.Direction != PaymentDirectionEnum.OUT)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.PaymentKeys.NotApplicable, payment.Id],
                AppHttpStatusCodes.BadRequest);
        }
        var poisOnPayApply = new List<PaymentOrderItemEntity>();
        var poisAfterPayApply = new List<PaymentOrderItemEntity>();
        foreach (var item in payment.PaymentOrder!.Items)
        {
            var billingServiceCurrency = item.BillingService!.Currencies.FirstOrDefault(c => c.CurrencyId == item.CurrencyId);
            switch (billingServiceCurrency?.ApplyType ?? ApplyTypeEnum.CRON_JOB)
            {
                case ApplyTypeEnum.ON_PAY:
                    poisOnPayApply.Add(item);
                    break;
                case ApplyTypeEnum.AFTER_PAY:
                    poisAfterPayApply.Add(item);
                    break;
                case ApplyTypeEnum.CRON_JOB:
                default:
                    break;
            }
        }
        int result = 0;
        if (poisOnPayApply.Count > 0)
        {
            var res = await ApplyLoadedPaymentsInternalAsync(poisOnPayApply);
            result = res.Value;
        }
        if (poisAfterPayApply.Count > 0)
        {
            await TriggerApplyPOIsJobAsync(payment.Id);
        }
        return Result<bool>.Success(result > 0);
    }

    private async Task TriggerApplyPOIsJobAsync(Guid paymentId)
    {
        var scheduler = await schedulerFactory.GetScheduler();
        // throws if job was not registered on startup
        if (!await scheduler.CheckExists(ApplyPOIsAfterPayJobConstants.JobKey))
        {
            throw new InvalidOperationException(
                localizer[LocalizationKeys.PaymentQuartzJobNotRegistered, ApplyPOIsAfterPayJobConstants.JobGroup, ApplyPOIsAfterPayJobConstants.JobName].Value);
        }

        var triggerKey = ApplyPOIsAfterPayJobConstants.CreateTriggerKey(paymentId);
        // Removes old registered job if exists
        if (await scheduler.CheckExists(triggerKey))
        {
            await scheduler.UnscheduleJob(triggerKey);
        }

        var trigger = TriggerBuilder.Create()
            .WithIdentity(triggerKey)
            .ForJob(ApplyPOIsAfterPayJobConstants.JobKey)
            .UsingJobData(ApplyPOIsAfterPayJobConstants.PaymentIdKey,paymentId.ToString())
            .StartAt(DateBuilder.FutureDate(
                ApplyPOIsAfterPayJobConstants.InitialDelaySeconds, // 60
                IntervalUnit.Second))
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(ApplyPOIsAfterPayJobConstants.IntervalSeconds) // 30
                .WithRepeatCount(ApplyPOIsAfterPayJobConstants.TotalAttempts - 1)) // 10
            .Build();

        await scheduler.ScheduleJob(trigger);
    }

    private async Task StopApplyPOIsJobAsync(Guid paymentId)
    {
        try
        {
            var scheduler = await schedulerFactory.GetScheduler();
            var triggerKey = ApplyPOIsAfterPayJobConstants.CreateTriggerKey(paymentId);
            if (await scheduler.CheckExists(triggerKey))
            {
                await scheduler.UnscheduleJob(triggerKey);
            }
        }
        catch (Exception)
        {

            logger.LogError($"Failed to stop ApplyPOIsAfterPayJob for payment {paymentId} after applying payments. Please check Quartz scheduler for any remaining jobs with trigger key {ApplyPOIsAfterPayJobConstants.CreateTriggerKey(paymentId)} and remove if exists.");
        }
        
    }
    public async Task<Result<bool>> ApplyCompletedPaidPaymentsForBillingServiceAsync(string billingServiceCode)
    {
        var pois = await unitOfWork.PaymentOrderItemRepository.GetPaidPaymentOrderItemsForBillingServiceCodeForApplyAsync(billingServiceCode);
        if (pois.Count == 0)
        {
            logger.LogInformation("No completed payments with eligible paid bill items were found for bulk apply.");
            return Result<bool>.Success(true);
        }

        await ApplyLoadedPaymentsInternalAsync(pois);
        return Result<bool>.Success(true);
    }
    public async Task<Result<bool>> ApplyCompletedPaidPaymentsAsync()
    {
        var pois = await unitOfWork.PaymentOrderItemRepository.GetPaidPaymentOrderItemsForApplyAsync();
        if (pois.Count == 0)
        {
            logger.LogInformation("No completed payments with eligible paid bill items were found for bulk apply.");
            return Result<bool>.Success(true);
        }

        await ApplyLoadedPaymentsInternalAsync(pois);
        return Result<bool>.Success(true);
    }

    public async Task<Result<InquireOutputModel>> Inquire(
        Guid customerSubscriptionId,
        bool unpaidOnly = true,
        CancellationToken cancellationToken = default)
    {
        var customerSubscription = await unitOfWork.CustomerSubscriptionRepository
            .GetByIdForInquiryAsync(customerSubscriptionId, cancellationToken);

        if (customerSubscription is null)
        {
            return Result<InquireOutputModel>.Failure(
                localizer[LocalizationKeys.BillingInquirySubscriptionNotFound, customerSubscriptionId],
                AppHttpStatusCodes.NotFound);
        }

        return await InquireUsingResolvedServiceAsync(
            customerSubscription.BillingService,
            customerSubscription.BillingRefValue,
            unpaidOnly,
            cancellationToken);
    }

    public async Task<Result<PagedInquireOutputModel>> InquirePaged(
        PagedBillingInquiryInputModel model,
        CancellationToken cancellationToken = default)
    {
        var customerSubscription = await unitOfWork.CustomerSubscriptionRepository
            .GetByIdForInquiryAsync(model.CustomerSubscriptionId, cancellationToken);

        if (customerSubscription is null)
        {
            return Result<PagedInquireOutputModel>.Failure(
                localizer[LocalizationKeys.BillingInquirySubscriptionNotFound, model.CustomerSubscriptionId],
                AppHttpStatusCodes.NotFound);
        }

        var pageNumber = model.PageNumber <= 0 ? 1 : model.PageNumber;
        var pageSize = model.PageSize <= 0 ? 20 : Math.Min(model.PageSize, 100);
        var search = string.IsNullOrWhiteSpace(model.Search) ? null : model.Search.Trim();
        var sortBy = string.IsNullOrWhiteSpace(model.SortBy) ? null : model.SortBy.Trim();

        return await InquirePagedUsingResolvedServiceAsync(
            customerSubscription.BillingService,
            customerSubscription.BillingRefValue,
            model.UnpaidOnly,
            pageNumber,
            pageSize,
            search,
            sortBy,
            model.IsAscending,
            cancellationToken);
    }

    public async Task<Result<InquireOutputModel>> Inquire(
        long billingServiceId,
        string billingRef,
        bool unpaidOnly = true,
        CancellationToken cancellationToken = default)
    {
        var billingService = await unitOfWork.BillingServiceRepository.GetByIdWithRelationsAsync(billingServiceId);
        if (billingService is null)
        {
            return Result<InquireOutputModel>.Failure(
                localizer[LocalizationKeys.BillingInquiryServiceIdNotFound, billingServiceId],
                AppHttpStatusCodes.NotFound);
        }

        return await InquireUsingResolvedServiceAsync(
            billingService,
            billingRef,
            unpaidOnly,
            cancellationToken);
    }
    public async Task<Result<InquireOutputModel>> Inquire(
        string billerCode,
        string serviceCode,
        string billingRef,
        bool unpaidOnly = true,
        CancellationToken cancellationToken = default)
    {
        var normalizedBillerCode = billerCode?.Trim();
        var normalizedServiceCode = serviceCode?.Trim();
        var normalizedBillingRef = billingRef?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedBillerCode) ||
            string.IsNullOrWhiteSpace(normalizedServiceCode) ||
            string.IsNullOrWhiteSpace(normalizedBillingRef))
        {
            return Result<InquireOutputModel>.Failure(
                localizer[LocalizationKeys.BillingInquiryInvalidRequest],
                AppHttpStatusCodes.BadRequest);
        }

        var billingService = await unitOfWork.BillingServiceRepository
            .GetByBillerCodeAndBillingServiceCodeWithCurrenciesAsync(normalizedBillerCode, normalizedServiceCode);

        if (billingService is null)
        {
            return Result<InquireOutputModel>.Failure(
                localizer[LocalizationKeys.BillingInquiryServiceNotFound, normalizedBillerCode, normalizedServiceCode],
                AppHttpStatusCodes.NotFound);
        }

        return await InquireUsingResolvedServiceAsync(
            billingService,
            normalizedBillingRef,
            unpaidOnly,
            cancellationToken);
    }

    private async Task<Result<InquireOutputModel>> InquireUsingResolvedServiceAsync(
        BillingServiceEntity billingService,
        string billingRef,
        bool unpaidOnly,
        CancellationToken cancellationToken)
    {
        var normalizedBillerCode = billingService.Biller.Code.Trim();
        var normalizedServiceCode = billingService.Code.Trim();
        var normalizedBillingRef = billingRef?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedBillingRef))
        {
            return Result<InquireOutputModel>.Failure(
                localizer[LocalizationKeys.BillingInquiryInvalidRequest],
                AppHttpStatusCodes.BadRequest);
        }

        if (!billingService.Active || !billingService.SupportInquiry)
        {
            return Result<InquireOutputModel>.Failure(
                localizer[LocalizationKeys.BillingInquiryNotSupported, normalizedBillerCode, normalizedServiceCode],
                AppHttpStatusCodes.BadRequest);
        }

        var connector = connectorResolver.Resolve(normalizedBillerCode);
        if (connector is null)
        {
            return Result<InquireOutputModel>.Failure(
                localizer[LocalizationKeys.BillingInquiryConnectorNotConfigured, normalizedBillerCode],
                AppHttpStatusCodes.BadRequest);
        }

        var request = new BillerInquiryRequestModel(
            billingService.Id,
            normalizedServiceCode,
            normalizedBillingRef,
            unpaidOnly);

        var inquireResult = await connector.InquireAsync(request, cancellationToken);
        return await EnrichInquireWithPaidOrderItemsAsync(inquireResult, unpaidOnly, cancellationToken);
    }

    private async Task<Result<PagedInquireOutputModel>> InquirePagedUsingResolvedServiceAsync(
        BillingServiceEntity billingService,
        string billingRef,
        bool unpaidOnly,
        int pageNumber,
        int pageSize,
        string? search,
        string? sortBy,
        bool isAscending,
        CancellationToken cancellationToken)
    {
        var normalizedBillerCode = billingService.Biller.Code.Trim();
        var normalizedServiceCode = billingService.Code.Trim();
        var normalizedBillingRef = billingRef?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedBillingRef))
        {
            return Result<PagedInquireOutputModel>.Failure(
                localizer[LocalizationKeys.BillingInquiryInvalidRequest],
                AppHttpStatusCodes.BadRequest);
        }

        if (!billingService.Active || !billingService.SupportInquiry)
        {
            return Result<PagedInquireOutputModel>.Failure(
                localizer[LocalizationKeys.BillingInquiryNotSupported, normalizedBillerCode, normalizedServiceCode],
                AppHttpStatusCodes.BadRequest);
        }

        var connector = connectorResolver.Resolve(normalizedBillerCode);
        if (connector is null)
        {
            return Result<PagedInquireOutputModel>.Failure(
                localizer[LocalizationKeys.BillingInquiryConnectorNotConfigured, normalizedBillerCode],
                AppHttpStatusCodes.BadRequest);
        }

        var request = new BillerPagedInquiryRequestModel(
            billingService.Id,
            normalizedServiceCode,
            normalizedBillingRef,
            unpaidOnly,
            pageNumber,
            pageSize,
            search,
            sortBy,
            isAscending);

        var inquireResult = await connector.InquirePagedAsync(request, cancellationToken);
        return await EnrichPagedInquireWithPaidOrderItemsAsync(
            inquireResult,
            unpaidOnly,
            cancellationToken);
    }

    public async Task<Result<InquireOutputModel>> InquireByInvoiceRef(
        string invoiceRef,
        bool unpaidOnly = true,
        CancellationToken cancellationToken = default)
    {
        var resolveResult = await billingInvoiceRefResolver.ResolveInvoiceByRefAsync(
            invoiceRef,
            requireOutstanding: unpaidOnly,
            cancellationToken);

        if (!resolveResult.IsSuccess)
        {
            return resolveResult.ToFailure<InquireOutputModel>();
        }

        var invoice = resolveResult.Value!;
        var bill = mapper.Map<InquireBillOutputModel>(invoice);
        var output = new InquireOutputModel(
            BillerCode: invoice.BillingService.Biller.Code,
            ServiceCode: invoice.BillingService.Code,
            BillingRef: invoice.BillingRef,
            BillingRefForDisplay: invoice.BillingRef.Contains('-') ? invoice.BillingRef.Substring(invoice.BillingRef.LastIndexOf('-')+1) : invoice.BillingRef,
            Balances: null,
            Bills: [bill],
            TotalOutstanding: InquireOutputTotals.CalculateTotalOutstandingByCurrency([bill]),
            RetrievedAtUtc: DateTimeOffset.UtcNow);

        return await EnrichInquireWithPaidOrderItemsAsync(
            Result<InquireOutputModel>.Success(output),
            unpaidOnly,
            cancellationToken);
    }

    public async Task<Result<PrepaidMeterInquiryOutputModel>> InquirePrepaidMeter(string meterNumber)
    {
        const string billerCode = "SNEL";
        const string prepaidServiceCode = "CONNECT-AFRICA";

        var normalizedMeterNumber = meterNumber?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedMeterNumber))
        {
            return Result<PrepaidMeterInquiryOutputModel>.Failure(
                localizer[LocalizationKeys.BillingInquiryInvalidRequest],
                AppHttpStatusCodes.BadRequest);
        }

        var billingService = await unitOfWork.BillingServiceRepository
            .GetByBillerCodeAndBillingServiceCodeWithCurrenciesAsync(billerCode, prepaidServiceCode);
        if (billingService is null)
        {
            return Result<PrepaidMeterInquiryOutputModel>.Failure(
                localizer[LocalizationKeys.BillingInquiryServiceNotFound, billerCode, prepaidServiceCode],
                AppHttpStatusCodes.NotFound);
        }

        if (!billingService.Active || !billingService.SupportInquiry)
        {
            return Result<PrepaidMeterInquiryOutputModel>.Failure(
                localizer[LocalizationKeys.BillingInquiryNotSupported, billerCode, prepaidServiceCode],
                AppHttpStatusCodes.BadRequest);
        }

        var connector = connectorResolver.Resolve(billerCode);
        if (connector is null)
        {
            return Result<PrepaidMeterInquiryOutputModel>.Failure(
                localizer[LocalizationKeys.BillingInquiryConnectorNotConfigured, billerCode],
                AppHttpStatusCodes.BadRequest);
        }

        var checkResult = await connector.CheckSubscriptionExistsAsync(
            new BillerSubscriptionExistsRequestModel(
                billingService.Id,
                string.Empty,
                normalizedMeterNumber));
        if (!checkResult.IsSuccess)
            return checkResult.ToFailure<PrepaidMeterInquiryOutputModel>();

        return Result<PrepaidMeterInquiryOutputModel>.Success(
            new PrepaidMeterInquiryOutputModel(
                normalizedMeterNumber,
                checkResult.Value.Info ?? string.Empty));
    }

    private async Task<Result<InquireOutputModel>> EnrichInquireWithPaidOrderItemsAsync(
        Result<InquireOutputModel> inquireResult,
        bool unpaidOnly,
        CancellationToken cancellationToken = default)
    {
        if (!inquireResult.IsSuccess || inquireResult.Value is null)
        {
            return inquireResult;
        }
        inquireResult.Value.Bills?.ToList().ForEach(b => b.Status = localizer[$"Invoice.Status.{b.Status}"]);
        var output = inquireResult.Value;

        if (output.Bills is null || output.Bills.Count == 0)
        {
            return Result<InquireOutputModel>.Success(WithTotalOutstanding(output));
        }

        var invoiceIds = output.Bills
            .Select(bill => bill.InvoiceId)
            .Distinct()
            .ToList();

        if (invoiceIds.Count == 0)
        {
            return Result<InquireOutputModel>.Success(WithTotalOutstanding(output));
        }

        var paidAmountByInvoiceId = await unitOfWork.PaymentOrderItemRepository
            .GetAmountTotalsByInvoicesAndTypeAndStatusAsync(
                invoiceIds,
                OrderItemType.PayBill,
                PaymentOrderItemStatus.Paid,
                cancellationToken);

        if (paidAmountByInvoiceId.Count == 0)
        {
            return Result<InquireOutputModel>.Success(WithTotalOutstanding(output));
        }

        var updatedBillsQuery = output.Bills
            .Select(bill =>
            {
                if (!paidAmountByInvoiceId.TryGetValue(bill.InvoiceId, out var paidFromOrders) ||
                    paidFromOrders <= 0)
                {
                    return bill;
                }

                var newAmountPaid = bill.AmountPaid + paidFromOrders;
                var newAmountOutstanding = bill.AmountTotal - newAmountPaid;
                if (newAmountOutstanding < 0)
                {
                    logger.LogError(
                        "Billing inquire: invoice has negative outstanding (overpaid). InvoiceId={InvoiceId}, InvoiceRef={InvoiceRef}, AmountTotal={AmountTotal}, AmountPaidFromSource={AmountPaidFromSource}, PaidFromOrders={PaidFromOrders}, NewAmountPaid={NewAmountPaid}, NewAmountOutstanding={NewAmountOutstanding}, CurrencyCode={CurrencyCode}, Status={Status}, IssueDate={IssueDate}, DueDate={DueDate}",
                        bill.InvoiceId,
                        bill.InvoiceRef,
                        bill.AmountTotal,
                        bill.AmountPaid,
                        paidFromOrders,
                        newAmountPaid,
                        newAmountOutstanding,
                        bill.CurrencyCode,
                        bill.Status,
                        bill.IssueDate,
                        bill.DueDate);
                }
                bill.AmountPaid = newAmountPaid;
                bill.AmountOutstanding = newAmountOutstanding;
                return bill;
            });

        if (unpaidOnly)
            updatedBillsQuery = updatedBillsQuery.Where(bill => bill.AmountOutstanding > 0);

        var updatedBills = updatedBillsQuery.ToArray();

        var updatedOutput = output with { Bills = updatedBills };

        return Result<InquireOutputModel>.Success(WithTotalOutstanding(updatedOutput));
    }

    private async Task<Result<PagedInquireOutputModel>> EnrichPagedInquireWithPaidOrderItemsAsync(
        Result<PagedInquireOutputModel> inquireResult,
        bool unpaidOnly,
        CancellationToken cancellationToken)
    {
        if (!inquireResult.IsSuccess || inquireResult.Value is null)
        {
            return inquireResult;
        }

        var pagedOutput = inquireResult.Value;
        var output = new InquireOutputModel(
            pagedOutput.BillerCode,
            pagedOutput.ServiceCode,
            pagedOutput.BillingRef,
            pagedOutput.BillingRefForDisplay,
            pagedOutput.Balances,
            pagedOutput.Bills.Items,
            pagedOutput.TotalOutstanding,
            pagedOutput.RetrievedAtUtc);

        var enrichedResult = await EnrichInquireWithPaidOrderItemsAsync(
            Result<InquireOutputModel>.Success(output),
            unpaidOnly,
            cancellationToken);

        if (!enrichedResult.IsSuccess || enrichedResult.Value is null)
        {
            return Result<PagedInquireOutputModel>.Failure(
                enrichedResult.Message ?? localizer[LocalizationKeys.BillingInquiryInvalidRequest],
                enrichedResult.StatusCode);
        }

        return Result<PagedInquireOutputModel>.Success(pagedOutput with
        {
            Bills = new PagedResult<InquireBillOutputModel>(
                enrichedResult.Value.Bills?.ToArray() ?? [],
                pagedOutput.Bills.TotalCount)
        });
    }

    private static InquireOutputModel WithTotalOutstanding(InquireOutputModel output) =>
        output with
        {
            TotalOutstanding = InquireOutputTotals.CalculateTotalOutstandingByCurrency(output.Bills)
        };

    public async Task<Result<List<CustomActionsOutputModel>>> GetCustomActions(Guid customerSubscriptionId)
    {
        var customerSubscription = await unitOfWork.CustomerSubscriptionRepository
            .GetByIdWithRelationsAsync(customerSubscriptionId);

        if (customerSubscription is null)
        {
            return Result<List<CustomActionsOutputModel>>.Failure(
                localizer[LocalizationKeys.BillingInquirySubscriptionNotFound, customerSubscriptionId],
                AppHttpStatusCodes.NotFound);
        }

        return await GetCustomActions(
            customerSubscription.BillingService.Biller.Code,
            customerSubscription.BillingService.Code);
    }
    public async Task<Result<List<CustomActionsOutputModel>>> GetCustomActions(
        long billingServiceId)
    {
        var billingService = await unitOfWork.BillingServiceRepository.GetByIdWithRelationsAsync(billingServiceId);
        if (billingService is null)
        {
            return Result<List<CustomActionsOutputModel>>.Failure(
                localizer[LocalizationKeys.BillingInquiryServiceIdNotFound, billingServiceId],
                AppHttpStatusCodes.NotFound);
        }

        return await GetCustomActions(
            billingService.Biller.Code,
            billingService.Code);
    }
    public async Task<Result<List<CustomActionsOutputModel>>> GetCustomActions(
        string billerCode,
        string serviceCode)
    {
        var normalizedBillerCode = billerCode?.Trim();
        var normalizedServiceCode = serviceCode?.Trim();        

        if (string.IsNullOrWhiteSpace(normalizedBillerCode) ||
            string.IsNullOrWhiteSpace(normalizedServiceCode))
        {
            return Result<List<CustomActionsOutputModel>>.Failure(
                localizer[LocalizationKeys.BillingCustomActionsInvalidRequest],
                AppHttpStatusCodes.BadRequest);
        }

        var billingService = await unitOfWork.BillingServiceRepository
            .GetByBillerCodeAndBillingServiceCodeWithCurrenciesAsync(normalizedBillerCode, normalizedServiceCode);

        if (billingService is null)
        {
            return Result<List<CustomActionsOutputModel>>.Failure(
                localizer[LocalizationKeys.BillingCustomActionsServiceNotFound, normalizedBillerCode, normalizedServiceCode],
                AppHttpStatusCodes.NotFound);
        }

        if (!billingService.Active || !billingService.SupportCustomActions)
        {
            return Result<List<CustomActionsOutputModel>>.Failure(
                localizer[LocalizationKeys.BillingInquiryNotSupported, normalizedBillerCode, normalizedServiceCode],
                AppHttpStatusCodes.BadRequest);
        }

        var connector = connectorResolver.Resolve(normalizedBillerCode);
        if (connector is null)
        {
            return Result<List<CustomActionsOutputModel>>.Failure(
                localizer[LocalizationKeys.BillingInquiryConnectorNotConfigured, normalizedBillerCode],
                AppHttpStatusCodes.BadRequest);
        }        

        var customActionsResult = connector.GetCustomActions(normalizedServiceCode);
        return customActionsResult;
    }
    public async Task<Result<object>> ExecuteCustomActions(
        ExecuteCustomActionsInputModel model
        )
    {
        var normalizedBillerCode = model.BillerCode?.Trim();
        var normalizedServiceCode = model.ServiceCode?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedBillerCode) ||
            string.IsNullOrWhiteSpace(normalizedServiceCode))
        {
            return Result<object>.Failure(
                localizer[LocalizationKeys.BillingCustomActionsInvalidRequest],
                AppHttpStatusCodes.BadRequest);
        }

        var billingService = await unitOfWork.BillingServiceRepository
            .GetByBillerCodeAndBillingServiceCodeWithCurrenciesAsync(normalizedBillerCode, normalizedServiceCode);

        if (billingService is null)
        {
            return Result<object>.Failure(
                localizer[LocalizationKeys.BillingCustomActionsServiceNotFound, normalizedBillerCode, normalizedServiceCode],
                AppHttpStatusCodes.NotFound);
        }

        if (!billingService.Active || !billingService.SupportCustomActions)
        {
            return Result<object>.Failure(
                localizer[LocalizationKeys.BillingInquiryNotSupported, normalizedBillerCode, normalizedServiceCode],
                AppHttpStatusCodes.BadRequest);
        }

        var connector = connectorResolver.Resolve(normalizedBillerCode);
        if (connector is null)
        {
            return Result<object>.Failure(
                localizer[LocalizationKeys.BillingInquiryConnectorNotConfigured, normalizedBillerCode],
                AppHttpStatusCodes.BadRequest);
        }

        var customActionsResult = await connector.ExecuteCustomAction(normalizedServiceCode, model.BillingRef, model.ActionCode, model.Parameters);
        return customActionsResult;
    }
    public async Task<Result<int>> ApplyLoadedPaymentsInternalAsync(
        List<PaymentOrderItemEntity> pois)
    {
        
        var billerGroups = pois
            .Where(x=>
            x.ItemType == OrderItemType.PayBill || 
            x.ItemType == OrderItemType.MeterCharge
            ).GroupBy(x => x.BillingService!.Biller!.Code)
            .ToList();

        logger.LogInformation(
            "Billing apply: starting. Total payment order items {TotalPoiCount}, biller group count {BillerGroupCount}.",
            pois.Count,
            billerGroups.Count);

        var affectedCount= 0;
        foreach (var billerGroup in billerGroups)
        {
            var groupPois = billerGroup.ToList();
            var connector = connectorResolver.Resolve(billerGroup.Key);
            if (connector is null)
            {
                logger.LogError("No connector configured for biller code {BillerCode}. Skipping applying payments for this biller.", billerGroup.Key);

                return Result<int>.Failure(localizer[LocalizationKeys.BillingApplyNoConnectorForBiller, billerGroup.Key], AppHttpStatusCodes.BadRequest);
            }

            logger.LogInformation(
                "Billing apply: calling connector for biller {BillerCode} with {PoiCount} payment order item(s).",
                billerGroup.Key,
                groupPois.Count);

            var applyByBillerResult= await connector.ApplyPaymentsAsync(groupPois);
            logger.LogInformation(
                "Billing apply: connector finished for biller {BillerCode}. IsSuccess={IsSuccess}, AppliedCount={AppliedCount}, StatusCode={StatusCode}, Message={Message}.",
                billerGroup.Key,
                applyByBillerResult.IsSuccess,
                applyByBillerResult.Value,
                applyByBillerResult.StatusCode,
                applyByBillerResult.Message);

            if (!applyByBillerResult.IsSuccess)
            {
                return Result<int>.Failure(
                    applyByBillerResult.Message ?? localizer[LocalizationKeys.UnexpectedError],
                    applyByBillerResult.StatusCode);
            }

            affectedCount += applyByBillerResult.Value;
        }
        logger.LogInformation(
            "Billing apply: completed. Total applied item count {TotalAppliedCount}.",
            affectedCount);
        return Result<int>.Success(affectedCount);
    }

    public async Task<Result> ApplyPOIsAfterPay(Guid paymentId)
    {
        string lockName = $"{LockNames.PAYMENT_APPLY_POI}:{paymentId}";
        await using var locker = await distributedLockProvider.TryAcquireLockAsync(lockName, TimeSpan.FromSeconds(0));
        if (locker is null)
        {
            return Result.Failure(localizer[LocalizationKeys.PaymentLockAcquireFailed], AppHttpStatusCodes.Conflict);
        }

        var pois = await unitOfWork.PaymentOrderItemRepository.GetByPaymentIdForApplyAsync(paymentId);
        var poisToApply = pois.Where(poi =>
        poi.BillingService!.Currencies.Any(c => c.ApplyType == ApplyTypeEnum.AFTER_PAY && c.CurrencyId==poi.CurrencyId) &&
        poi.Status == PaymentOrderItemStatus.Paid).ToList();
        if (poisToApply.Count > 0)
        {
            var applyResult = await ApplyLoadedPaymentsInternalAsync(poisToApply);
            if (applyResult.IsSuccess && applyResult.Value>0)
            {
                await unitOfWork.CompleteAsync();
                logger.LogInformation($"Applied {applyResult.Value} payment order items for ApplyPOIsAfterPay for payment {paymentId}.", paymentId, applyResult.Value);
                if (poisToApply.Count == applyResult.Value)
                {
                    await StopApplyPOIsJobAsync(paymentId);
                }
            }            
        }
        else
        {
            logger.LogInformation($"No eligible payment order items found for ApplyPOIsAfterPay for payment {paymentId}.", paymentId);
            await StopApplyPOIsJobAsync(paymentId);
        }
        return Result.Success();
    }
}
