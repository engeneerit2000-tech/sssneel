using Microsoft.EntityFrameworkCore.Storage;
using SNEL.WalletPlatform.Application.Accounting.Interfaces;
using SNEL.WalletPlatform.Application.Billers.Interfaces;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.Customers.Interfaces;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Invoices.Interfaces;
using SNEL.WalletPlatform.Application.Lookups.Interfaces;
using SNEL.WalletPlatform.Application.Partners.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Users.Interfaces;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;
using SNEL.WalletPlatform.Application.Webhooks.Interfaces;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Application.Auth.Interfaces;
using SNEL.WalletPlatform.Application.Notifications.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.UnitOfWork;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext dbContext;
    private readonly ICurrentUserContext currentUserContext;
    private IDbContextTransaction? currentTransaction;
    private IUserRepository? userRepository;
    private IOtpChallengeRepository? otpChallengeRepository;
    private ICustomerRepository? customerRepository;
    private ICustomerSubscriptionRepository? customerSubscriptionRepository;
    private ICustomerFavoritesRepository? customerFavoritesRepository;
    private IAreaRepository? areaRepository;
    private IChannelRepository? channelRepository;
    private ILookupRepository? lookupRepository;
    private IBillerRepository? billerRepository;
    private IBillerCustomerRepository? billerCustomerRepository;
    private IBillerConfigRepository? billerConfigRepository;
    private IBillingServiceRepository? billingServiceRepository;
    private IBillingServiceCurrencyRepository? billingServiceCurrencyRepository;
    private IInvoiceRepository? invoiceRepository;
    private IPaymentControlRepository? paymentControlRepository;

    private IAccountTypeRepository? accountTypeRepository;
    private IAccountTypeTranslatableRepository? accountTypeTranslatableRepository;

    private IAccountRepository? accountRepository;
    private IJournalTypeRepository? journalTypeRepository;
    private IJournalRepository? journalRepository;
    private IJournalLineTypeRepository? journalLineTypeRepository;
    private IJournalLineRepository? journalLineRepository;
    private IPlatformInvoiceRepository? platformInvoiceRepository;
    private IPartnerRepository? partnerRepository;
    private IPaymentRepository? paymentRepository;
    private IPaymentMethodCurrencyRepository? paymentMethodCurrencyRepository;
    private IPaymentCategoryRepository? paymentCategoryRepository;
    private IPaymentMethodRepository? paymentMethodRepository;
    private IPaymentOrderRepository? paymentOrderRepository;
    private IPaymentOrderItemRepository? paymentOrderItemRepository;
    private IPaymentProviderRepository? paymentProviderRepository;
    private IPaymentProviderRouterRepository? paymentProviderRouterRepository;
    private IPaymentTxnRepository? paymentTxnRepository;
    private IInterswitchWebhookEventRepository? interswitchWebhookEventRepository;
    private IAkibaPayWebhookEventRepository? akibaPayWebhookEventRepository;
    private IEasypayWebhookEventRepository? easypayWebhookEventRepository;
    private IAfrimonyWebhookEventRepository? afrimonyWebhookEventRepository;
    private IMpesaWebhookEventRepository? mpesaWebhookEventRepository;
    private IOrangeWebhookEventRepository? orangeWebhookEventRepository;
    private ICurrencyRepository? currencyRepository;
    private IGenderRepository? genderRepository;
    private IGenderTranslatableRepository? genderTranslatableRepository;
    private ICenterRepository? centerRepository;
    private IPlatformConstantRepository platformConstantRepository;
    private ICampaignRepository? campaignRepository;        


    public UnitOfWork(ApplicationDbContext dbContext, ICurrentUserContext currentUserContext)
    {
        this.dbContext = dbContext;
        this.currentUserContext = currentUserContext;
    }

    public IUserRepository UserRepository =>
        userRepository ??= new UserRepository(dbContext);

    public IOtpChallengeRepository OtpChallengeRepository =>
        otpChallengeRepository ??= new OtpChallengeRepository(dbContext);

    public ICustomerRepository CustomerRepository =>
        customerRepository ??= new CustomerRepository(dbContext);

    public ICustomerSubscriptionRepository CustomerSubscriptionRepository =>
        customerSubscriptionRepository ??= new CustomerSubscriptionRepository(dbContext);

    public ICustomerFavoritesRepository CustomerFavoritesRepository =>
        customerFavoritesRepository ??= new CustomerFavoritesRepository(dbContext);

    public IAreaRepository AreaRepository =>
        areaRepository ??= new AreaRepository(dbContext);

    public IChannelRepository ChannelRepository =>
        channelRepository ??= new ChannelRepository(dbContext);

    public ILookupRepository LookupRepository =>
        lookupRepository ??= new LookupRepository(dbContext, currentUserContext);

    public IBillerRepository BillerRepository =>
        billerRepository ??= new BillerRepository(dbContext);

    public IBillerCustomerRepository BillerCustomerRepository =>
        billerCustomerRepository ??= new BillerCustomerRepository(dbContext);

    public IBillerConfigRepository BillerConfigRepository =>
        billerConfigRepository ??= new BillerConfigRepository(dbContext);

    public IBillingServiceRepository BillingServiceRepository =>
        billingServiceRepository ??= new BillingServiceRepository(dbContext);

    public IBillingServiceCurrencyRepository BillingServiceCurrencyRepository =>
        billingServiceCurrencyRepository ??= new BillingServiceCurrencyRepository(dbContext);

    public IInvoiceRepository InvoiceRepository =>
        invoiceRepository ??= new InvoiceRepository(dbContext);

    public IPaymentControlRepository PaymentControlRepository =>
        paymentControlRepository ??= new PaymentControlRepository(dbContext);

    public IAccountTypeRepository AccountTypeRepository =>
        accountTypeRepository ??= new AccountTypeRepository(dbContext);

    public IAccountTypeTranslatableRepository AccountTypeTranslatableRepository =>
        accountTypeTranslatableRepository ??= new AccountTypeTranslatableRepository(dbContext);

    public IAccountRepository AccountRepository =>
        accountRepository ??= new AccountRepository(dbContext);

    public IJournalTypeRepository JournalTypeRepository =>
        journalTypeRepository ??= new JournalTypeRepository(dbContext);

    public IJournalRepository JournalRepository =>
        journalRepository ??= new JournalRepository(dbContext);

    public IPlatformInvoiceRepository PlatformInvoiceRepository =>
        platformInvoiceRepository ??= new PlatformInvoiceRepository(dbContext);

    public IJournalLineTypeRepository JournalLineTypeRepository =>
        journalLineTypeRepository ??= new JournalLineTypeRepository(dbContext);

    public IJournalLineRepository JournalLineRepository =>
        journalLineRepository ??= new JournalLineRepository(dbContext);

    public IPartnerRepository PartnerRepository =>
        partnerRepository ??= new PartnerRepository(dbContext);

    public IPaymentRepository PaymentRepository =>
        paymentRepository ??= new PaymentRepository(dbContext);

    public IPaymentMethodCurrencyRepository PaymentMethodCurrencyRepository =>
        paymentMethodCurrencyRepository ??= new PaymentMethodCurrencyRepository(dbContext);

    public IPaymentCategoryRepository PaymentCategoryRepository =>
        paymentCategoryRepository ??= new PaymentCategoryRepository(dbContext);

    public IPaymentMethodRepository PaymentMethodRepository =>
        paymentMethodRepository ??= new PaymentMethodRepository(dbContext);

    public IPaymentOrderRepository PaymentOrderRepository =>
        paymentOrderRepository ??= new PaymentOrderRepository(dbContext);

    public IPaymentOrderItemRepository PaymentOrderItemRepository =>
        paymentOrderItemRepository ??= new PaymentOrderItemRepository(dbContext);

    public IPaymentProviderRepository PaymentProviderRepository =>
        paymentProviderRepository ??= new PaymentProviderRepository(dbContext);

    public IPaymentProviderRouterRepository PaymentProviderRouterRepository =>
        paymentProviderRouterRepository ??= new PaymentProviderRouterRepository(dbContext);
    public IPaymentTxnRepository PaymentTxnRepository =>
        paymentTxnRepository ??= new PaymentTxnRepository(dbContext);

    public IInterswitchWebhookEventRepository InterswitchWebhookEventRepository =>
        interswitchWebhookEventRepository ??= new InterswitchWebhookEventRepository(dbContext);

    public IAkibaPayWebhookEventRepository AkibaPayWebhookEventRepository =>
        akibaPayWebhookEventRepository ??= new AkibaPayWebhookEventRepository(dbContext);

    public IEasypayWebhookEventRepository EasypayWebhookEventRepository =>
        easypayWebhookEventRepository ??= new EasypayWebhookEventRepository(dbContext);

    public IAfrimonyWebhookEventRepository AfrimonyWebhookEventRepository =>
        afrimonyWebhookEventRepository ??= new AfrimonyWebhookEventRepository(dbContext);

    public IMpesaWebhookEventRepository MpesaWebhookEventRepository =>
        mpesaWebhookEventRepository ??= new MpesaWebhookEventRepository(dbContext);

    public IOrangeWebhookEventRepository OrangeWebhookEventRepository =>
        orangeWebhookEventRepository ??= new OrangeWebhookEventRepository(dbContext);

    public ICurrencyRepository CurrencyRepository =>
        currencyRepository ??= new CurrencyRepository(dbContext);

    public IGenderRepository GenderRepository =>
        genderRepository ??= new GenderRepository(dbContext);

    public IGenderTranslatableRepository GenderTranslatableRepository =>
        genderTranslatableRepository ??= new GenderTranslatableRepository(dbContext);

    public ICenterRepository CenterRepository =>
        centerRepository ??= new CenterRepository(dbContext);       
    public IPlatformConstantRepository PlatformConstantRepository => platformConstantRepository ??= new PlatformConstantRepository(dbContext);
    public ICampaignRepository CampaignRepository =>
        campaignRepository ??= new CampaignRepository(dbContext);    

    public int Complete()
    {
        return dbContext.SaveChanges();
    }

    public Task<int> CompleteAsync()
    {
        return dbContext.SaveChangesAsync();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        if (currentTransaction != null)
        {
            return currentTransaction;
        }

        currentTransaction = await dbContext.Database.BeginTransactionAsync(ct);
        return currentTransaction;
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (currentTransaction == null)
        {
            throw new InvalidOperationException("No active transaction to commit.");
        }

        try
        {
            await currentTransaction.CommitAsync(ct);
        }
        finally
        {
            await currentTransaction.DisposeAsync();
            currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (currentTransaction == null)
        {
            return;
        }

        try
        {
            await currentTransaction.RollbackAsync(ct);
        }
        finally
        {
            await currentTransaction.DisposeAsync();
            currentTransaction = null;
        }
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken ct = default)
    {
        var ownsTransaction = currentTransaction == null;
        var tx = ownsTransaction
            ? await dbContext.Database.BeginTransactionAsync(ct)
            : currentTransaction!;

        if (ownsTransaction)
        {
            currentTransaction = tx;
        }

        try
        {
            await action(ct);

            if (ownsTransaction)
            {
                await tx.CommitAsync(ct);
            }
        }
        catch
        {
            if (ownsTransaction)
            {
                await tx.RollbackAsync(ct);
            }

            throw;
        }
        finally
        {
            if (ownsTransaction)
            {
                await tx.DisposeAsync();
                currentTransaction = null;
            }
        }
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken = default)
    {
        bool ownsTransaction = currentTransaction == null;
        var transaction = ownsTransaction
            ? await dbContext.Database.BeginTransactionAsync(cancellationToken)
            : currentTransaction!;

        if (ownsTransaction)
        {
            currentTransaction = transaction;
        }

        try
        {
            TResult result = await action(cancellationToken);

            if (ownsTransaction)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }

            return result;
        }
        catch
        {
            if (ownsTransaction)
            {
                await transaction.RollbackAsync(cancellationToken);
            }

            throw;
        }
        finally
        {
            if (ownsTransaction)
            {
                await transaction.DisposeAsync();
                currentTransaction = null;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (currentTransaction != null)
        {
            await currentTransaction.DisposeAsync();
            currentTransaction = null;
        }
    }
}
