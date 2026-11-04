using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SNEL.WalletPlatform.Application.Payments;
using SNEL.WalletPlatform.Application.Payments.Models.External.Orange;

namespace SNEL.WalletPlatform.Infrastructure.Payments.Clients;

public sealed class OrangeHttpClient(HttpClient httpClient, ILogger<OrangeHttpClient> logger)
{
    public Task<HttpResponseMessage> RequestTokenAsync(
        PaymentMethodConfigReader.OrangeCredentials credentials,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, BuildUri(credentials.BaseUrl, credentials.TokenPath))
        {
            Content = new FormUrlEncodedContent([new("grant_type", "client_credentials")])
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", NormalizeBasic(credentials.BasicAuth));
        return SendAsync(request, cancellationToken);
    }

    public Task<HttpResponseMessage> WithdrawAsync(
        PaymentMethodConfigReader.OrangeCredentials credentials,
        string country,
        string token,
        OrangeWithdrawRequestModel body,
        CancellationToken cancellationToken = default)
    {
        var path = credentials.WithdrawPath.Replace("{country}", Uri.EscapeDataString(country), StringComparison.OrdinalIgnoreCase);
        var request = new HttpRequestMessage(HttpMethod.Post, BuildUri(credentials.BaseUrl, path))
        {
            Content = JsonContent.Create(body)
        };
        AddBearerHeader(request, token);
        return SendAsync(request, cancellationToken);
    }

    public Task<HttpResponseMessage> GetStatusAsync(
        PaymentMethodConfigReader.OrangeCredentials credentials,
        string country,
        string transactionId,
        string token,
        CancellationToken cancellationToken = default)
    {
        var path = credentials.StatusPath
            .Replace("{country}", Uri.EscapeDataString(country), StringComparison.OrdinalIgnoreCase)
            .Replace("{transactionId}", Uri.EscapeDataString(transactionId), StringComparison.OrdinalIgnoreCase);
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUri(credentials.BaseUrl, path));
        AddBearerHeader(request, token);
        return SendAsync(request, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using (request)
        {
            logger.LogDebug("Orange {Method} {Url}", request.Method, request.RequestUri);
            return await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }

    private static void AddBearerHeader(HttpRequestMessage request, string token) =>
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    private static string NormalizeBasic(string value) =>
        value.Trim().StartsWith("Basic ", StringComparison.OrdinalIgnoreCase)
            ? value.Trim()["Basic ".Length..].Trim()
            : value.Trim();

    private static Uri BuildUri(string baseUrl, string path) =>
        new(new Uri(baseUrl.TrimEnd('/') + "/"), path.TrimStart('/'));
}
