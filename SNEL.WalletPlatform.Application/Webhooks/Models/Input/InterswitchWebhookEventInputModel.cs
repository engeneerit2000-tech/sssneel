namespace SNEL.WalletPlatform.Application.Webhooks.Models.Input;

public class InterswitchWebhookEventInputModel
{
    public required string Event { get; init; }
    public required string Uuid { get; init; }
    public long Timestamp { get; init; }
    public required InterswitchWebhookEventDataInputModel Data { get; init; }
}