namespace SNEL.WalletPlatform.Application.Payments.Models.External.AkibaPay;

public sealed class AkibaPayTransactionStatusResponseModel
{
    public string? TranId { get; set; }

    public string? MerchantReference { get; set; }

    public string? MerchantId { get; set; }

    public decimal Amount { get; set; }

    public string? Currency { get; set; }

    public string? TransactionStatus { get; set; }

    public string? AkibaReference { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
