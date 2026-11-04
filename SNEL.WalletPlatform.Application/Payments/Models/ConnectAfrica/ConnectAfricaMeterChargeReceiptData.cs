using System.Text.Json;
using System.Text.Json.Serialization;

using SNEL.WalletPlatform.Application.Payments.ConnectAfrica;

namespace SNEL.WalletPlatform.Application.Payments.Models.ConnectAfrica;

/// <summary>
/// Structured Connect Africa vend receipt derived from the raw vend XML stored on
/// <see cref="Domain.Payments.PaymentOrderItemEntity.WebResponse"/>.
/// </summary>
public sealed class ConnectAfricaMeterChargeReceiptData
{
    public string? Date { get; init; }
    public string? Time { get; init; }

    [JsonPropertyName("receiptNumber")]
    public string? ReceiptNumber { get; init; }

    [JsonPropertyName("meterNumber")]
    public string? MeterNumber { get; init; }

    [JsonPropertyName("customerName")]
    public string? CustomerName { get; init; }

    [JsonPropertyName("pointOfConnection")]
    public string? PointOfConnection { get; init; }

    [JsonPropertyName("pocStandNumber")]
    public string? PocStandNumber { get; init; }

    [JsonPropertyName("tokenNumbers")]
    public IReadOnlyList<string> TokenNumbers { get; init; } = [];

    [JsonPropertyName("tokens")]
    public IReadOnlyList<string> Tokens { get; init; } = [];

    [JsonPropertyName("transactionNumber")]
    public string? TransactionNumber { get; init; }

    [JsonPropertyName("supplyGroupCode")]
    public string? SupplyGroupCode { get; init; }

    [JsonPropertyName("tariffIndex")]
    public string? TariffIndex { get; init; }

    [JsonPropertyName("krn")]
    public string? Krn { get; init; }

    [JsonPropertyName("taxNumber")]
    public string? TaxNumber { get; init; }

    [JsonPropertyName("duplicate")]
    public string? Duplicate { get; init; }

    [JsonPropertyName("transactionAmount")]
    public string? TransactionAmount { get; init; }

    [JsonPropertyName("quantityUnitsKwh")]
    public string? QuantityUnitsKwh { get; init; }

    [JsonPropertyName("paidAmount")]
    public string? PaidAmount { get; init; }

    [JsonPropertyName("tariffAmounts")]
    public IReadOnlyList<string> TariffAmounts { get; init; } = [];

    [JsonPropertyName("tariffNames")]
    public IReadOnlyList<string> TariffNames { get; init; } = [];

    [JsonPropertyName("tariffName")]
    public string? TariffName { get; init; }

    [JsonPropertyName("pricePerKwh")]
    public IReadOnlyList<string> PricePerKwh { get; init; } = [];

    [JsonPropertyName("taxLines")]
    public IReadOnlyList<ConnectAfricaReceiptTaxLine> TaxLines { get; init; } = [];

    [JsonPropertyName("taxNames")]
    public IReadOnlyList<string> TaxNames { get; init; } = [];

    [JsonPropertyName("taxPercentages")]
    public IReadOnlyList<string> TaxPercentages { get; init; } = [];

    [JsonPropertyName("taxAmount")]
    public string? TaxAmount { get; init; }

    [JsonPropertyName("tepNames")]
    public IReadOnlyList<string> TepNames { get; init; } = [];

    [JsonPropertyName("tepPercentages")]
    public IReadOnlyList<string> TepPercentages { get; init; } = [];

    [JsonPropertyName("tepAmount")]
    public string? TepAmount { get; init; }

    [JsonPropertyName("outstandingArrears")]
    public string? OutstandingArrears { get; init; }

    [JsonPropertyName("advanceOnWorks")]
    public string? AdvanceOnWorks { get; init; }

    [JsonPropertyName("debts1")]
    public string? Debts1 { get; init; }

    [JsonPropertyName("arrearsTotal")]
    public string? ArrearsTotal { get; init; }

    [JsonPropertyName("fixedChargeAmounts")]
    public IReadOnlyList<string> FixedChargeAmounts { get; init; } = [];

    [JsonPropertyName("fixedChargeNames")]
    public IReadOnlyList<string> FixedChargeNames { get; init; } = [];

    [JsonPropertyName("fixedChargeRates")]
    public IReadOnlyList<string> FixedChargeRates { get; init; } = [];

    [JsonPropertyName("fixedChargeMonthCounts")]
    public IReadOnlyList<string> FixedChargeMonthCounts { get; init; } = [];

    [JsonPropertyName("levyAmounts")]
    public IReadOnlyList<string> LevyAmounts { get; init; } = [];

    [JsonPropertyName("levyNames")]
    public IReadOnlyList<string> LevyNames { get; init; } = [];

