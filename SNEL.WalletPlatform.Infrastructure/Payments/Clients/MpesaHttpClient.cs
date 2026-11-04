using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using SNEL.WalletPlatform.Application.Payments;
using SNEL.WalletPlatform.Application.Payments.Models.External.Mpesa;

namespace SNEL.WalletPlatform.Infrastructure.Payments.Clients;

public sealed class MpesaHttpClient(HttpClient httpClient, ILogger<MpesaHttpClient> logger)
{
    public Task<HttpResponseMessage> GetSessionAsync(
        PaymentMethodConfigReader.MpesaCredentials credentials,
        CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Get, credentials, credentials.SessionPath, credentials.ApiKey, null, cancellationToken);

    public Task<HttpResponseMessage> PostC2bPaymentAsync(
        PaymentMethodConfigReader.MpesaCredentials credentials,
        string sessionId,
        MpesaC2bPaymentRequestModel request,
        CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Post, credentials, credentials.PaymentPath, sessionId, request, cancellationToken);

    public Task<HttpResponseMessage> QueryTransactionStatusAsync(
        PaymentMethodConfigReader.MpesaCredentials credentials,
        string sessionId,
        IReadOnlyDictionary<string, string> query,
        CancellationToken cancellationToken = default)
    {
        var queryString = string.Join("&", query.Select(x =>
            $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));
        var path = credentials.StatusPath +
            (credentials.StatusPath.Contains('?') ? "&" : "?") + queryString;
        return SendAsync(HttpMethod.Get, credentials, path, sessionId, null, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        PaymentMethodConfigReader.MpesaCredentials credentials,
        string path,
        string key,
        object? body,
        CancellationToken cancellationToken)
    {
        var uri = new Uri(new Uri(credentials.BaseUrl.TrimEnd('/') + "/"), path.TrimStart('/'));
        using var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            EncryptKey(key, credentials.PublicKey));
        request.Headers.TryAddWithoutValidation("Origin", credentials.Origin);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        logger.LogDebug("MPesa {Method} {Url}", method, uri);
        return await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    internal static string EncryptKey(string value, string publicKey)
    {
        var normalized = publicKey
            .Replace("-----BEGIN PUBLIC KEY-----", string.Empty, StringComparison.Ordinal)
            .Replace("-----END PUBLIC KEY-----", string.Empty, StringComparison.Ordinal)
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", string.Empty, StringComparison.Ordinal)
            .Trim();
        using var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(normalized), out _);
        return Convert.ToBase64String(
            rsa.Encrypt(Encoding.UTF8.GetBytes(value), RSAEncryptionPadding.Pkcs1));
    }
}
