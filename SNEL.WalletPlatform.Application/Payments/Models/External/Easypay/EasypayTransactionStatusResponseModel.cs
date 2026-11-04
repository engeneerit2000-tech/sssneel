using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Payments.Models.External.Easypay;

public sealed class EasypayTransactionStatusResponseModel
{
    [JsonPropertyName("success")]
    public int Success { get; set; }

    [JsonPropertyName("errormsg")]
    public string? ErrorMsg { get; set; }

    [JsonPropertyName("errorcode")]
    public int? ErrorCode { get; set; }

    [JsonPropertyName("data")]
    public EasypayTransactionStatusReponseDataModel? Data { get; set; }    
}

public sealed class EasypayTransactionStatusReponseDataModel
{
    [JsonPropertyName("transactionId")]
    public string? TransactionId { get; set; }
    [JsonPropertyName("referenceId")]
    public string? ReferenceId { get; set; }
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }
    [JsonPropertyName("amount")]
    public string? Amount { get; set; }
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
    [JsonPropertyName("date")]
    public string? Date { get; set; }
    [JsonPropertyName("telecomId")]
    public string? TelecomId { get; set; }
    [JsonPropertyName("provider")]
    public string? Provider { get; set; }
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("paymentUrl")]
    public string? PaymentUrl { get; set; }

    [JsonPropertyName("expiresAt")]
    public DateTimeOffset? ExpiresAt { get; set; }
}

public class CustomDateTimeConverter : JsonConverter<DateTime?>
{
    private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var dateString = reader.GetString();
        if (string.IsNullOrEmpty(dateString))
            return null;

        if (DateTime.TryParseExact(dateString, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date;

        // Fallback to standard parsing
        return DateTime.TryParse(dateString, out date) ? date : null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.Value.ToString(DateFormat));
        else
            writer.WriteNullValue();
    }
}
