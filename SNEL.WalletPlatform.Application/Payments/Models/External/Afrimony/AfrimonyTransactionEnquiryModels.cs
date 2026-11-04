using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Payments.Models.External.Afrimony;

public sealed class AfrimonyTransactionEnquiryRequestModel
{
    [JsonPropertyName("COMMAND")]
    public AfrimonyTransactionEnquiryCommandRequest Command { get; init; } = new();
}

public sealed class AfrimonyTransactionEnquiryCommandRequest
{
    [JsonPropertyName("TYPE")]
    public string Type { get; init; } = "TRANSREQ";

    [JsonPropertyName("REFERENCEID")]
    public string ReferenceId { get; init; } = string.Empty;
}

public sealed class AfrimonyTransactionEnquiryResponseModel
{
    [JsonPropertyName("COMMAND")]
    public AfrimonyTransactionEnquiryCommandResponse? Command { get; set; }
}

public sealed class AfrimonyTransactionEnquiryCommandResponse
{
    [JsonPropertyName("FTXNID")]
    public string? FtxnId { get; set; }

    [JsonPropertyName("STATUSCODE")]
    public string? StatusCode { get; set; }

    [JsonPropertyName("STATUSVALUE")]
    public string? StatusValue { get; set; }

    [JsonPropertyName("REFTXNSTATE")]
    public string? RefTxnState { get; set; }

    [JsonPropertyName("REFTXNSTATUS")]
    public string? RefTxnStatus { get; set; }

    [JsonPropertyName("TXNSTATUS")]
    public string? TxnStatus { get; set; }

    [JsonPropertyName("MESSAGE")]
    public string? Message { get; set; }

    [JsonPropertyName("REFMESSAGE")]
    public string? RefMessage { get; set; }

    [JsonPropertyName("TRANSID")]
    public string? TransId { get; set; }

    [JsonPropertyName("TXNID")]
    public string? TxnId { get; set; }

    [JsonPropertyName("TXNAMT")]
    public string? TxnAmt { get; set; }

    [JsonPropertyName("FROM")]
    public string? From { get; set; }

    [JsonPropertyName("TO")]
    public string? To { get; set; }

    [JsonPropertyName("TYPE")]
    public string? Type { get; set; }

    [JsonPropertyName("TRID")]
    public string? TrId { get; set; }

    [JsonPropertyName("TXNDATE")]
    public string? TxnDate { get; set; }
}
