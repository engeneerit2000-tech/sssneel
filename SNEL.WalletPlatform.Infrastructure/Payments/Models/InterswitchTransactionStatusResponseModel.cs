using System.Text.Json;
using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Infrastructure.Payments.Models;

public sealed record InterswitchTransactionStatusResponseModel(
    [property: JsonPropertyName("Amount")] decimal Amount,
    [property: JsonPropertyName("CardNumber")] string CardNumber,
    [property: JsonPropertyName("MerchantReference")] string MerchantReference,
    [property: JsonPropertyName("PaymentReference")] string PaymentReference,
    [property: JsonPropertyName("RetrievalReferenceNumber")] string RetrievalReferenceNumber,
    [property: JsonPropertyName("Stan")] string Stan,
    [property: JsonPropertyName("Channel")] string Channel,
    [property: JsonPropertyName("TerminalId")] string TerminalId,
    [property: JsonPropertyName("SplitAccounts")] IReadOnlyList<JsonElement> SplitAccounts,
    [property: JsonPropertyName("TransactionDate")] DateTime TransactionDate,
    [property: JsonPropertyName("ResponseCode")] string ResponseCode,
    [property: JsonPropertyName("ResponseDescription")] string ResponseDescription,
    [property: JsonPropertyName("BankCode")] string BankCode,
    [property: JsonPropertyName("PaymentId")] long PaymentId,
    [property: JsonPropertyName("RemittanceAmount")] decimal RemittanceAmount,
    [property: JsonPropertyName("PaymentCancelled")] bool PaymentCancelled);
