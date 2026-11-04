using AutoMapper;
using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.Accounting.Models.Output;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Models.Input;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Accounting.Enums;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Domain.Partners;
using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Caching;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Application.Payments.Services;

public sealed class PaymentProviderService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IStringLocalizer<SharedResource> localizer,
    IPaymentProviderAccessPolicy paymentProviderAccessPolicy) : IPaymentProviderService
{
    public async Task<Result<IReadOnlyCollection<PaymentProviderOutputModel>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var readScopeResult = await paymentProviderAccessPolicy.GetPaymentProvidersReadScopeAsync(cancellationToken);
        if (!readScopeResult.IsSuccess)
        {
            return Result<IReadOnlyCollection<PaymentProviderOutputModel>>.Failure(readScopeResult.Message!, readScopeResult.StatusCode);
        }

        var entities = await unitOfWork.PaymentProviderRepository
            .GetAllScopedByPartnerAsync(readScopeResult.Value, cancellationToken);
        var output = new List<PaymentProviderOutputModel>();
        foreach (var entity in entities)
        {
            output.Add(await GetPaymentProviderWithAccountAsync(entity, cancellationToken));
        }

        return Result<IReadOnlyCollection<PaymentProviderOutputModel>>.Success(output);
    }

    public async Task<Result<PaymentProviderOutputModel>> CreatePaymentProviderAsync(
        CreatePaymentProviderInputModel model,
        CancellationToken cancellationToken = default)
    {
        var legalName = model.LegalName?.Trim() ?? string.Empty;
        var code = model.Code?.Trim() ?? string.Empty;
        var name = model.Name?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(legalName) || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
        {
            return Result<PaymentProviderOutputModel>.Failure(
                localizer[LocalizationKeys.BillerAddInvalidRequest],
                AppHttpStatusCodes.BadRequest);
        }

        if (await unitOfWork.PaymentProviderRepository.ExistsByCodeAsync(code, cancellationToken))
        {
            return Result<PaymentProviderOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentProviderCodeAlreadyExists, code],
                AppHttpStatusCodes.Conflict);
        }

        if (await unitOfWork.PartnerRepository.ExistsByLegalNameAsync(legalName))
        {
            return Result<PaymentProviderOutputModel>.Failure(
                localizer[LocalizationKeys.BillerAddPartnerLegalNameAlreadyExists, legalName],
                AppHttpStatusCodes.Conflict);
        }

        var pspLiabilityType = CachingFactory.GetValue<AccountTypeEntity>(x => x.Id == (int)AccountTypesEnum.PSP_LIABILITY).FirstOrDefault();
        if (pspLiabilityType is null)
        {
            return Result<PaymentProviderOutputModel>.Failure(
                localizer[LocalizationKeys.BillerAddAccountTypeNotFound, AccountTypesEnum.PSP_LIABILITY.ToString()],
                AppHttpStatusCodes.BadRequest);
        }
        var bankAccountType = CachingFactory.GetValue<AccountTypeEntity>(x => x.Id == (int)AccountTypesEnum.BANK).FirstOrDefault();
        //var bankAccountType = await unitOfWork.AccountTypeRepository.GetByIdAsync((int)AccountTypesEnum.BANK, cancellationToken);
        if (bankAccountType is null)
        {
            return Result<PaymentProviderOutputModel>.Failure(
                localizer[LocalizationKeys.BillerAddAccountTypeNotFound, AccountTypesEnum.BANK.ToString()],
                AppHttpStatusCodes.BadRequest);
        }

        var partner = PartnerEntity.Create(
            PartnerTypeConstants.Psp,
            legalName,
            model.DisplayName,
            model.PartnerActive);
        unitOfWork.PartnerRepository.Add(partner);

        var entity = PaymentProviderEntity.Create(
            partner,
            code,
            name);

        await unitOfWork.PaymentProviderRepository.AddAsync(entity, cancellationToken);

        var currencies = CachingFactory.GetValue<CurrencyEntity>();
        foreach (var currency in currencies)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var existingPspLiability = await unitOfWork.AccountRepository.GetByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(
                partner.Id,
                currency.Id,
                (int)AccountTypesEnum.PSP_LIABILITY);
            if (existingPspLiability is null)
            {
                var pspLiabilityAccount = AccountEntity.Create(
                    (int)AccountTypesEnum.PSP_LIABILITY,
                    partner,
                    currency.Id,
                    order: currency.Order,
                    balance: 0);
                unitOfWork.AccountRepository.Add(pspLiabilityAccount);
            }

            var existingBank = await unitOfWork.AccountRepository.GetByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(
                partner.Id,
                currency.Id,
                (int)AccountTypesEnum.BANK);
            if (existingBank is null)
            {
                var bankAccount = AccountEntity.Create(
                    (int)AccountTypesEnum.BANK,
                    partner,
                    currency.Id,
                    order: currency.Order,
                    balance: 0);
                unitOfWork.AccountRepository.Add(bankAccount);
            }
        }

        await unitOfWork.CompleteAsync();

        var created = await unitOfWork.PaymentProviderRepository.GetByIdAsync(entity.Id, cancellationToken);
        return Result<PaymentProviderOutputModel>.Success(
            await GetPaymentProviderWithAccountAsync(created!, cancellationToken),
            localizer[LocalizationKeys.PaymentProviderCreateSuccess],
            AppHttpStatusCodes.Created            );
    }

    public async Task<Result<PaymentProviderOutputModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.PaymentProviderRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return Result<PaymentProviderOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentProviderNotFound, id],
                AppHttpStatusCodes.NotFound);
        }

        return Result<PaymentProviderOutputModel>.Success(await GetPaymentProviderWithAccountAsync(entity, cancellationToken));
    }
    public async Task<Result<PaymentProviderOutputModel>> GetByPartnerIdAsync(long partnerId, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.PaymentProviderRepository.GetAllQueryable(x => x.PartnerId == partnerId).FirstOrDefaultAsync(cancellationToken);
        if (entity is null)
        {
            return Result<PaymentProviderOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentProviderNotFound, partnerId],
                AppHttpStatusCodes.NotFound);
        }

        return Result<PaymentProviderOutputModel>.Success(await GetPaymentProviderWithAccountAsync(entity, cancellationToken));
    }

    public async Task<Result<PaymentProviderOutputModel>> UpdateAsync(
        long id,
        UpdatePaymentProviderInputModel model,
        CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.PaymentProviderRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return Result<PaymentProviderOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentProviderNotFound, id],
                AppHttpStatusCodes.NotFound);
        }

        if (await unitOfWork.PaymentProviderRepository.ExistsByCodeExceptIdAsync(model.Code, id, cancellationToken))
        {
            return Result<PaymentProviderOutputModel>.Failure(
                localizer[LocalizationKeys.PaymentProviderCodeAlreadyExists, model.Code],
                AppHttpStatusCodes.Conflict);
        }

        var legalName = model.LegalName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(legalName))
        {
            return Result<PaymentProviderOutputModel>.Failure(
                localizer[LocalizationKeys.BillerAddInvalidRequest],
                AppHttpStatusCodes.BadRequest);
        }

        var partner = await unitOfWork.PartnerRepository.GetByIdAsync(entity.PartnerId, cancellationToken);
        if (partner is null)
        {
            return Result<PaymentProviderOutputModel>.Failure(
                localizer[LocalizationKeys.PartnerNotFound],
                AppHttpStatusCodes.NotFound);
        }

        if (await unitOfWork.PartnerRepository.ExistsByLegalNameExceptIdAsync(legalName, partner.Id, cancellationToken))
        {
            return Result<PaymentProviderOutputModel>.Failure(
                localizer[LocalizationKeys.BillerAddPartnerLegalNameAlreadyExists, legalName],
                AppHttpStatusCodes.Conflict);
        }

        partner.UpdateProfile(legalName, model.DisplayName, model.PartnerActive);
        unitOfWork.PartnerRepository.Update(partner);
        entity.Update(model.Code, model.Name);
        unitOfWork.PaymentProviderRepository.Update(entity);
        await unitOfWork.CompleteAsync();

        var updated = await unitOfWork.PaymentProviderRepository.GetByIdAsync(id, cancellationToken);
        return Result<PaymentProviderOutputModel>.Success(
            await GetPaymentProviderWithAccountAsync(updated!, cancellationToken),
            localizer[LocalizationKeys.PaymentProviderUpdateSuccess]);
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.PaymentProviderRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.PaymentProviderNotFound, id],
                AppHttpStatusCodes.NotFound);
        }

        var hasReferences = await unitOfWork.PartnerRepository.HasCustomersAsync(entity.PartnerId, cancellationToken)
                           || await unitOfWork.PartnerRepository.HasOtherBillersAsync(entity.PartnerId, cancellationToken: cancellationToken)
                           || await unitOfWork.PartnerRepository.HasOtherPaymentProvidersAsync(entity.PartnerId, id, cancellationToken)
                           || await unitOfWork.PartnerRepository.HasPaymentMethodsAsync(entity.PartnerId, cancellationToken)
                           || await unitOfWork.PartnerRepository.HasPaymentsAsync(entity.PartnerId, cancellationToken)
                           || await unitOfWork.PartnerRepository.HasJournalLinesOnAccountsAsync(entity.PartnerId, cancellationToken)
                           || await unitOfWork.PartnerRepository.HasPartnerContactsAsync(entity.PartnerId, cancellationToken);
        if (hasReferences)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.PartnerDeleteBlockedByReferences],
                AppHttpStatusCodes.Conflict);
        }

        unitOfWork.PaymentProviderRepository.Remove(entity);
        var partner = await unitOfWork.PartnerRepository.GetByIdAsync(entity.PartnerId, cancellationToken);
        var accounts = await unitOfWork.AccountRepository.GetByPartnerIdWithRelationsAsync(entity.PartnerId, cancellationToken);

        if (accounts.Count > 0)
        {
            unitOfWork.AccountRepository.RemoveRange(accounts);
        }

        if (partner is not null)
        {
            unitOfWork.PartnerRepository.Remove(partner);
        }

        await unitOfWork.CompleteAsync();

        return Result<bool>.Success(
            true,
            localizer[LocalizationKeys.PaymentProviderDeleteSuccess]);
    }

    private async Task<PaymentProviderOutputModel> GetPaymentProviderWithAccountAsync(
        PaymentProviderEntity entity,
        CancellationToken cancellationToken)
    {
        var partner = entity.Partner
            ?? await unitOfWork.PartnerRepository.GetByIdAsync(entity.PartnerId, cancellationToken);
        if (partner is null)
        {
            throw new InvalidOperationException(
                localizer[LocalizationKeys.PaymentProviderPartnerNotFound, entity.PartnerId, entity.Id]);
        }

        var accounts = await unitOfWork.AccountRepository.GetByPartnerIdWithRelationsAsync(entity.PartnerId, cancellationToken);
        var accountModels = mapper.Map<IReadOnlyCollection<AccountOutputModel>>(accounts);

        return new PaymentProviderOutputModel(
            entity.Id,
            entity.PartnerId,
            entity.Code,
            entity.Name,
            partner.Active,
            accountModels,
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc,
            entity.CreatedBy,
            entity.UpdatedBy);
    }
}
