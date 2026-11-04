using AutoMapper;
using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.Accounting.Models.Output;
using SNEL.WalletPlatform.Application.Billers.Interfaces;
using SNEL.WalletPlatform.Application.Billers.Models.Input;
using SNEL.WalletPlatform.Application.Billers.Models.Output;
using SNEL.WalletPlatform.Application;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Accounting.Enums;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Partners;
using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Caching;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Application.Billers.Services;

public sealed class BillerService(
    IUnitOfWork unitOfWork,
    IStringLocalizer<SharedResource> localizer,
    IMapper mapper,
    IBillerAccessPolicy billerAccessPolicy) : IBillerService
{
    public async Task<Result<AddBillerOutputModel>> AddBillerAsync(AddBillerInputModel model, CancellationToken cancellationToken = default)
    {
        var createResult = await CreateBillerWithPartnerAndAccountsAsync(model, cancellationToken);
        if (!createResult.IsSuccess)
            return Result<AddBillerOutputModel>.Failure(createResult.Message!, createResult.StatusCode);

        var (partner, biller, _) = createResult.Value!;
        await unitOfWork.CompleteAsync();
        var persistedAccounts = await unitOfWork.AccountRepository.GetByPartnerIdWithRelationsAsync(partner.Id, cancellationToken);

        var output = new AddBillerOutputModel
        {
            Biller = mapper.Map<BillerInfoOutputModel>(biller),
            Partner = mapper.Map<PartnerInfoOutputModel>(partner),
            Accounts = mapper.Map<IReadOnlyCollection<AccountOutputModel>>(persistedAccounts)
        };

        return Result<AddBillerOutputModel>.Success(
            output,
            localizer[LocalizationKeys.BillerAddSuccess],
            AppHttpStatusCodes.Created);
    }

    public async Task<Result<IReadOnlyCollection<BillerOutputModel>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var readScopeResult = await billerAccessPolicy.GetBillersReadScopeAsync(cancellationToken);
        if (!readScopeResult.IsSuccess)
        {
            return Result<IReadOnlyCollection<BillerOutputModel>>.Failure(readScopeResult.Message!, readScopeResult.StatusCode);
        }

        var entities = await unitOfWork.BillerRepository.GetAllScopedByPartnerAsync(readScopeResult.Value, cancellationToken);
        var output = new List<BillerOutputModel>();
        foreach (var entity in entities)
        {
            output.Add(await GetBillerWithAccountsAndPartnerAsync(entity, cancellationToken));
        }

        return Result<IReadOnlyCollection<BillerOutputModel>>.Success(output);
    }

    public async Task<Result<BillerOutputModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.BillerRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return Result<BillerOutputModel>.Failure(
                localizer[LocalizationKeys.BillerNotFound, id],
                AppHttpStatusCodes.NotFound);
        }

        return Result<BillerOutputModel>.Success(await GetBillerWithAccountsAndPartnerAsync(entity, cancellationToken));
    }

    public async Task<Result<BillerOutputModel>> GetByPartnerIdAsync(long partnerId, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.BillerRepository.GetAllQueryable(x => x.PartnerId == partnerId).FirstOrDefaultAsync(cancellationToken);
        if (entity is null)
        {
            return Result<BillerOutputModel>.Failure(
                localizer[LocalizationKeys.BillerNotFound, partnerId],
                AppHttpStatusCodes.NotFound);
        }

        return Result<BillerOutputModel>.Success(await GetBillerWithAccountsAndPartnerAsync(entity, cancellationToken));
    }

    public async Task<Result<BillerOutputModel>> UpdateAsync(long id, UpdateBillerInputModel model, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.BillerRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return Result<BillerOutputModel>.Failure(
                localizer[LocalizationKeys.BillerNotFound, id],
                AppHttpStatusCodes.NotFound);
        }

        if (await unitOfWork.BillerRepository.ExistsByCodeExceptIdAsync(model.BillerCode, id, cancellationToken))
        {
            return Result<BillerOutputModel>.Failure(
                localizer[LocalizationKeys.BillerAddCodeAlreadyExists, model.BillerCode],
                AppHttpStatusCodes.Conflict);
        }

        var legalName = model.LegalName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(legalName))
        {
            return Result<BillerOutputModel>.Failure(
                localizer[LocalizationKeys.BillerAddInvalidRequest],
                AppHttpStatusCodes.BadRequest);
        }

        var partner = entity.Partner
            ?? await unitOfWork.PartnerRepository.GetByIdAsync(entity.PartnerId, cancellationToken);
        if (partner is null)
        {
            return Result<BillerOutputModel>.Failure(
                localizer[LocalizationKeys.PartnerNotFound],
                AppHttpStatusCodes.NotFound);
        }

        if (await unitOfWork.PartnerRepository.ExistsByLegalNameExceptIdAsync(legalName, partner.Id, cancellationToken))
        {
            return Result<BillerOutputModel>.Failure(
                localizer[LocalizationKeys.BillerAddPartnerLegalNameAlreadyExists, legalName],
                AppHttpStatusCodes.Conflict);
        }

        partner.UpdateProfile(legalName, model.DisplayName, model.Active);
        unitOfWork.PartnerRepository.Update(partner);
        entity.Update(model.BillerCode, model.BillerName, model.ApplyAfterPay);
        unitOfWork.BillerRepository.Update(entity);
        await unitOfWork.CompleteAsync();

        return Result<BillerOutputModel>.Success(
            await GetBillerWithAccountsAndPartnerAsync(entity, cancellationToken),
            localizer[LocalizationKeys.BillerUpdateSuccess]);
    }

    public async Task<Result<bool>> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.BillerRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.BillerNotFound, id],
                AppHttpStatusCodes.NotFound);
        }

        var hasReferences = await unitOfWork.PartnerRepository.HasCustomersAsync(entity.PartnerId, cancellationToken)
                           || await unitOfWork.PartnerRepository.HasOtherBillersAsync(entity.PartnerId, id, cancellationToken)
                           || await unitOfWork.PartnerRepository.HasOtherPaymentProvidersAsync(entity.PartnerId, cancellationToken: cancellationToken)
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

        unitOfWork.BillerRepository.Remove(entity);
        var partner = entity.Partner
            ?? await unitOfWork.PartnerRepository.GetByIdAsync(entity.PartnerId, cancellationToken);
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

        return Result<bool>.Success(true, localizer[LocalizationKeys.BillerDeleteSuccess]);
    }

    private async Task<BillerOutputModel> GetBillerWithAccountsAndPartnerAsync(BillerEntity entity, CancellationToken cancellationToken)
    {
        var output = mapper.Map<BillerOutputModel>(entity);
        var partner = entity.Partner
            ?? await unitOfWork.PartnerRepository.GetByIdAsync(entity.PartnerId, cancellationToken);
        var accounts = await unitOfWork.AccountRepository.GetByPartnerIdWithRelationsAsync(entity.PartnerId, cancellationToken);
        output.Partner = partner is null ? null : mapper.Map<PartnerInfoOutputModel>(partner);
        output.Accounts = mapper.Map<IReadOnlyCollection<AccountOutputModel>>(accounts);
        return output;
    }

    private async Task<Result<(PartnerEntity Partner, BillerEntity Biller, IReadOnlyCollection<AccountEntity> Accounts)>> CreateBillerWithPartnerAndAccountsAsync(
        AddBillerInputModel model,
        CancellationToken cancellationToken)
    {
        var legalName = model.LegalName?.Trim();
        var displayName = string.IsNullOrWhiteSpace(model.DisplayName) ? null : model.DisplayName.Trim();
        var billerCode = model.BillerCode?.Trim() ?? string.Empty;
        var billerName = model.BillerName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(legalName) || string.IsNullOrWhiteSpace(billerCode) || string.IsNullOrWhiteSpace(billerName))
        {
            return Result<(PartnerEntity, BillerEntity, IReadOnlyCollection<AccountEntity>)>.Failure(
                localizer[LocalizationKeys.BillerAddInvalidRequest],
                AppHttpStatusCodes.BadRequest);
        }

        var existsByCode = await unitOfWork.BillerRepository.ExistsByCodeAsync(billerCode);
        if (existsByCode)
        {
            return Result<(PartnerEntity, BillerEntity, IReadOnlyCollection<AccountEntity>)>.Failure(
                localizer[LocalizationKeys.BillerAddCodeAlreadyExists, billerCode],
                AppHttpStatusCodes.Conflict);
        }

        var existsByLegalName = await unitOfWork.PartnerRepository.ExistsByLegalNameAsync(legalName);
        if (existsByLegalName)
        {
            return Result<(PartnerEntity, BillerEntity, IReadOnlyCollection<AccountEntity>)>.Failure(
                localizer[LocalizationKeys.BillerAddPartnerLegalNameAlreadyExists, legalName],
                AppHttpStatusCodes.Conflict);
        }

        var accountType = CachingFactory.GetValue<AccountTypeEntity>(x => x.Id == (int)AccountTypesEnum.BILLER_LIABILITY).FirstOrDefault();
        // await unitOfWork.AccountTypeRepository.GetByIdAsync((int)AccountTypesEnum.BILLER_LIABILITY, cancellationToken);
        if (accountType is null)
        {
            return Result<(PartnerEntity, BillerEntity, IReadOnlyCollection<AccountEntity>)>.Failure(
                localizer[LocalizationKeys.BillerAddAccountTypeNotFound, AccountTypesEnum.BILLER_LIABILITY.ToString()],
                AppHttpStatusCodes.BadRequest);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var partner = PartnerEntity.Create(
            PartnerTypeConstants.Biller,
            legalName,
            displayName,
            model.Active);
        unitOfWork.PartnerRepository.Add(partner);

        var biller = BillerEntity.Create(partner, billerCode, billerName);
        biller.ApplyAfterPay = model.ApplyAfterPay;
        unitOfWork.BillerRepository.Add(biller);

        var currencies = CachingFactory.GetValue<CurrencyEntity>();// await unitOfWork.LookupRepository.GetAllCurrenciesAsync();
        var accounts = new List<AccountEntity>();
        foreach (var currency in currencies)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var accountLiability = AccountEntity.Create(
                (int)AccountTypesEnum.BILLER_LIABILITY,
                partner,
                currency.Id,
                order: currency.Order,
                balance: 0);
            unitOfWork.AccountRepository.Add(accountLiability);
            accounts.Add(accountLiability);
            
            var accountBank = AccountEntity.Create(
                (int)AccountTypesEnum.BANK,
                partner,
                currency.Id,
                order: currency.Order,
                balance: 0);
            unitOfWork.AccountRepository.Add(accountBank);
            accounts.Add(accountBank);
        }

        return Result<(PartnerEntity, BillerEntity, IReadOnlyCollection<AccountEntity>)>.Success(
            (partner, biller, accounts),
            AppHttpStatusCodes.Created);
    }
}
