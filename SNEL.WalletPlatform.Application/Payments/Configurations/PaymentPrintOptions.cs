namespace SNEL.WalletPlatform.Application.Payments.Configurations;

public sealed class PaymentPrintOptions
{
    public const string SectionName = "PaymentPrint";

    /// <summary>
    /// When false, tax and fees are omitted from the payment receipt PDF (summary line and item columns).
    /// </summary>
    public bool ShowTaxAndFees { get; set; } = true;

    /// <summary>Prefix for the virtual cash desk on digital receipts (SNEL format: "00").</summary>
    public string VirtualCashRegisterPrefix { get; set; } = "00";

    /// <summary>Virtual cash desk code assigned to the wallet platform (SNEL format: "9999").</summary>
    public string VirtualCashRegisterCode { get; set; } = "9999";

    public const string DefaultOperatorDisplayName = "Snel Wallet";
}
