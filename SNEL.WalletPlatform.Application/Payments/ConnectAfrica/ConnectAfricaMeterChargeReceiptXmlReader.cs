using System.Text;
using System.Xml.Linq;
using SNEL.WalletPlatform.Application.Payments.Models.ConnectAfrica;

namespace SNEL.WalletPlatform.Application.Payments.ConnectAfrica;

public static class ConnectAfricaMeterChargeReceiptXmlReader
{
    private static readonly XNamespace XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";

    public static ConnectAfricaMeterChargeReceiptData ReadReceipt(XElement? tokenRoot)
    {
        if (tokenRoot is null)
        {
            return new ConnectAfricaMeterChargeReceiptData();
        }

        var tokens = GetElementValues(tokenRoot, "tk60");
        var taxLines = ParseTaxLines(tokenRoot);
        var vatLine = taxLines.FirstOrDefault(line => line.IsVat);
        var tepLine = taxLines.FirstOrDefault(line => line.IsTep);
        var tariffNames = GetElementValues(tokenRoot, "tk90");
        var preTaxLevies = ParseTk44Tk93PreTaxLevies(tokenRoot);
        var tk93Tk70Section = ParseTk93Tk70Section(tokenRoot);

        return new ConnectAfricaMeterChargeReceiptData
        {
            Date = GetElementValue(tokenRoot, "tk2"),
            Time = GetElementValue(tokenRoot, "tk3"),
            ReceiptNumber = GetElementValue(tokenRoot, "tk10"),
            MeterNumber = GetElementValue(tokenRoot, "tk20"),
            CustomerName = FixEncoding(GetElementValue(tokenRoot, "tk30")),
            PointOfConnection = GetElementValue(tokenRoot, "tk31"),
            PocStandNumber = GetElementValue(tokenRoot, "tk32"),
            TokenNumbers = GetElementValues(tokenRoot, "tk11"),
            Tokens = tokens.Count > 0
                ? tokens
                : tokenRoot
                    .Document?
                    .Descendants("tk60")
                    .Select(element => NormalizeElementValue(element))
                    .Where(value => value is not null)
                    .Cast<string>()
                    .ToArray()
                ?? [],
            TransactionNumber = GetElementValue(tokenRoot, "tk14"),
            SupplyGroupCode = GetElementValue(tokenRoot, "tk61"),
            TariffIndex = GetElementValue(tokenRoot, "tk62"),
            Krn = GetElementValue(tokenRoot, "tk63"),
            TaxNumber = GetElementValue(tokenRoot, "tk4"),
            Duplicate = FixEncoding(GetElementValue(tokenRoot, "tk5")),
            TransactionAmount = GetElementValue(tokenRoot, "tk40"),
            QuantityUnitsKwh = GetElementValue(tokenRoot, "tk50"),
            PaidAmount = GetElementValue(tokenRoot, "tk80"),
            TariffAmounts = GetElementValues(tokenRoot, "tk41"),
            TariffNames = tariffNames,
            TariffName = tariffNames.FirstOrDefault(),
            PricePerKwh = GetElementValues(tokenRoot, "tk71"),
            TaxLines = taxLines,
            TaxNames = vatLine?.Name is null ? [] : [vatLine.Name],
            TaxPercentages = vatLine?.Percentage is null ? [] : [vatLine.Percentage],
            TaxAmount = vatLine?.Amount,
            TepNames = tepLine?.Name is null ? [] : [tepLine.Name],
            TepPercentages = tepLine?.Percentage is null ? [] : [tepLine.Percentage],
            TepAmount = tepLine?.Amount,
            OutstandingArrears = tk93Tk70Section.Total ?? GetElementValue(tokenRoot, "tk70"),
            AdvanceOnWorks = tk93Tk70Section.AdvanceOnWorks,
            Debts1 = tk93Tk70Section.Debts1,
            ArrearsTotal = tk93Tk70Section.Total,
            PreTaxLevyAmounts = preTaxLevies.Amounts,
            PreTaxLevyNames = preTaxLevies.Names,
            FixedChargeAmounts = GetElementValues(tokenRoot, "tk45"),
            FixedChargeNames = GetElementValues(tokenRoot, "tk94").Select(FixEncoding).Where(v => v is not null).Cast<string>().ToArray(),
            FixedChargeRates = GetElementValues(tokenRoot, "tk74").Select(FixEncoding).Where(v => v is not null).Cast<string>().ToArray(),
            FixedChargeMonthCounts = GetElementValues(tokenRoot, "tk75"),
            LevyAmounts = GetElementValues(tokenRoot, "tk46"),
            LevyNames = GetElementValues(tokenRoot, "tk95").Select(FixEncoding).Where(v => v is not null).Cast<string>().ToArray(),
            Operator = GetElementValue(tokenRoot, "tk12"),
            Cashpower = GetElementValue(tokenRoot, "tk15"),
            TransactionTypeLabel = FixEncoding(GetElementValue(tokenRoot, "tk21")),
            TokenTypeLabel = FixEncoding(GetElementValue(tokenRoot, "tk22")),
            ServiceTypeLabel = FixEncoding(GetElementValue(tokenRoot, "tk23"))
        };
    }

