using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Webhooks;

public sealed class MpesaWebhookEventEntity : TrackedEntity<Guid>
{
    private MpesaWebhookEventEntity()
    {
        ThirdPartyConversationId = string.Empty;
        ResultCode = string.Empty;
    }

    private MpesaWebhookEventEntity(string? originalConversationId, string thirdPartyConversationId,
        string? transactionId, string resultCode, string? resultDescription, WebhookEventStatus status)
    {
        OriginalConversationId = originalConversationId;
        ThirdPartyConversationId = thirdPartyConversationId;
        TransactionId = transactionId;
        ResultCode = resultCode;
        ResultDescription = resultDescription;
        Status = status;
    }

    public string? OriginalConversationId { get; private set; }
    public string ThirdPartyConversationId { get; private set; }
    public string? TransactionId { get; private set; }
    public string ResultCode { get; private set; }
    public string? ResultDescription { get; private set; }
    public WebhookEventStatus Status { get; private set; }

    public static MpesaWebhookEventEntity Create(string? originalConversationId,
        string thirdPartyConversationId, string? transactionId, string resultCode, string? resultDescription)
    {
        if (string.IsNullOrWhiteSpace(thirdPartyConversationId))
            throw new ArgumentException("ThirdPartyConversationId is required.", nameof(thirdPartyConversationId));
        if (string.IsNullOrWhiteSpace(resultCode))
            throw new ArgumentException("ResultCode is required.", nameof(resultCode));

        return new MpesaWebhookEventEntity(
            originalConversationId?.Trim(), thirdPartyConversationId.Trim(), transactionId?.Trim(),
            resultCode.Trim(), resultDescription?.Trim(), WebhookEventStatus.Pending);
    }

    public void ChangeStatus(WebhookEventStatus status) => Status = status;
}
