using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Webhooks.Models.Input;

public sealed class AfrimonyWebhookEventInputModel
{
    [JsonPropertyName("MMTransactionID")]
    public string MmTransactionId { get; init; } = string.Empty;

    [JsonPropertyName("txnStatus")]
    public string TxnStatus { get; init; } = string.Empty;

    [JsonPropertyName("Amount")]
    [JsonConverter(typeof(FlexibleDecimalConverter))]
    public decimal Amount { get; init; }

    [JsonPropertyName("externalId")]
    public string ExternalId { get; init; } = string.Empty;

    [JsonPropertyName("SenderMSISDN")]
    public string? SenderMsisdn { get; init; }
}

internal sealed class FlexibleDecimalConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetDecimal(),
            JsonTokenType.String => decimal.TryParse(
                reader.GetString(),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var parsed)
                ? parsed
                : 0m,
            _ => 0m
        };
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value);
}
