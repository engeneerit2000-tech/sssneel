using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Lookups.Enums;
using SNEL.WalletPlatform.Application.Lookups.Interfaces;
using SNEL.WalletPlatform.Application.Lookups.Models.Output;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Partners;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic repository backing the lookup-translation API. Dispatches on
/// <see cref="LookupType"/> to the appropriate pair of (parent lookup, translation)
/// <see cref="DbSet{TEntity}"/>s. Every supported translation table derives from
/// <see cref="TranslationEntity{TEntityKey}"/> with an <c>int</c> foreign key, so
/// the insert/update/list logic is shared via a single generic helper.
/// </summary>
public sealed class LookupTranslationRepository(ApplicationDbContext context) : ILookupTranslationRepository
{
    public Task<bool> LookupExistsAsync(LookupType type, int lookupId, CancellationToken cancellationToken = default)
    {
        return type switch
        {
            LookupType.Country => context.Countries.AnyAsync(x => x.Id == lookupId, cancellationToken),
            LookupType.Region => context.Regions.AnyAsync(x => x.Id == lookupId, cancellationToken),
            LookupType.City => context.Cities.AnyAsync(x => x.Id == lookupId, cancellationToken),
            LookupType.Area => context.Areas.AnyAsync(x => x.Id == lookupId, cancellationToken),
            LookupType.Center => context.Centers.AnyAsync(x => x.Id == lookupId, cancellationToken),
            LookupType.Channel => context.Channels.AnyAsync(x => x.Id == lookupId, cancellationToken),
            LookupType.Currency => context.Currencies.AnyAsync(x => x.Id == lookupId, cancellationToken),
            LookupType.Gender => context.Genders.AnyAsync(x => x.Id == lookupId, cancellationToken),
            LookupType.PartnerType => context.PartnerTypes.AnyAsync(x => x.Id == lookupId, cancellationToken),
            LookupType.ContactType => context.ContactTypes.AnyAsync(x => x.Id == lookupId, cancellationToken),
            LookupType.BillingServiceType => context.BillingServiceTypes.AnyAsync(x => x.Id == lookupId, cancellationToken),
            LookupType.AccountType => context.AccountTypes.AnyAsync(x => x.Id == lookupId, cancellationToken),
            LookupType.JournalType => context.JournalTypes.AnyAsync(x => x.Id == lookupId, cancellationToken),
            LookupType.JournalLineType => context.JournalLineTypes.AnyAsync(x => x.Id == lookupId, cancellationToken),
            LookupType.BillingCategory => context.BillingCategories.AnyAsync(x => x.Id == lookupId, cancellationToken),
            LookupType.FavoriteType => context.FavoriteTypes.AnyAsync(x => x.Id == lookupId, cancellationToken),
            LookupType.PaymentCategory => context.PaymentCategories.AnyAsync(x => x.Id == lookupId, cancellationToken),
            LookupType.CompletionMode => context.CompletionModes.AnyAsync(x => x.Id == lookupId, cancellationToken),
            LookupType.ConfirmationMode => context.ConfirmationModes.AnyAsync(x => x.Id == lookupId, cancellationToken),
            LookupType.ConfirmationType => context.ConfirmationTypes.AnyAsync(x => x.Id == lookupId, cancellationToken),
            LookupType.PaymentInitMode => context.PaymentInitModes.AnyAsync(x => x.Id == lookupId, cancellationToken),
            LookupType.PaymentMethodPurpose => context.PaymentMethodPurposes.AnyAsync(x => x.Id == lookupId, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported lookup type."),
        };
    }

    public Task<IReadOnlyList<LookupTranslationOutputModel>> GetAsync(
        LookupType type,
        int lookupId,
        CancellationToken cancellationToken = default)
    {
        return type switch
        {
            LookupType.Country => GetGenericAsync(context.CountryTranslations, lookupId, cancellationToken),
            LookupType.Region => GetGenericAsync(context.RegionTranslations, lookupId, cancellationToken),
            LookupType.City => GetGenericAsync(context.CityTranslations, lookupId, cancellationToken),
            LookupType.Area => GetGenericAsync(context.AreaTranslations, lookupId, cancellationToken),
            LookupType.Center => GetGenericAsync(context.CenterTranslations, lookupId, cancellationToken),
            LookupType.Channel => GetGenericAsync(context.ChannelTranslations, lookupId, cancellationToken),
            LookupType.Currency => GetGenericAsync(context.CurrencyTranslations, lookupId, cancellationToken),
            LookupType.Gender => GetGenericAsync(context.GenderTranslations, lookupId, cancellationToken),
            LookupType.PartnerType => GetGenericAsync(context.PartnerTypeTranslations, lookupId, cancellationToken),
            LookupType.ContactType => GetGenericAsync(context.ContactTypeTranslations, lookupId, cancellationToken),
            LookupType.BillingServiceType => GetGenericAsync(context.BillingServiceTypeTranslations, lookupId, cancellationToken),
            LookupType.AccountType => GetGenericAsync(context.AccountTypeTranslations, lookupId, cancellationToken),
            LookupType.JournalType => GetGenericAsync(context.JournalTypeTranslations, lookupId, cancellationToken),
            LookupType.JournalLineType => GetGenericAsync(context.JournalLineTypeTranslations, lookupId, cancellationToken),
            LookupType.BillingCategory => GetGenericAsync(context.BillingCategoryTranslations, lookupId, cancellationToken),
            LookupType.FavoriteType => GetGenericAsync(context.FavoriteTypeTranslations, lookupId, cancellationToken),
            LookupType.PaymentCategory => GetGenericAsync(context.PaymentCategoryTranslations, lookupId, cancellationToken),
            LookupType.CompletionMode => GetGenericAsync(context.CompletionModeTranslations, lookupId, cancellationToken),
            LookupType.ConfirmationMode => GetGenericAsync(context.ConfirmationModeTranslations, lookupId, cancellationToken),
            LookupType.ConfirmationType => GetGenericAsync(context.ConfirmationTypeTranslations, lookupId, cancellationToken),
            LookupType.PaymentInitMode => GetGenericAsync(context.PaymentInitModeTranslations, lookupId, cancellationToken),
            LookupType.PaymentMethodPurpose => GetGenericAsync(context.PaymentMethodPurposeTranslations, lookupId, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported lookup type."),
        };
    }

    public Task<(LookupTranslationOutputModel Translation, bool WasAdded)> UpsertAsync(
        LookupType type,
        int lookupId,
        string languageCode,
        string name,
        CancellationToken cancellationToken = default)
    {
        return type switch
        {
            LookupType.Country => UpsertGenericAsync<CountryTranslationEntity>(context.CountryTranslations, lookupId, languageCode, name, cancellationToken),
            LookupType.Region => UpsertGenericAsync<RegionTranslationEntity>(context.RegionTranslations, lookupId, languageCode, name, cancellationToken),
            LookupType.City => UpsertGenericAsync<CityTranslationEntity>(context.CityTranslations, lookupId, languageCode, name, cancellationToken),
            LookupType.Area => UpsertGenericAsync<AreaTranslationEntity>(context.AreaTranslations, lookupId, languageCode, name, cancellationToken),
            LookupType.Center => UpsertGenericAsync<CenterTranslationEntity>(context.CenterTranslations, lookupId, languageCode, name, cancellationToken),
            LookupType.Channel => UpsertGenericAsync<ChannelTranslationEntity>(context.ChannelTranslations, lookupId, languageCode, name, cancellationToken),
            LookupType.Currency => UpsertGenericAsync<CurrencyTranslationEntity>(context.CurrencyTranslations, lookupId, languageCode, name, cancellationToken),
            LookupType.Gender => UpsertGenericAsync<GenderTranslationEntity>(context.GenderTranslations, lookupId, languageCode, name, cancellationToken),
            LookupType.PartnerType => UpsertGenericAsync<PartnerTypeTranslationEntity>(context.PartnerTypeTranslations, lookupId, languageCode, name, cancellationToken),
            LookupType.ContactType => UpsertGenericAsync<ContactTypeTranslationEntity>(context.ContactTypeTranslations, lookupId, languageCode, name, cancellationToken),
            LookupType.BillingServiceType => UpsertGenericAsync<BillingServiceTypeTranslationEntity>(context.BillingServiceTypeTranslations, lookupId, languageCode, name, cancellationToken),
            LookupType.AccountType => UpsertGenericAsync<AccountTypeTranslationEntity>(context.AccountTypeTranslations, lookupId, languageCode, name, cancellationToken),
            LookupType.JournalType => UpsertGenericAsync<JournalTypeTranslationEntity>(context.JournalTypeTranslations, lookupId, languageCode, name, cancellationToken),
            LookupType.JournalLineType => UpsertGenericAsync<JournalLineTypeTranslationEntity>(context.JournalLineTypeTranslations, lookupId, languageCode, name, cancellationToken),
            LookupType.BillingCategory => UpsertGenericAsync<BillingCategoryTranslationEntity>(context.BillingCategoryTranslations, lookupId, languageCode, name, cancellationToken),
            LookupType.FavoriteType => UpsertGenericAsync<FavoriteTypeTranslationEntity>(context.FavoriteTypeTranslations, lookupId, languageCode, name, cancellationToken),
            LookupType.PaymentCategory => UpsertGenericAsync<PaymentCategoryTranslationEntity>(context.PaymentCategoryTranslations, lookupId, languageCode, name, cancellationToken),
            LookupType.CompletionMode => UpsertGenericAsync<CompletionModeTranslationEntity>(context.CompletionModeTranslations, lookupId, languageCode, name, cancellationToken),
            LookupType.ConfirmationMode => UpsertGenericAsync<ConfirmationModeTranslationEntity>(context.ConfirmationModeTranslations, lookupId, languageCode, name, cancellationToken),
            LookupType.ConfirmationType => UpsertGenericAsync<ConfirmationTypeTranslationEntity>(context.ConfirmationTypeTranslations, lookupId, languageCode, name, cancellationToken),
            LookupType.PaymentInitMode => UpsertGenericAsync<PaymentInitModeTranslationEntity>(context.PaymentInitModeTranslations, lookupId, languageCode, name, cancellationToken),
            LookupType.PaymentMethodPurpose => UpsertGenericAsync<PaymentMethodPurposeTranslationEntity>(context.PaymentMethodPurposeTranslations, lookupId, languageCode, name, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported lookup type."),
        };
    }

    private static async Task<IReadOnlyList<LookupTranslationOutputModel>> GetGenericAsync<TTrans>(
        DbSet<TTrans> set,
        int lookupId,
        CancellationToken cancellationToken)
        where TTrans : TranslationEntity<int>
    {
        var rows = await set
            .AsNoTracking()
            .Where(x => x.EntityId == lookupId)
            .OrderBy(x => x.LanguageCode)
            .Select(x => new LookupTranslationOutputModel(x.Id, x.EntityId, x.LanguageCode, x.Name))
            .ToListAsync(cancellationToken);

        return rows;
    }

    private async Task<(LookupTranslationOutputModel Translation, bool WasAdded)> UpsertGenericAsync<TTrans>(
        DbSet<TTrans> set,
        int lookupId,
        string languageCode,
        string name,
        CancellationToken cancellationToken)
        where TTrans : TranslationEntity<int>, new()
    {
        var existing = await set.FirstOrDefaultAsync(
            x => x.EntityId == lookupId && x.LanguageCode == languageCode,
            cancellationToken);

        if (existing is not null)
        {
            existing.Name = name;
            await context.SaveChangesAsync(cancellationToken);
            return (ToOutput(existing), false);
        }

        var entity = new TTrans
        {
            EntityId = lookupId,
            LanguageCode = languageCode,
            Name = name,
        };
        set.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
        return (ToOutput(entity), true);
    }

    private static LookupTranslationOutputModel ToOutput<TTrans>(TTrans entity)
        where TTrans : TranslationEntity<int>
        => new(entity.Id, entity.EntityId, entity.LanguageCode, entity.Name);
}
