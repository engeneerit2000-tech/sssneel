using Duende.AccessTokenManagement;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Common;
using Keycloak.AuthServices.Sdk;
using Medallion.Threading;
using Medallion.Threading.Postgres;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.FileExport.Interfaces;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Application.Interfaces.IAM;
using SNEL.WalletPlatform.Application.Interfaces.Repositories;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Invoicing.Interfaces;
using SNEL.WalletPlatform.Application.Jobs.Interfaces;
using SNEL.WalletPlatform.Application.Lookups.Interfaces;
using SNEL.WalletPlatform.Application.Notification.Configuration;
using SNEL.WalletPlatform.Application.Notification.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Configurations;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Infrastructure.Auth;
using SNEL.WalletPlatform.Infrastructure.BillingServices;
using SNEL.WalletPlatform.Infrastructure.BillingServices.ConnectAfrica;
using SNEL.WalletPlatform.Infrastructure.BillingServices.Connectors;
using SNEL.WalletPlatform.Infrastructure.Configuration;
using SNEL.WalletPlatform.Infrastructure.FileExport;
using SNEL.WalletPlatform.Infrastructure.IAM;
using SNEL.WalletPlatform.Infrastructure.Invoicing;
using SNEL.WalletPlatform.Infrastructure.Jobs;
using SNEL.WalletPlatform.Infrastructure.Notification;
using SNEL.WalletPlatform.Infrastructure.Oracle;
using SNEL.WalletPlatform.Infrastructure.Payments;
using SNEL.WalletPlatform.Infrastructure.Payments.Clients;
using SNEL.WalletPlatform.Infrastructure.Payments.Connectors;
using SNEL.WalletPlatform.Infrastructure.Payments.Easypay;
using SNEL.WalletPlatform.Infrastructure.Payments.Services;
using SNEL.WalletPlatform.Infrastructure.Persistence;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;
using SNEL.WalletPlatform.Infrastructure.Persistence.Seed;
using SNEL.WalletPlatform.Infrastructure.Persistence.UnitOfWork;
using System.Text.Json;
using System.Text.Json.Serialization;
//using SNEL.WalletPlatform.Infrastructure.Payments.Interswitch;

