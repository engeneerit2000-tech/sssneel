using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Auth;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Notifications;
using SNEL.WalletPlatform.Domain.Partners;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Domain.Users;
using SNEL.WalletPlatform.Domain.Wallets;
using SNEL.WalletPlatform.Domain.Webhooks;
using SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

namespace SNEL.WalletPlatform.Infrastructure.Persistence;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ICurrentUserContext currentUserContext) : DbContext(options)
{
    public DbSet<CustomerEntity> Customers => Set<CustomerEntity>();
    public DbSet<CustomerFavoritesEntity> CustomerFavorites => Set<CustomerFavoritesEntity>();
    public DbSet<CustomerSubscriptionEntity> CustomerSubscriptions => Set<CustomerSubscriptionEntity>();
    public DbSet<FavoriteTypeEntity> FavoriteTypes => Set<FavoriteTypeEntity>();
    public DbSet<FavoriteTypeTranslationEntity> FavoriteTypeTranslations => Set<FavoriteTypeTranslationEntity>();
    public DbSet<BillingCategoryEntity> BillingCategories => Set<BillingCategoryEntity>();
    public DbSet<BillingCategoryTranslationEntity> BillingCategoryTranslations => Set<BillingCategoryTranslationEntity>();
    public DbSet<BillerEntity> Billers => Set<BillerEntity>();
    public DbSet<BillerCustomerEntity> BillerCustomers => Set<BillerCustomerEntity>();
    public DbSet<BillerCustomerStagingEntity> BillerCustomerStagings => Set<BillerCustomerStagingEntity>();
    public DbSet<BillerConfigEntity> BillerConfigs => Set<BillerConfigEntity>();
    public DbSet<BillingServiceEntity> BillingServices => Set<BillingServiceEntity>();
    public DbSet<BillingServiceTranslationEntity> BillingServiceTranslations => Set<BillingServiceTranslationEntity>();
    public DbSet<BillingServiceCurrencyEntity> BillingServiceCurrencies => Set<BillingServiceCurrencyEntity>();
    public DbSet<InvoiceEntity> Invoices => Set<InvoiceEntity>();
    public DbSet<NewInvoiceNotificationEntity> NewInvoiceNotifications => Set<NewInvoiceNotificationEntity>();
    public DbSet<CampaignEntity> Campaigns => Set<CampaignEntity>();
    public DbSet<CampaignTranslationEntity> CampaignTranslations => Set<CampaignTranslationEntity>();
    public DbSet<InvoiceStagingEntity> InvoiceStagings => Set<InvoiceStagingEntity>();
    public DbSet<InvoiceSecondaryStagingEntity> InvoiceSecondaryStagings => Set<InvoiceSecondaryStagingEntity>();
    public DbSet<PaymentControlEntity> PaymentControls => Set<PaymentControlEntity>();
    public DbSet<UserNotificationPreferenceEntity> UserNotificationPreferences => Set<UserNotificationPreferenceEntity>();
    public DbSet<CustomerRegistrationHistoryEntity> CustomerRegistrationHistories => Set<CustomerRegistrationHistoryEntity>();
    public DbSet<CountryEntity> Countries => Set<CountryEntity>();
    public DbSet<CountryTranslationEntity> CountryTranslations => Set<CountryTranslationEntity>();
    public DbSet<ChannelEntity> Channels => Set<ChannelEntity>();
    public DbSet<ChannelTranslationEntity> ChannelTranslations => Set<ChannelTranslationEntity>();
    public DbSet<CenterEntity> Centers => Set<CenterEntity>();
    public DbSet<CenterTranslationEntity> CenterTranslations => Set<CenterTranslationEntity>();
    public DbSet<RegionEntity> Regions => Set<RegionEntity>();
    public DbSet<RegionTranslationEntity> RegionTranslations => Set<RegionTranslationEntity>();
    public DbSet<CityEntity> Cities => Set<CityEntity>();
    public DbSet<CityTranslationEntity> CityTranslations => Set<CityTranslationEntity>();
    public DbSet<AreaEntity> Areas => Set<AreaEntity>();
    public DbSet<AreaTranslationEntity> AreaTranslations => Set<AreaTranslationEntity>();
    public DbSet<PartnerTypeEntity> PartnerTypes => Set<PartnerTypeEntity>();
    public DbSet<PartnerTypeTranslationEntity> PartnerTypeTranslations => Set<PartnerTypeTranslationEntity>();
    public DbSet<BillingServiceTypeEntity> BillingServiceTypes => Set<BillingServiceTypeEntity>();
    public DbSet<BillingServiceTypeTranslationEntity> BillingServiceTypeTranslations => Set<BillingServiceTypeTranslationEntity>();
    public DbSet<CurrencyEntity> Currencies => Set<CurrencyEntity>();
    public DbSet<CurrencyTranslationEntity> CurrencyTranslations => Set<CurrencyTranslationEntity>();
    public DbSet<GenderTranslationEntity> GenderTranslations => Set<GenderTranslationEntity>();
    public DbSet<PartnerEntity> Partners => Set<PartnerEntity>();
    public DbSet<PartnerContactEntity> PartnerContacts => Set<PartnerContactEntity>();
    public DbSet<ContactTypeEntity> ContactTypes => Set<ContactTypeEntity>();
    public DbSet<ContactTypeTranslationEntity> ContactTypeTranslations => Set<ContactTypeTranslationEntity>();
    public DbSet<OtpChallengeEntity> OtpChallenges => Set<OtpChallengeEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<AccountTypeEntity> AccountTypes => Set<AccountTypeEntity>();
    public DbSet<AccountTypeTranslationEntity> AccountTypeTranslations => Set<AccountTypeTranslationEntity>();
    public DbSet<AccountEntity> Accounts => Set<AccountEntity>();
    public DbSet<JournalTypeEntity> JournalTypes => Set<JournalTypeEntity>();
    public DbSet<JournalTypeTranslationEntity> JournalTypeTranslations => Set<JournalTypeTranslationEntity>();
    public DbSet<JournalLineTypeEntity> JournalLineTypes => Set<JournalLineTypeEntity>();
    public DbSet<JournalLineTypeTranslationEntity> JournalLineTypeTranslations => Set<JournalLineTypeTranslationEntity>();
    public DbSet<JournalEntity> Journals => Set<JournalEntity>();
    public DbSet<JournalLineEntity> JournalLines => Set<JournalLineEntity>();
    public DbSet<PlatformInvoiceEntity> PlatformInvoices => Set<PlatformInvoiceEntity>();
    public DbSet<InterswitchWebhookEventEntity> InterswitchWebhookEvents => Set<InterswitchWebhookEventEntity>();
    public DbSet<AkibaPayWebhookEventEntity> AkibaPayWebhookEvents => Set<AkibaPayWebhookEventEntity>();
    public DbSet<EasypayWebhookEventEntity> EasypayWebhookEvents => Set<EasypayWebhookEventEntity>();
    public DbSet<AfrimonyWebhookEventEntity> AfrimonyWebhookEvents => Set<AfrimonyWebhookEventEntity>();
    public DbSet<MpesaWebhookEventEntity> MpesaWebhookEvents => Set<MpesaWebhookEventEntity>();
    public DbSet<OrangeWebhookEventEntity> OrangeWebhookEvents => Set<OrangeWebhookEventEntity>();

