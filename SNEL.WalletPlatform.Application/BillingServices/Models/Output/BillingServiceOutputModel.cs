using SNEL.WalletPlatform.Application.Billers.Models.Output;
using SNEL.WalletPlatform.Domain.Billing;

namespace SNEL.WalletPlatform.Application.BillingServices.Models.Output;

public sealed record BillingServiceOutputModel
{
    public BillingServiceOutputModel() { }

    public BillingServiceOutputModel(
        long Id,
        int BillingCategoryId,
        BillingCategoryOutputModel? BillingCategory,
        long BillerId,
        BillerOutputModel? Biller,
        bool IsActive,
        string Code,
        string Name,
        long ServiceTypeId,
        BillingServiceTypeOutputModel? ServiceType,
        bool SupportPartial,
        bool SupportInquiry,
        bool SupportCustomActions,
        string? BillingRefFormat,
        IReadOnlyCollection<BillingServiceCurrencyOutputModel>? Currencies,
        IReadOnlyCollection<BillerConfigOutputModel>? BillerConfigs,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc,
        DateTimeOffset? DeletedAtUtc,
        string CreatedBy,
        string UpdatedBy,
        string? DeletedBy)
    {
        this.Id = Id;
        this.BillingCategoryId = BillingCategoryId;
        this.BillingCategory = BillingCategory;
        this.BillerId = BillerId;
        this.Biller = Biller;
        this.IsActive = IsActive;
        this.Code = Code;
        this.Name = Name;
        this.ServiceTypeId = ServiceTypeId;
        this.ServiceType = ServiceType;
        this.SupportPartial = SupportPartial;
        this.SupportInquiry = SupportInquiry;
        this.SupportCustomActions = SupportCustomActions;
        this.BillingRefFormat = BillingRefFormat;
        this.Currencies = Currencies;
        this.BillerConfigs = BillerConfigs;
        this.CreatedAtUtc = CreatedAtUtc;
        this.UpdatedAtUtc = UpdatedAtUtc;
        this.DeletedAtUtc = DeletedAtUtc;
        this.CreatedBy = CreatedBy;
        this.UpdatedBy = UpdatedBy;
        this.DeletedBy = DeletedBy;
    }

    public long Id { get; init; }
    public int BillingCategoryId { get; init; }
    public BillingCategoryOutputModel? BillingCategory { get; init; }
    public long BillerId { get; init; }
    public BillerOutputModel? Biller { get; init; }
    public bool IsActive { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public long ServiceTypeId { get; init; }
    public BillingServiceTypeOutputModel? ServiceType { get; init; }
    public bool SupportPartial { get; init; }
    public bool SupportInquiry { get; init; }
    public bool SupportCustomActions { get; init; }
    public string? BillingRefFormat { get; init; }
    public IReadOnlyCollection<BillingServiceCurrencyOutputModel>? Currencies { get; init; }
    public IReadOnlyCollection<BillerConfigOutputModel>? BillerConfigs { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset UpdatedAtUtc { get; init; }
    public DateTimeOffset? DeletedAtUtc { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public string UpdatedBy { get; init; } = string.Empty;
    public string? DeletedBy { get; init; }
    
}

public sealed record BillerConfigOutputModel(
    long Id,
    string Name,
    string BillerName,
    string Value);