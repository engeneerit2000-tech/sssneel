namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

public sealed record PaymentSummaryOutputModel()
{
    public decimal Amount { get; set; }
    public decimal Fees { get; set; }
    public decimal FeesTax { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PaymentSummaryItemModel> Items { get; set; }

    public string CurrencyAbbreviation { get; set; } = string.Empty;

    public string? PaymentServiceProviderCode { get; set; }

    public string? PaymentMethodCode { get; set; }
    public bool? RequirePhoneNumber { get; set; }
}
public sealed record PaymentSummaryItemModel()
{
    public decimal Amount { get; set; }
    public string Reference { get; set; }
    public DateTimeOffset Date { get; set; }
}