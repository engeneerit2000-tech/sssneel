using AutoMapper;
using Microsoft.Extensions.Localization;
using Moq;
using SNEL.WalletPlatform.Application.Accounting.Interfaces;
using SNEL.WalletPlatform.Application.Auth.Interfaces;
using SNEL.WalletPlatform.Application.Billers.Interfaces;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.BillingServices.Models.Input;
using SNEL.WalletPlatform.Application.BillingServices.Models.Output;
using SNEL.WalletPlatform.Application.BillingServices.Mapper;
using SNEL.WalletPlatform.Application.BillingServices.Services;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Customers.Interfaces;
using SNEL.WalletPlatform.Application.Customers.Models.Output;
using SNEL.WalletPlatform.Application.Customers.Models.Internal;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Invoices.Interfaces;
using SNEL.WalletPlatform.Application.Lookups.Interfaces;
using SNEL.WalletPlatform.Application.Notifications.Interfaces;
using SNEL.WalletPlatform.Application.Partners.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Users.Interfaces;
using SNEL.WalletPlatform.Application.Webhooks.Interfaces;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Partners;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.BillingServices.Connectors;
using System.Globalization;
using System.Reflection;
using SNEL.WalletPlatform.Infrastructure.BillingServices.ConnectAfrica;
using Medallion.Threading;
using Microsoft.EntityFrameworkCore.Storage;

namespace SNEL.WalletPlatform.Application.Tests;

public sealed class BillingInquiryTests
{
    [Fact]
    public async Task Inquire_UnpaidOnlyAndAllPaid_Returns200WithEmptyBills()
    {
        var setup = CreateSystemUnderTest();
        var result = await setup.Service.Inquire("SNEL", "SNEL", "REF-1", unpaidOnly: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(AppHttpStatusCodes.Ok, result.StatusCode);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!.Bills);
        Assert.Empty(result.Value.TotalOutstanding);
    }

