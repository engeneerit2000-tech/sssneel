using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Billing;

/// <summary>
/// Tracks freshly inserted invoices that should trigger a later notification.
/// </summary>
public sealed class NewInvoiceNotificationEntity : TrackedEntity<Guid>
{
    private NewInvoiceNotificationEntity() { }

    public Guid InvoiceId { get; set; }
    public InvoiceEntity Invoice { get; set; } = null!;
    public TimeSpan NotificationTriggerTime { get; set; }
    public bool Processed { get; set; }
    public DateTimeOffset? ProcessedAtUtc { get; set; }

    public static NewInvoiceNotificationEntity Create(Guid invoiceId, TimeSpan notificationTriggerTime)
    {
        return new NewInvoiceNotificationEntity
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            NotificationTriggerTime = notificationTriggerTime,
            Processed = false
        };
    }
}
