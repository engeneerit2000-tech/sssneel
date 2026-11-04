using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SNEL.WalletPlatform.Application.Payments.Models.External.AkibaPay;

namespace SNEL.WalletPlatform.Infrastructure.Payments.Clients;

public sealed class AkibaPayHttpClient(HttpClient httpClient, ILogger<AkibaPayHttpClient> logger)
{
    public async Task<HttpResponseMessage> PostLoginAsync(
        string loginUrl,
        AkibaPayLoginRequestModel request,
        CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(loginUrl, UriKind.Absolute, out var loginUri))
        {
            throw new InvalidOperationException("AkibaPay login URL must be a valid absolute URL.");
        }

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, loginUri)
        {
            Content = JsonContent.Create(request)
        };

        logger.LogDebug("AkibaPay management login POST {Url} for user {Login}", loginUri, request.Login);

        return await httpClient
            .SendAsync(httpRequest, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> GetTransactionStatusAsync(
        string managementBaseUrl,
        string accessToken,
        string merchantReference,
        CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(managementBaseUrl.TrimEnd('/') + "/", UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException("AkibaPay management base URL must be a valid absolute URL.");
        }

        var requestUri = new Uri(baseUri, $"api/transaction/status/{Uri.EscapeDataString(merchantReference)}");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        logger.LogDebug(
            "AkibaPay management GET transaction status {Url} for merchantReference {MerchantReference}",
            requestUri,
            merchantReference);

        return await httpClient
            .SendAsync(httpRequest, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> PostCustomerDepositAsync(
        string baseUrl,
        AkibaPayCustomerDepositRequestModel request,
        string merchantId,
        CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException("AkibaPay base URL must be a valid absolute URL.");
        }

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, baseUri)
        {
            Content = JsonContent.Create(request)
        };

        // AkibaPay gateway auth: Bearer {merchantId} — not the SNEL user JWT.
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", merchantId);

        logger.LogDebug(
            "AkibaPay HTTP POST {Url} for merchantReference {MerchantReference} (Authorization: Bearer merchantId)",
            baseUri,
            request.MerchantReference);

        var response = await httpClient
            .SendAsync(httpRequest, cancellationToken)
            .ConfigureAwait(false);

        logger.LogDebug(
            "AkibaPay HTTP response for merchantReference {MerchantReference}: {StatusCode}",
            request.MerchantReference,
            (int)response.StatusCode);

        return response;
    }
}
