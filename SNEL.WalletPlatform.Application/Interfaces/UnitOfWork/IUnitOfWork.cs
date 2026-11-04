using Microsoft.EntityFrameworkCore.Storage;
using SNEL.WalletPlatform.Application.Customers.Interfaces;
using SNEL.WalletPlatform.Application.Lookups.Interfaces;
using SNEL.WalletPlatform.Application.Notifications.Interfaces;
using SNEL.WalletPlatform.Application.Billers.Interfaces;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.Invoices.Interfaces;
using SNEL.WalletPlatform.Application.Partners.Interfaces;
using SNEL.WalletPlatform.Application.Users.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Accounting.Interfaces;
using SNEL.WalletPlatform.Application.Webhooks.Interfaces;
using SNEL.WalletPlatform.Application.Auth.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;

public interface IUnitOfWork
{
    ICustomerRepository CustomerRepository { get; }
    ICustomerSubscriptionRepository CustomerSubscriptionRepository { get; }
    ICustomerFavoritesRepository CustomerFavoritesRepository { get; }
    IAreaRepository AreaRepository { get; }
    IChannelRepository ChannelRepository { get; }
    ILookupRepository LookupRepository { get; }
    IBillerRepository BillerRepository { get; }
    IBillerCustomerRepository BillerCustomerRepository { get; }
    IBillerConfigRepository BillerConfigRepository { get; }
    IBillingServiceRepository BillingServiceRepository { get; }
    IBillingServiceCurrencyRepository BillingServiceCurrencyRepository { get; }
    IInvoiceRepository InvoiceRepository { get; }
    IPaymentControlRepository PaymentControlRepository { get; }
    IAccountTypeRepository AccountTypeRepository { get; }
    IAccountTypeTranslatableRepository AccountTypeTranslatableRepository { get; }
    IAccountRepository AccountRepository { get; }
    IJournalTypeRepository JournalTypeRepository { get; }
    IJournalRepository JournalRepository { get; }
    IPlatformInvoiceRepository PlatformInvoiceRepository { get; }
    IJournalLineTypeRepository JournalLineTypeRepository { get; }
    IJournalLineRepository JournalLineRepository { get; }
    IUserRepository UserRepository { get; }
    IOtpChallengeRepository OtpChallengeRepository { get; }
    IPartnerRepository PartnerRepository { get; }
    IPaymentRepository PaymentRepository { get; }
    IPaymentMethodCurrencyRepository PaymentMethodCurrencyRepository { get; }
    IPaymentCategoryRepository PaymentCategoryRepository { get; }
    IPaymentMethodRepository PaymentMethodRepository { get; }
    IPaymentOrderRepository PaymentOrderRepository { get; }
    IPaymentOrderItemRepository PaymentOrderItemRepository { get; }
    IPaymentProviderRepository PaymentProviderRepository { get; }
    IPaymentProviderRouterRepository PaymentProviderRouterRepository { get; }
    IPaymentTxnRepository PaymentTxnRepository { get; }
    IInterswitchWebhookEventRepository InterswitchWebhookEventRepository { get; }
    IAkibaPayWebhookEventRepository AkibaPayWebhookEventRepository { get; }
    IEasypayWebhookEventRepository EasypayWebhookEventRepository { get; }
    IAfrimonyWebhookEventRepository AfrimonyWebhookEventRepository { get; }
    IMpesaWebhookEventRepository MpesaWebhookEventRepository { get; }
    IOrangeWebhookEventRepository OrangeWebhookEventRepository { get; }
    ICurrencyRepository CurrencyRepository { get; }
    IGenderRepository GenderRepository { get; }
    IGenderTranslatableRepository GenderTranslatableRepository { get; }
    
    ICenterRepository CenterRepository { get; }

    IPlatformConstantRepository PlatformConstantRepository { get; }
    ICampaignRepository CampaignRepository { get; }



    int Complete();
    Task<int> CompleteAsync();
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default);

    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken = default);
}