    [JsonPropertyName("preTaxLevyAmounts")]
    public IReadOnlyList<string> PreTaxLevyAmounts { get; init; } = [];

    [JsonPropertyName("preTaxLevyNames")]
    public IReadOnlyList<string> PreTaxLevyNames { get; init; } = [];

    [JsonPropertyName("operator")]
    public string? Operator { get; init; }

    [JsonPropertyName("cashpower")]
    public string? Cashpower { get; init; }

    [JsonPropertyName("transactionTypeLabel")]
    public string? TransactionTypeLabel { get; init; }

    [JsonPropertyName("tokenTypeLabel")]
    public string? TokenTypeLabel { get; init; }

    [JsonPropertyName("serviceTypeLabel")]
    public string? ServiceTypeLabel { get; init; }

    public bool Acknowledged { get; init; }

    public ConnectAfricaReceiptTaxLine? ResolveVatLine() =>
        TaxLines.FirstOrDefault(line => line.IsVat);

    public ConnectAfricaReceiptTaxLine? ResolveTepLine() =>
        TaxLines.FirstOrDefault(line => line.IsTep);

    public ConnectAfricaReceiptTaxLine? ResolvePublicLightingLine() =>
        TaxLines.FirstOrDefault(line => line.IsPublicLighting);

    public static bool TryParse(string? externalRef, out ConnectAfricaMeterChargeReceiptData? receipt)
    {
        receipt = null;
        if (string.IsNullOrWhiteSpace(externalRef))
            return false;

        var trimmed = externalRef.Trim();
        if (trimmed.Length < 2 || trimmed[0] != '{' || trimmed[^1] != '}')
            return false;

        try
        {
            using var doc = JsonDocument.Parse(trimmed);
            var root = doc.RootElement.Clone();
            receipt = JsonSerializer.Deserialize<ConnectAfricaMeterChargeReceiptData>(trimmed, JsonOptions);
            if (receipt is null)
                return false;

            receipt = EnrichFromLegacyJson(receipt, root);

            if (!HasStructuredReceiptShape(receipt))
            {
                receipt = null;
                return false;
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Resolves a stored meter-charge web response: raw vend XML (current) or legacy JSON.
    /// </summary>
    public static bool TryResolve(string? storedResponse, out ConnectAfricaMeterChargeReceiptData? receipt)
    {
        receipt = null;
        if (string.IsNullOrWhiteSpace(storedResponse))
            return false;

        var trimmed = storedResponse.Trim();
        if (trimmed.Length < 2)
            return false;

        if (trimmed[0] == '{')
            return TryParse(trimmed, out receipt);

        if (!LooksLikeXml(trimmed))
            return false;

        try
        {
            receipt = ConnectAfricaMeterChargeReceiptXmlReader.ReadFromVendXml(trimmed);
            if (!HasStructuredReceiptShape(receipt))
            {
                receipt = null;
                return false;
            }

            return true;
        }
        catch (System.Xml.XmlException)
        {
            return false;
        }
    }

    private static bool LooksLikeXml(string value) =>
        value.StartsWith('<')
        || value.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase);

    private static bool HasStructuredReceiptShape(ConnectAfricaMeterChargeReceiptData receipt) =>
        !string.IsNullOrWhiteSpace(receipt.ReceiptNumber)
        || receipt.Tokens.Count > 0
        || !string.IsNullOrWhiteSpace(receipt.TransactionNumber)
        || !string.IsNullOrWhiteSpace(receipt.CustomerName);

    private static ConnectAfricaMeterChargeReceiptData EnrichFromLegacyJson(
        ConnectAfricaMeterChargeReceiptData receipt,
        JsonElement root)
    {
        var receiptNumber = receipt.ReceiptNumber;
        if (string.IsNullOrWhiteSpace(receiptNumber) &&
            root.TryGetProperty("ReceiptNo", out var legacyReceipt) &&
            legacyReceipt.ValueKind == JsonValueKind.String)
        {
            receiptNumber = legacyReceipt.GetString();
        }

        IReadOnlyList<string> tokens = receipt.Tokens;
        if (tokens.Count == 0 &&
            root.TryGetProperty("Tokens", out var legacyTokens) &&
            legacyTokens.ValueKind == JsonValueKind.Array)
        {
            tokens = legacyTokens
                .EnumerateArray()
                .Where(element => element.ValueKind == JsonValueKind.String)
                .Select(element => element.GetString()!)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToArray();
        }

        var acknowledged = receipt.Acknowledged;
        if (!acknowledged &&
            root.TryGetProperty("Acknowledged", out var acknowledgedElement) &&
            (acknowledgedElement.ValueKind == JsonValueKind.True ||
             acknowledgedElement.ValueKind == JsonValueKind.False))
        {
            acknowledged = acknowledgedElement.GetBoolean();
        }

        var tariffNames = receipt.TariffNames.Count > 0
            ? receipt.TariffNames
            : string.IsNullOrWhiteSpace(receipt.TariffName)
                ? []
                : [receipt.TariffName];

        var taxLines = receipt.TaxLines.Count > 0
            ? receipt.TaxLines
            : BuildLegacyTaxLines(receipt);

        var vatLine = taxLines.FirstOrDefault(line => line.IsVat);
        var tepLine = taxLines.FirstOrDefault(line => line.IsTep);

        return new ConnectAfricaMeterChargeReceiptData
        {
            Date = receipt.Date,
            Time = receipt.Time,
            ReceiptNumber = receiptNumber,
            MeterNumber = receipt.MeterNumber,
            CustomerName = receipt.CustomerName,
            PointOfConnection = receipt.PointOfConnection,
            PocStandNumber = receipt.PocStandNumber,
            TokenNumbers = receipt.TokenNumbers,
            Tokens = tokens,
            TransactionNumber = receipt.TransactionNumber,
            SupplyGroupCode = receipt.SupplyGroupCode,
            TariffIndex = receipt.TariffIndex,
            Krn = receipt.Krn,
            TransactionAmount = receipt.TransactionAmount,
            QuantityUnitsKwh = receipt.QuantityUnitsKwh,
            PaidAmount = receipt.PaidAmount,
            TariffAmounts = receipt.TariffAmounts,
            TariffNames = tariffNames,
            TariffName = tariffNames.FirstOrDefault() ?? receipt.TariffName,
            PricePerKwh = receipt.PricePerKwh,
            TaxLines = taxLines,
            TaxNames = vatLine?.Name is null ? receipt.TaxNames : [vatLine.Name],
            TaxPercentages = vatLine?.Percentage is null ? receipt.TaxPercentages : [vatLine.Percentage],
            TaxAmount = vatLine?.Amount ?? receipt.TaxAmount,
            TepNames = tepLine?.Name is null ? receipt.TepNames : [tepLine.Name],
            TepPercentages = tepLine?.Percentage is null ? receipt.TepPercentages : [tepLine.Percentage],
            TepAmount = tepLine?.Amount ?? receipt.TepAmount,
            TaxNumber = receipt.TaxNumber,
            Duplicate = receipt.Duplicate,
            OutstandingArrears = receipt.OutstandingArrears,
            AdvanceOnWorks = receipt.AdvanceOnWorks,
            Debts1 = receipt.Debts1,
            ArrearsTotal = receipt.ArrearsTotal,
            FixedChargeAmounts = receipt.FixedChargeAmounts,
            FixedChargeNames = receipt.FixedChargeNames,
            FixedChargeRates = receipt.FixedChargeRates,
            FixedChargeMonthCounts = receipt.FixedChargeMonthCounts,
            LevyAmounts = receipt.LevyAmounts,
            LevyNames = receipt.LevyNames,
            PreTaxLevyAmounts = receipt.PreTaxLevyAmounts,
            PreTaxLevyNames = receipt.PreTaxLevyNames,
            Operator = receipt.Operator,
            Cashpower = receipt.Cashpower,
            TransactionTypeLabel = receipt.TransactionTypeLabel,
            TokenTypeLabel = receipt.TokenTypeLabel,
            ServiceTypeLabel = receipt.ServiceTypeLabel,
            Acknowledged = acknowledged
        };
    }

    private static IReadOnlyList<ConnectAfricaReceiptTaxLine> BuildLegacyTaxLines(
        ConnectAfricaMeterChargeReceiptData receipt)
    {
        var lines = new List<ConnectAfricaReceiptTaxLine>();

        if (!string.IsNullOrWhiteSpace(receipt.TaxAmount) ||
            receipt.TaxNames.Count > 0 ||
            receipt.TaxPercentages.Count > 0)
        {
            lines.Add(new ConnectAfricaReceiptTaxLine
            {
                Amount = receipt.TaxAmount,
                Name = receipt.TaxNames.FirstOrDefault() ?? "TVA",
                Percentage = receipt.TaxPercentages.FirstOrDefault()
            });
        }

        if (!string.IsNullOrWhiteSpace(receipt.TepAmount) ||
            receipt.TepNames.Count > 0 ||
            receipt.TepPercentages.Count > 0)
        {
            lines.Add(new ConnectAfricaReceiptTaxLine
            {
                Amount = receipt.TepAmount,
                Name = receipt.TepNames.FirstOrDefault() ?? "TEP",
                Percentage = receipt.TepPercentages.FirstOrDefault()
            });
        }

        return lines;
    }

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };
}