    public static ConnectAfricaMeterChargeReceiptData ReadFromVendXml(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            return new ConnectAfricaMeterChargeReceiptData();
        }

        var document = XDocument.Parse(xml);
        var vend = document.Descendants("vend").FirstOrDefault();
        if (vend is null)
        {
            return new ConnectAfricaMeterChargeReceiptData();
        }

        var token = vend.Descendants("token").FirstOrDefault() ?? vend;
        return ReadReceipt(token);
    }

    private static IReadOnlyList<ConnectAfricaReceiptTaxLine> ParseTaxLines(XElement tokenRoot)
    {
        var lines = new List<ConnectAfricaReceiptTaxLine>();
        string? pendingAmount = null;
        string? pendingName = null;

        foreach (var element in tokenRoot.Elements())
        {
            var localName = element.Name.LocalName;
            var value = NormalizeElementValue(element);
            switch (localName)
            {
                case "tk43":
                    if (pendingAmount is not null && pendingName is not null)
                    {
                        lines.Add(new ConnectAfricaReceiptTaxLine
                        {
                            Amount = pendingAmount,
                            Name = pendingName
                        });
                    }

                    pendingAmount = value;
                    pendingName = null;
                    break;
                case "tk92":
                    pendingName = value;
                    break;
                case "tk73":
                    if (pendingAmount is not null)
                    {
                        lines.Add(new ConnectAfricaReceiptTaxLine
                        {
                            Amount = pendingAmount,
                            Name = pendingName,
                            Percentage = value
                        });
                        pendingAmount = null;
                        pendingName = null;
                    }

                    break;
            }
        }

        if (pendingAmount is not null && pendingName is not null)
        {
            lines.Add(new ConnectAfricaReceiptTaxLine
            {
                Amount = pendingAmount,
                Name = pendingName
            });
        }

        return lines;
    }

    private sealed record PreTaxLevyParseResult(
        IReadOnlyList<string> Names,
        IReadOnlyList<string> Amounts);

    private static PreTaxLevyParseResult ParseTk44Tk93PreTaxLevies(XElement tokenRoot)
    {
        var elements = tokenRoot.Elements().ToArray();
        var firstVatLabelIndex = FindFirstVatLabelIndex(elements);
        var names = new List<string>();
        var amounts = new List<string>();

        for (var index = 0; index < elements.Length - 1; index++)
        {
            if (firstVatLabelIndex >= 0 && index >= firstVatLabelIndex)
                break;

            if (elements[index].Name.LocalName != "tk44" ||
                elements[index + 1].Name.LocalName != "tk93")
            {
                continue;
            }

            var amount = NormalizeElementValue(elements[index]);
            var name = FixEncoding(NormalizeElementValue(elements[index + 1]));
            if (amount is null || name is null)
                continue;

            names.Add(name);
            amounts.Add(amount);
        }

        return new PreTaxLevyParseResult(names, amounts);
    }

    private sealed record Tk93Tk70ParseResult(
        string? AdvanceOnWorks,
        string? Debts1,
        string? Total);

    private static Tk93Tk70ParseResult ParseTk93Tk70Section(XElement tokenRoot)
    {
        var elements = tokenRoot.Elements().ToArray();
        var firstVatLabelIndex = FindFirstVatLabelIndex(elements);
        var lastTaxRateIndex = FindLastTaxRateIndex(elements);

        string? pendingName = null;
        string? advanceOnWorks = null;
        string? debts1 = null;
        string? total = null;
        var hasPostTaxNamedLines = false;

        for (var index = 0; index < elements.Length; index++)
        {
            var element = elements[index];
            var localName = element.Name.LocalName;
            var value = NormalizeElementValue(element);
            if (value is null)
                continue;

            switch (localName)
            {
                case "tk93":
                    pendingName = FixEncoding(value);
                    break;
                case "tk70":
                    if (pendingName is not null)
                    {
                        if (IsArrearsTk93Tk70Pair(index, firstVatLabelIndex, lastTaxRateIndex))
                        {
                            hasPostTaxNamedLines = true;
                            if (ContainsLabel(pendingName, "Avance"))
                                advanceOnWorks = value;
                            else if (ContainsLabel(pendingName, "Dettes"))
                                debts1 = value;
                        }

                        pendingName = null;
                    }
                    else if (IsArrearsTk93Tk70Pair(index, firstVatLabelIndex, lastTaxRateIndex))
                    {
                        total = value;
                    }

                    break;
            }
        }

        if (!hasPostTaxNamedLines)
        {
            return new Tk93Tk70ParseResult(null, null, total);
        }

        return new Tk93Tk70ParseResult(advanceOnWorks, debts1, total);
    }

    private static int FindFirstVatLabelIndex(XElement[] elements)
    {
        for (var index = 0; index < elements.Length; index++)
        {
            if (elements[index].Name.LocalName != "tk92")
                continue;

            var value = NormalizeElementValue(elements[index]);
            if (value is not null &&
                value.Contains("TVA", StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return -1;
    }

    private static int FindLastTaxRateIndex(XElement[] elements)
    {
        for (var index = elements.Length - 1; index >= 0; index--)
        {
            if (elements[index].Name.LocalName == "tk73")
                return index;
        }

        return -1;
    }

    private static bool IsArrearsTk93Tk70Pair(
        int index,
        int firstVatLabelIndex,
        int lastTaxRateIndex)
    {
        if (lastTaxRateIndex >= 0)
            return index > lastTaxRateIndex;

        return firstVatLabelIndex >= 0 && index > firstVatLabelIndex;
    }

    private static bool ContainsLabel(string name, string fragment) =>
        name.Contains(fragment, StringComparison.OrdinalIgnoreCase);

    private static string? GetElementValue(XElement parent, string elementName) =>
        parent
            .Elements(elementName)
            .Select(NormalizeElementValue)
            .FirstOrDefault(value => value is not null);

    private static IReadOnlyList<string> GetElementValues(XElement parent, string elementName) =>
        parent
            .Elements(elementName)
            .Select(NormalizeElementValue)
            .Where(value => value is not null)
            .Cast<string>()
            .ToArray();

    private static string? NormalizeElementValue(XElement element)
    {
        var nilAttribute = element.Attribute(XsiNamespace + "nil");
        if (string.Equals(nilAttribute?.Value, "true", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return Normalize(element.Value);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? FixEncoding(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        if (!value.Contains('Ã', StringComparison.Ordinal))
            return value;

        var bytes = Encoding.Latin1.GetBytes(value);
        return Encoding.UTF8.GetString(bytes);
    }
}
