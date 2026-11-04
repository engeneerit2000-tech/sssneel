using System.Text.Json.Serialization;
using SNEL.WalletPlatform.Application.BillingServices.Models.Output;

namespace SNEL.WalletPlatform.Application.Customers.Models.Output;

public sealed class CustomerSubscriptionOutputModel
{
    public CustomerSubscriptionOutputModel() { }

    public CustomerSubscriptionOutputModel(
        Guid id,
        Guid customerId,
        CustomerOutputModel? customer,
        long billingServiceId,
        BillingServiceOutputModel? billingService,
        string customerRef,
        string billingRefValue,
        string billingRefForDisplay,
        string nickName,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        string createdBy,
        string updatedBy)
    {
        Id = id;
        CustomerId = customerId;
        Customer = customer;
        BillingServiceId = billingServiceId;
        BillingService = billingService;
        CustomerRef = customerRef;
        BillingRefValue = billingRefValue;
        BillingRefForDisplay = billingRefForDisplay;
        NickName = nickName;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        CreatedBy = createdBy;
        UpdatedBy = updatedBy;
    }

    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public CustomerOutputModel? Customer { get; set; }
    public long BillingServiceId { get; set; }
    public BillingServiceOutputModel? BillingService { get; set; }
    public string CustomerRef { get; set; } = string.Empty;
    public string BillingRefValue { get; set; } = string.Empty;
    public string BillingRefForDisplay { get; set; } = string.Empty;
    public string NickName { get; set; } = string.Empty;

    [JsonPropertyName("is_favorite")]
    public bool IsFavorite { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public string? DeletedBy { get; set; }
}