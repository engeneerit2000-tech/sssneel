namespace SNEL.WalletPlatform.Application.Webhooks.Models.Input;

public class InterswitchWebhookEventDataInputModel
{
    public decimal RemittanceAmount { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public List<object> SplitAccounts { get; set; } = new();
    public string RetrievalReferenceNumber { get; set; } = string.Empty;
    public long TransactionDate { get; set; }
    public string? AccountNumber { get; set; }
    public string ResponseCode { get; set; } = string.Empty;
    public string? Token { get; set; }
    public string ResponseDescription { get; set; } = string.Empty;
    public long PaymentId { get; set; }
    public string MerchantCustomerId { get; set; } = string.Empty;
    public bool Escrow { get; set; }
    public string MerchantReference { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public string MerchantCustomerName { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
}