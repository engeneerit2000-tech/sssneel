using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.Accounting.Interfaces;
using SNEL.WalletPlatform.Application.Accounting.Models.Input;
using SNEL.WalletPlatform.Application.Accounting.Models.Internal;
using SNEL.WalletPlatform.Application.Accounting.Models.Output;
using SNEL.WalletPlatform.Application.Billers.Models.Output;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Accounting.Enums;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Accounting.Services;

public sealed class AccountingService(
    IUnitOfWork unitOfWork, 
    IMapper mapper, 
    IStringLocalizer<SharedResource> localizer, 
    ICurrentUserContext currentUserContext) : IAccountingService
{
    #region ApplicationService Methods

    public async Task<Result<PagedResult<AccountOutputModel>>> GetCustomerAccountsAsync(AccountsQueryInputModel model)
    {        
        var customer = await GetCurrentCustomerByPartnerAsync();
        if (customer is null)
        {
            return Result<PagedResult<AccountOutputModel>>.Failure(localizer[LocalizationKeys.CustomerProfileNotFound], AppHttpStatusCodes.NotFound);
        }

        model.PartnerId = customer.PartnerId;
        var normalizedQuery = NormalizeQuery(model);
        var subscriptions = await unitOfWork.AccountRepository.GetPagedAsync(normalizedQuery);

        var totalCount = await unitOfWork.AccountRepository.CountAsync(normalizedQuery);
        var items = mapper.Map<AccountOutputModel[]>(subscriptions);

        return Result<PagedResult<AccountOutputModel>>.Success(new PagedResult<AccountOutputModel>(items, totalCount));
    }
    public async Task<Result<PagedResult<AccountOutputModel>>> GetAccountsAsync(AccountsQueryInputModel model)
    {
        var normalizedQuery = NormalizeQuery(model);
        var subscriptions = await unitOfWork.AccountRepository.GetPagedAsync(normalizedQuery);

        var totalCount = await unitOfWork.AccountRepository.CountAsync(normalizedQuery);
        var items = mapper.Map<AccountOutputModel[]>(subscriptions);

        return Result<PagedResult<AccountOutputModel>>.Success(new PagedResult<AccountOutputModel>(items, totalCount));
    }
    private AccountsQueryInputModel NormalizeQuery(AccountsQueryInputModel model)
    {
        var mapped = mapper.Map<AccountsQueryInputModel>(model);

        mapped.PageNumber = mapped.PageNumber <= 0 ? 1 : mapped.PageNumber;
        mapped.PageSize = mapped.PageSize <= 0 ? 20 : Math.Min(mapped.PageSize, 100);
        mapped.SortBy = string.IsNullOrWhiteSpace(mapped.SortBy) ? null : mapped.SortBy.Trim();

        return mapped;
    }

    public async Task<Result<bool>> UpdateCustomerAccountsOrderAsync(IReadOnlyList<UpdateAccountOrderItemInputModel> items)
    {
        if (items is null || items.Count == 0)
        {
            return Result<bool>.Failure(localizer[LocalizationKeys.AccountOrdersUpdateItemsRequired], AppHttpStatusCodes.BadRequest);
        }

        var customer = await GetCurrentCustomerByPartnerAsync();
        if (customer is null)
        {
            return Result<bool>.Failure(localizer[LocalizationKeys.CustomerProfileNotFound], AppHttpStatusCodes.NotFound);
        }

        foreach (var item in items)
        {
            if (item.AccountId == Guid.Empty)
            {
                return Result<bool>.Failure(localizer[LocalizationKeys.AccountNotFoundForCustomer, item.AccountId], AppHttpStatusCodes.BadRequest);
            }

            var account = await unitOfWork.AccountRepository.GetByIdAsync(item.AccountId);
            if (account is null)
            {
                return Result<bool>.Failure(localizer[LocalizationKeys.AccountNotFoundForCustomer, item.AccountId], AppHttpStatusCodes.NotFound);
            }

            if (account.PartnerId != customer.PartnerId)
            {
                return Result<bool>.Failure(localizer[LocalizationKeys.AccountNotBelongToCustomerPartner, item.AccountId], AppHttpStatusCodes.BadRequest);
            }

            account.SetOrder(item.Order);
            unitOfWork.AccountRepository.Update(account);
        }

        await unitOfWork.CompleteAsync();

        return Result<bool>.Success(
            true,
            localizer[LocalizationKeys.AccountOrderReorderedSuccessfully],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<(List<JournalEntity> journals, List<PlatformInvoiceEntity> platformInvoices)>> CreateJournalsForPOI(long customerPartnerId, long pspPartnerId, long? billerPartnerId, PaymentOrderItemEntity poi, List<AccountEntity> accounts, bool autoSettleForPSP)
    {
        var result = new List<JournalEntity>();
        var platformInvoicesForPoi = new List<PlatformInvoiceEntity>();
        switch (poi.ItemType)
        {
            case OrderItemType.Topup:
            {
                var customerAccount = accounts.FirstOrDefault(x => x.PartnerId == customerPartnerId && x.CurrencyId == poi.CurrencyId);
                if (customerAccount == null)
                {
                    customerAccount = await unitOfWork.AccountRepository.GetByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(customerPartnerId, poi.CurrencyId.Value,(int)AccountTypesEnum.CUSTOMER_LIABILITY);
                    if (customerAccount is null) throw new Exception(localizer[LocalizationKeys.AccountingCustomerAccountNotFound]);
                    accounts.Add(customerAccount);
                }

                var pspLiabilityAccount = accounts.FirstOrDefault(x => x.PartnerId == pspPartnerId && x.CurrencyId == poi.CurrencyId && x.AccountTypeId==(int)AccountTypesEnum.PSP_LIABILITY);
                if (pspLiabilityAccount == null)
                {
                    pspLiabilityAccount = await unitOfWork.AccountRepository.GetByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(pspPartnerId, poi.CurrencyId.Value,(int)AccountTypesEnum.PSP_LIABILITY);
                    if (pspLiabilityAccount is null) throw new Exception(localizer[LocalizationKeys.AccountingPspAccountNotFound]);
                    accounts.Add(pspLiabilityAccount);
                }
                var pspBankAccount = accounts.FirstOrDefault(x => x.PartnerId == pspPartnerId && x.CurrencyId == poi.CurrencyId && x.AccountTypeId==(int)AccountTypesEnum.BANK);
                if (pspBankAccount == null)
                {
                    pspBankAccount = await unitOfWork.AccountRepository.GetByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(pspPartnerId, poi.CurrencyId.Value,(int)AccountTypesEnum.BANK);
                    if (pspBankAccount is null) throw new Exception(localizer[LocalizationKeys.AccountingPspBankAccountNotFound]);
                    accounts.Add(pspBankAccount);
                }

                var revenueAccount = accounts.FirstOrDefault(x => x.PartnerId == 1 && x.AccountTypeId == (int)AccountTypesEnum.REVENUE && x.CurrencyId == poi.CurrencyId);
                if (revenueAccount == null)
                {
                    revenueAccount = (await unitOfWork.AccountRepository.GetAllQueryableAsync(x => x.PartnerId == 1 && x.AccountTypeId == (int)AccountTypesEnum.REVENUE && x.CurrencyId == poi.CurrencyId)).FirstOrDefault();
                    if (revenueAccount is null) throw new Exception(localizer[LocalizationKeys.AccountingRevenueAccountNotFound]);
                    accounts.Add(revenueAccount);
                }

                var taxPayableAccount = accounts.FirstOrDefault(x => x.PartnerId == 1 && x.AccountTypeId == (int)AccountTypesEnum.TAX_PAYABLE && x.CurrencyId == poi.CurrencyId);
                if (taxPayableAccount == null)
                {
                    taxPayableAccount = (await unitOfWork.AccountRepository.GetAllQueryableAsync(x => x.PartnerId == 1 && x.AccountTypeId == (int)AccountTypesEnum.TAX_PAYABLE && x.CurrencyId == poi.CurrencyId)).FirstOrDefault();
                    if (taxPayableAccount is null) throw new Exception(localizer[LocalizationKeys.AccountingTaxPayableAccountNotFound]);
                    accounts.Add(taxPayableAccount);
                }

                var topupCostAccount = accounts.FirstOrDefault(x => x.PartnerId == 1 && x.AccountTypeId == (int)AccountTypesEnum.TOPUP_COST && x.CurrencyId == poi.CurrencyId);
                if (topupCostAccount == null)
                {
                    topupCostAccount = (await unitOfWork.AccountRepository.GetAllQueryableAsync(x => x.PartnerId == 1 && x.AccountTypeId == (int)AccountTypesEnum.TOPUP_COST && x.CurrencyId == poi.CurrencyId)).FirstOrDefault();
                    if (topupCostAccount is null) throw new Exception(localizer[LocalizationKeys.AccountingTopupCostAccountNotFound]);
                    accounts.Add(topupCostAccount);
                }

                var platformInvoiceDate = poi.CreatedAtUtc;

                PlatformInvoiceEntity? topupFeesInvoice = null;
                if (poi.PaymentProviderFees > 0 || poi.PaymentProviderFeesTax > 0)
                {
                    topupFeesInvoice = PlatformInvoiceEntity.Create(
                        customerPartnerId,
                        poi.CurrencyId!.Value,
                        LocalizationKeys.PlatformInvoiceDescriptionKeys.TopupFees,
                        SerializeInvoiceDescriptionParams(
                            customerPartnerId.ToString(),
                            pspPartnerId.ToString(),
                            poi.Id.ToString()),
                        platformInvoiceDate,
                        poi.PaymentProviderFees,
                        poi.PaymentProviderFeesTax,
                        poi.PaymentProviderFees + poi.PaymentProviderFeesTax);
                    platformInvoicesForPoi.Add(topupFeesInvoice);
                }

                PlatformInvoiceEntity? topupCostInvoice = null;
                if (poi.PaymentProviderCost > 0 || poi.PaymentProviderCostTax > 0)
                {
                    topupCostInvoice = PlatformInvoiceEntity.Create(
                        customerPartnerId,
                        poi.CurrencyId!.Value,
                        LocalizationKeys.PlatformInvoiceDescriptionKeys.TopupCost,
                        SerializeInvoiceDescriptionParams(
                            customerPartnerId.ToString(),
                            pspPartnerId.ToString(),
                            poi.Id.ToString()),
                        platformInvoiceDate,
                        poi.PaymentProviderCost,
                        poi.PaymentProviderCostTax,
                        poi.PaymentProviderCost + poi.PaymentProviderCostTax);
                    platformInvoicesForPoi.Add(topupCostInvoice);
                }

                if(poi.TotalAmount>0) result.Add(CreateJournalForTopup(poi.Id, customerAccount.Id, pspLiabilityAccount.Id, poi.TotalAmount,1));
                if(poi.PaymentProviderFees>0) result.Add(CreateJournalForTopupFees(poi.Id, customerAccount.Id, revenueAccount.Id, poi.PaymentProviderFees,2, topupFeesInvoice));
                if(poi.PaymentProviderFeesTax>0) result.Add(CreateJournalForTopupFeesTax(poi.Id, customerAccount.Id, taxPayableAccount.Id, poi.PaymentProviderFeesTax,3, topupFeesInvoice));
                if(poi.PaymentProviderCost>0) result.Add(CreateJournalForTopupCost(poi.Id, topupCostAccount.Id, pspLiabilityAccount.Id, poi.PaymentProviderCost,4, topupCostInvoice));
                if(poi.PaymentProviderCostTax>0) result.Add(CreateJournalForTopupCostTax(poi.Id, pspLiabilityAccount.Id, taxPayableAccount.Id, poi.PaymentProviderCostTax,5, topupCostInvoice));
                if(autoSettleForPSP && poi.PSPSettlementAmount>0)result.Add(CreateJournalForTopupSettlement(poi.Id, pspBankAccount.Id, pspLiabilityAccount.Id, poi.PSPSettlementAmount,6));
                
            }
                break;
            case OrderItemType.PayBill:
            case OrderItemType.MeterCharge:
            {
                var billingServiceCurrency = poi.BillingService.Currencies.FirstOrDefault(x=>x.CurrencyId==poi.CurrencyId && x.Active)?? throw new Exception(localizer[LocalizationKeys.AccountingBillingServiceCurrencyNotConfigured, poi.CurrencyId!, poi.BillingService.Name]);
                var customerAccount = accounts.FirstOrDefault(x => x.PartnerId == customerPartnerId && x.CurrencyId == poi.CurrencyId);
                if (customerAccount == null)
                {
                    customerAccount = await unitOfWork.AccountRepository.GetByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(customerPartnerId, poi.CurrencyId.Value
                    ,(int)AccountTypesEnum.CUSTOMER_LIABILITY);
                    if (customerAccount is null) throw new Exception(localizer[LocalizationKeys.AccountingCustomerAccountNotFound]);
                    accounts.Add(customerAccount);
                }

                if (billerPartnerId == null)throw new Exception(localizer[LocalizationKeys.AccountingBillerNotFound]);

                var billerLiabilityAccount = accounts.FirstOrDefault(x => x.PartnerId == billerPartnerId && x.CurrencyId == poi.CurrencyId && x.AccountTypeId==(int)AccountTypesEnum.BILLER_LIABILITY);
                if (billerLiabilityAccount == null)
                {
                    billerLiabilityAccount = await unitOfWork.AccountRepository.GetByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(billerPartnerId.Value, poi.CurrencyId.Value
                    ,(int)AccountTypesEnum.BILLER_LIABILITY);
                    if (billerLiabilityAccount is null) throw new Exception(localizer[LocalizationKeys.AccountingBillerAccountNotFound]);
                    accounts.Add(billerLiabilityAccount);
                }
                var billerBankAccount = accounts.FirstOrDefault(x => x.PartnerId == billerPartnerId && x.CurrencyId == poi.CurrencyId && x.AccountTypeId==(int)AccountTypesEnum.BANK);
                if (billerBankAccount == null)
                {
                    billerBankAccount = await unitOfWork.AccountRepository.GetByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(billerPartnerId.Value, poi.CurrencyId.Value
                        ,(int)AccountTypesEnum.BANK);
                    if (billerBankAccount is null) throw new Exception(localizer[LocalizationKeys.AccountingBillerBankAccountNotFound]);
                    accounts.Add(billerBankAccount);
                }

                var revenueAccount = accounts.FirstOrDefault(x => x.PartnerId == 1 && x.AccountTypeId == (int)AccountTypesEnum.REVENUE && x.CurrencyId == poi.CurrencyId);
                if (revenueAccount == null)
                {
                    revenueAccount = (await unitOfWork.AccountRepository.GetAllQueryableAsync(x => x.PartnerId == 1 && x.AccountTypeId == (int)AccountTypesEnum.REVENUE && x.CurrencyId == poi.CurrencyId)).FirstOrDefault();
                    if (revenueAccount is null) throw new Exception(localizer[LocalizationKeys.AccountingRevenueAccountNotFound]);
                    accounts.Add(revenueAccount);
                }

                var taxPayableAccount = accounts.FirstOrDefault(x => x.PartnerId == 1 && x.AccountTypeId == (int)AccountTypesEnum.TAX_PAYABLE && x.CurrencyId == poi.CurrencyId);
                if (taxPayableAccount == null)
                {
                    taxPayableAccount = (await unitOfWork.AccountRepository.GetAllQueryableAsync(x => x.PartnerId == 1 && x.AccountTypeId == (int)AccountTypesEnum.TAX_PAYABLE && x.CurrencyId == poi.CurrencyId)).FirstOrDefault();
                    if (taxPayableAccount is null) throw new Exception(localizer[LocalizationKeys.AccountingTaxPayableAccountNotFound]);
                    accounts.Add(taxPayableAccount);
                }

                var paymentCostAccount = accounts.FirstOrDefault(x => x.PartnerId == 1 && x.AccountTypeId == (int)AccountTypesEnum.PAYMENT_COST && x.CurrencyId == poi.CurrencyId);
                if (paymentCostAccount == null)
                {
                    paymentCostAccount = (await unitOfWork.AccountRepository.GetAllQueryableAsync(x => x.PartnerId == 1 && x.AccountTypeId == (int)AccountTypesEnum.PAYMENT_COST && x.CurrencyId == poi.CurrencyId)).FirstOrDefault();
                    if (paymentCostAccount is null) throw new Exception(localizer[LocalizationKeys.AccountingPaymentCostAccountNotFound]);
                    accounts.Add(paymentCostAccount);
                }

                var platformInvoiceDate = poi.CreatedAtUtc;
                var billerPid = billerPartnerId!.Value;

                PlatformInvoiceEntity? paymentFeesInvoice = null;
                if (poi.BillerFees > 0 || poi.BillerFeesTax > 0)
                {
                    paymentFeesInvoice = PlatformInvoiceEntity.Create(
                        customerPartnerId,
                        poi.CurrencyId!.Value,
                        LocalizationKeys.PlatformInvoiceDescriptionKeys.PaymentFees,
                        SerializeInvoiceDescriptionParams(
                            customerPartnerId.ToString(),
                            billerPid.ToString()),
                        platformInvoiceDate,
                        poi.BillerFees,
                        poi.BillerFeesTax,
                        poi.BillerFees + poi.BillerFeesTax);
                    platformInvoicesForPoi.Add(paymentFeesInvoice);
                }

                PlatformInvoiceEntity? paymentCostInvoice = null;
                if (poi.BillerCost > 0 || poi.BillerCostTax > 0)
                {
                    paymentCostInvoice = PlatformInvoiceEntity.Create(
                        customerPartnerId,
                        poi.CurrencyId!.Value,
                        LocalizationKeys.PlatformInvoiceDescriptionKeys.PaymentCost,
                        SerializeInvoiceDescriptionParams(
                            customerPartnerId.ToString(),
                            billerPid.ToString()),
                        platformInvoiceDate,
                        poi.BillerCost,
                        poi.BillerCostTax,
                        poi.BillerCost + poi.BillerCostTax);
                    platformInvoicesForPoi.Add(paymentCostInvoice);
                }

                PlatformInvoiceEntity? paymentCommissionInvoice = null;
                if (poi.BillerCommission > 0 || poi.BillerCommissionTax > 0)
                {
                    paymentCommissionInvoice = PlatformInvoiceEntity.Create(
                        billerPid,
                        poi.CurrencyId!.Value,
                        LocalizationKeys.PlatformInvoiceDescriptionKeys.PaymentCommission,
                        SerializeInvoiceDescriptionParams(
                            billerPid.ToString(),
                            customerPartnerId.ToString()),
                        platformInvoiceDate,
                        poi.BillerCommission,
                        poi.BillerCommissionTax,
                        poi.BillerCommission + poi.BillerCommissionTax);
                    platformInvoicesForPoi.Add(paymentCommissionInvoice);
                }

                if(poi.Amount>0) result.Add(CreateJournalForPayment(poi.Id, customerAccount.Id, billerLiabilityAccount.Id, poi.Amount,1));
                if(poi.BillerFees>0) result.Add(CreateJournalForPaymentFees(poi.Id, customerAccount.Id, revenueAccount.Id, poi.BillerFees,2, paymentFeesInvoice));
                if(poi.BillerFeesTax>0) result.Add(CreateJournalForPaymentFeesTax(poi.Id, customerAccount.Id, taxPayableAccount.Id, poi.BillerFeesTax,3, paymentFeesInvoice));
                if(poi.BillerCost>0) result.Add(CreateJournalForPaymentCost(poi.Id, paymentCostAccount.Id, billerLiabilityAccount.Id, poi.BillerCost,4, paymentCostInvoice));
                if(poi.BillerCostTax>0) result.Add(CreateJournalForPaymentCostTax(poi.Id, billerLiabilityAccount.Id, taxPayableAccount.Id, poi.BillerCostTax,5, paymentCostInvoice));
                if(poi.BillerCommission>0) result.Add(CreateJournalForPaymentCommission(poi.Id, billerLiabilityAccount.Id, revenueAccount.Id, poi.BillerCommission,6, paymentCommissionInvoice));
                if(poi.BillerCommissionTax>0) result.Add(CreateJournalForPaymentCommissionTax(poi.Id, billerLiabilityAccount.Id, taxPayableAccount.Id, poi.BillerCommissionTax,7, paymentCommissionInvoice));
                if(billingServiceCurrency.AutoSettleJournal && poi.BillerSettlementAmount>0)result.Add(CreateJournalForPaymentSettlement(poi.Id, billerBankAccount.Id, billerLiabilityAccount.Id, poi.BillerSettlementAmount,8));
            }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(poi), localizer[LocalizationKeys.AccountingUnsupportedItemType]);
        }


        return Result<(List<JournalEntity>, List<PlatformInvoiceEntity>)>.Success((result, platformInvoicesForPoi));
    }

    #endregion

    #region DomainService Methods

    #region TOPUP POI Journals

    private JournalEntity CreateJournalForTopup(Guid poiId, Guid customerAccountId, Guid pspAccountId, decimal amount, int order)
    {
        var model = new CreateJournalModel
        {
            JournalType = JournalTypesEnum.TOPUP,
            DescriptionTemplate = LocalizationKeys.JournalPostingDescriptionKeys.Topup,
            Amount = amount,
            SourceType = JournalSourceTypesEnum.PAYMENT_ORDER_ITEM,
            SourceRef = poiId.ToString(),
            SourceDate = DateTimeOffset.UtcNow,
            FirstAccountCredit = true,
            FirstAccountId = customerAccountId,
            FirstJournalLineType = JournalLineTypesEnum.CUSTOMER_LIABILITY,
            SecondAccountCredit = false,
            SecondAccountId = pspAccountId,
            SecondJournalLineType = JournalLineTypesEnum.PSP_LIABILITY,
            JournalPostingDate = DateTimeOffset.UtcNow,
            Order = order
        };


        return CreateJournalWithLines(model);
    }

    private JournalEntity CreateJournalForTopupFees(Guid poiId, Guid customerAccountId, Guid revenueAccountId, decimal amount, int order, PlatformInvoiceEntity? platformInvoice = null)
    {
        var model = new CreateJournalModel
        {
            JournalType = JournalTypesEnum.TOPUP_FEES,
            DescriptionTemplate = LocalizationKeys.JournalPostingDescriptionKeys.TopupFees,
            Amount = amount,
            SourceType = JournalSourceTypesEnum.PAYMENT_ORDER_ITEM,
            SourceRef = poiId.ToString(),
            SourceDate = DateTimeOffset.UtcNow,
            FirstAccountCredit = false,
            FirstAccountId = customerAccountId,
            FirstJournalLineType = JournalLineTypesEnum.TOPUP_FEES,
            SecondAccountCredit = true,
            SecondAccountId = revenueAccountId,
            SecondJournalLineType = JournalLineTypesEnum.TOPUP_FEES,
            JournalPostingDate = DateTimeOffset.UtcNow,
            Order = order,
            PlatformInvoice = platformInvoice
        };


        return CreateJournalWithLines(model);
    }

    private JournalEntity CreateJournalForTopupFeesTax(Guid poiId, Guid customerAccountId, Guid taxPayableAccountId, decimal amount, int order, PlatformInvoiceEntity? platformInvoice = null)
    {
        var model = new CreateJournalModel
        {
            JournalType = JournalTypesEnum.TOPUP_FEES_TAX,
            DescriptionTemplate = LocalizationKeys.JournalPostingDescriptionKeys.TopupFeesTax,
            Amount = amount,
            SourceType = JournalSourceTypesEnum.PAYMENT_ORDER_ITEM,
            SourceRef = poiId.ToString(),
            SourceDate = DateTimeOffset.UtcNow,
            FirstAccountCredit = false,
            FirstAccountId = customerAccountId,
            FirstJournalLineType = JournalLineTypesEnum.TOPUP_FEES_TAX,
            SecondAccountCredit = true,
            SecondAccountId = taxPayableAccountId,
            SecondJournalLineType = JournalLineTypesEnum.TOPUP_FEES_TAX,
            JournalPostingDate = DateTimeOffset.UtcNow,
            Order=order,
            PlatformInvoice = platformInvoice
        };


        return CreateJournalWithLines(model);
    }

    private JournalEntity CreateJournalForTopupCost(Guid poiId, Guid topupExpenseAccountId, Guid pspAccountId, decimal amount, int order, PlatformInvoiceEntity? platformInvoice = null)
    {
        var model = new CreateJournalModel
        {
            JournalType = JournalTypesEnum.TOPUP_COST,
            DescriptionTemplate = LocalizationKeys.JournalPostingDescriptionKeys.TopupCost,
            Amount = amount,
            SourceType = JournalSourceTypesEnum.PAYMENT_ORDER_ITEM,
            SourceRef = poiId.ToString(),
            SourceDate = DateTimeOffset.UtcNow,
            FirstAccountCredit = true,
            FirstAccountId = topupExpenseAccountId,
            FirstJournalLineType = JournalLineTypesEnum.TOPUP_COST,
            SecondAccountCredit = true,
            SecondAccountId = pspAccountId,
            SecondJournalLineType = JournalLineTypesEnum.TOPUP_COST,
            JournalPostingDate = DateTimeOffset.UtcNow,
            Order=order,
            PlatformInvoice = platformInvoice
        };


        return CreateJournalWithLines(model);
    }

    private JournalEntity CreateJournalForTopupCostTax(Guid poiId, Guid pspAccountId, Guid taxPayableAccountId, decimal amount, int order, PlatformInvoiceEntity? platformInvoice = null)
    {
        var model = new CreateJournalModel
        {
            JournalType = JournalTypesEnum.TOPUP_COST_TAX,
            DescriptionTemplate = LocalizationKeys.JournalPostingDescriptionKeys.TopupCostTax,
            Amount = amount,
            SourceType = JournalSourceTypesEnum.PAYMENT_ORDER_ITEM,
            SourceRef = poiId.ToString(),
            SourceDate = DateTimeOffset.UtcNow,
            FirstAccountCredit = true,
            FirstAccountId = pspAccountId,
            FirstJournalLineType = JournalLineTypesEnum.TOPUP_COST_TAX,
            SecondAccountCredit = false,
            SecondAccountId = taxPayableAccountId,
            SecondJournalLineType = JournalLineTypesEnum.TOPUP_COST_TAX,
            JournalPostingDate = DateTimeOffset.UtcNow,
            Order=order,
            PlatformInvoice = platformInvoice
        };


        return CreateJournalWithLines(model);
    }
    private JournalEntity CreateJournalForTopupSettlement(Guid poiId, Guid pspBankAccountId, Guid pspLiabilityAccountId, decimal amount, int order)
    {
        var model = new CreateJournalModel
        {
            JournalType = JournalTypesEnum.TOPUP_SETTLEMENT,
            DescriptionTemplate = LocalizationKeys.JournalPostingDescriptionKeys.TopupSettlement,
            Amount = amount,
            SourceType = JournalSourceTypesEnum.PAYMENT_ORDER_ITEM,
            SourceRef = poiId.ToString(),
            SourceDate = DateTimeOffset.UtcNow,
            FirstAccountCredit = true,
            FirstAccountId = pspBankAccountId,
            FirstJournalLineType = JournalLineTypesEnum.TOPUP_SETTLEMENT,
            SecondAccountCredit = true,
            SecondAccountId = pspLiabilityAccountId,
            SecondJournalLineType = JournalLineTypesEnum.TOPUP_SETTLEMENT,
            JournalPostingDate = DateTimeOffset.UtcNow,
            Order=order
        };


        return CreateJournalWithLines(model);
    }

    #endregion

    #region BILL POI Journals

    private JournalEntity CreateJournalForPayment(Guid poiId, Guid customerAccountId, Guid billerAccountId, decimal amount, int order)
    {
        var model = new CreateJournalModel
        {
            JournalType = JournalTypesEnum.PAYMENT,
            DescriptionTemplate = LocalizationKeys.JournalPostingDescriptionKeys.BillPayment,
            Amount = amount,
            SourceType = JournalSourceTypesEnum.PAYMENT_ORDER_ITEM,
            SourceRef = poiId.ToString(),
            SourceDate = DateTimeOffset.UtcNow,
            FirstAccountCredit = false,
            FirstAccountId = customerAccountId,
            FirstJournalLineType = JournalLineTypesEnum.CUSTOMER_LIABILITY,
            SecondAccountCredit = true,
            SecondAccountId = billerAccountId,
            SecondJournalLineType = JournalLineTypesEnum.BILLER_LIABILITY,
            JournalPostingDate = DateTimeOffset.UtcNow,
            Order = order
        };


        return CreateJournalWithLines(model);
    }

    private JournalEntity CreateJournalForPaymentFees(Guid poiId, Guid customerAccountId, Guid revenueAccountId, decimal amount, int order, PlatformInvoiceEntity? platformInvoice = null)
    {
        var model = new CreateJournalModel
        {
            JournalType = JournalTypesEnum.PAYMENT_FEES,
            DescriptionTemplate = LocalizationKeys.JournalPostingDescriptionKeys.PaymentFees,
            Amount = amount,
            SourceType = JournalSourceTypesEnum.PAYMENT_ORDER_ITEM,
            SourceRef = poiId.ToString(),
            SourceDate = DateTimeOffset.UtcNow,
            FirstAccountCredit = false,
            FirstAccountId = customerAccountId,
            FirstJournalLineType = JournalLineTypesEnum.PAYMENT_FEES,
            SecondAccountCredit = true,
            SecondAccountId = revenueAccountId,
            SecondJournalLineType = JournalLineTypesEnum.PAYMENT_FEES,
            JournalPostingDate = DateTimeOffset.UtcNow,
            Order = order,
            PlatformInvoice = platformInvoice
        };


        return CreateJournalWithLines(model);
    }

    private JournalEntity CreateJournalForPaymentFeesTax(Guid poiId, Guid customerAccountId, Guid taxPayableAccountId, decimal amount, int order, PlatformInvoiceEntity? platformInvoice = null)
    {
        var model = new CreateJournalModel
        {
            JournalType = JournalTypesEnum.PAYMENT_FEES_TAX,
            DescriptionTemplate = LocalizationKeys.JournalPostingDescriptionKeys.PaymentFeesTax,
            Amount = amount,
            SourceType = JournalSourceTypesEnum.PAYMENT_ORDER_ITEM,
            SourceRef = poiId.ToString(),
            SourceDate = DateTimeOffset.UtcNow,
            FirstAccountCredit = false,
            FirstAccountId = customerAccountId,
            FirstJournalLineType = JournalLineTypesEnum.PAYMENT_FEES_TAX,
            SecondAccountCredit = true,
            SecondAccountId = taxPayableAccountId,
            SecondJournalLineType = JournalLineTypesEnum.PAYMENT_FEES_TAX,
            JournalPostingDate = DateTimeOffset.UtcNow,
            Order = order,
            PlatformInvoice = platformInvoice
        };


        return CreateJournalWithLines(model);
    }

    private JournalEntity CreateJournalForPaymentCost(Guid poiId, Guid paymentExpenseAccountId, Guid billerAccountId, decimal amount, int order, PlatformInvoiceEntity? platformInvoice = null)
    {
        var model = new CreateJournalModel
        {
            JournalType = JournalTypesEnum.PAYMENT_COST,
            DescriptionTemplate = LocalizationKeys.JournalPostingDescriptionKeys.PaymentCost,
            Amount = amount,
            SourceType = JournalSourceTypesEnum.PAYMENT_ORDER_ITEM,
            SourceRef = poiId.ToString(),
            SourceDate = DateTimeOffset.UtcNow,
            FirstAccountCredit = true,
            FirstAccountId = paymentExpenseAccountId,
            FirstJournalLineType = JournalLineTypesEnum.PAYMENT_COST,
            SecondAccountCredit = true,
            SecondAccountId = billerAccountId,
            SecondJournalLineType = JournalLineTypesEnum.PAYMENT_COST,
            JournalPostingDate = DateTimeOffset.UtcNow,
            Order = order,
            PlatformInvoice = platformInvoice
        };


        return CreateJournalWithLines(model);
    }

    private JournalEntity CreateJournalForPaymentCostTax(Guid poiId, Guid billerAccountId, Guid taxPayableAccountId, decimal amount, int order, PlatformInvoiceEntity? platformInvoice = null)
    {
        var model = new CreateJournalModel
        {
            JournalType = JournalTypesEnum.PAYMENT_COST_TAX,
            DescriptionTemplate = LocalizationKeys.JournalPostingDescriptionKeys.PaymentCostTax,
            Amount = amount,
            SourceType = JournalSourceTypesEnum.PAYMENT_ORDER_ITEM,
            SourceRef = poiId.ToString(),
            SourceDate = DateTimeOffset.UtcNow,
            FirstAccountCredit = true,
            FirstAccountId = billerAccountId,
            FirstJournalLineType = JournalLineTypesEnum.PAYMENT_COST_TAX,
            SecondAccountCredit = false,
            SecondAccountId = taxPayableAccountId,
            SecondJournalLineType = JournalLineTypesEnum.PAYMENT_COST_TAX,
            JournalPostingDate = DateTimeOffset.UtcNow,
            Order = order,
            PlatformInvoice = platformInvoice
        };


        return CreateJournalWithLines(model);
    }

    private JournalEntity CreateJournalForPaymentCommission(Guid poiId, Guid billerAccountId, Guid revenueAccountId, decimal amount, int order, PlatformInvoiceEntity? platformInvoice = null)
    {
        var model = new CreateJournalModel
        {
            JournalType = JournalTypesEnum.PAYMENT_COMMISSION,
            DescriptionTemplate = LocalizationKeys.JournalPostingDescriptionKeys.PaymentCommission,
            Amount = amount,
            SourceType = JournalSourceTypesEnum.PAYMENT_ORDER_ITEM,
            SourceRef = poiId.ToString(),
            SourceDate = DateTimeOffset.UtcNow,
            FirstAccountCredit = false,
            FirstAccountId = billerAccountId,
            FirstJournalLineType = JournalLineTypesEnum.PAYMENT_COMMISSION,
            SecondAccountCredit = true,
            SecondAccountId = revenueAccountId,
            SecondJournalLineType = JournalLineTypesEnum.PAYMENT_COMMISSION,
            JournalPostingDate = DateTimeOffset.UtcNow,
            Order = order,
            PlatformInvoice = platformInvoice
        };


        return CreateJournalWithLines(model);
    }

    private JournalEntity CreateJournalForPaymentCommissionTax(Guid poiId, Guid billerAccountId, Guid taxPayableAccountId, decimal amount, int order, PlatformInvoiceEntity? platformInvoice = null)
    {
        var model = new CreateJournalModel
        {
            JournalType = JournalTypesEnum.PAYMENT_COMMISSION_TAX,
            DescriptionTemplate = LocalizationKeys.JournalPostingDescriptionKeys.PaymentCommissionTax,
            Amount = amount,
            SourceType = JournalSourceTypesEnum.PAYMENT_ORDER_ITEM,
            SourceRef = poiId.ToString(),
            SourceDate = DateTimeOffset.UtcNow,
            FirstAccountCredit = false,
            FirstAccountId = billerAccountId,
            FirstJournalLineType = JournalLineTypesEnum.PAYMENT_COMMISSION_TAX,
            SecondAccountCredit = true,
            SecondAccountId = taxPayableAccountId,
            SecondJournalLineType = JournalLineTypesEnum.PAYMENT_COMMISSION_TAX,
            JournalPostingDate = DateTimeOffset.UtcNow,
            Order = order,
            PlatformInvoice = platformInvoice
        };


        return CreateJournalWithLines(model);
    }
    private JournalEntity CreateJournalForPaymentSettlement(Guid poiId, Guid billerBankAccountId, Guid billerLiabilityAccountId, decimal amount, int order)
    {
        var model = new CreateJournalModel
        {
            JournalType = JournalTypesEnum.PAYMENT_SETTLEMENT,
            DescriptionTemplate = LocalizationKeys.JournalPostingDescriptionKeys.PaymentSettlement,
            Amount = amount,
            SourceType = JournalSourceTypesEnum.PAYMENT_ORDER_ITEM,
            SourceRef = poiId.ToString(),
            SourceDate = DateTimeOffset.UtcNow,
            FirstAccountCredit = false,
            FirstAccountId = billerBankAccountId,
            FirstJournalLineType = JournalLineTypesEnum.PAYMENT_SETTLEMENT,
            SecondAccountCredit = false,
            SecondAccountId = billerLiabilityAccountId,
            SecondJournalLineType = JournalLineTypesEnum.PAYMENT_SETTLEMENT,
            JournalPostingDate = DateTimeOffset.UtcNow,
            Order=order
        };


        return CreateJournalWithLines(model);
    }

    #endregion

    #region Vouchers and Notes

    public async Task<Result<bool>> CreateJournalForPaymentVoucher(CreateJournalForJornalTypeInputModel model)
    {
        try
        {
            var partner = await unitOfWork.PartnerRepository.GetByIdAsync(model.PartnerId);
            if (partner is null)
            {
                return Result<bool>.Failure(localizer[LocalizationKeys.PartnerNotFound], AppHttpStatusCodes.NotFound);
            }
            var accountTypeId = GetAccountTypeIdByPartnerTypeId(partner.PartnerTypeId);
            var partnerAccount = await unitOfWork.AccountRepository.GetByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(model.PartnerId, model.CurrencyId, accountTypeId);
            if (partnerAccount is null)
            {
                return Result<bool>.Failure(localizer[LocalizationKeys.AccountNotFoundForCustomer], AppHttpStatusCodes.NotFound);
            }
            AccountEntity? bankAccount = unitOfWork.AccountRepository.GetById(model.PartnerBankAccountId);
            if (bankAccount is null)
            {
                return Result<bool>.Failure(localizer[LocalizationKeys.AccountNotFoundForCustomer], AppHttpStatusCodes.NotFound);
            }
            var journal = CreateJournalForPaymentVoucher(partnerAccount.Id, bankAccount.Id, model.Amount, model.SourceRef, DateTime.UtcNow);
            unitOfWork.JournalRepository.Add(journal);
            await unitOfWork.CompleteAsync();
            return Result<bool>.Success(true, localizer[LocalizationKeys.JournalPaymentVoucherCreateSuccess]);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(localizer[LocalizationKeys.UnexpectedError], AppHttpStatusCodes.Failed);
        }
    }

    public async Task<Result<bool>> CreateJournalForReciptVoucher(CreateJournalForJornalTypeInputModel model)
    {
        try
        {
            var partner = await unitOfWork.PartnerRepository.GetByIdAsync(model.PartnerId);
            if (partner is null)
            {
                return Result<bool>.Failure(localizer[LocalizationKeys.PartnerNotFound], AppHttpStatusCodes.NotFound);
            }
            var accountTypeId = GetAccountTypeIdByPartnerTypeId(partner.PartnerTypeId);
            var partnerAccount = await unitOfWork.AccountRepository.GetByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(model.PartnerId, model.CurrencyId, accountTypeId);
            if (partnerAccount is null)
            {
                return Result<bool>.Failure(localizer[LocalizationKeys.AccountNotFoundForCustomer], AppHttpStatusCodes.NotFound);
            }
            AccountEntity? bankAccount = unitOfWork.AccountRepository.GetById(model.PartnerBankAccountId);
            if (bankAccount is null)
            {
                return Result<bool>.Failure(localizer[LocalizationKeys.AccountNotFoundForCustomer], AppHttpStatusCodes.NotFound);
            }
            var journal = CreateJournalForReciptVoucher(partnerAccount.Id, bankAccount.Id, model.Amount, model.SourceRef, DateTime.UtcNow);
            unitOfWork.JournalRepository.Add(journal);
            await unitOfWork.CompleteAsync();
            return Result<bool>.Success(true, localizer[LocalizationKeys.JournalReceiptVoucherCreateSuccess]);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(localizer[LocalizationKeys.UnexpectedError], AppHttpStatusCodes.Failed);
        }
    }

    public async Task<Result<bool>> CreateJournalForCreditNote(CreateJournalForJornalTypeInputModel model)
    {
        try
        {
            var partner = await unitOfWork.PartnerRepository.GetByIdAsync(model.PartnerId);
            if (partner is null)
            {
                return Result<bool>.Failure(localizer[LocalizationKeys.PartnerNotFound], AppHttpStatusCodes.NotFound);
            }
            var accountTypeId = GetAccountTypeIdByPartnerTypeId(partner.PartnerTypeId);
            var partnerAccount = await unitOfWork.AccountRepository.GetByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(model.PartnerId, model.CurrencyId, accountTypeId);
            if (partnerAccount is null)
            {
                return Result<bool>.Failure(localizer[LocalizationKeys.AccountNotFoundForCustomer], AppHttpStatusCodes.NotFound);
            }
            AccountEntity? bankAccount = unitOfWork.AccountRepository.GetById(model.PartnerBankAccountId);
            if (bankAccount is null)
            {
                return Result<bool>.Failure(localizer[LocalizationKeys.AccountNotFoundForCustomer], AppHttpStatusCodes.NotFound);
            }
            var journal = CreateJournalForCreditNote(partnerAccount.Id, bankAccount.Id, model.Amount, model.SourceRef, DateTime.UtcNow);
            unitOfWork.JournalRepository.Add(journal);
            await unitOfWork.CompleteAsync();
            return Result<bool>.Success(true, localizer[LocalizationKeys.JournalCreditNoteCreateSuccess]);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(localizer[LocalizationKeys.UnexpectedError], AppHttpStatusCodes.Failed);
        }
    }
    public async Task<Result<bool>> CreateJournalForDebitNote(CreateJournalForJornalTypeInputModel model)
    {
        try
        {
            var partner = await unitOfWork.PartnerRepository.GetByIdAsync(model.PartnerId);
            if (partner is null)
            {
                return Result<bool>.Failure(localizer[LocalizationKeys.PartnerNotFound], AppHttpStatusCodes.NotFound);
            }
            var accountTypeId = GetAccountTypeIdByPartnerTypeId(partner.PartnerTypeId);
            var partnerAccount = await unitOfWork.AccountRepository.GetByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(model.PartnerId, model.CurrencyId, accountTypeId);
            if (partnerAccount is null)
            {
                return Result<bool>.Failure(localizer[LocalizationKeys.AccountNotFoundForCustomer], AppHttpStatusCodes.NotFound);
            }
            AccountEntity? bankAccount = unitOfWork.AccountRepository.GetById(model.PartnerBankAccountId);
            if (bankAccount is null)
            {
                return Result<bool>.Failure(localizer[LocalizationKeys.AccountNotFoundForCustomer], AppHttpStatusCodes.NotFound);
            }
            var journal = CreateJournalForDebitNote(partnerAccount.Id, bankAccount.Id, model.Amount, model.SourceRef, DateTime.UtcNow);
            unitOfWork.JournalRepository.Add(journal);
            await unitOfWork.CompleteAsync();
            return Result<bool>.Success(true, localizer[LocalizationKeys.JournalDebitNoteCreateSuccess]);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(localizer[LocalizationKeys.UnexpectedError], AppHttpStatusCodes.Failed);
        }
    }
    public async Task<Result<bool>> CreateJournalForManualJournal(CreateJournalForManualInputModel model)
    {
        try
        {
            var firstAccount = await unitOfWork.AccountRepository.GetByIdAsync(model.FirstAccountId);
            if (firstAccount is null)
            {
                return Result<bool>.Failure(localizer[LocalizationKeys.AccountNotFoundForCustomer], AppHttpStatusCodes.NotFound);
            }
            var secondAccount = await unitOfWork.AccountRepository.GetByIdAsync(model.SecondAccountId);
            if (secondAccount is null)
            {
                return Result<bool>.Failure(localizer[LocalizationKeys.AccountNotFoundForCustomer], AppHttpStatusCodes.NotFound);
            }        
            var journal = CreateJournalForManualJournal(model.FirstAccountId, model.FirstAccountCredit, model.SecondAccountId, model.SecondAccountCredit, model.Amount, model.SourceRef, DateTime.UtcNow);
            unitOfWork.JournalRepository.Add(journal);
            await unitOfWork.CompleteAsync();
            return Result<bool>.Success(true, localizer[LocalizationKeys.JournalManualJournalCreateSuccess]);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(localizer[LocalizationKeys.UnexpectedError], AppHttpStatusCodes.Failed);
        }
    }
    
    #region Private Methods

    private JournalEntity CreateJournalForPaymentVoucher(Guid partnerAccountId,Guid bankAccountId, decimal amount,string sourceRef, DateTimeOffset sourceDate)
    {
        var model = new CreateJournalModel
        {
            JournalType = JournalTypesEnum.PAYMENT_VOUCHER,
            DescriptionTemplate = LocalizationKeys.JournalPostingDescriptionKeys.PaymentVoucher,
            Amount = amount,
            SourceType = JournalSourceTypesEnum.EXTERNAL_PAYMENT,
            SourceRef = sourceRef,
            SourceDate = sourceDate,
            FirstAccountCredit = false,
            FirstAccountId = partnerAccountId,
            FirstJournalLineType = JournalLineTypesEnum.PAYMENT_VOUCHER,
            SecondAccountCredit = false,
            SecondAccountId = bankAccountId,
            SecondJournalLineType = JournalLineTypesEnum.PAYMENT_VOUCHER,
            JournalPostingDate = DateTimeOffset.UtcNow,
            Order = 1
        };


        return CreateJournalWithLines(model);
    }
    private JournalEntity CreateJournalForReciptVoucher(Guid partnerAccountId, Guid bankAccountId, decimal amount, string sourceRef, DateTimeOffset sourceDate)
    {
        var model = new CreateJournalModel
        {
            JournalType = JournalTypesEnum.RECEIPT_VOUCHER,
            DescriptionTemplate = LocalizationKeys.JournalPostingDescriptionKeys.ReceiptVoucher,
            Amount = amount,
            SourceType = JournalSourceTypesEnum.EXTERNAL_PAYMENT,
            SourceRef = sourceRef,
            SourceDate = sourceDate,
            FirstAccountCredit = true,
            FirstAccountId = partnerAccountId,
            FirstJournalLineType = JournalLineTypesEnum.RECEIPT_VOUCHER,
            SecondAccountCredit = true,
            SecondAccountId = bankAccountId,
            SecondJournalLineType = JournalLineTypesEnum.RECEIPT_VOUCHER,
            JournalPostingDate = DateTimeOffset.UtcNow,
            Order = 1
        };


        return CreateJournalWithLines(model);
    }
    private JournalEntity CreateJournalForCreditNote(Guid partnerAccountId, Guid bankAccountId, decimal amount, string sourceRef, DateTimeOffset sourceDate)
    {
        var model = new CreateJournalModel
        {
            JournalType = JournalTypesEnum.CREDIT_NOTE,
            DescriptionTemplate = LocalizationKeys.JournalPostingDescriptionKeys.CreditNote,
            Amount = amount,
            SourceType = JournalSourceTypesEnum.ADJUSTMENT_NOTE,
            SourceRef = sourceRef,
            SourceDate = sourceDate,
            FirstAccountCredit = true,
            FirstAccountId = partnerAccountId,
            FirstJournalLineType = JournalLineTypesEnum.CREDIT_NOTE,
            SecondAccountCredit = true,
            SecondAccountId = bankAccountId,
            SecondJournalLineType = JournalLineTypesEnum.CREDIT_NOTE,
            JournalPostingDate = DateTimeOffset.UtcNow,
            Order = 1
        };


        return CreateJournalWithLines(model);
    }
    private JournalEntity CreateJournalForDebitNote(Guid partnerAccountId, Guid bankAccountId, decimal amount, string sourceRef, DateTimeOffset sourceDate)
    {
        var model = new CreateJournalModel
        {
            JournalType = JournalTypesEnum.DEBIT_NOTE,
            DescriptionTemplate = LocalizationKeys.JournalPostingDescriptionKeys.DebitNote,
            Amount = amount,
            SourceType = JournalSourceTypesEnum.ADJUSTMENT_NOTE,
            SourceRef = sourceRef,
            SourceDate = sourceDate,
            FirstAccountCredit = false,
            FirstAccountId = partnerAccountId,
            FirstJournalLineType = JournalLineTypesEnum.DEBIT_NOTE,
            SecondAccountCredit = false,
            SecondAccountId = bankAccountId,
            SecondJournalLineType = JournalLineTypesEnum.DEBIT_NOTE,
            JournalPostingDate = DateTimeOffset.UtcNow,
            Order = 1
        };


        return CreateJournalWithLines(model);
    }
    private JournalEntity CreateJournalForManualJournal(Guid firstAccountId, bool firstAccountCredit, Guid secondAccountId, bool secondAccountCredit, decimal amount, string sourceRef, DateTimeOffset sourceDate)
    {
        var model = new CreateJournalModel
        {
            JournalType = JournalTypesEnum.MANUAL_JOURNAL,
            DescriptionTemplate = LocalizationKeys.JournalPostingDescriptionKeys.ManualJournal,
            Amount = amount,
            SourceType = JournalSourceTypesEnum.MANUAL,
            SourceRef = sourceRef,
            SourceDate = sourceDate,
            FirstAccountCredit = firstAccountCredit,
            FirstAccountId = firstAccountId,
            FirstJournalLineType = JournalLineTypesEnum.MANUAL_JOURNAL,
            SecondAccountCredit = secondAccountCredit,
            SecondAccountId = secondAccountId,
            SecondJournalLineType = JournalLineTypesEnum.MANUAL_JOURNAL,
            JournalPostingDate = DateTimeOffset.UtcNow,
            Order = 1
        };


        return CreateJournalWithLines(model);
    }
    private int GetAccountTypeIdByPartnerTypeId(int partnerTypeId)
    {
        int accountTypeId = 0;
        switch (partnerTypeId)
        {
            case 1:
                accountTypeId = (int)AccountTypesEnum.BILLER_LIABILITY;
                break;
            case 2:
                accountTypeId = (int)AccountTypesEnum.PSP_LIABILITY;
                break;
            case 3:
                accountTypeId = (int)AccountTypesEnum.CUSTOMER_LIABILITY;
                break;
            default:
                accountTypeId = (int)AccountTypesEnum.CUSTOMER_LIABILITY;
                break;
        }
        return accountTypeId;
    }
    public async Task<Result<IReadOnlyList<AccountOutputModel>>> GetAllByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(GetAccountsInputModel model)
    {
        var entity = await unitOfWork.AccountRepository.GetAllByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(model.PartnerId, model.CurrencyId, model.AccountTypeId);        
        var output = mapper.Map<IReadOnlyCollection<AccountOutputModel>>(entity);        
        return Result<IReadOnlyList<AccountOutputModel>>.Success(output.ToList());
    }
    public async Task<Result<IReadOnlyList<AccountOutputModel>>> GetAllBankAccountsForPartnerAsync(GetBankAccountsInputModel model)
    {
        var data_basic = await unitOfWork.AccountRepository.GetAllByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(model.PartnerId, model.CurrencyId, (int)AccountTypesEnum.BANK);
        var data = data_basic.ToList();
        if (model.IncludeDefaultAccount == true)
        {
            var platformBankAccount = await unitOfWork.AccountRepository.GetByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(1, model.CurrencyId, (int)AccountTypesEnum.BANK);
            if(platformBankAccount != null)
                data = (data.Append(platformBankAccount)).ToList();            
        }
        var output = mapper.Map<IReadOnlyList<AccountOutputModel>>(data);
        return Result<IReadOnlyList<AccountOutputModel>>.Success(output.ToList());
    }
    #endregion

    #endregion
    private JournalEntity CreateJournalWithLines(CreateJournalModel model)
    {
        var journal = JournalEntity.Create(model.JournalType, model.DescriptionTemplate, model.Amount, model.SourceType, model.SourceRef, model.SourceDate, DateTimeOffset.UtcNow,model.Order);

        journal.AddLine(model.FirstJournalLineType, model.FirstAccountId, model.FirstAccountCredit, model.Amount, 0m,1, model.PlatformInvoice);
        journal.AddLine(model.SecondJournalLineType, model.SecondAccountId, model.SecondAccountCredit, model.Amount, 0m,2, model.PlatformInvoice);
        return journal;
    }

    private static string? SerializeInvoiceDescriptionParams(params string[] values)
    {
        if (values.Length == 0) return null;
        return JsonSerializer.Serialize(values);
    }

    private async Task<Domain.Customers.CustomerEntity?> GetCurrentCustomerByPartnerAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currentUserContext.UserId))
            return null;

        var user = await unitOfWork.UserRepository.GetByIamRefWithPartnerAsync(currentUserContext.UserId, cancellationToken);
        if (user?.PartnerId is null)
            return null;

        return await unitOfWork.CustomerRepository.GetByPartnerIdAsync(user.PartnerId.Value, cancellationToken);
    }

    #endregion
}