    [Fact]
    public async Task Inquire_UnknownRef_Returns200WithEmptyBills()
    {
        var setup = CreateSystemUnderTest();
        var result = await setup.Service.Inquire("SNEL", "SNEL", "REF-404");

        Assert.True(result.IsSuccess);
        Assert.Equal(AppHttpStatusCodes.Ok, result.StatusCode);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!.Bills);
    }

    [Fact]
    public async Task Inquire_ByCustomerSubscriptionId_UsesSubscriptionData()
    {
        var setup = CreateSystemUnderTest();
        var result = await setup.Service.Inquire(setup.SubscriptionId, unpaidOnly: false);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("REF-2", result.Value!.BillingRef);
        Assert.Equal(2, result.Value.Bills.Count);
    }

    [Fact]
    public async Task Inquire_ByBillingServiceIdAndRef_WithUnpaidOnly_ReturnsOnlyUnpaid()
    {
        var setup = CreateSystemUnderTest();
        var result = await setup.Service.Inquire(setup.BillingServiceId, "REF-2", unpaidOnly: true);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value!.Bills);
        Assert.All(result.Value.Bills, x => Assert.True(x.AmountOutstanding > 0));
        Assert.Single(result.Value.TotalOutstanding);
        Assert.Equal("CDF", result.Value.TotalOutstanding.First().CurrencyCode);
        Assert.Equal(
            result.Value.Bills.Sum(x => x.AmountOutstanding),
            result.Value.TotalOutstanding.First().Amount);
    }

    [Fact]
    public async Task InquirePaged_UsesRequestedPagingAndAllowedSorting()
    {
        var setup = CreateSystemUnderTest();
        var query = new PagedBillingInquiryInputModel
        {
            CustomerSubscriptionId = setup.SubscriptionId,
            UnpaidOnly = false,
            PageNumber = 1,
            PageSize = 1,
            SortBy = "amountTotal",
            IsAscending = true
        };

        var result = await setup.Service.InquirePaged(query);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value!.Bills.Items);
        Assert.Equal(2, result.Value.Bills.TotalCount);
        Assert.Equal("INV-PAID-2", result.Value.Bills.Items.First().InvoiceRef);
        Assert.Single(result.Value.TotalOutstanding);
        Assert.Equal(60m, result.Value.TotalOutstanding.First().Amount);
    }

    [Fact]
    public async Task InquireByInvoiceRef_Unpaid_ReturnsSingleBill()
    {
        var setup = CreateSystemUnderTest();
        var result = await setup.Service.InquireByInvoiceRef("INV-UNPAID-1", unpaidOnly: true);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("REF-2", result.Value!.BillingRef);
        Assert.Single(result.Value.Bills);
        Assert.Equal("INV-UNPAID-1", result.Value.Bills.First().InvoiceRef);
        Assert.Equal("Oracle End Customer", result.Value.Bills.First().CustomerName);
        Assert.Single(result.Value.TotalOutstanding);
        Assert.Equal("CDF", result.Value.TotalOutstanding.First().CurrencyCode);
        Assert.Equal(60m, result.Value.TotalOutstanding.First().Amount);
    }

    [Fact]
    public async Task InquireByInvoiceRef_PaidWithUnpaidOnly_ReturnsBadRequest()
    {
        var setup = CreateSystemUnderTest();
        var result = await setup.Service.InquireByInvoiceRef("INV-PAID-1", unpaidOnly: true);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppHttpStatusCodes.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task InquireByInvoiceRef_UnknownRef_ReturnsNotFound()
    {
        var setup = CreateSystemUnderTest();
        var result = await setup.Service.InquireByInvoiceRef("INV-MISSING", unpaidOnly: true);

        Assert.False(result.IsSuccess);
        Assert.Equal(AppHttpStatusCodes.NotFound, result.StatusCode);
    }

    [Fact]
    public void CalculateTotalOutstandingByCurrency_GroupsByCurrency()
    {
        var bills = new[]
        {
            CreateBill("INV-1", 60m, "CDF"),
            CreateBill("INV-2", 50m, "USD"),
            CreateBill("INV-3", 40m, "CDF"),
        };

        var totals = InquireOutputTotals.CalculateTotalOutstandingByCurrency(bills);

        Assert.Equal(2, totals.Count);
        Assert.Equal(("CDF", 100m), (totals.First().CurrencyCode, totals.First().Amount));
        Assert.Equal(("USD", 50m), (totals.Last().CurrencyCode, totals.Last().Amount));
    }

    private static InquireBillOutputModel CreateBill(string invoiceRef, decimal amountOutstanding, string currencyCode) =>
        new(
            Guid.NewGuid(),
            invoiceRef,
            amountOutstanding,
            0m,
            amountOutstanding,
            currencyCode,
            "Issued",
            1,
            DateTimeOffset.UtcNow,
            null,
            null,
            null);

    private static TestSetup CreateSystemUnderTest()
    {
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<BillingServiceMapper>()).CreateMapper();
        var localizer = new PassthroughStringLocalizer<SharedResource>();

        var partner = PartnerEntity.Create(
            partnerTypeId: 1,
            legalName: "Societe Nationale d'Electricite",
            displayName: "SNEL",
            active: true);

        var biller = BillerEntity.Create(
            partnerId: partner.Id,
            code: "SNEL",
            name: "SNEL");

        var billingService = BillingServiceEntity.Create(
            billingCategoryId: 1,
            billerId: biller.Id,
            isActive: true,
            code: "SNEL",
            name: "SNEL Electricity Billing",
            serviceTypeId: 1,
            supportPartial: false,
            supportInquiry: true,
            supportCustomActions: true,
            isTransactional: false,
            billingRefFormat: "11-43-513-777");

        var currency = new CurrencyEntity
        {
            Id = 1,
            Code = "CDF",
            Name = "Congolese Franc",
            Active = true
        };

        SetPrivateProperty(billingService, nameof(BillingServiceEntity.Biller), biller);
        SetPrivateProperty(biller, nameof(BillerEntity.Partner), partner);

        var subscription = CustomerSubscriptionEntity.Create(
            customerId: Guid.NewGuid(),
            billingServiceId: billingService.Id,
            customerRef: "CUST-001",
            billingRefValue: "REF-2",
            nickName: "nickName");
        SetPrivateProperty(subscription, nameof(CustomerSubscriptionEntity.BillingService), billingService);

        var paidInvoice = InvoiceEntity.Create(
            billingServiceId: billingService.Id,
            billingRef: "REF-1",
            originInvoiceId: null,
            invoiceRef: "INV-PAID-1",
            amountOutstanding: 0m,
            amountPaid: 100m,
            amountTotal: 100m,
            currencyId: currency.Id,
            issueDate: DateTimeOffset.UtcNow.AddDays(-7),
            dueDate: DateTimeOffset.UtcNow.AddDays(7),
            status: "Paid",
            invoiceInfo: "paid",
            invoiceUpdateDate: null);

        var unpaidInvoice = InvoiceEntity.Create(
            billingServiceId: billingService.Id,
            billingRef: "REF-2",
            originInvoiceId: null,
            invoiceRef: "INV-UNPAID-1",
            amountOutstanding: 60m,
            amountPaid: 40m,
            amountTotal: 100m,
            currencyId: currency.Id,
            issueDate: DateTimeOffset.UtcNow.AddDays(-2),
            dueDate: DateTimeOffset.UtcNow.AddDays(10),
            status: "PartiallyPaid",
            invoiceInfo: """{"CustomerName":"Oracle End Customer"}""",
            invoiceUpdateDate: null);

        var paidInvoiceSameRef = InvoiceEntity.Create(
            billingServiceId: billingService.Id,
            billingRef: "REF-2",
            originInvoiceId: null,
            invoiceRef: "INV-PAID-2",
            amountOutstanding: 0m,
            amountPaid: 80m,
            amountTotal: 80m,
            currencyId: currency.Id,
            issueDate: DateTimeOffset.UtcNow.AddDays(-1),
            dueDate: DateTimeOffset.UtcNow.AddDays(12),
            status: "Paid",
            invoiceInfo: "paid",
            invoiceUpdateDate: null);

        foreach (var invoice in new[] { paidInvoice, unpaidInvoice, paidInvoiceSameRef })
        {
            SetPrivateProperty(invoice, nameof(InvoiceEntity.Currency), currency);
            SetPrivateProperty(invoice, nameof(InvoiceEntity.BillingService), billingService);
        }

        var paymentOrderItemRepository = new Mock<IPaymentOrderItemRepository>();
        paymentOrderItemRepository
            .Setup(x => x.GetAmountTotalsByInvoicesAndTypeAndStatusAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<OrderItemType>(),
                It.IsAny<PaymentOrderItemStatus>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, decimal>());

        var unitOfWork = new FakeUnitOfWork(
            customerSubscriptionRepository: new FakeCustomerSubscriptionRepository(new[] { subscription }),
            billingServiceRepository: new FakeBillingServiceRepository(new[] { billingService }),
            invoiceRepository: new FakeInvoiceRepository(new[] { paidInvoice, unpaidInvoice, paidInvoiceSameRef }),
            customerFavoritesRepository: new FakeCustomerFavoritesRepository(),
            paymentOrderItemRepository: paymentOrderItemRepository.Object);

        var connector = new SnelBillerConnector(            
            mapper,
            unitOfWork,
            localizer,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<SnelBillerConnector>>(),
            Microsoft.Extensions.Options.Options.Create(new SNEL.WalletPlatform.Infrastructure.Configuration.SnelOracleLoggingOptions()),
            Mock.Of<ISnelConfigService>(),
            Mock.Of<ISnelOracleApplyCommandBuilder>(),
            new ConnectAfricaHttpClient(
                new HttpClient(),
                Microsoft.Extensions.Options.Options.Create(new Infrastructure.Configuration.ConnectAfricaOptions()),
                Microsoft.Extensions.Logging.Abstractions.NullLogger<ConnectAfricaHttpClient>.Instance),
            Mock.Of<Infrastructure.Oracle.IOracleSnelConnectionFactory>(),
            Mock.Of<IDistributedLockProvider>());

        var resolver = new BillerConnectorResolver(new[] { connector });
        var billingInvoiceRefResolver = new BillingInvoiceRefResolver(unitOfWork, localizer);
        var billingServiceAccessPolicy = new Mock<IBillingServiceAccessPolicy>();
        billingServiceAccessPolicy
            .Setup(x => x.GetBillingServiceManagementScopeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<long?>.Success(null));

        var service = new BillingServiceService(
            unitOfWork,
            resolver,
            billingInvoiceRefResolver,
            billingServiceAccessPolicy.Object,
            localizer,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<BillingServiceService>>(),
            mapper,
            Mock.Of<Quartz.ISchedulerFactory>(),
            Mock.Of<IDistributedLockProvider>());

        return new TestSetup(service, subscription.Id, billingService.Id);
    }

    private static void SetPrivateProperty<TTarget, TValue>(TTarget target, string propertyName, TValue value)
    {
        var property = typeof(TTarget).GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property is null)
        {
            throw new InvalidOperationException($"Property '{propertyName}' was not found on '{typeof(TTarget).Name}'.");
        }

        property.SetValue(target, value);
    }

    private sealed record TestSetup(
        BillingServiceService Service,
        Guid SubscriptionId,
        long BillingServiceId);
}

