using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SNEL.WalletPlatform.Application.Payments.Models.External.Afrimony;

namespace SNEL.WalletPlatform.Infrastructure.Payments.Clients;

public sealed class AfrimonyHttpClient(HttpClient httpClient, ILogger<AfrimonyHttpClient> logger)
{
    public async Task<HttpResponseMessage> PostOAuthTokenAsync(
        string oauthUrl,
        string oauthBasic,
        CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(oauthUrl, UriKind.Absolute, out var oauthUri))
        {
            throw new InvalidOperationException("Afrimony OAuth URL must be a valid absolute URL.");
        }

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, oauthUri)
        {
            Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            ])
        };

        var basicValue = oauthBasic.Trim();
        if (basicValue.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            basicValue = basicValue["Basic ".Length..].Trim();
        }

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicValue);

        logger.LogDebug("Afrimony OAuth POST {Url}", oauthUri);

        return await httpClient
            .SendAsync(httpRequest, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> PostMerchantPayAsync(
        string baseUrl,
        AfrimonyMerchantPayRequestModel request,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var requestUri = BuildApiUri(baseUrl, "v1.1/mm/merchantpay");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        logger.LogDebug(
            "Afrimony merchantpay POST {Url} for externalReferenceId {ExternalReferenceId}",
            requestUri,
            request.ExternalReferenceId);

        return await httpClient
            .SendAsync(httpRequest, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> PostTransactionEnquiryAsync(
        string baseUrl,
        AfrimonyTransactionEnquiryRequestModel request,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var requestUri = BuildApiUri(baseUrl, "v1.1/mm/TransactionEnquiry");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        logger.LogDebug(
            "Afrimony TransactionEnquiry POST {Url} for REFERENCEID {ReferenceId}",
            requestUri,
            request.Command.ReferenceId);

        return await httpClient
            .SendAsync(httpRequest, cancellationToken)
            .ConfigureAwait(false);
    }

    private static Uri BuildApiUri(string baseUrl, string relativePath)
    {
        if (!Uri.TryCreate(baseUrl.TrimEnd('/') + "/", UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException("Afrimony base URL must be a valid absolute URL.");
        }

        return new Uri(baseUri, relativePath);
    }
}