    public DbSet<GenderEntity> Genders => Set<GenderEntity>();
    public DbSet<PaymentCategoryEntity> PaymentCategories => Set<PaymentCategoryEntity>();
    public DbSet<PaymentCategoryTranslationEntity> PaymentCategoryTranslations => Set<PaymentCategoryTranslationEntity>();
    public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();
    public DbSet<PaymentMethodCurrencyEntity> PaymentMethodCurrencies => Set<PaymentMethodCurrencyEntity>();
    public DbSet<PaymentMethodEntity> PaymentMethods => Set<PaymentMethodEntity>();
    public DbSet<PaymentProviderRouterEntity> PaymentProviderRouters => Set<PaymentProviderRouterEntity>();
    public DbSet<PaymentOrderEntity> PaymentOrders => Set<PaymentOrderEntity>();
    public DbSet<PaymentOrderItemEntity> PaymentOrderItems => Set<PaymentOrderItemEntity>();
    public DbSet<PaymentProviderEntity> PaymentProviders => Set<PaymentProviderEntity>();
    public DbSet<PaymentTxnEntity> PaymentTxns => Set<PaymentTxnEntity>();

    public DbSet<CompletionModeEntity> CompletionModes => Set<CompletionModeEntity>();
    public DbSet<CompletionModeTranslationEntity> CompletionModeTranslations => Set<CompletionModeTranslationEntity>();
    public DbSet<ConfirmationModeEntity> ConfirmationModes => Set<ConfirmationModeEntity>();
    public DbSet<ConfirmationModeTranslationEntity> ConfirmationModeTranslations => Set<ConfirmationModeTranslationEntity>();
    public DbSet<ConfirmationTypeEntity> ConfirmationTypes => Set<ConfirmationTypeEntity>();
    public DbSet<ConfirmationTypeTranslationEntity> ConfirmationTypeTranslations => Set<ConfirmationTypeTranslationEntity>();
    public DbSet<PaymentInitModeEntity> PaymentInitModes => Set<PaymentInitModeEntity>();
    public DbSet<PaymentInitModeTranslationEntity> PaymentInitModeTranslations => Set<PaymentInitModeTranslationEntity>();
    public DbSet<PaymentMethodPurposeEntity> PaymentMethodPurposes => Set<PaymentMethodPurposeEntity>();
    public DbSet<PaymentMethodPurposeTranslationEntity> PaymentMethodPurposeTranslations => Set<PaymentMethodPurposeTranslationEntity>();

    public DbSet<PlatformConstantEntity> PlatformConstants => Set<PlatformConstantEntity>();
    public DbSet<PlatformConstantTranslationEntity> PlatformConstantTranslations => Set<PlatformConstantTranslationEntity>();

    public DbSet<PaymentTypeEntity> PaymentTypes => Set<PaymentTypeEntity>();
    public DbSet<PaymentTypeTranslationEntity> PaymentTypeTranslations => Set<PaymentTypeTranslationEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
        modelBuilder.SeedData();
    }

    public override int SaveChanges()
    {
        ApplyAuditFields();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditFields();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public string GetCurrentActor() =>
        string.IsNullOrWhiteSpace(currentUserContext.UserId)
            ? "SYSTEM"
            : currentUserContext.UserId;

    private void ApplyAuditFields()
    {
        var now = DateTimeOffset.UtcNow;
        var actor = GetCurrentActor();

        foreach (var entry in ChangeTracker.Entries<ITrackedEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = now;
                entry.Entity.CreatedBy = actor;

                entry.Entity.UpdatedAtUtc = now;
                entry.Entity.UpdatedBy = actor;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = now;
                entry.Entity.UpdatedBy = actor;

                // Prevent overwriting creation fields on update
                entry.Property(x => x.CreatedAtUtc).IsModified = false;
                entry.Property(x => x.CreatedBy).IsModified = false;
            }
        }
    }
}