file sealed class PassthroughStringLocalizer<T> : IStringLocalizer<T>
{
    public LocalizedString this[string name] => new(name, name, resourceNotFound: true);

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var formatted = string.Format(CultureInfo.InvariantCulture, name, arguments);
            return new LocalizedString(name, formatted, resourceNotFound: true);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
}

file sealed class FakeUnitOfWork(
    ICustomerSubscriptionRepository customerSubscriptionRepository,
    IBillingServiceRepository billingServiceRepository,
    IInvoiceRepository invoiceRepository,
    ICustomerFavoritesRepository customerFavoritesRepository,
    IPaymentOrderItemRepository paymentOrderItemRepository) : IUnitOfWork
{
    public ICustomerRepository CustomerRepository => throw new NotImplementedException();
    public ICustomerSubscriptionRepository CustomerSubscriptionRepository => customerSubscriptionRepository;
    public ICustomerFavoritesRepository CustomerFavoritesRepository => customerFavoritesRepository;
    public IAreaRepository AreaRepository => throw new NotImplementedException();
    public IChannelRepository ChannelRepository => throw new NotImplementedException();
    public ILookupRepository LookupRepository => throw new NotImplementedException();
    public IBillerRepository BillerRepository => throw new NotImplementedException();
    public IBillerCustomerRepository BillerCustomerRepository => throw new NotImplementedException();
    public IBillerConfigRepository BillerConfigRepository => throw new NotImplementedException();
    public IBillingServiceRepository BillingServiceRepository => billingServiceRepository;
    public IBillingServiceCurrencyRepository BillingServiceCurrencyRepository => throw new NotImplementedException();
    public IInvoiceRepository InvoiceRepository => invoiceRepository;
    public IAccountTypeRepository AccountTypeRepository => throw new NotImplementedException();
    public IAccountRepository AccountRepository => throw new NotImplementedException();
    public IJournalTypeRepository JournalTypeRepository => throw new NotImplementedException();
    public IJournalRepository JournalRepository => throw new NotImplementedException();
    public IPlatformInvoiceRepository PlatformInvoiceRepository => throw new NotImplementedException();
    public IJournalLineTypeRepository JournalLineTypeRepository => throw new NotImplementedException();
    public IJournalLineRepository JournalLineRepository => throw new NotImplementedException();
    public IPartnerRepository PartnerRepository => throw new NotImplementedException();
    public IUserRepository UserRepository => throw new NotImplementedException();
    public IPaymentMethodRepository PaymentMethodRepository => throw new NotImplementedException();
    public IPaymentMethodCurrencyRepository PaymentMethodCurrencyRepository => throw new NotImplementedException();
    public IPaymentRepository PaymentRepository => throw new NotImplementedException();
    public IPaymentOrderRepository PaymentOrderRepository => throw new NotImplementedException();
    public IPaymentOrderItemRepository PaymentOrderItemRepository => paymentOrderItemRepository;
    public IPaymentProviderRepository PaymentProviderRepository => throw new NotImplementedException();
    public IPaymentProviderRouterRepository PaymentProviderRouterRepository => throw new NotImplementedException();
    public IPaymentTxnRepository PaymentTxnRepository => throw new NotImplementedException();
    public IPaymentCategoryRepository PaymentCategoryRepository=> throw new NotImplementedException();
    public IInterswitchWebhookEventRepository InterswitchWebhookEventRepository => throw new NotImplementedException();
    public IAkibaPayWebhookEventRepository AkibaPayWebhookEventRepository => throw new NotImplementedException();
    public IEasypayWebhookEventRepository EasypayWebhookEventRepository => throw new NotImplementedException();
    public IAfrimonyWebhookEventRepository AfrimonyWebhookEventRepository => throw new NotImplementedException();
    public IMpesaWebhookEventRepository MpesaWebhookEventRepository => throw new NotImplementedException();
    public IOrangeWebhookEventRepository OrangeWebhookEventRepository => throw new NotImplementedException();
    public ICurrencyRepository CurrencyRepository=> throw new NotImplementedException();
    public IOtpChallengeRepository OtpChallengeRepository => throw new NotImplementedException();
    public IAccountTypeTranslatableRepository AccountTypeTranslatableRepository => throw new NotImplementedException();
    public IGenderRepository GenderRepository => throw new NotImplementedException();
    public IGenderTranslatableRepository GenderTranslatableRepository => throw new NotImplementedException();
    public ICenterRepository CenterRepository => throw new NotImplementedException();

    public IPaymentControlRepository PaymentControlRepository => throw new NotImplementedException();

    public IPlatformConstantRepository PlatformConstantRepository => throw new NotImplementedException();
    public ICampaignRepository CampaignRepository => throw new NotImplementedException();

    public int Complete() => throw new NotImplementedException();
    public Task<int> CompleteAsync() => throw new NotImplementedException();
    public Task BeginTransactionAsync() => throw new NotImplementedException();
    public Task CommitTransactionAsync() => throw new NotImplementedException();
    public Task RollbackTransactionAsync() => throw new NotImplementedException();

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

file sealed class FakeCustomerFavoritesRepository : ICustomerFavoritesRepository
{
    public Task<bool> ExistsAsync(Guid customerId, int favoriteTypeId, Guid favoriteId) =>
        Task.FromResult(false);

    public Task<bool> FavoriteTypeExistsAsync(int favoriteTypeId) =>
        Task.FromResult(true);

    public Task<bool> SubscriptionExistsAsync(Guid subscriptionId) =>
        Task.FromResult(true);

    public Task<bool> SubscriptionBelongsToCustomerAsync(Guid customerId, Guid subscriptionId) =>
        Task.FromResult(true);

    public Task AddAsync(CustomerFavoritesEntity entity, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<int?> GetFavoriteTypeIdByCodeAsync(
        string code,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<int?>(code == "SUBSCRIPTION" ? 1 : null);

    public Task DeleteAsync(
        Guid customerId,
        int favoriteTypeId,
        Guid favoriteId,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<IReadOnlyCollection<Guid>> GetSubscriptionFavoriteIdsAsync(
        Guid customerId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Guid>>([]);

    public Task<IReadOnlyCollection<CustomerFavoritesByTypeInternalModel>> GetFavoritesGroupedByTypeAsync(
        Guid customerId,
        string languageCode,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<CustomerFavoritesByTypeInternalModel>>([]);
}

file sealed class FakeCustomerSubscriptionRepository(IEnumerable<CustomerSubscriptionEntity> subscriptions)
    : FakeEntityRepository<CustomerSubscriptionEntity, Guid>(subscriptions), ICustomerSubscriptionRepository
{
    public Task<CustomerSubscriptionEntity?> GetByIdWithRelationsAsync(Guid id) =>
        Task.FromResult(Items.FirstOrDefault(x => x.Id == id));

    public Task<CustomerSubscriptionEntity?> GetByIdForInquiryAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(x => x.Id == id));

    public Task<IReadOnlyCollection<CustomerSubscriptionEntity>> GetByIdsWithRelationsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<CustomerSubscriptionEntity>>(Items.Where(x => ids.Contains(x.Id)).ToList());

    public Task<List<CustomerSubscriptionEntity>> GetByCustomerIdAsync(Guid customerId) =>
        Task.FromResult(Items.Where(x => x.CustomerId == customerId).ToList());

    public Task<bool> ExistsAsync(Guid customerId, long billingServiceId, string billingRefValue)
    {
        var normalized = billingRefValue.Trim().ToLowerInvariant();
        return Task.FromResult(Items.Any(x =>
            x.CustomerId == customerId &&
            x.BillingServiceId == billingServiceId &&
            x.BillingRefValue.ToLowerInvariant() == normalized));
    }

    public Task<bool> ExistsAsync(Guid customerId, long billingServiceId, string billingRefValue, Guid[]? excludedIds = null)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<CustomerSubscriptionEntity>> GetPagedAsync(CustomerSubscriptionsQueryInternalModel query)
    {
        throw new NotImplementedException();
    }

    public Task<int> CountAsync(CustomerSubscriptionsQueryInternalModel query)
    {
        throw new NotImplementedException();
    }
}

file sealed class FakeBillingServiceRepository(IEnumerable<BillingServiceEntity> services)
    : FakeEntityRepository<BillingServiceEntity, long>(services), IBillingServiceRepository
{
    public Task<BillingServiceEntity?> GetByIdWithRelationsAsync(long id) =>
        Task.FromResult(Items.FirstOrDefault(x => x.Id == id));

    public Task<IReadOnlyCollection<BillingServiceEntity>> GetAllWithRelationsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<BillingServiceEntity>>(Items.ToList());

    public Task<BillingServiceEntity?> GetByBillerCodeAndBillingServiceCodeAsync(string billerCode, string billingServiceCode)
    {
        var normalizedBillerCode = billerCode.Trim().ToLowerInvariant();
        var normalizedServiceCode = billingServiceCode.Trim().ToLowerInvariant();
        return Task.FromResult(Items.FirstOrDefault(x =>
            x.Biller.Code.ToLowerInvariant() == normalizedBillerCode &&
            x.Code.ToLowerInvariant() == normalizedServiceCode));
    }

    public Task<bool> ExistsByCodeAsync(long billerId, string code)
    {
        var normalized = code.Trim().ToLowerInvariant();
        return Task.FromResult(Items.Any(x => x.BillerId == billerId && x.Code.ToLowerInvariant() == normalized));
    }

    public Task<BillingServiceEntity?> GetByBillerCodeAndBillingServiceCodeWithCurrenciesAsync(string billerCode, string billingServiceCode)
    {
        var normalizedBillerCode = billerCode.Trim().ToLowerInvariant();
        var normalizedServiceCode = billingServiceCode.Trim().ToLowerInvariant();
        return Task.FromResult(Items.FirstOrDefault(x =>
            x.Biller.Code.ToLowerInvariant() == normalizedBillerCode &&
            x.Code.ToLowerInvariant() == normalizedServiceCode));
    }

    public Task<bool> ExistsByCodeExceptIdAsync(long billerId, string code, long id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HasInvoicesAsync(long billingServiceId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HasCustomerSubscriptionsAsync(long billingServiceId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<BillingServiceEntity>> GetAllWithRelationsScopedByBillerPartnerAsync(
        long? billerPartnerId,
        bool? isActive = true,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
