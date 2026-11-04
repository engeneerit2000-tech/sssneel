using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SNEL.WalletPlatform.Application.Payments.Models.External.Interswitch;

namespace SNEL.WalletPlatform.Infrastructure.Payments.Clients;

public sealed class InterswitchHttpClient(HttpClient httpClient, ILogger<InterswitchHttpClient> logger)
{
    // TODO use Duende package to manage the token
    private readonly HttpClient httpClient = httpClient;
    private readonly ILogger<InterswitchHttpClient> logger = logger;

    public void SetBearerToken(string accessToken)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task<string> GetAccessTokenAsync(
        string interswitchBaseUrl,
        string clientId,
        string secretKey,
        CancellationToken cancellationToken = default)
    {
        var requestUri = new Uri(
            new Uri(interswitchBaseUrl.TrimEnd('/') + "/"),
            "passport/oauth/token?grant_type=client_credentials");

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{secretKey}")));
        request.Content = new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>());

        var response = await httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            logger.LogWarning(
                "Interswitch token request failed with status code {StatusCode}. Body: {ResponseBody}",
                response.StatusCode,
                responseBody);

            throw new InvalidOperationException(
                $"Failed to get Interswitch access token. HTTP {(int)response.StatusCode}.");
        }

        using var document = JsonDocument.Parse(responseBody);
        if (!document.RootElement.TryGetProperty("access_token", out var accessTokenElement))
        {
            throw new InvalidOperationException("Interswitch token response does not include 'access_token'.");
        }

        var accessToken = accessTokenElement.GetString();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException("Interswitch token response includes an empty 'access_token'.");
        }

        return accessToken;
    }

    public Task<HttpResponseMessage> GetTransactionStatusAsync(
        string interswitchBaseUrl,
        string merchantCode,
        string paymentTransactionReference,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        var requestUri = new Uri(
            new Uri(interswitchBaseUrl.TrimEnd('/') + "/"),
            $"collections/api/v1/gettransaction?merchantcode={Uri.EscapeDataString(merchantCode)}&transactionreference={Uri.EscapeDataString(paymentTransactionReference)}&amount={Uri.EscapeDataString(amount.ToString(CultureInfo.InvariantCulture))}");

        return httpClient.GetAsync(requestUri, cancellationToken);
    }

    public async Task<HttpResponseMessage> PostMobileWalletInitializeAsync(
        string interswitchBaseUrl,
        string relativePath,
        InterswitchMobileWalletInitializeRequestModel body,
        CancellationToken cancellationToken = default)
    {
        var baseUri = new Uri(interswitchBaseUrl.TrimEnd('/') + "/");
        var requestUri = new Uri(baseUri, relativePath.TrimStart('/'));
        var json = JsonSerializer.Serialize(body);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await httpClient.PostAsync(requestUri, content, cancellationToken).ConfigureAwait(false);
    }
}
