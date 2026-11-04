using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.Accounting;
using SNEL.WalletPlatform.Application.Accounting.Interfaces;
using SNEL.WalletPlatform.Application.Accounting.Models.Input;
using SNEL.WalletPlatform.Application.Accounting.Models.Output;
using SNEL.WalletPlatform.Application.Billers.Interfaces;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.FileExport.Services;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Accounting.Enums;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Partners;

namespace SNEL.WalletPlatform.Application.Accounting.Services;

public sealed class JournalLineService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IStringLocalizer<SharedResource> localizer,
    IBillerAccessPolicy billerAccessPolicy,
    ExcelFileExportService excelFileExportService,
    IPaymentProviderAccessPolicy paymentProviderAccessPolicy) : IJournalLineService
{
    public async Task<Result<PagedResult<JournalLineOutputModel>>> GetJournalLinesAsync(
        JournalLinesQueryInputModel model,
        CancellationToken cancellationToken = default)
    {
        var normalizedQuery = NormalizeQuery(model);
        var journalLines = await unitOfWork.JournalLineRepository.GetPagedAsync(normalizedQuery);

        var totalCount = await unitOfWork.JournalLineRepository.CountAsync(normalizedQuery);
        var items = mapper.Map<JournalLineOutputModel[]>(journalLines);
        await EnrichJournalLinePlatformInvoicesAsync(items, cancellationToken);

        return Result<PagedResult<JournalLineOutputModel>>.Success(
            new PagedResult<JournalLineOutputModel>(items, totalCount));
    }

    public async Task<Result<PagedResultWithSummary<JournalLineStatementOutputModel, JournalLineStatementSummary>>> GetJournalLineStatementAsync(JournalLinesStatementInputModel model)
    {
        var paymentProvider = await paymentProviderAccessPolicy.GetPaymentProvidersReadScopeAsync();
        if (paymentProvider.IsSuccess)
        {
            if (paymentProvider.Value is not null)
            {
                model.PartnerId = paymentProvider.Value;
                model.AccountTypeId = (int)AccountTypesEnum.PSP_LIABILITY;
            }
        }
        else 
        {
            var biller = await billerAccessPolicy.GetBillersReadScopeAsync();
            if (biller.IsSuccess)
            {
                if (biller.Value is not null)
                {
                    model.PartnerId = biller.Value;
                    model.AccountTypeId = (int)AccountTypesEnum.BILLER_LIABILITY;
                }
            }
            else
            {
                return Result<PagedResultWithSummary<JournalLineStatementOutputModel, JournalLineStatementSummary>>.Failure(localizer[LocalizationKeys.UnAuthorizedDefaultMessage], AppHttpStatusCodes.Unauthorized);
            }
        }        

        var normalizedQuery = NormalizeQuery(model);
        
        var result = new List<JournalLineEntity>();
        if (normalizedQuery.PageNumber == 1)
        {
            var latest = unitOfWork.JournalLineRepository
                            .GetAllQueryable(x => x.UpdatedAtUtc < model.FromDate && 
                            (model.AccountTypeId != null ? x.Account.AccountTypeId == model.AccountTypeId : true) &&
                            (model.PartnerId != null ? x.Account.PartnerId == model.PartnerId : true) &&
                            (model.JournalLineTypeId != null ? x.JournalLineTypeId == model.JournalLineTypeId : true) &&
                            x.Account.CurrencyId == model.CurrencyId
                            )
                            .Include(x => x.Account)
                            .Include(x => x.Journal)
                            .Include(x => x.JournalLineType)
                            .OrderByDescending(x => x.CreatedAtUtc)
                            .AsNoTracking()
                            .FirstOrDefault();

            if (latest != null)
            {
                latest.Debit = 0;
                latest.Credit = 0;
                latest.Journal.Description = localizer[LocalizationKeys.JournalOpenBalance];
                latest.JournalLineType.Name = localizer[LocalizationKeys.JournalOpenBalance];
                latest.JournalLineType.Code = localizer[LocalizationKeys.JournalOpenBalance];
                latest.JournalLineType.Id = 0;
                latest.JournalLineTypeId = 0;
                latest.CreatedAtUtc = model.FromDate;
                latest.UpdatedAtUtc = model.FromDate;
                result.Add(latest);                
            }            
        }
        var internalQuery = mapper.Map<JournalLinesStatementInternalModel>(normalizedQuery);
        internalQuery.AddingFirstRecord = result.Count > 0;
        var journalLines = await unitOfWork.JournalLineRepository.GetJournalLineStatementAsync(internalQuery);
        var totalCount = await unitOfWork.JournalLineRepository.CountAsync(internalQuery) + result.Count;
        result.AddRange(journalLines);
        var items = mapper.Map<JournalLineStatementOutputModel[]>(result);
        var summary = new JournalLineStatementSummary(items.Sum(x => x.Credit), items.Sum(x => x.Debit));        
        return Result<PagedResultWithSummary<JournalLineStatementOutputModel, JournalLineStatementSummary>>.Success(
            new PagedResultWithSummary<JournalLineStatementOutputModel, JournalLineStatementSummary>(items, summary, totalCount));
    }

    public async Task<Result<FileContentResult>> ExportJournalLineStatementAsync(JournalLinesStatementInputModel model)
    {
        var paymentProvider = await paymentProviderAccessPolicy.GetPaymentProvidersReadScopeAsync();
        if (paymentProvider.IsSuccess)
        {
            if (paymentProvider.Value is not null)
            {
                model.PartnerId = paymentProvider.Value;
                model.AccountTypeId = (int)AccountTypesEnum.PSP_LIABILITY;
            }
        }
        else 
        {
            var biller = await billerAccessPolicy.GetBillersReadScopeAsync();
            if (biller.IsSuccess)
            {
                if (biller.Value is not null)
                {
                    model.PartnerId = biller.Value;
                    model.AccountTypeId = (int)AccountTypesEnum.BILLER_LIABILITY;
                }
            }
            else
            {
                return Result<FileContentResult>.Failure(localizer[LocalizationKeys.UnAuthorizedDefaultMessage], AppHttpStatusCodes.Unauthorized);
            }
        }       
        model.PageNumber = 1;
        model.PageSize = int.MaxValue;
        var normalizedQuery = NormalizeQuery(model);

        var result = new List<JournalLineEntity>();

        var latest = unitOfWork.JournalLineRepository
                        .GetAllQueryable(x => x.CreatedAtUtc < model.FromDate &&
                        (model.AccountTypeId != null ? x.Account.AccountTypeId == model.AccountTypeId : true) &&
                        (model.PartnerId != null ? x.Account.PartnerId == model.PartnerId : true) &&
                        (model.JournalLineTypeId != null ? x.JournalLineTypeId == model.JournalLineTypeId : true) &&
                        x.Account.CurrencyId == model.CurrencyId
                        )
                        .Include(x => x.Account)
                        .Include(x => x.Journal)
                        .Include(x => x.JournalLineType)
                        .OrderByDescending(x => x.CreatedAtUtc)
                        .AsNoTracking()
                        .FirstOrDefault();

        if (latest != null)
        {
            latest.Debit = 0;
            latest.Credit = 0;
            latest.Journal.Description = localizer[LocalizationKeys.JournalOpenBalance];
            latest.JournalLineType.Name = localizer[LocalizationKeys.JournalOpenBalance];
            latest.JournalLineType.Code = localizer[LocalizationKeys.JournalOpenBalance];
            latest.JournalLineType.Id = 0;
            latest.JournalLineTypeId = 0;
            result.Add(latest);
        }        
        var internalQuery = mapper.Map<JournalLinesStatementInternalModel>(normalizedQuery);
        internalQuery.AddingFirstRecord = result.Count > 0;
        var journalLines = await unitOfWork.JournalLineRepository.GetJournalLineStatementAsync(internalQuery);
        result.AddRange(journalLines);
        var items = mapper.Map<List<JournalLineStatementExcelModel>>(result);
        var data_objects = mapper.Map<List<object>>(items);
        var export = await excelFileExportService.ExportExcelFileAsync("JournalLineStatement", data_objects, typeof(JournalLineStatementExcelModel));
        if (!export.IsSuccess)
        {
            return export;
        }

        return Result<FileContentResult>.Success(
            export.Value!,
            localizer[LocalizationKeys.JournalLineStatementExportSuccess],
            AppHttpStatusCodes.Ok);
    }

    //private async Task<Result<bool>> ValidateModelForAdmin(JournalLinesStatementInputModel model)
    //{
    //    if (model.PartnerId == null) 
    //    {
    //        return Result<bool>.Failure(localizer[LocalizationKeys.Required, "PartnerId"], AppHttpStatusCodes.BadRequest);
    //    }
    //    if (model.CurrencyId == null)
    //    {
    //        return Result<bool>.Failure(localizer[LocalizationKeys.Required, "CurrencyId"], AppHttpStatusCodes.BadRequest);
    //    }
    //    if (model.AccountTypeId == null)
    //    {
    //        return Result<bool>.Failure(localizer[LocalizationKeys.Required, "AccountTypeId"], AppHttpStatusCodes.BadRequest);
    //    }
    //    return Result<bool>.Success(true);
    //}

    private JournalLinesQueryInputModel NormalizeQuery(JournalLinesQueryInputModel model)
    {
        var mapped = mapper.Map<JournalLinesQueryInputModel>(model);

        mapped.PageNumber = mapped.PageNumber <= 0 ? 1 : mapped.PageNumber;
        mapped.PageSize = mapped.PageSize <= 0 ? 20 : Math.Min(mapped.PageSize, 100);
        mapped.SortBy = string.IsNullOrWhiteSpace(mapped.SortBy) ? null : mapped.SortBy.Trim();

        return mapped;
    }
    private JournalLinesStatementInputModel NormalizeQuery(JournalLinesStatementInputModel model)
    {
        var mapped = mapper.Map<JournalLinesStatementInputModel>(model);

        mapped.PageNumber = mapped.PageNumber <= 0 ? 1 : mapped.PageNumber;
        mapped.PageSize = mapped.PageSize <= 0 ? 20 : Math.Min(mapped.PageSize, 100);
        mapped.SortBy = string.IsNullOrWhiteSpace(mapped.SortBy) ? null : mapped.SortBy.Trim();
        if(!model.ToDate.HasValue)
            model.ToDate = DateTime.UtcNow;
        return mapped;
    }

    private async Task EnrichJournalLinePlatformInvoicesAsync(
        JournalLineOutputModel[] items,
        CancellationToken cancellationToken)
    {
        var invs = items
            .Select(i => i.PlatformInvoice)
            .OfType<PlatformInvoiceOutputModel>()
            .ToList();
        if (invs.Count == 0)
        {
            return;
        }

        var partnerIds = new HashSet<long>();
        var customerPartnerIds = new HashSet<long>();
        foreach (var m in invs)
        {
            var args = PlatformInvoiceContextResolver.TryParseParameterStrings(m.DescriptionParametersJson);
            if (args is null || !PlatformInvoiceContextResolver.TryGetPartyIds(
                    m.DescriptionTemplateKey, args, out var cp, out var sp, out _))
            {
                continue;
            }

            partnerIds.Add(cp);
            if (sp is { } sid)
            {
                partnerIds.Add(sid);
            }

            customerPartnerIds.Add(cp);
        }

        IReadOnlyDictionary<long, PartnerEntity> partnersById =
            partnerIds.Count == 0
                ? new Dictionary<long, PartnerEntity>()
                : (await unitOfWork.PartnerRepository.GetByIdsAsync(partnerIds.ToList(), cancellationToken))
                .ToDictionary(p => p.Id);

        var customersByPartnerId = new Dictionary<long, CustomerEntity?>();
        foreach (var pid in customerPartnerIds)
        {
            customersByPartnerId[pid] =
                await unitOfWork.CustomerRepository.GetByPartnerIdAsync(pid, cancellationToken);
        }

        foreach (var m in invs)
        {
            PlatformInvoiceContextResolver.EnrichPlatformInvoiceOutput(m, localizer, partnersById, customersByPartnerId);
        }
    }
}
