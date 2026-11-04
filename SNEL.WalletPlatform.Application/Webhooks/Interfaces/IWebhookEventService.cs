
using SNEL.WalletPlatform.Application.Webhooks.Models;
using SNEL.WalletPlatform.Application.Webhooks.Models.Input;

namespace SNEL.WalletPlatform.Application.Webhooks.Interfaces;

public interface IWebhookEventService
{
    Task CreateInterswitchWebhookEventAsync(string rawBody, string? signature);
    Task<AkibaPayWebhookHandleResult> CreateAkibaPayWebhookEventAsync(string rawBody);
    Task<EasypayWebhookHandleResult> CreateEasypayWebhookEventAsync(EasypayWebhookEventInputModel inputModel);
    Task<AfrimonyWebhookHandleResult> CreateAfrimonyWebhookEventAsync(string rawBody);
    Task<MpesaWebhookHandleResult> CreateMpesaWebhookEventAsync(string rawBody);
    Task<OrangeWebhookHandleResult> CreateOrangeWebhookEventAsync(string rawBody);
}
