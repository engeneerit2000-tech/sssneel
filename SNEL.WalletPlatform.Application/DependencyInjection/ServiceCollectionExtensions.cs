using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SNEL.WalletPlatform.Application.Accounting.Interfaces;
using SNEL.WalletPlatform.Application.Accounting.Services;
using SNEL.WalletPlatform.Application.Auth.Interfaces;
using SNEL.WalletPlatform.Application.Auth.Models.Options;
using SNEL.WalletPlatform.Application.Auth.Services;
using SNEL.WalletPlatform.Application.Billers.Interfaces;
using SNEL.WalletPlatform.Application.Billers.Services;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.BillingServices.Services;
using SNEL.WalletPlatform.Application.Customers.Interfaces;
using SNEL.WalletPlatform.Application.Customers.Services;
using SNEL.WalletPlatform.Application.FileExport.Services;
using SNEL.WalletPlatform.Application.Invoices.Interfaces;
using SNEL.WalletPlatform.Application.Invoices.Services;
using SNEL.WalletPlatform.Application.Lookups.Interfaces;
using SNEL.WalletPlatform.Application.Lookups.Services;
using SNEL.WalletPlatform.Application.Notification.Services;
using SNEL.WalletPlatform.Application.Notifications.Interfaces;
using SNEL.WalletPlatform.Application.Notifications.Services;
using SNEL.WalletPlatform.Application.Partners.Interfaces;
using SNEL.WalletPlatform.Application.Partners.Services;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Mapper;
using SNEL.WalletPlatform.Application.Payments.Services;
using SNEL.WalletPlatform.Application.Test;
using SNEL.WalletPlatform.Application.Webhooks.Interfaces;
using SNEL.WalletPlatform.Application.Webhooks.Services;
using SNEL.WalletPlatform.Application.Users.Interfaces;
using SNEL.WalletPlatform.Application.Users.Services;

namespace SNEL.WalletPlatform.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    //TODO: inject all services not used in infrastructure directly without using interfaces. i.e. Application services
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OtpConfigurationOptions>(configuration.GetSection(OtpConfigurationOptions.SectionName));
        services.Configure<Notifications.Configuration.CampaignNotificationSettings>(
            configuration.GetSection(Notifications.Configuration.CampaignNotificationSettings.SectionName));

        services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);
        services.AddScoped<PaymentHistoryMapper>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOtpChallengeService, OtpChallengeService>();
        services.AddScoped<IBillerAccessPolicy, BillerAccessPolicy>();
        services.AddScoped<IBillerService, BillerService>();
        services.AddScoped<IBillingServiceAccessPolicy, BillingServiceAccessPolicy>();
        services.AddScoped<IBillingInvoiceRefResolver, BillingInvoiceRefResolver>();
        services.AddScoped<IBillingServiceService, BillingServiceService>();
        services.AddScoped<ICustomerService, CustomerService>();
        // services.AddScoped<IPaymentInitializationService, PaymentInitializationService>();
        services.AddScoped<IPaymentRefGenerator, PaymentRefGenerator>();
        services.AddScoped<NotificationService>();
        services.AddScoped<ExcelFileExportService>();
        services.AddScoped<TestService>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<ILookupTranslationService, LookupTranslationService>();
        services.AddScoped<IWebhookEventService, WebhookEventService>();
        services.AddScoped<ICustomerSubscriptionService, CustomerSubscriptionService>();
        services.AddScoped<ICustomerFavoritesService, CustomerFavoritesService>();
        services.AddScoped<IAccountingService, AccountingService>();
        services.AddScoped<IJournalLineService, JournalLineService>();
        services.AddScoped<IPlatformInvoiceService, PlatformInvoiceService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IPaymentProviderAccessPolicy, PaymentProviderAccessPolicy>();
        services.AddScoped<IPaymentProviderService, PaymentProviderService>();
        services.AddScoped<IPaymentMethodAccessPolicy, PaymentMethodAccessPolicy>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();
        //services.AddScoped<IInitPaymentService, ClientSideInitPaymentService>();
        //services.AddScoped<IInitPaymentService, ServerSideInitPaymentService>();
        services.AddScoped<IInitPaymentServiceResolver, InitPaymentServiceResolver>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPlatformConstantService, PlatformConstantService>();
        services.AddScoped<ICampaignService, CampaignService>();
        services.AddScoped<ICampaignNotificationDispatcher, CampaignNotificationDispatcher>();
        services.AddScoped<CampaignAudienceResolver, AllCustomersCampaignAudienceResolver>();
        services.AddScoped<CampaignAudienceResolver, AllPspAdminsCampaignAudienceResolver>();
        services.AddScoped<CampaignAudienceResolver, AllBillerAdminsCampaignAudienceResolver>();
        services.AddScoped<CampaignAudienceResolver, QueryableCampaignAudienceResolver>();
        services.AddSingleton<Jobs.BatchProcessing.IBatchSleepCalculator, Jobs.BatchProcessing.BatchSleepCalculator>();
        services.AddScoped<IUserManagementAccessPolicy, UserManagementAccessPolicy>();
        services.AddScoped<IUserPartnerResolver, UserPartnerResolver>();
        services.AddScoped<IUserRoleResolver, UserRoleResolver>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IPartnerService, PartnerService>();
        return services;
    }
}
