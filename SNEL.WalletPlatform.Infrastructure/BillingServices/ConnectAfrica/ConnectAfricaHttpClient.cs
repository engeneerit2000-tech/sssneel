using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SNEL.WalletPlatform.Application.Payments.ConnectAfrica;
using SNEL.WalletPlatform.Infrastructure.BillingServices.ConnectAfrica.Models;
using SNEL.WalletPlatform.Infrastructure.Configuration;

namespace SNEL.WalletPlatform.Infrastructure.BillingServices.ConnectAfrica;

public sealed class ConnectAfricaHttpClient(
    HttpClient httpClient,
    IOptions<ConnectAfricaOptions> optionsAccessor,
    ILogger<ConnectAfricaHttpClient> logger)
{
    private static readonly Regex LogPasswordTagRegex = new(
        @"<password>\s*[\s\S]*?</password>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    private const string ContentType = "application/xml";
    private const string TidValue = "yes";
    private readonly ConnectAfricaOptions options = optionsAccessor.Value;

    public async Task<ConnectAfricaCheckResponse?> CheckAsync(
        ConnectAfricaCheckRequest request)
    {
        try
        {


            ArgumentNullException.ThrowIfNull(request);

            var requestUri = BuildRequestUri(options.CheckEndpointPath);
            var xmlBody = BuildCheckPayload(request);

            using var message = BuildXmlRequestMessage(HttpMethod.Post, requestUri, xmlBody);

            logger.LogInformation(
                "ConnectAfrica {What} Meter={Meter}, Unit={Unit}, Amount={Amount}.",
                "Check (consumer-chk) — verify meter and amount before a sale; returns eligibility and minimum allowed charge (tk30).",
                request.Meter,
                ResolveUnit(),
                request.Amount);

            LogHttpRequest(
                "Check (consumer-chk) — verify meter and amount before a sale; returns eligibility and minimum allowed charge (tk30).",
                message,
                xmlBody);

            using var response = await httpClient.SendAsync(message).ConfigureAwait(false);
            //response.EnsureSuccessStatusCode();

            var xml = await ReadResponseBodyAsync(response).ConfigureAwait(false);
            LogHttpResponse(
                "Check (consumer-chk) — verify meter and amount before a sale; returns eligibility and minimum allowed charge (tk30).",
                response,
                xml);

            if (string.IsNullOrWhiteSpace(xml))
            {
                logger.LogError($"ConnectAfrica check response was empty. for request: {message}");
                return null;
            }

            var document = XDocument.Parse(xml);
            var consumerCheck = document.Descendants("consumer-chk").FirstOrDefault();
            if (consumerCheck is null)
            {
                logger.LogError($"ConnectAfrica check response does not contain consumer-chk, for message{message}");
                return null;
            }

            var successValue = consumerCheck.Element("success")?.Value?.Trim();
            var isSuccess = string.Equals(successValue, "1", StringComparison.OrdinalIgnoreCase);
            if (isSuccess)
            {
                var tk30Value = consumerCheck
                    .Descendants("tk30")
                    .FirstOrDefault()?
                    .Value?
                    .Trim();

                decimal? minimumAllowedChargeValue = null;
                if (!string.IsNullOrWhiteSpace(tk30Value))
                {
                    if (decimal.TryParse(
                            tk30Value,
                            NumberStyles.Number,
                            CultureInfo.InvariantCulture,
                            out var parsedTk30))
                    {
                        minimumAllowedChargeValue = parsedTk30;
                    }
                    else
                    {
                        logger.LogWarning(
                            "ConnectAfrica check response contained a non-numeric tk30 value: {Tk30Value}",
                            tk30Value);
                    }
                }
                var tk3Value = consumerCheck
                    .Descendants("tk3")
                    .FirstOrDefault()?
                    .Value?
                    .Trim();
                return new ConnectAfricaCheckResponse(isSuccess, minimumAllowedChargeValue, tk3Value, string.Empty);
            }
            else
            {
                var error = consumerCheck.Descendants("error").First().Value;
                return new ConnectAfricaCheckResponse(isSuccess, null, null, error);
            }
        }
        catch (Exception exp)
        {
            logger.LogError($"ConnectAfrica: Unable to check for request: {JsonSerializer.Serialize(request.Meter)}");
            logger.LogError(exp, exp.Message);
            return null;
        }
    }

    public async Task<ConnectAfricaVendResponse> VendAsync(
        ConnectAfricaVendRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestUri = BuildRequestUri(options.VendEndpointPath);
        var xmlBody = BuildVendPayload(request);

        using var message = BuildXmlRequestMessage(HttpMethod.Post, requestUri, xmlBody);

        logger.LogInformation(
            "ConnectAfrica {What} Meter={Meter}, Unit={Unit}, Amount={Amount}.",
            "Vend — purchase prepayment token / kWh; creates the sale, returns tokens (tk60) and receipt (tk10).",
            request.Meter,
            ResolveUnit(),
            request.Amount);

        LogHttpRequest(
            "Vend — purchase prepayment token / kWh; creates the sale, returns tokens (tk60) and receipt (tk10).",
            message,
            xmlBody);

        using var response = await httpClient.SendAsync(message).ConfigureAwait(false);
        var responseBody = await ReadResponseBodyAsync(response).ConfigureAwait(false);
        LogHttpResponse(
            "Vend — purchase prepayment token / kWh; creates the sale, returns tokens (tk60) and receipt (tk10).",
            response,
            responseBody);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError(
                "ConnectAfrica vend failed with HTTP {StatusCode}. Response body: {ResponseBody}",
                (int)response.StatusCode,
                responseBody);
        }

        if (TryParseVendResponse(xmlBody, responseBody, out var vendResponse))
        {
            return vendResponse;
        }

        return new ConnectAfricaVendResponse(
            false,
            null,
            null,
            [],
            null,
            xmlBody,
            responseBody ?? string.Empty,
            ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture));
    }

    public async Task<ConnectAfricaAckResponse> AckAsync(
        ConnectAfricaAckRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestUri = BuildRequestUri(options.AckEndpointPath);
        var xmlBody = BuildAckPayload(request);

        using var message = BuildXmlRequestMessage(HttpMethod.Post, requestUri, xmlBody);

        logger.LogInformation(
            "ConnectAfrica {What} ReceiptNumber={ReceiptNumber}, Unit={Unit}.",
            "Ack — confirm vendor receipt of tokens after a successful vend (acknowledge receipt number with Connect Africa).",
            request.ReceiptNumber,
            ResolveUnit());

        LogHttpRequest(
            "Ack — confirm vendor receipt of tokens after a successful vend (acknowledge receipt number with Connect Africa).",
            message,
            xmlBody);

        using var response = await httpClient.SendAsync(message).ConfigureAwait(false);
        var responseBody = await ReadResponseBodyAsync(response).ConfigureAwait(false);
        LogHttpResponse(
            "Ack — confirm vendor receipt of tokens after a successful vend (acknowledge receipt number with Connect Africa).",
            response,
            responseBody);
        response.EnsureSuccessStatusCode();

        if (response.StatusCode != System.Net.HttpStatusCode.Created)
        {
            throw new HttpRequestException(
                $"ConnectAfrica ack returned unexpected status code '{(int)response.StatusCode}'.");
        }

        return new ConnectAfricaAckResponse(
            true,
            xmlBody,
            string.IsNullOrWhiteSpace(responseBody) ? null : responseBody,
            ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture));
    }

    public async Task<ConnectAfricaEosResponse> EosAsync()
    {
        var requestUri = BuildRequestUri(options.EosEndpointPath);
        var xmlBody = BuildCredentialsPayload();

        using var message = BuildXmlRequestMessage(HttpMethod.Post, requestUri, xmlBody);

        logger.LogInformation(
            "ConnectAfrica {What} Unit={Unit}.",
            "Eos (end of shift) — submit end-of-shift / terminal wrap-up totals to Connect Africa (eos payload).",
            ResolveUnit());

        LogHttpRequest(
            "Eos (end of shift) — submit end-of-shift / terminal wrap-up totals to Connect Africa (eos payload).",
            message,
            xmlBody);

        using var response = await httpClient.SendAsync(message).ConfigureAwait(false);
        var responseBody = await ReadResponseBodyAsync(response).ConfigureAwait(false);
        LogHttpResponse(
            "Eos (end of shift) — submit end-of-shift / terminal wrap-up totals to Connect Africa (eos payload).",
            response,
            responseBody);
        response.EnsureSuccessStatusCode();

        return ParseEosResponse(xmlBody, responseBody);
    }

    public async Task<ConnectAfricaReprintResponse> ReprintAsync(
        ConnectAfricaReprintRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestUri = BuildRequestUri(options.ReprintEndpointPath);
        var xmlBody = BuildReprintPayload(request);

        using var message = BuildXmlRequestMessage(HttpMethod.Post, requestUri, xmlBody);

        logger.LogInformation(
            "ConnectAfrica {What} ReceiptNumber={ReceiptNumber}, Unit={Unit}.",
            "Reprint (token-reprint) — request a duplicate print of a past purchase using the receipt number.",
            request.ReceiptNumber,
            ResolveUnit());

        LogHttpRequest(
            "Reprint (token-reprint) — request a duplicate print of a past purchase using the receipt number.",
            message,
            xmlBody);

        using var response = await httpClient.SendAsync(message).ConfigureAwait(false);
        var responseBody = await ReadResponseBodyAsync(response).ConfigureAwait(false);
        LogHttpResponse(
            "Reprint (token-reprint) — request a duplicate print of a past purchase using the receipt number.",
            response,
            responseBody);
        response.EnsureSuccessStatusCode();

        return ParseReprintResponse(xmlBody, responseBody);
    }

    public async Task<ConnectAfricaTokenListResponse> ListAsync(
        ConnectAfricaTokenListRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestUri = BuildRequestUri(options.ListEndpointPath);
        var xmlBody = BuildListPayload(request);

        using var message = BuildXmlRequestMessage(HttpMethod.Post, requestUri, xmlBody);

        logger.LogInformation(
            "ConnectAfrica {What} Meter={Meter}, Unit={Unit}, NumberOfTokens={NumberOfTokens}, ListOption={ListOption}.",
            "Token list (token-list) — list recent token lines for a meter; controls how many rows and list mode via number_of_tokens and list_option.",
            request.Meter,
            ResolveUnit(),
            request.NumberOfTokens,
            request.ListOption);

        LogHttpRequest(
            "Token list (token-list) — list recent token lines for a meter; controls how many rows and list mode via number_of_tokens and list_option.",
            message,
            xmlBody);

        using var response = await httpClient.SendAsync(message).ConfigureAwait(false);
        var responseBody = await ReadResponseBodyAsync(response).ConfigureAwait(false);
        LogHttpResponse(
            "Token list (token-list) — list recent token lines for a meter; controls how many rows and list mode via number_of_tokens and list_option.",
            response,
            responseBody);
        response.EnsureSuccessStatusCode();

        return ParseTokenListResponse(xmlBody, responseBody);
    }

    public async Task<ConnectAfricaLastTransactionsResponse> LastTransactionsAsync(
        ConnectAfricaLastTransactionsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestUri = BuildRequestUri(options.LastTransactionsEndpointPath);
        var xmlBody = BuildMeterPayload(request.Meter);

        using var message = BuildXmlRequestMessage(HttpMethod.Get, requestUri, xmlBody);

        logger.LogInformation(
            "ConnectAfrica {What} Meter={Meter}, Unit={Unit}.",
            "Last transactions — retrieve historical transaction rows for a meter (transaction list in response).",
            request.Meter,
            ResolveUnit());

        LogHttpRequest(
            "Last transactions — retrieve historical transaction rows for a meter (transaction list in response).",
            message,
            xmlBody);

        using var response = await httpClient.SendAsync(message).ConfigureAwait(false);
        var responseBody = await ReadResponseBodyAsync(response).ConfigureAwait(false);
        LogHttpResponse(
            "Last transactions — retrieve historical transaction rows for a meter (transaction list in response).",
            response,
            responseBody);
        response.EnsureSuccessStatusCode();

        return ParseLastTransactionsResponse(xmlBody, responseBody);
    }

    public async Task<ConnectAfricaLastTransactionStatusResponse> LastTransactionStatusAsync(
        ConnectAfricaLastTransactionStatusRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestUri = BuildRequestUri(options.LastTransactionStatusEndpointPath);
        var xmlBody = BuildMeterPayload(request.Meter);

        using var message = BuildXmlRequestMessage(HttpMethod.Get, requestUri, xmlBody);

        logger.LogInformation(
            "ConnectAfrica {What} Meter={Meter}, Unit={Unit}.",
            "Last transaction status — get status and id of the most recent transaction for a meter (transactions node).",
            request.Meter,
            ResolveUnit());

        LogHttpRequest(
            "Last transaction status — get status and id of the most recent transaction for a meter (transactions node).",
            message,
            xmlBody);

        using var response = await httpClient.SendAsync(message).ConfigureAwait(false);
        var responseBody = await ReadResponseBodyAsync(response).ConfigureAwait(false);
        LogHttpResponse(
            "Last transaction status — get status and id of the most recent transaction for a meter (transactions node).",
            response,
            responseBody);
        response.EnsureSuccessStatusCode();

        return ParseLastTransactionStatusResponse(xmlBody, responseBody);
    }

    private string BuildCheckPayload(ConnectAfricaCheckRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Meter))
        {
            throw new InvalidOperationException("ConnectAfrica meter is required.");
        }

        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("ConnectAfrica amount must be greater than zero.");
        }

        var xml = new XElement("catvend",
            BuildCredentialElements(),
            new XElement("meter", request.Meter.Trim()),
            new XElement("amount", request.Amount.ToString(CultureInfo.InvariantCulture)));

        return xml.ToString(SaveOptions.DisableFormatting);
    }

    private string BuildVendPayload(ConnectAfricaVendRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Meter))
        {
            throw new InvalidOperationException("ConnectAfrica meter is required.");
        }

        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("ConnectAfrica amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(request.CustomerPhone))
        {
            throw new InvalidOperationException("ConnectAfrica customer phone is required.");
        }

        var xml = new XElement("catvend",
            BuildCredentialElements(),
            new XElement("meter", request.Meter.Trim()),
            new XElement("amount", request.Amount.ToString(CultureInfo.InvariantCulture)),
            new XElement("customer_phone", request.CustomerPhone.Trim()),
            new XElement("tid", TidValue));

        return xml.ToString(SaveOptions.DisableFormatting);
    }

    private string BuildAckPayload(ConnectAfricaAckRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ReceiptNumber))
        {
            throw new InvalidOperationException("ConnectAfrica receipt number is required.");
        }

        EnsureCredentialsConfigured();

        var xml = new XElement("catvend",
            new XElement("unit", ResolveUnit()),
            new XElement("validation_code", options.ValidationCode),
            new XElement("receipt", request.ReceiptNumber.Trim()));

        return xml.ToString(SaveOptions.DisableFormatting);
    }

    private string BuildReprintPayload(ConnectAfricaReprintRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ReceiptNumber))
        {
            throw new InvalidOperationException("ConnectAfrica receipt number is required.");
        }

        var xml = new XElement("catvend",
            BuildCredentialElements(),
            new XElement("receipt", request.ReceiptNumber.Trim()));

        return xml.ToString(SaveOptions.DisableFormatting);
    }

    private string BuildListPayload(ConnectAfricaTokenListRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Meter))
        {
            throw new InvalidOperationException("ConnectAfrica meter is required.");
        }

        if (request.NumberOfTokens <= 0)
        {
            throw new InvalidOperationException("ConnectAfrica number of tokens must be greater than zero.");
        }

        if (request.ListOption <= 0)
        {
            throw new InvalidOperationException("ConnectAfrica list option must be greater than zero.");
        }

        var xml = new XElement("catvend",
            BuildCredentialElements(),
            new XElement("meter", request.Meter.Trim()),
            new XElement("number_of_tokens", request.NumberOfTokens.ToString(CultureInfo.InvariantCulture)),
            new XElement("list_option", request.ListOption.ToString(CultureInfo.InvariantCulture)));

        return xml.ToString(SaveOptions.DisableFormatting);
    }

    private string BuildMeterPayload(string meter)
    {
        if (string.IsNullOrWhiteSpace(meter))
        {
            throw new InvalidOperationException("ConnectAfrica meter is required.");
        }

        var xml = new XElement("catvend",
            BuildCredentialElements(),
            new XElement("meter", meter.Trim()));

        return xml.ToString(SaveOptions.DisableFormatting);
    }

    private string BuildCredentialsPayload()
    {
        var xml = new XElement("catvend", BuildCredentialElements());
        return xml.ToString(SaveOptions.DisableFormatting);
    }

    private object[] BuildCredentialElements()
    {
        EnsureCredentialsConfigured();

        return
        [
            new XElement("validation_code", options.ValidationCode),
            new XElement("unit", ResolveUnit()),
            new XElement("username", options.Username),
            new XElement("password", options.Password)
        ];
    }

    private HttpRequestMessage BuildXmlRequestMessage(HttpMethod method, Uri requestUri, string xmlBody)
    {
        var message = new HttpRequestMessage(method, requestUri)
        {
            Content = new StringContent(xmlBody, Encoding.UTF8, ContentType)
        };

        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentType));
        return message;
    }

    private void LogHttpRequest(string operationDescription, HttpRequestMessage message, string xmlBody)
    {
        logger.LogInformation(
            "ConnectAfrica HTTP outbound. What={What}, Method={Method}, Uri={Uri}, Body={Body}",
            operationDescription,
            message.Method,
            message.RequestUri,
            RedactSensitiveXmlForLog(xmlBody));
    }

    private void LogHttpResponse(string operationDescription, HttpResponseMessage response, string responseBody)
    {
        logger.LogInformation(
            "ConnectAfrica HTTP inbound. What={What}, StatusCode={StatusCode}, ReasonPhrase={ReasonPhrase}, Body={Body}",
            operationDescription,
            (int)response.StatusCode,
            response.ReasonPhrase,
            RedactSensitiveXmlForLog(responseBody));
    }

    private static string RedactSensitiveXmlForLog(string? xml)
    {
        if (string.IsNullOrEmpty(xml))
        {
            return string.Empty;
        }

        // Mask <password>…</password> in request/response XML so logs are safe to store.
        return LogPasswordTagRegex.Replace(xml, "<password>***</password>");
    }

    private void EnsureCredentialsConfigured()
    {
        if (string.IsNullOrWhiteSpace(options.ValidationCode) ||
            string.IsNullOrWhiteSpace(options.Username) ||
            string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException(
                "ConnectAfrica credentials are not fully configured in app settings.");
        }
    }

    private string ResolveUnit()
    {
        if (string.IsNullOrWhiteSpace(options.DefaultUnit))
        {
            throw new InvalidOperationException("ConnectAfrica unit is required in app settings.");
        }

        return options.DefaultUnit.Trim();
    }

    private Uri BuildRequestUri(string relativePath)
    {
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException("ConnectAfrica BaseUrl must be a valid absolute URL.");
        }

        return new Uri(baseUri, relativePath.TrimStart('/'));
    }

    private static bool TryParseVendResponse(
        string requestBody,
        string? xml,
        out ConnectAfricaVendResponse response)
    {
        response = null!;
        if (string.IsNullOrWhiteSpace(xml))
        {
            return false;
        }

        try
        {
            var document = XDocument.Parse(xml);
            var vend = document.Descendants("vend").FirstOrDefault();
            if (vend is null)
            {
                return false;
            }

            var successValue = vend.Element("success")?.Value?.Trim();
            var isSuccess = string.Equals(successValue, "1", StringComparison.OrdinalIgnoreCase);

            var receipt = ConnectAfricaMeterChargeReceiptXmlReader.ReadFromVendXml(xml);
            var receiptNumber = receipt.ReceiptNumber;
            var tokens = receipt.Tokens;

            var tokenCountValue = vend.Element("token-count")?.Value?.Trim();
            int? tokenCount = null;
            if (!string.IsNullOrWhiteSpace(tokenCountValue) &&
                int.TryParse(tokenCountValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedCount))
            {
                tokenCount = parsedCount;
            }
            else if (tokens.Count > 0)
            {
                tokenCount = tokens.Count;
            }

            response = new ConnectAfricaVendResponse(
                isSuccess,
                receiptNumber,
                tokenCount,
                tokens,
                receipt,
                requestBody,
                xml,
                successValue);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private ConnectAfricaVendResponse ParseVendResponse(string requestBody, string xml)
    {
        if (TryParseVendResponse(requestBody, xml, out var response))
        {
            return response;
        }

        throw new InvalidOperationException("ConnectAfrica vend response was empty or invalid.");
    }

    private ConnectAfricaEosResponse ParseEosResponse(string requestBody, string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            throw new InvalidOperationException("ConnectAfrica eos response was empty.");
        }

        var document = XDocument.Parse(xml);
        var eos = document.Descendants("eos").FirstOrDefault();
        if (eos is null)
        {
            throw new InvalidOperationException("ConnectAfrica eos response does not contain eos.");
        }

        var successValue = eos.Element("success")?.Value?.Trim();
        var token = eos.Descendants("token").FirstOrDefault();

        return new ConnectAfricaEosResponse(
            string.Equals(successValue, "1", StringComparison.OrdinalIgnoreCase),
            GetElementValue(token, "eos1"),
            GetElementValue(token, "eos11"),
            GetElementValue(token, "eos2"),
            GetElementValue(token, "eos3"),
            GetElementValue(token, "eos10"),
            GetElementValue(token, "eos60"),
            GetElementValue(token, "eos12"),
            GetElementValue(token, "eos99"),
            requestBody,
            xml,
            successValue);
    }

    private ConnectAfricaReprintResponse ParseReprintResponse(string requestBody, string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            throw new InvalidOperationException("ConnectAfrica reprint response was empty.");
        }

        var document = XDocument.Parse(xml);
        var reprint = document.Descendants("token-reprint").FirstOrDefault();
        if (reprint is null)
        {
            throw new InvalidOperationException("ConnectAfrica reprint response does not contain token-reprint.");
        }

        var successValue = reprint.Element("success")?.Value?.Trim();
        var token = reprint.Descendants("token").FirstOrDefault();

        return new ConnectAfricaReprintResponse(
            string.Equals(successValue, "1", StringComparison.OrdinalIgnoreCase),
            GetElementValue(token, "tk1"),
            GetElementValue(token, "tk4"),
            GetElementValue(token, "tk5"),
            GetElementValue(token, "tk2"),
            GetElementValue(token, "tk3"),
            GetElementValue(token, "tk10"),
            GetElementValue(token, "tk20"),
            GetElementValue(token, "tk23"),
            GetElementValue(token, "tk30"),
            GetElementValue(token, "tk31"),
            GetElementValue(token, "tk32"),
            GetElementValue(token, "tk21"),
            GetElementValue(token, "tk7"),
            GetElementValue(token, "tk22"),
            GetElementValue(token, "tk11"),
            GetElementValues(token, "tk60"),
            GetElementValue(token, "tk14"),
            GetElementValue(token, "tk61"),
            GetElementValue(token, "tk62"),
            GetElementValue(token, "tk63"),
            GetElementValue(token, "tk80"),
            GetElementValue(token, "tk50"),
            GetElementValue(token, "tk40"),
            GetElementValue(token, "tk41"),
            GetElementValue(token, "tk90"),
            GetElementValue(token, "tk71"),
            GetElementValues(token, "tk46"),
            GetElementValues(token, "tk95"),
            GetElementValue(token, "tk43"),
            GetElementValues(token, "tk92"),
            GetElementValues(token, "tk73"),
            GetElementValue(token, "tk12"),
            GetElementValue(token, "tk15"),
            requestBody,
            xml,
            successValue);
    }

    private ConnectAfricaTokenListResponse ParseTokenListResponse(string requestBody, string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            throw new InvalidOperationException("ConnectAfrica token list response was empty.");
        }

        var document = XDocument.Parse(xml);
        var tokenList = document.Descendants("token-list").FirstOrDefault();
        if (tokenList is null)
        {
            throw new InvalidOperationException("ConnectAfrica token list response does not contain token-list.");
        }

        var successValue = tokenList.Element("success")?.Value?.Trim();
        var numberOfTokensValue = tokenList.Element("no-of-tokens")?.Value?.Trim();

        int? numberOfTokens = null;
        if (!string.IsNullOrWhiteSpace(numberOfTokensValue) &&
            int.TryParse(numberOfTokensValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedCount))
        {
            numberOfTokens = parsedCount;
        }

        var entries = ParseTokenListEntries(tokenList.Element("token")?.Value);

        return new ConnectAfricaTokenListResponse(
            string.Equals(successValue, "1", StringComparison.OrdinalIgnoreCase),
            numberOfTokens,
            entries,
            requestBody,
            xml,
            successValue);
    }

    private ConnectAfricaLastTransactionsResponse ParseLastTransactionsResponse(string requestBody, string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            throw new InvalidOperationException("ConnectAfrica last transactions response was empty.");
        }

        var document = XDocument.Parse(xml);
        var transactions = document
            .Descendants("transaction")
            .Select(ParseLastTransactionEntry)
            .ToArray();

        return new ConnectAfricaLastTransactionsResponse(
            transactions,
            requestBody,
            xml);
    }

    private ConnectAfricaLastTransactionStatusResponse ParseLastTransactionStatusResponse(string requestBody, string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            throw new InvalidOperationException("ConnectAfrica last transaction status response was empty.");
        }

        var document = XDocument.Parse(xml);
        var transactionNode = document.Descendants("transactions").FirstOrDefault();
        if (transactionNode is null)
        {
            throw new InvalidOperationException("ConnectAfrica last transaction status response does not contain transactions.");
        }

        return new ConnectAfricaLastTransactionStatusResponse(
            GetElementValue(transactionNode, "transaction_id"),
            GetElementValue(transactionNode, "status"),
            requestBody,
            xml);
    }

    private ConnectAfricaLastTransactionEntry ParseLastTransactionEntry(XElement element)
    {
        var dateValue = GetElementValue(element, "date");
        DateOnly? date = null;
        if (!string.IsNullOrWhiteSpace(dateValue) &&
            DateOnly.TryParseExact(dateValue, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            date = parsedDate;
        }

        return new ConnectAfricaLastTransactionEntry(
            date,
            ParseDecimal(GetElementValue(element, "price")),
            GetElementValue(element, "receipt_number"));
    }

    private IReadOnlyList<ConnectAfricaTokenListEntry> ParseTokenListEntries(string? tokenValue)
    {
        if (string.IsNullOrWhiteSpace(tokenValue))
        {
            return Array.Empty<ConnectAfricaTokenListEntry>();
        }

        var lines = tokenValue
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var entries = new List<ConnectAfricaTokenListEntry>(lines.Length);
        foreach (var line in lines)
        {
            var parts = line
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length == 0)
            {
                continue;
            }

            var receiptNumber = parts[0];
            decimal? amount = parts.Length > 1 ? ParseDecimal(parts[^1]) : null;
            entries.Add(new ConnectAfricaTokenListEntry(receiptNumber, amount));
        }

        return entries;
    }

    private string? GetElementValue(XElement? parent, string elementName)
    {
        return parent?
            .Elements(elementName)
            .Select(element => NormalizeElementValue(element.Value))
            .FirstOrDefault(value => value is not null);
    }

    private IReadOnlyList<string> GetElementValues(XElement? parent, string elementName)
    {
        if (parent is null)
        {
            return Array.Empty<string>();
        }

        return parent
            .Elements(elementName)
            .Select(element => NormalizeElementValue(element.Value))
            .Where(value => value is not null)
            .Cast<string>()
            .ToArray();
    }

    private string? NormalizeElementValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (decimal.TryParse(value.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedValue))
        {
            return parsedValue;
        }

        if (decimal.TryParse(value.Trim(), NumberStyles.Number, CultureInfo.GetCultureInfo("fr-FR"), out parsedValue))
        {
            return parsedValue;
        }

        logger.LogWarning("ConnectAfrica response contained a non-numeric decimal value: {Value}", value);
        return null;
    }

    private static async Task<string> ReadResponseBodyAsync(HttpResponseMessage response)
    {
        var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        if (bytes.Length == 0)
            return string.Empty;

        var charset = response.Content.Headers.ContentType?.CharSet;
        if (!string.IsNullOrWhiteSpace(charset))
        {
            try
            {
                return Encoding.GetEncoding(charset.Trim('\"', ' ')).GetString(bytes);
            }
            catch (ArgumentException)
            {
                // Fall back to ISO-8859-1 below.
            }
        }

        return Encoding.Latin1.GetString(bytes);
    }
}
