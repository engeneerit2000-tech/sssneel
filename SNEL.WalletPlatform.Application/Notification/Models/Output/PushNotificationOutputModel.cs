

namespace SNEL.WalletPlatform.Application.Notification.Models.Output;

public class PushNotificationOutputModel
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public bool IsRead { get; set; }

    public int? ReferenceTypeId { get; set; }
    public string? ReferenceId { get; set; }
    public DateTime SendDate { get; set; }
}