namespace SNEL.WalletPlatform.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=snel_wallet;Username=postgres;Password=postgres";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.ConfigureDataSource(dataSourceBuilder =>
                {
                    dataSourceBuilder.ConfigureJsonOptions(
                        new JsonSerializerOptions(JsonSerializerDefaults.Web)
                        {
                            PropertyNamingPolicy = null,
                            PropertyNameCaseInsensitive = true,
                            Converters =
                            {
                                new JsonStringEnumConverter()
                            }
                        });
                    dataSourceBuilder.EnableDynamicJson();
                });
            });
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            
        });

        services.AddSingleton<IOracleSnelConnectionFactory, OracleSnelConnectionFactory>();
        services.AddTransient<IInvoiceRetrieval, PullInvoicesOracleQuery>();
        services.AddTransient<IBillerCustomerOraclePull, PullBillerCustomersOracleQuery>();

        services.Configure<KeycloakOptions>(configuration.GetSection(KeycloakOptions.SectionName));
        services.Configure<ConnectAfricaOptions>(configuration.GetSection(ConnectAfricaOptions.SectionName));
        services.Configure<SnelOracleLoggingOptions>(configuration.GetSection(SnelOracleLoggingOptions.SectionName));
        services.Configure<NotificationConfigurations>(configuration.GetSection(NotificationConfigurations.SectionName));
        services.Configure<PaymentPrintOptions>(configuration.GetSection(PaymentPrintOptions.SectionName));
        services.Configure<AkibaPayOptions>(configuration.GetSection(AkibaPayOptions.SectionName));
        services.Configure<EasypayOptions>(configuration.GetSection(EasypayOptions.SectionName));
        services.Configure<AfrimonyOptions>(configuration.GetSection(AfrimonyOptions.SectionName));
        services.Configure<MpesaOptions>(configuration.GetSection(MpesaOptions.SectionName));
        services.Configure<OrangeOptions>(configuration.GetSection(OrangeOptions.SectionName));
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.AddTransient<IClaimsTransformation, KeycloakRealmRoleClaimsTransformation>();
        services.AddHttpClient("KeycloakAdmin");
        services.AddHttpClient<ConnectAfricaHttpClient>();
        services.AddHttpClient<InterswitchHttpClient>();
        services.AddHttpClient<AkibaPayHttpClient>();
        services.AddHttpClient<EasypayHttpClient>();
        services.AddHttpClient<AfrimonyHttpClient>();
        services.AddHttpClient<MpesaHttpClient>();
        services.AddHttpClient<OrangeHttpClient>();
        //services.AddScoped<InterswitchMobileMoneyPaymentInitializer>();
        services.AddScoped<IIdentityUserManager, KeycloakIdentityUserManager>();
        services.AddScoped<ISnelPaymentTypeResolver, SnelPaymentTypeResolver>();
        services.AddScoped<ISnelOracleApplyCommandBuilder, SnelOracleApplyCommandBuilder>();
        services.AddScoped<ISnelOracleJournalService, SnelOracleJournalService>();
        services.AddScoped<ISnelConfigService, SnelConfigService>();
        services.AddScoped<SnelWalletConfigService>();
        services.AddScoped<ISnelBootstrapService, SnelBootstrapService>();
        services.AddScoped<BillerConnector, SnelBillerConnector>();
        services.AddScoped<IBillerConnectorResolver, BillerConnectorResolver>();
        services.AddScoped<PspConnector, InterswitchPspConnector>();
        services.AddScoped<PspConnector, AkibaPayPspConnector>();
        services.AddScoped<PspConnector, EasypayPspConnector>();
        services.AddScoped<PspConnector, AfrimonyPspConnector>();
        services.AddScoped<PspConnector, MpesaPspConnector>();
        services.AddScoped<PspConnector, OrangePspConnector>();
        services.AddScoped<PspConnector, SnelWalletPspConnector>();
        services.AddScoped<IPspConnectorResolver, PspConnectorResolver>();
        services.AddScoped<IAkibaPayPaymentInitContext, AkibaPayPaymentInitContext>();
        services.AddSingleton<IAkibaPayManagementTokenProvider, AkibaPayManagementTokenProvider>();
        services.AddSingleton<AkibaPayStatusPollingCoordinator>();
        services.AddSingleton<IAkibaPayStatusPollingScheduler>(sp =>
            sp.GetRequiredService<AkibaPayStatusPollingCoordinator>());
        services.AddHostedService<AkibaPayStatusPollingBackgroundService>();
        services.AddScoped<IEasypayPaymentInitContext, EasypayPaymentInitContext>();
        services.AddSingleton<IEasypayManagementTokenProvider, EasypayManagementTokenProvider>();
        services.AddSingleton<EasypayStatusPollingCoordinator>();
        services.AddSingleton<IEasypayStatusPollingScheduler>(sp =>
            sp.GetRequiredService<EasypayStatusPollingCoordinator>());
        services.AddHostedService<EasypayStatusPollingBackgroundService>();
        services.AddScoped<IAfrimonyPaymentInitContext, AfrimonyPaymentInitContext>();
        services.AddSingleton<IAfrimonyTokenProvider, AfrimonyTokenProvider>();
        services.AddSingleton<AfrimonyStatusPollingCoordinator>();
        services.AddSingleton<IAfrimonyStatusPollingScheduler>(sp =>
            sp.GetRequiredService<AfrimonyStatusPollingCoordinator>());
        services.AddHostedService<AfrimonyStatusPollingBackgroundService>();
        services.AddScoped<IMpesaPaymentInitContext, MpesaPaymentInitContext>();
        services.AddSingleton<IMpesaSessionProvider, MpesaSessionProvider>();
        services.AddHostedService<MpesaSessionRefreshBackgroundService>();
        services.AddScoped<IOrangePaymentInitContext, OrangePaymentInitContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ILookupTranslationRepository, LookupTranslationRepository>();
        services.AddScoped<INotificationManager, NotificationManager>();
        services.AddScoped<IExcelFileExportManager, ExcelFileExportManager>();
        services.AddScoped<ICachingRepository, CacheRepository>();
        services.AddHostedService<CacheInitializationService>();
        // Configure the Keycloak service communication http client with keycloak authentication
        var keycloakConfigs = configuration.GetKeycloakOptions<KeycloakAdminClientOptions>()!;
        services
            .AddClientCredentialsTokenManagement()
            .AddClient(
                IAM.KeycloakConstants.ServiceCommunicationClient,
                client =>
                {
                    client.ClientId =  ClientId.Parse(keycloakConfigs.Resource);
                    client.ClientSecret = ClientSecret.Parse(keycloakConfigs.Credentials.Secret);
                    client.TokenEndpoint = new Uri(keycloakConfigs.KeycloakTokenEndpoint);
                }
            );

        // prepare the notification http client with keycloak authentication
        var notificationServiceSettings = configuration.GetSection(NotificationConfigurations.SectionName).Get<NotificationConfigurations>();
        services.AddHttpClient(NotificationConstants.NotificationServiceClient, client =>
        {
            client.BaseAddress = new Uri(notificationServiceSettings.BaseURL!);
        }).AddClientCredentialsTokenHandler(ClientCredentialsClientName.Parse(IAM.KeycloakConstants.ServiceCommunicationClient));


        services.AddScoped<IInvoiceStagingSyncService, InvoiceStagingSyncService>();
        services.AddScoped<IBillerCustomerStagingMergeService, BillerCustomerStagingMergeService>();
        services.AddScoped<IManualJobTriggerService, ManualJobTriggerService>();
        services.AddScoped<SNEL.WalletPlatform.Application.Notifications.Interfaces.ICampaignNotificationScheduler, Notifications.CampaignNotificationScheduler>();
        services.AddScoped<SNEL.WalletPlatform.Application.Notifications.Interfaces.ICampaignAudienceResolver, Notifications.CampaignAudienceResolverRegistry>();
        services.AddSingleton<IDistributedLockProvider>(sp => new PostgresDistributedSynchronizationProvider(connectionString));
        services.AddHostedService<AdminIdentityBootstrapHostedService>();

        return services;
    }

    public static async Task InitializeInfrastructureAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await DbInitializer.InitializeAsync(dbContext);
    }
}
