using System.Text.Json;
using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Invoicing.Models;

/// <summary>
/// One payment entry from InvoiceStagings.PaymentsJson (Oracle GCO_REGLEMENTS).
/// </summary>
public sealed class StagingPaymentItem
{
    [JsonPropertyName("payment_id")]
    public long? PaymentId { get; init; }

    [JsonPropertyName("paid_at")]
    public string? PaidAt { get; init; }

    [JsonPropertyName("amount_usd")]
    public decimal? AmountUsd { get; init; }

    [JsonPropertyName("amount_fc")]
    public decimal? AmountFc { get; init; }

    [JsonPropertyName("mode")]
    public string? Mode { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>
    /// Parses PaymentsJson from InvoiceStagings into a list of payment items.
    /// Returns an empty list if json is null, empty, or invalid.
    /// </summary>
    public static IReadOnlyList<StagingPaymentItem> ParsePayments(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<StagingPaymentItem>();

        try
        {
            var list = JsonSerializer.Deserialize<List<StagingPaymentItem>>(json);
            if (list == null || list.Count == 0)
                return Array.Empty<StagingPaymentItem>();
            return list;
        }
        catch
        {
            return Array.Empty<StagingPaymentItem>();
        }
    }
}
