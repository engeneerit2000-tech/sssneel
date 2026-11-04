using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Domain.Billing;

public sealed class BillingServiceEntity : TrackedEntity<long>, ITranslatableEntity<BillingServiceTranslationEntity, long>
{
    private BillingServiceEntity() { }

    public int BillingCategoryId { get; private set; }
    public BillingCategoryEntity BillingCategory { get; private set; } = null!;
    public long BillerId { get; private set; }
    public BillerEntity Biller { get; private set; } = null!;
    public bool Active { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public int ServiceTypeId { get; private set; }
    public BillingServiceTypeEntity ServiceType { get; private set; } = null!;
    public bool SupportPartial { get; private set; }
    public bool SupportInquiry { get; private set; }
    public bool SupportCustomActions { get; private set; }
    public bool IsTransactional { get; private set; }
    public string? BillingRefFormat { get; private set; }
    public ICollection<BillingServiceCurrencyEntity> Currencies { get; private set; } = new List<BillingServiceCurrencyEntity>();
    public ICollection<CustomerSubscriptionEntity> CustomerSubscriptions { get; private set; } = new List<CustomerSubscriptionEntity>();
    public ICollection<InvoiceEntity> Invoices { get; private set; } = new List<InvoiceEntity>();

    public ICollection<BillingServiceTranslationEntity> Translations { get; set; } =
        new List<BillingServiceTranslationEntity>();

    public static BillingServiceEntity Create(
        int billingCategoryId,
        long billerId,
        bool isActive,
        string code,
        string name,
        int serviceTypeId,
        bool supportPartial,
        bool supportInquiry,
        bool supportCustomActions,
        bool isTransactional,
        string? billingRefFormat)
    {
        return new BillingServiceEntity
        {
            BillingCategoryId = billingCategoryId,
            BillerId = billerId,
            Active = isActive,
            Code = code.Trim(),
            Name = name.Trim(),
            ServiceTypeId = serviceTypeId,
            SupportPartial = supportPartial,
            SupportInquiry = supportInquiry,
            SupportCustomActions = supportCustomActions,
            IsTransactional = isTransactional,
            BillingRefFormat = string.IsNullOrWhiteSpace(billingRefFormat) ? null : billingRefFormat.Trim()
        };
    }

    public void Update(
        int billingCategoryId,
        bool isActive,
        string code,
        string name,
        int serviceTypeId,
        bool supportPartial,
        bool supportInquiry,
        bool supportCustomActions,
        bool isTransactional,
        string? billingRefFormat)
    {
        BillingCategoryId = billingCategoryId;
        Active = isActive;
        Code = code.Trim();
        Name = name.Trim();
        ServiceTypeId = serviceTypeId;
        SupportPartial = supportPartial;
        SupportInquiry = supportInquiry;
        SupportCustomActions = supportCustomActions;
        IsTransactional = isTransactional;
        BillingRefFormat = string.IsNullOrWhiteSpace(billingRefFormat) ? null : billingRefFormat.Trim();
    }
    
}
