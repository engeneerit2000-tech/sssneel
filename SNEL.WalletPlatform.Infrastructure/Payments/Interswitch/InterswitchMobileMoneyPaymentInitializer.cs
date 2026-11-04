//using System.Net;
//using System.Text.Json;
//using Microsoft.Extensions.Logging;
//using SNEL.WalletPlatform.Application.Payments.Models.External.Interswitch;
//using SNEL.WalletPlatform.Application.Payments.Models.Internal;
//using SNEL.WalletPlatform.Application.Payments.Models.Output;
//using SNEL.WalletPlatform.Infrastructure.Payments.Clients;

//namespace SNEL.WalletPlatform.Infrastructure.Payments.Interswitch;

///// <summary>
///// Sends the mobile-wallet initialize POST and interprets the response (request body and bearer token are prepared on the connector).
///// </summary>
//public sealed class InterswitchMobileMoneyPaymentInitializer(
//    InterswitchHttpClient interswitchHttpClient,
//    ILogger<InterswitchMobileMoneyPaymentInitializer> logger)
//{
//    public async Task<PaymentInitializeOutputModel> InitializeAsync(
//        PaymentInitContextInternalModel context,
//        InterswitchMobileWalletInitializeRequestModel request,
//        string interswitchBaseUrl,
//        CancellationToken cancellationToken = default)
//    {
//        var response = await interswitchHttpClient.PostMobileWalletInitializeAsync(
//            interswitchBaseUrl,
//            "collections/api/v2/mobile-wallet/initialize",
//            request,
//            cancellationToken).ConfigureAwait(false);

//        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
//        if (response.StatusCode != HttpStatusCode.OK)
//        {
//            logger.LogWarning(
//                "Interswitch mobile wallet initialize failed with status {StatusCode}. Body: {ResponseBody}",
//                response.StatusCode,
//                responseContent);

//            throw new InvalidOperationException(
//                $"Interswitch mobile wallet initialize failed. HTTP {(int)response.StatusCode}. {responseContent}");
//        }

//        try
//        {
//            var parsed = JsonSerializer.Deserialize<InterswitchMobileWalletInitializeResponseModel>(responseContent);
//            if (parsed?.ResponseCode is { Length: > 0 } code &&
//                !string.Equals(code, "00", StringComparison.OrdinalIgnoreCase) &&
//                !string.Equals(code, "0", StringComparison.OrdinalIgnoreCase))
//            {
//                logger.LogWarning(
//                    "Interswitch mobile wallet initialize returned responseCode {Code}. Body: {Body}",
//                    code,
//                    responseContent);
//            }
//        }
//        catch (JsonException)
//        {
//            // Non-JSON success body; treat as success if HTTP status is OK.
//        }

//        return context.Output;
//    }
//}
