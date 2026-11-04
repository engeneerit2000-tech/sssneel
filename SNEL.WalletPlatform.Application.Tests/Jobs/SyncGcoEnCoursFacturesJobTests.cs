using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Quartz;
using SNEL.WalletPlatform.Application.Invoicing;
using SNEL.WalletPlatform.Application.Invoicing.Interfaces;
using SNEL.WalletPlatform.Application.Invoicing.Models;
using SNEL.WalletPlatform.Application.Invoicing.Mapper;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Partners;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Infrastructure.Persistence.UnitOfWork;
using SNEL.WalletPlatform.Infrastructure.Invoicing;
using SNEL.WalletPlatform.Infrastructure.Jobs;
using SNEL.WalletPlatform.Infrastructure.Persistence;
using Xunit;
using SNEL.WalletPlatform.Application.Common.Enums;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.BillingServices.Models.Internal;

namespace SNEL.WalletPlatform.Application.Tests.Jobs;

/// <summary>
/// Unit tests for SyncGcoEnCoursFacturesJob. Uses IInvoiceRetrieval abstraction so we can run full flow (staging + secondary staging + clear)
/// without a real Oracle DB.
/// </summary>
public sealed class SyncGcoEnCoursFacturesJobTests
{
    [Fact]
    public async Task Execute_WhenPullInvoicesQueryThrows_PropagatesException()
    {
        var factory = new Mock<IInvoiceRetrieval>();
        factory.Setup(f => f.GetAllAsync(
                "06-03",
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ApplicationDbContext>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Fake query failure."));

        var job = CreateJob(factory.Object);
        var context = CreateJobContext();

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await job.Execute(context));
    }

    [Fact]
    public async Task Execute_WhenNoRowsReturned_StopsWithoutWritingStaging()
    {
        var retrieval = new FakeInvoiceRetrieval(Array.Empty<OracleInvoiceRowModel>());
        var (job, provider) = CreateJobAndServices(retrieval);

        var context = CreateJobContext();

        await job.Execute(context);

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.Equal(0, await db.InvoiceStagings.CountAsync());
            Assert.Equal(0, await db.InvoiceSecondaryStagings.CountAsync());
            Assert.Equal(0, await db.Invoices.CountAsync());
        }
    }

    [Fact]
    public async Task Execute_WhenOneRowReturned_StoresStaging_RunsSync_ClearsStaging_WritesSecondaryStaging_NewInvoice()
    {
        var rows = new[] { CreateSampleOracleRow("FAC-TEST-1") };
        var retrieval = new FakeInvoiceRetrieval(rows);
        var (job, provider) = CreateJobAndServices(retrieval);

        var context = CreateJobContext();

        await job.Execute(context);

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var stagingCount = await db.InvoiceStagings.CountAsync();
            Assert.Equal(0, stagingCount);
            Assert.Equal(0, await db.InvoiceSecondaryStagings.CountAsync());
            var invoice = await db.Invoices.SingleAsync(i => i.InvoiceRef == "FAC-TEST-1");
            Assert.Equal(50, invoice.AmountTotal);
            var currencies = await db.Currencies.AsNoTracking().ToListAsync();
            var currencyByAbbreviationOrCode = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in currencies)
            {
                if (!string.IsNullOrEmpty(c.Abbreviation))
                    currencyByAbbreviationOrCode[c.Abbreviation] = c.Id;
                if (!string.IsNullOrEmpty(c.Code))
                    currencyByAbbreviationOrCode[c.Code] = c.Id;
            }

