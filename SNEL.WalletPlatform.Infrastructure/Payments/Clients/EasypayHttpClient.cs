using Microsoft.Extensions.Logging;
using SNEL.WalletPlatform.Application.Payments.Models.External.Easypay;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SNEL.WalletPlatform.Infrastructure.Payments.Clients;

public sealed class EasypayHttpClient(HttpClient httpClient, ILogger<EasypayHttpClient> logger)
{
    public async Task<HttpResponseMessage> PostLoginAsync(
        string loginUrl,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(loginUrl, UriKind.Absolute, out var loginUri))
        {
            throw new InvalidOperationException("Easypay login URL must be a valid absolute URL.");
        }

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, loginUri);
        httpRequest.Headers.Add("Authorization", $"Basic {apiKey}");        
        logger.LogDebug("Easypay management login POST {Url} for user api-key", loginUri);

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
        if (!Uri.TryCreate(managementBaseUrl, UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException("Easypay base URL must be a valid absolute URL.");
        }

        var requestUri = new Uri(baseUri, "status");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(new { referenceId = merchantReference })
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);        
        logger.LogDebug(
            "Easypay management POST transaction status {Url} for merchantReference {MerchantReference}",
            requestUri,
            merchantReference);

        return await httpClient
            .SendAsync(httpRequest, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> PostCustomerDepositAsync(
        string baseUrl,
        EasypayCustomerDepositRequestModel request,
        string token,
        CancellationToken cancellationToken = default)
        => await PostCollectionAsync(
            baseUrl,
            request,
            request.ReferenceId,
            token,
            cancellationToken).ConfigureAwait(false);

    public async Task<HttpResponseMessage> PostCardCollectionAsync(
        string baseUrl,
        EasypayCardCollectionRequestModel request,
        string token,
        CancellationToken cancellationToken = default)
        => await PostCollectionAsync(
            baseUrl,
            request,
            request.ReferenceId,
            token,
            cancellationToken).ConfigureAwait(false);

    private async Task<HttpResponseMessage> PostCollectionAsync<TRequest>(
        string baseUrl,
        TRequest request,
        string referenceId,
        string token,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException("Easypay base URL must be a valid absolute URL.");
        }
        var requestUri = new Uri(baseUri, "collections");
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(request)
        };

        // 
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        logger.LogDebug(
            "Easypay HTTP POST {Url} for merchantReference {Reference} (Authorization: Bearer token)",
            requestUri,
            referenceId);

        var response = await httpClient
            .SendAsync(httpRequest, cancellationToken)
            .ConfigureAwait(false);

        logger.LogDebug(
            "Easypay HTTP response for merchantReference {MerchantReference}: {StatusCode}",
            referenceId,
            (int)response.StatusCode);

        return response;
    }
}
