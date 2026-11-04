using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application;
using SNEL.WalletPlatform.Application.Caching;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Application.Lookups.Interfaces;
using SNEL.WalletPlatform.Application.Lookups.Models.Output;
using SNEL.WalletPlatform.Application.Resources;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Partners;
using SNEL.WalletPlatform.Domain.Payments;
using System.Globalization;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class LookupRepository(
    ApplicationDbContext context,    
    ICurrentUserContext currentUserContext) : ILookupRepository
{
    const string EN = "en";
    const string FR = "fr";
    public async Task<AllLookupsOutputModel> GetAllAsync_old(IStringLocalizer<SharedResource> localizer)
    {
        var langCode = (string.IsNullOrWhiteSpace(currentUserContext.LanguageCode)
            ? "en"
            : currentUserContext.LanguageCode).ToLowerInvariant();
        
        var countryTranslations = await context.CountryTranslations.AsNoTracking().ToDictionaryAsync(x => $"{x.EntityId}-{x.LanguageCode}", x => x.Name);
        var countries = await context.Countries.Select(x =>
            new LookupItemOutputModel(
                x.Id,
                x.Code,
                countryTranslations.ContainsKey($"{x.Id}-{EN}") ? countryTranslations[$"{x.Id}-{EN}"] : x.Name,
                countryTranslations.ContainsKey($"{x.Id}-{FR}") ? countryTranslations[$"{x.Id}-{FR}"] : x.Name)).ToListAsync();

        var regionTranslations = await context.RegionTranslations.AsNoTracking().ToDictionaryAsync(x => $"{x.EntityId}-{x.LanguageCode}", x => x.Name);
        var regions = await context.Regions.Select(x =>
            new RegionLookupOutputModel(
                x.Id,
                x.CountryId,
                x.Code,                
                regionTranslations.ContainsKey($"{x.Id}-{EN}") ? regionTranslations[$"{x.Id}-{EN}"] : x.Name,
                regionTranslations.ContainsKey($"{x.Id}-{FR}") ? regionTranslations[$"{x.Id}-{FR}"] : x.Name)).ToListAsync();
        
        var cityTranslations = await context.CityTranslations.AsNoTracking().ToDictionaryAsync(x => $"{x.EntityId}-{x.LanguageCode}", x => x.Name);
        var cities = await context.Cities.Select(x =>
            new CityLookupOutputModel(
                x.Id,
                x.RegionId,
                x.Code,
                cityTranslations.ContainsKey($"{x.Id}-{EN}") ? cityTranslations[$"{x.Id}-{EN}"] : x.Name,
                cityTranslations.ContainsKey($"{x.Id}-{FR}") ? cityTranslations[$"{x.Id}-{FR}"] : x.Name)).ToListAsync();

        var areaTranslations = await context.AreaTranslations.AsNoTracking().ToDictionaryAsync(x => $"{x.EntityId}-{x.LanguageCode}", x => x.Name);
        var areas = await context.Areas.Select(x =>
            new AreaLookupOutputModel(
                x.Id,
                x.CityId,
                x.Code,
                areaTranslations.ContainsKey($"{x.Id}-{EN}") ? areaTranslations[$"{x.Id}-{EN}"] : x.Name,
                areaTranslations.ContainsKey($"{x.Id}-{FR}") ? areaTranslations[$"{x.Id}-{FR}"] : x.Name)).ToListAsync();

        var genderTranslations = await context.GenderTranslations.AsNoTracking().ToDictionaryAsync(x => $"{x.EntityId}-{x.LanguageCode}", x => x.Name);
        var genders = await context.Genders.Select(x =>
            new LookupWithActiveOutputModel(
                x.Id,                
                x.Code,
                genderTranslations.ContainsKey($"{x.Id}-{EN}") ? genderTranslations[$"{x.Id}-{EN}"] : x.Name,
                genderTranslations.ContainsKey($"{x.Id}-{FR}") ? genderTranslations[$"{x.Id}-{FR}"] : x.Name,
                x.Active)).ToListAsync();
        var channelTranslations = await context.ChannelTranslations.AsNoTracking().ToDictionaryAsync(x => $"{x.EntityId}-{x.LanguageCode}", x => x.Name);
        var channels = await context.Channels.Select(x =>
            new LookupWithActiveOutputModel(
                x.Id,
                x.Code,
                channelTranslations.ContainsKey($"{x.Id}-{EN}") ? channelTranslations[$"{x.Id}-{EN}"] : x.Name,
                channelTranslations.ContainsKey($"{x.Id}-{FR}") ? channelTranslations[$"{x.Id}-{FR}"] : x.Name,
                x.Active)).ToListAsync();

        var currencyTranslations = await context.CurrencyTranslations.AsNoTracking().ToDictionaryAsync(x => $"{x.EntityId}-{x.LanguageCode}", x => x.Name);
        var currencies = await context.Currencies.Select(x =>
            new CurrencyLookupOutputModel(
                x.Id,
                x.Code,
                int.Parse(x.Code),
                x.Minor,
                currencyTranslations.ContainsKey($"{x.Id}-{EN}") ? currencyTranslations[$"{x.Id}-{EN}"] : x.Name,
                currencyTranslations.ContainsKey($"{x.Id}-{FR}") ? currencyTranslations[$"{x.Id}-{FR}"] : x.Name,
                x.Active)).ToListAsync();

        var partnerTypeTranslations = await context.PartnerTypeTranslations.AsNoTracking().ToDictionaryAsync(x => $"{x.EntityId}-{x.LanguageCode}", x => x.Name);
        var partnerTypes = await context.PartnerTypes.Select(x =>
            new LookupWithActiveOutputModel(
                x.Id,
                x.Code,
                partnerTypeTranslations.ContainsKey($"{x.Id}-{EN}") ? partnerTypeTranslations[$"{x.Id}-{EN}"] : x.Name,
                partnerTypeTranslations.ContainsKey($"{x.Id}-{FR}") ? partnerTypeTranslations[$"{x.Id}-{FR}"] : x.Name,
                x.Active)).ToListAsync();
        var billingCategoryTranslations = await context.BillingCategoryTranslations.AsNoTracking().ToDictionaryAsync(x => $"{x.EntityId}-{x.LanguageCode}", x => x.Name);
        var billingCategories = await context.BillingCategories.Select(x =>
            new LookupWithActiveOutputModel(
                x.Id,
                x.Code,
                billingCategoryTranslations.ContainsKey($"{x.Id}-{EN}") ? billingCategoryTranslations[$"{x.Id}-{EN}"] : x.Name,
                billingCategoryTranslations.ContainsKey($"{x.Id}-{FR}") ? billingCategoryTranslations[$"{x.Id}-{FR}"] : x.Name,
                x.Active)).ToListAsync();
        var billingServiceTypeTranslations = await context.BillingServiceTypeTranslations.AsNoTracking().ToDictionaryAsync(x => $"{x.EntityId}-{x.LanguageCode}", x => x.Name);
        var billingServiceTypes = await context.BillingServiceTypes.Select(x =>
            new LookupWithActiveOutputModel(
                x.Id,
                x.Code,
                billingServiceTypeTranslations.ContainsKey($"{x.Id}-{EN}") ? billingServiceTypeTranslations[$"{x.Id}-{EN}"] : x.Name,
                billingServiceTypeTranslations.ContainsKey($"{x.Id}-{FR}") ? billingServiceTypeTranslations[$"{x.Id}-{FR}"] : x.Name,
                x.Active)).ToListAsync();
        var contactTypeTranslations = await context.ContactTypeTranslations.AsNoTracking().ToDictionaryAsync(x => $"{x.EntityId}-{x.LanguageCode}", x => x.Name);
        var contactTypes = await context.ContactTypes.Select(x =>
            new LookupWithActiveOutputModel(
                x.Id,
                x.Code,
                contactTypeTranslations.ContainsKey($"{x.Id}-{EN}") ? contactTypeTranslations[$"{x.Id}-{EN}"] : x.Name,
                contactTypeTranslations.ContainsKey($"{x.Id}-{FR}") ? contactTypeTranslations[$"{x.Id}-{FR}"] : x.Name,
                x.Active)).ToListAsync();
        var paymentCategoryTranslations = await context.PaymentCategoryTranslations.AsNoTracking().ToDictionaryAsync(x => $"{x.EntityId}-{x.LanguageCode}", x => x.Name);
        var paymentCategories = await context.PaymentCategories.Select(x =>
            new LookupWithActiveOutputModel(
                x.Id,
                x.Code,
                paymentCategoryTranslations.ContainsKey($"{x.Id}-{EN}") ? paymentCategoryTranslations[$"{x.Id}-{EN}"] : x.Name,
                paymentCategoryTranslations.ContainsKey($"{x.Id}-{FR}") ? paymentCategoryTranslations[$"{x.Id}-{FR}"] : x.Name,
                x.Active)).ToListAsync();
        var centerTranslations = await context.CenterTranslations.AsNoTracking().ToDictionaryAsync(x => $"{x.EntityId}-{x.LanguageCode}", x => x.Name);
        var centers = await context.Centers.Select(x =>
            new LookupWithActiveOutputModel(
                x.Id,
                x.Code,
                centerTranslations.ContainsKey($"{x.Id}-{EN}") ? centerTranslations[$"{x.Id}-{EN}"] : x.Name,
                centerTranslations.ContainsKey($"{x.Id}-{FR}") ? centerTranslations[$"{x.Id}-{FR}"] : x.Name,
                x.Active)).ToListAsync();
        var completionModeTranslations = await context.CompletionModeTranslations.AsNoTracking().ToDictionaryAsync(x => $"{x.EntityId}-{x.LanguageCode}", x => x.Name);
        var completionModes = await context.CompletionModes.Select(x =>
            new LookupWithActiveOutputModel(
                x.Id,
                x.Code,
                completionModeTranslations.ContainsKey($"{x.Id}-{EN}") ? completionModeTranslations[$"{x.Id}-{EN}"] : x.Name,
                completionModeTranslations.ContainsKey($"{x.Id}-{FR}") ? completionModeTranslations[$"{x.Id}-{FR}"] : x.Name,
                x.Active)).ToListAsync();
        var confirmationModeTranslations = await context.ConfirmationModeTranslations.AsNoTracking().ToDictionaryAsync(x => $"{x.EntityId}-{x.LanguageCode}", x => x.Name);
        var confirmationModes = await context.ConfirmationModes.Select(x =>
            new LookupWithActiveOutputModel(
                x.Id,
                x.Code,
                confirmationModeTranslations.ContainsKey($"{x.Id}-{EN}") ? confirmationModeTranslations[$"{x.Id}-{EN}"] : x.Name,
                confirmationModeTranslations.ContainsKey($"{x.Id}-{FR}") ? confirmationModeTranslations[$"{x.Id}-{FR}"] : x.Name,
                x.Active)).ToListAsync();
        var confirmationTypeTranslations = await context.ConfirmationTypeTranslations.AsNoTracking().ToDictionaryAsync(x => $"{x.EntityId}-{x.LanguageCode}", x => x.Name);
        var confirmationTypes = await context.ConfirmationTypes.Select(x =>
            new LookupWithActiveOutputModel(
                x.Id,
                x.Code,
                confirmationTypeTranslations.ContainsKey($"{x.Id}-{EN}") ? confirmationTypeTranslations[$"{x.Id}-{EN}"] : x.Name,
                confirmationTypeTranslations.ContainsKey($"{x.Id}-{FR}") ? confirmationTypeTranslations[$"{x.Id}-{FR}"] : x.Name,
                x.Active)).ToListAsync();
        var paymentInitModeTranslations = await context.PaymentInitModeTranslations.AsNoTracking().ToDictionaryAsync(x => $"{x.EntityId}-{x.LanguageCode}", x => x.Name);
        var paymentInitModes = await context.PaymentInitModes.Select(x =>
            new LookupWithActiveOutputModel(
                x.Id,
                x.Code,
                paymentInitModeTranslations.ContainsKey($"{x.Id}-{EN}") ? paymentInitModeTranslations[$"{x.Id}-{EN}"] : x.Name,
                paymentInitModeTranslations.ContainsKey($"{x.Id}-{FR}") ? paymentInitModeTranslations[$"{x.Id}-{FR}"] : x.Name,
                x.Active)).ToListAsync();
        var paymentMethodPurposeTranslations = await context.PaymentMethodPurposeTranslations.AsNoTracking().ToDictionaryAsync(x => $"{x.EntityId}-{x.LanguageCode}", x => x.Name);
        var paymentMethodPurposes = await context.PaymentMethodPurposes.Select(x =>
            new LookupWithActiveOutputModel(
                x.Id,
                x.Code,
                paymentMethodPurposeTranslations.ContainsKey($"{x.Id}-{EN}") ? paymentMethodPurposeTranslations[$"{x.Id}-{EN}"] : x.Name,
                paymentMethodPurposeTranslations.ContainsKey($"{x.Id}-{FR}") ? paymentMethodPurposeTranslations[$"{x.Id}-{FR}"] : x.Name,
                x.Active)).ToListAsync();
        var journalLineTypeTranslations = await context.JournalLineTypeTranslations.AsNoTracking().ToDictionaryAsync(x => $"{x.EntityId}-{x.LanguageCode}", x => x.Name);
        var journalLineTypes = await context.JournalLineTypes.Select(x =>
            new LookupWithActiveOutputModel(
                x.Id,
                x.Code,
                journalLineTypeTranslations.ContainsKey($"{x.Id}-{EN}") ? journalLineTypeTranslations[$"{x.Id}-{EN}"] : x.Name,
                journalLineTypeTranslations.ContainsKey($"{x.Id}-{FR}") ? journalLineTypeTranslations[$"{x.Id}-{FR}"] : x.Name,
                x.Active)).ToListAsync();
        var accountTypeTranslations = await context.AccountTypeTranslations.AsNoTracking().ToDictionaryAsync(x => $"{x.EntityId}-{x.LanguageCode}", x => x.Name);
        var accountTypes = await context.AccountTypes.Select(x =>
            new LookupWithActiveOutputModel(
                x.Id,
                x.Code,
                accountTypeTranslations.ContainsKey($"{x.Id}-{EN}") ? accountTypeTranslations[$"{x.Id}-{EN}"] : x.Name,
                accountTypeTranslations.ContainsKey($"{x.Id}-{FR}") ? accountTypeTranslations[$"{x.Id}-{FR}"] : x.Name,
                x.Active)).ToListAsync();
        var paymentTypeTranslations = await context.PaymentTypeTranslations.AsNoTracking().ToDictionaryAsync(x => $"{x.EntityId}-{x.LanguageCode}", x => x.Name);
        var paymentTypes = await context.PaymentTypes.Select(x =>
            new LookupWithActiveOutputModel(
                x.Id,
                x.Code,
                paymentTypeTranslations.ContainsKey($"{x.Id}-{EN}") ? paymentTypeTranslations[$"{x.Id}-{EN}"] : x.Name,
                paymentTypeTranslations.ContainsKey($"{x.Id}-{FR}") ? paymentTypeTranslations[$"{x.Id}-{FR}"] : x.Name,
                x.Active)).ToListAsync();
        var paymentStatuses = Enum
            .GetValues<PaymentStatus>()
            .Select(status => new LookupWithActiveOutputModel(
                (int)status,
                status.ToString(),
                status.ToString(),
                localizer.GetStringForCulture(new CultureInfo("fr"), $"Payment.Status.{status.ToString()}"),
                true))
            .ToList();          
        
        return new AllLookupsOutputModel(
            countries,
            regions,
            cities,
            areas,
            genders,
            channels,
            currencies,
            partnerTypes,
            [],
            [],
            billingCategories,
            billingServiceTypes,
            contactTypes,
            paymentCategories,
            centers,
            completionModes,
            confirmationModes,
            confirmationTypes,
            paymentInitModes,
            paymentMethodPurposes,
            journalLineTypes,
            accountTypes,
            paymentTypes,
            paymentStatuses,
            []);
    }

    public async Task<AllLookupsOutputModel> GetAllAsync(IStringLocalizer<SharedResource> localizer)
    {
        var langCode = (string.IsNullOrWhiteSpace(currentUserContext.LanguageCode)
            ? "en"
            : currentUserContext.LanguageCode).ToLowerInvariant();
        var countries = CachingFactory.GetValue<CountryEntity>()?
                            .Select(x => new LookupItemOutputModel(
                                x.Id,
                                x.Code,
                                x.Translations.FirstOrDefault(t => t.LanguageCode == EN)?.Name ?? x.Name,
                                x.Translations.FirstOrDefault(t => t.LanguageCode == FR)?.Name ?? x.Name)).ToList();                

        var regions = CachingFactory.GetValue<RegionEntity>()?
                            .Select(x => new RegionLookupOutputModel(
                                x.Id,
                                x.CountryId,
                                x.Code,
                                x.Translations.FirstOrDefault(t => t.LanguageCode == EN)?.Name ?? x.Name,
                                x.Translations.FirstOrDefault(t => t.LanguageCode == FR)?.Name ?? x.Name)).ToList();        

        var cities = CachingFactory.GetValue<CityEntity>()?
                            .Select(x => new CityLookupOutputModel(
                                x.Id,
                                x.RegionId,
                                x.Code,
                                x.Translations.FirstOrDefault(t => t.LanguageCode == EN)?.Name ?? x.Name,
                                x.Translations.FirstOrDefault(t => t.LanguageCode == FR)?.Name ?? x.Name)).ToList();        

        var areas = CachingFactory.GetValue<AreaEntity>()?
                            .Select(x => new AreaLookupOutputModel(
                                x.Id,
                                x.CityId,
                                x.Code,
                                x.Translations.FirstOrDefault(t => t.LanguageCode == EN)?.Name ?? x.Name,
                                x.Translations.FirstOrDefault(t => t.LanguageCode == FR)?.Name ?? x.Name)).ToList();
        
        var genders = CachingFactory.GetValue<GenderEntity>()?
                            .Select(x => new LookupWithActiveOutputModel(
                                x.Id,
                                x.Code,
                                x.Translations.FirstOrDefault(t => t.LanguageCode == EN)?.Name ?? x.Name,
                                x.Translations.FirstOrDefault(t => t.LanguageCode == FR)?.Name ?? x.Name,
                                x.Active)).ToList();
       
        var channels = CachingFactory.GetValue<ChannelEntity>()?
                           .Select(x => new LookupWithActiveOutputModel(
                               x.Id,
                               x.Code,
                               x.Translations.FirstOrDefault(t => t.LanguageCode == EN)?.Name ?? x.Name,
                               x.Translations.FirstOrDefault(t => t.LanguageCode == FR)?.Name ?? x.Name,
                               x.Active)).ToList();
        
        var currencies = CachingFactory.GetValue<CurrencyEntity>()?
                          .Select(x => new CurrencyLookupOutputModel(
                                    x.Id,
                                    x.Code,
                                    int.Parse(x.Code),
                                    x.Minor,
                                    x.Translations.FirstOrDefault(t => t.LanguageCode == EN)?.Name ?? x.Name,
                                    x.Translations.FirstOrDefault(t => t.LanguageCode == FR)?.Name ?? x.Name,
                                    x.Active)).ToList();        

        var partnerTypes = CachingFactory.GetValue<PartnerTypeEntity>()?
                         .Select(x => new LookupWithActiveOutputModel(
                                   x.Id,
                                   x.Code,
                                   x.Translations.FirstOrDefault(t => t.LanguageCode == EN)?.Name ?? x.Name,
                                   x.Translations.FirstOrDefault(t => t.LanguageCode == FR)?.Name ?? x.Name,
                                   x.Active)).ToList();
        
        var billingCategories = CachingFactory.GetValue<BillingCategoryEntity>()?
                         .Select(x => new LookupWithActiveOutputModel(
                                   x.Id,
                                   x.Code,
                                   x.Translations.FirstOrDefault(t => t.LanguageCode == EN)?.Name ?? x.Name,
                                   x.Translations.FirstOrDefault(t => t.LanguageCode == FR)?.Name ?? x.Name,
                                   x.Active)).ToList();
        
        var billingServiceTypes = CachingFactory.GetValue<BillingServiceTypeEntity>()?
                         .Select(x => new LookupWithActiveOutputModel(
                                   x.Id,
                                   x.Code,
                                   x.Translations.FirstOrDefault(t => t.LanguageCode == EN)?.Name ?? x.Name,
                                   x.Translations.FirstOrDefault(t => t.LanguageCode == FR)?.Name ?? x.Name,
                                   x.Active)).ToList();
       
        var contactTypes = CachingFactory.GetValue<ContactTypeEntity>()?
                        .Select(x => new LookupWithActiveOutputModel(
                                  x.Id,
                                  x.Code,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == EN)?.Name ?? x.Name,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == FR)?.Name ?? x.Name,
                                  x.Active)).ToList();
      
        var paymentCategories = CachingFactory.GetValue<PaymentCategoryEntity>()?
                        .Select(x => new LookupWithActiveOutputModel(
                                  x.Id,
                                  x.Code,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == EN)?.Name ?? x.Name,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == FR)?.Name ?? x.Name,
                                  x.Active)).ToList();
        
        var centers = CachingFactory.GetValue<CenterEntity>()?
                        .Select(x => new LookupWithActiveOutputModel(
                                  x.Id,
                                  x.Code,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == EN)?.Name ?? x.Name,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == FR)?.Name ?? x.Name,
                                  x.Active)).ToList();
        
        var completionModes = CachingFactory.GetValue<CompletionModeEntity>()?
                        .Select(x => new LookupWithActiveOutputModel(
                                  x.Id,
                                  x.Code,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == EN)?.Name ?? x.Name,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == FR)?.Name ?? x.Name,
                                  x.Active)).ToList();
       
        var confirmationModes = CachingFactory.GetValue<ConfirmationModeEntity>()?
                        .Select(x => new LookupWithActiveOutputModel(
                                  x.Id,
                                  x.Code,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == EN)?.Name ?? x.Name,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == FR)?.Name ?? x.Name,
                                  x.Active)).ToList();
        
        var confirmationTypes = CachingFactory.GetValue<ConfirmationTypeEntity>()?
                        .Select(x => new LookupWithActiveOutputModel(
                                  x.Id,
                                  x.Code,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == EN)?.Name ?? x.Name,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == FR)?.Name ?? x.Name,
                                  x.Active)).ToList();
        
        var paymentInitModes = CachingFactory.GetValue<PaymentInitModeEntity>()?
                        .Select(x => new LookupWithActiveOutputModel(
                                  x.Id,
                                  x.Code,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == EN)?.Name ?? x.Name,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == FR)?.Name ?? x.Name,
                                  x.Active)).ToList();
        
        var paymentMethodPurposes = CachingFactory.GetValue<PaymentMethodPurposeEntity>()?
                        .Select(x => new LookupWithActiveOutputModel(
                                  x.Id,
                                  x.Code,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == EN)?.Name ?? x.Name,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == FR)?.Name ?? x.Name,
                                  x.Active)).ToList();
        
        var journalLineTypes = CachingFactory.GetValue<JournalLineTypeEntity>()?
                        .Select(x => new LookupWithActiveOutputModel(
                                  x.Id,
                                  x.Code,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == EN)?.Name ?? x.Name,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == FR)?.Name ?? x.Name,
                                  x.Active)).ToList();
        
        var accountTypes = CachingFactory.GetValue<AccountTypeEntity>()?
                        .Select(x => new LookupWithActiveOutputModel(
                                  x.Id,
                                  x.Code,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == EN)?.Name ?? x.Name,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == FR)?.Name ?? x.Name,
                                  x.Active)).ToList();
        
        var paymentTypes = CachingFactory.GetValue<PaymentTypeEntity>()?
                        .Select(x => new LookupWithActiveOutputModel(
                                  x.Id,
                                  x.Code,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == EN)?.Name ?? x.Name,
                                  x.Translations.FirstOrDefault(t => t.LanguageCode == FR)?.Name ?? x.Name,
                                  x.Active)).ToList();

        var paymentStatuses = Enum
            .GetValues<PaymentStatus>()
            .Select(status => new LookupWithActiveOutputModel(
                (int)status,
                status.ToString(),
                status.ToString(),
                localizer.GetStringForCulture(new CultureInfo("fr"), $"Payment.Status.{status.ToString()}"),
                true))
            .ToList();

        return new AllLookupsOutputModel(
            countries,
            regions,
            cities,
            areas,
            genders,
            channels,
            currencies,
            partnerTypes,
            [],
            [],
            billingCategories,
            billingServiceTypes,
            contactTypes,
            paymentCategories,
            centers,
            completionModes,
            confirmationModes,
            confirmationTypes,
            paymentInitModes,
            paymentMethodPurposes,
            journalLineTypes,
            accountTypes,
            paymentTypes,
            paymentStatuses,
            []);
    }

    public Task<List<CurrencyEntity>> GetAllCurrenciesAsync()
    {
        return context.Currencies.ToListAsync();
    }
}
