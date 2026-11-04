using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Invoicing.Models;

/// <summary>
/// Oracle invoice details stored in <see cref="Domain.Billing.InvoiceEntity.InvoiceInfo"/>.
/// Excludes fields that are persisted as dedicated invoice columns.
/// JSON uses short keys; <see cref="Parse"/> also accepts legacy long keys.
/// </summary>
public sealed class InvoiceInfoModel
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [JsonPropertyName("cn")]
    public string? CustomerNum { get; init; }

    [JsonPropertyName("cnm")]
    public string? CustomerName { get; init; }

    [JsonPropertyName("ctn")]
    public string? CustomerTypeName { get; init; }

    [JsonPropertyName("tt")]
    public string? TariffType { get; init; }

    [JsonPropertyName("cons")]
    public decimal? Consumption { get; init; }

    [JsonPropertyName("bcons")]
    public decimal? BilledConsumption { get; init; }

    /// <summary>Alternate-currency total when the invoice column amount is in the other currency.</summary>
    [JsonPropertyName("ausd")]
    public decimal? AmountUsd { get; init; }

    /// <summary>Alternate-currency total when the invoice column amount is in the other currency.</summary>
    [JsonPropertyName("acdf")]
    public decimal? AmountFc { get; init; }

    [JsonPropertyName("vat")]
    public decimal? Vat { get; init; }

    [JsonPropertyName("ptax")]
    public decimal? ProvincialTax { get; init; }

    [JsonPropertyName("ctr")]
    public string? CenterDesignation { get; init; }

    [JsonPropertyName("pa")]
    public string? Pa { get; init; }

    [JsonPropertyName("pos")]
    public string? PointOfSaleName { get; init; }

    [JsonPropertyName("ss")]
    public string? SubstationCode { get; init; }

    [JsonPropertyName("cpn")]
    public string? ClientPropertyNumber { get; init; }

    [JsonPropertyName("cst")]
    public string? ClientStreet { get; init; }

    [JsonPropertyName("ccm")]
    public string? ClientCommune { get; init; }

    [JsonPropertyName("mn")]
    public decimal? MeterNumber { get; init; }

    [JsonPropertyName("cc")]
    public decimal? CalculationCoefficient { get; init; }

    [JsonPropertyName("pbi")]
    public decimal? PreviousBillingIndex { get; init; }

    [JsonPropertyName("cbi")]
    public decimal? CurrentBillingIndex { get; init; }

    [JsonPropertyName("dc")]
    public decimal? DiscountCode { get; init; }

    [JsonPropertyName("aetusd")]
    public decimal? AmountExcludingTaxUsd { get; init; }

    [JsonPropertyName("aetcdf")]
    public decimal? AmountExcludingTaxFc { get; init; }

    [JsonPropertyName("stcdf")]
    public decimal? SubtotalFc { get; init; }

    public static InvoiceInfoModel FromOracleRow(OracleInvoiceRowModel row) =>
        new()
        {
            CustomerNum = row.CustomerNum,
            CustomerName = row.CustomerName,
            CustomerTypeName = row.CustomerTypeName,
            TariffType = row.TariffType,
            Consumption = row.Consumption,
            BilledConsumption = row.BilledConsumption,
            AmountUsd = row.AmountUsd,
            AmountFc = row.AmountFc,
            Vat = row.Vat,
            ProvincialTax = row.ProvincialTax,
            CenterDesignation = row.CenterDesignation,
            Pa = row.Pa,
            PointOfSaleName = row.PointOfSaleName,
            SubstationCode = row.SubstationCode,
            ClientPropertyNumber = row.ClientPropertyNumber,
            ClientStreet = row.ClientStreet,
            ClientCommune = row.ClientCommune,
            MeterNumber = row.MeterNumber,
            CalculationCoefficient = row.CalculationCoefficient,
            PreviousBillingIndex = row.PreviousBillingIndex,
            CurrentBillingIndex = row.CurrentBillingIndex,
            DiscountCode = row.DiscountCode,
            AmountExcludingTaxUsd = row.AmountExcludingTaxUsd,
            AmountExcludingTaxFc = row.AmountExcludingTaxFc,
            SubtotalFc = row.SubtotalFc
        };

    public string ToJson() => JsonSerializer.Serialize(this, SerializerOptions);

    public static InvoiceInfoModel? Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return null;

            return new InvoiceInfoModel
            {
                CustomerNum = ReadString(root, "cn", "CustomerNum"),
                CustomerName = ReadString(root, "cnm", "CustomerName"),
                CustomerTypeName = ReadString(root, "ctn", "CustomerTypeName"),
                TariffType = ReadString(root, "tt", "TariffType"),
                Consumption = ReadDecimal(root, "cons", "Consumption"),
                BilledConsumption = ReadDecimal(root, "bcons", "BilledConsumption"),
                AmountUsd = ReadDecimal(root, "ausd", "AmountUsd"),
                AmountFc = ReadDecimal(root, "acdf", "AmountFc"),
                Vat = ReadDecimal(root, "vat", "Vat"),
                ProvincialTax = ReadDecimal(root, "ptax", "ProvincialTax"),
                CenterDesignation = ReadString(root, "ctr", "CenterDesignation"),
                Pa = ReadString(root, "pa", "Pa"),
                PointOfSaleName = ReadString(root, "pos", "PointOfSaleName"),
                SubstationCode = ReadString(root, "ss", "SubstationCode"),
                ClientPropertyNumber = ReadString(root, "cpn", "ClientPropertyNumber"),
                ClientStreet = ReadString(root, "cst", "ClientStreet"),
                ClientCommune = ReadString(root, "ccm", "ClientCommune"),
                MeterNumber = ReadDecimal(root, "mn", "MeterNumber"),
                CalculationCoefficient = ReadDecimal(root, "cc", "CalculationCoefficient"),
                PreviousBillingIndex = ReadDecimal(root, "pbi", "PreviousBillingIndex"),
                CurrentBillingIndex = ReadDecimal(root, "cbi", "CurrentBillingIndex"),
                DiscountCode = ReadDecimal(root, "dc", "DiscountCode"),
                AmountExcludingTaxUsd = ReadDecimal(root, "aetusd", "AmountExcludingTaxUsd"),
                AmountExcludingTaxFc = ReadDecimal(root, "aetcdf", "AmountExcludingTaxFc"),
                SubtotalFc = ReadDecimal(root, "stcdf", "SubtotalFc")
            };
        }
        catch
        {
            return null;
        }
    }

    private static string? ReadString(JsonElement root, string shortKey, string legacyKey)
    {
        if (TryGetProperty(root, shortKey, out var value) || TryGetProperty(root, legacyKey, out value))
        {
            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.GetRawText(),
                JsonValueKind.True => bool.TrueString,
                JsonValueKind.False => bool.FalseString,
                _ => null
            };
        }

        return null;
    }

    private static decimal? ReadDecimal(JsonElement root, string shortKey, string legacyKey)
    {
        if (!TryGetProperty(root, shortKey, out var value) && !TryGetProperty(root, legacyKey, out value))
            return null;

        return value.ValueKind switch
        {
            JsonValueKind.Number => value.TryGetDecimal(out var number) ? number : null,
            JsonValueKind.String when decimal.TryParse(
                value.GetString(),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var parsed) => parsed,
            _ => null
        };
    }

    private static bool TryGetProperty(JsonElement root, string propertyName, out JsonElement value) =>
        root.TryGetProperty(propertyName, out value);
}
