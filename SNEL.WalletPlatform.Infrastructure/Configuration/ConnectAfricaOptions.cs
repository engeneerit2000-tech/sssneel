namespace SNEL.WalletPlatform.Infrastructure.Configuration;

public sealed class ConnectAfricaOptions
{
    public const string SectionName = "ConnectAfrica";

    public string BaseUrl { get; set; } = string.Empty;
    public string CheckEndpointPath { get; set; } = "api/transactions/check";
    public string VendEndpointPath { get; set; } = "api/transactions/vend";
    public string AckEndpointPath { get; set; } = "api/transactions/ack";
    public string EosEndpointPath { get; set; } = "api/transactions/eos";
    public string ReprintEndpointPath { get; set; } = "api/transactions/reprint";
    public string ListEndpointPath { get; set; } = "api/transactions/list";
    public string LastTransactionsEndpointPath { get; set; } = "api/transactions/last_transactions";
    public string LastTransactionStatusEndpointPath { get; set; } = "api/transactions/last_transaction_status";
    public string ValidationCode { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? DefaultUnit { get; set; }
}
