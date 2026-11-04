using System.Text.Json.Serialization;
using SNEL.WalletPlatform.Application.BillingServices.Models.Output;

namespace SNEL.WalletPlatform.Application.Customers.Models.Output;

public sealed class CustomerSubscriptionInfoOutputModel
{
    public CustomerSubscriptionInfoOutputModel() { }

    public CustomerSubscriptionInfoOutputModel(
        string customerName,
        string customerNumber, 
        string? message)
    {
        CustomerName = customerName;
        CustomerNumber = customerNumber;
        Message = message;
    }

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerNumber { get; set; } = string.Empty;
    public string? Message { get; set; } = string.Empty;
}