            Assert.True(currencyByAbbreviationOrCode.TryGetValue("USD", out var expectedUsdCurrencyId));
            Assert.Equal(expectedUsdCurrencyId, invoice.CurrencyId);
        }
    }

    [Fact]
    public async Task Execute_WhenMultipleRowsReturned_StoresAll_RunsSync_ClearsStaging_WritesSecondaryStaging()
    {
        var rows = new[]
        {
            CreateSampleOracleRow("FAC-A"),
            CreateSampleOracleRow("FAC-B"),
            CreateSampleOracleRow("FAC-C")
        };
        var retrieval = new FakeInvoiceRetrieval(rows);
        var (job, provider) = CreateJobAndServices(retrieval);

        var context = CreateJobContext();

        await job.Execute(context);

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var stagingCount = await db.InvoiceStagings.CountAsync();
            Assert.Equal(0, stagingCount);
            Assert.Equal(0, await db.InvoiceSecondaryStagings.CountAsync());
            Assert.Equal(3, await db.Invoices.CountAsync());
        }
    }

    private static SyncGcoEnCoursFacturesJob CreateJob(IInvoiceRetrieval invoiceRetrieval)
    {
        var (job, _) = CreateJobAndServices(invoiceRetrieval);
        return job;
    }

    private static (SyncGcoEnCoursFacturesJob Job, IServiceProvider ServiceProvider) CreateJobAndServices(IInvoiceRetrieval invoiceRetrieval)
    {
        var services = new ServiceCollection();
        var dbName = Guid.NewGuid().ToString();
        services.AddDbContext<ApplicationDbContext>(opts => opts.UseInMemoryDatabase(dbName));
        services.AddScoped<ICurrentUserContext, TestCurrentUserContext>();
        services.AddScoped<IInvoiceRetrieval>(_ => invoiceRetrieval);
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IInvoiceStagingSyncService, InvoiceStagingSyncService>();
        var configServiceMock = new Mock<ISnelConfigService>();
        configServiceMock
            .Setup(x => x.GetAllRequiredConfigAsync(
                It.IsAny<string[]>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, SnelConfigModel>
            {
                ["06-03"] = new("9999", 9999, 1, 1, "8")
            });
        services.AddSingleton(configServiceMock.Object);
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<InvoiceMapper>());
        services.AddSingleton<IMapper>(mapperConfig.CreateMapper());
        services.AddLogging(b => b.ClearProviders());

        var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
            SeedJobTestData(db);
        }

        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var snelConfigService = provider.GetRequiredService<ISnelConfigService>();
        var mapper = provider.GetRequiredService<IMapper>();
        var logger = provider.GetRequiredService<ILogger<SyncGcoEnCoursFacturesJob>>();

        var job = new SyncGcoEnCoursFacturesJob(invoiceRetrieval, scopeFactory, mapper, snelConfigService, logger);
        return (job, provider);
    }

    /// <summary>
    /// Seeds minimal data so the job's "insert new invoices" path can run: Currency (USD) and BillingService (Id=Guid.Empty) with required lookups.
    /// </summary>
    private static void SeedJobTestData(ApplicationDbContext db)
    {
        // Some test environments may already have seeded currencies/lookups; ensure the required Oracle-sync BillingService exists.

        if (!db.PartnerTypes.Any())
            db.PartnerTypes.Add(new PartnerTypeEntity { Id = 1, Code = "TEST", Name = "Test", Active = true });
        if (!db.BillingCategories.Any())
            db.BillingCategories.Add(new BillingCategoryEntity { Id = 1, Code = "TEST", Name = "Test", Active = true });
        if (!db.BillingServiceTypes.Any())
            db.BillingServiceTypes.Add(new BillingServiceTypeEntity { Id = 1, Code = "TEST", Name = "Test", Active = true });
        if (!db.Currencies.Any(c => c.Id == 1))
            db.Currencies.Add(new CurrencyEntity { Id = 1, Code = "USD", Name = "US Dollar", Active = true });
        db.SaveChanges();

        var partner = db.Partners.FirstOrDefault(p => p.Id == 1) ?? PartnerEntity.Create(1, "Test Partner", "Test", true);
        if (db.Entry(partner).State == EntityState.Detached)
            db.Partners.Add(partner);
        db.SaveChanges();

        var snelBiller = db.Billers.FirstOrDefault(b => b.Code == "SNEL");
        if (snelBiller == null)
        {
            snelBiller = BillerEntity.Create(partner.Id, "SNEL", "SNEL");
            db.Billers.Add(snelBiller);
            db.SaveChanges();
        }

        if (!db.BillingServices.Any(bs => bs.BillerId == snelBiller.Id && bs.Code == "SNEL"))
        {
            var snelBillingService = BillingServiceEntity.Create(
                billingCategoryId: 1,
                billerId: snelBiller.Id,
                isActive: true,
                code: "SNEL",
                name: "SNEL",
                serviceTypeId: 1,
                supportPartial: true,
                supportInquiry: false,
                supportCustomActions: false,
                isTransactional: false,
                billingRefFormat: null);
            db.BillingServices.Add(snelBillingService);
        }

        db.SaveChanges();
    }

    private static IJobExecutionContext CreateJobContext()
    {
        var mock = new Mock<IJobExecutionContext>();
        var map = new JobDataMap
        {
            { "PageSize", 100 }
        };
        mock.Setup(c => c.MergedJobDataMap).Returns(map);
        mock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
        var jobDetail = new Mock<IJobDetail>();
        jobDetail.Setup(d => d.Key).Returns(new JobKey("SyncGcoEnCoursFacturesJob", "default"));
        mock.Setup(c => c.JobDetail).Returns(jobDetail.Object);
        return mock.Object;
    }

    private static OracleInvoiceRowModel CreateSampleOracleRow(string invoiceNum)
    {
        return new OracleInvoiceRowModel
        {
            InvoiceId = 12345,
            InvoiceNum = invoiceNum,
            InvoiceDate = new DateTime(2022, 6, 29, 0, 0, 0, DateTimeKind.Utc),
            InvoiceUpdateDate = new DateTime(2022, 6, 29, 12, 0, 0, DateTimeKind.Utc),
            CustomerId = 100,
            CustomerNum = "CUST-001",
            CustomerName = "Test Customer",
            CustomerTypeCode = "RES",
            CustomerTypeName = "Residential",
            TariffType = "STD",
            BillingPeriod = 202206,
            Consumption = 100,
            BilledConsumption = 100,
            InvoiceCurrencyCode = "USD",
            AmountUsd = 50,
            AmountFc = null,
            PaidUsd = 0,
            PaidFc = null,
            PaymentUpdateDate = null,
            Vat = 5,
            ProvincialTax = 2
        };
    }

    private sealed class FakeInvoiceRetrieval : IInvoiceRetrieval
    {
        private readonly IReadOnlyList<OracleInvoiceRowModel> _rows;

        public FakeInvoiceRetrieval(IReadOnlyList<OracleInvoiceRowModel> rows)
        {
            _rows = rows;
        }

        public Task GetPageAsync(DateTime startTs, DateTime endTs, int pageIndex, int pageSize, Func<IReadOnlyList<OracleInvoiceRowModel>, CancellationToken, Task> onPageAsync, CancellationToken cancellationToken = default)
        {
            if (pageIndex == 1 && _rows.Count > 0)
                return onPageAsync(_rows, cancellationToken);
            return Task.CompletedTask;
        }

        public async Task<long> GetAllAsync(
            DateTime startTs,
            DateTime endTs,
            int pageSize,
            string jobKey,
            string sourceSystem,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken = default)
        {
            if (_rows.Count == 0)
                return 0;
            var stagingRows = _rows.Select(x => InvoiceStagingEntity.FromOracleRow(
                jobKey, sourceSystem,
                x.InvoiceId, x.InvoiceNum, x.InvoiceDate, x.InvoiceUpdateDate,
                x.RegionCode, x.DistrictCode, x.CustomerId, x.CustomerNum, x.CustomerName, x.CustomerTypeCode, x.CustomerTypeName,
                x.TariffType, x.BillingPeriod, x.Consumption, x.BilledConsumption,
                x.InvoiceCurrencyCode, x.AmountUsd, x.AmountFc, x.PaidUsd, x.PaidFc,
                x.PaymentUpdateDate, x.Vat, x.ProvincialTax,
                x.CenterDesignation, x.Pa, x.PointOfSaleName, x.SubstationCode,
                x.ClientPropertyNumber, x.ClientStreet, x.ClientCommune,
                x.MeterNumber, x.CalculationCoefficient, x.PreviousBillingIndex, x.CurrentBillingIndex,
                x.DiscountCode, x.AmountExcludingTaxUsd, x.AmountExcludingTaxFc,
                x.SubtotalUsd, x.SubtotalFc, x.BillingYear, x.BillingMonth))
                .ToList();
            dbContext.InvoiceStagings.AddRange(stagingRows);
            await dbContext.SaveChangesAsync(cancellationToken);
            return _rows.Count;
        }

        public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(true);

        public Task GetPageAsync(string areaCode, DateTime startTs, DateTime endTs, int pageIndex, int pageSize, Func<IReadOnlyList<OracleInvoiceRowModel>, CancellationToken, Task> onPageAsync, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<long> GetAllAsync(string areaCode, DateTime startTs, DateTime endTs, int pageSize, string jobKey, string sourceSystem, ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TestConnectionAsync(string areaCode, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class TestCurrentUserContext : ICurrentUserContext
    {
        public bool IsAuthenticated => false;
        public string? UserId => null;
        public string? UserName => null;
        public string LanguageCode { get; } = "en";
        public IReadOnlyCollection<string> Roles => Array.Empty<string>();
        public bool IsInRole(string role) => false;
        public LanguageEnum PreferredLanguage => LanguageEnum.FR;

    }
}
