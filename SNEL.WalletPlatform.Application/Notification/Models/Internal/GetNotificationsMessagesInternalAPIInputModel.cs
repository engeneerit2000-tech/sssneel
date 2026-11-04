using SNEL.WalletPlatform.Application.Common.Enums;


namespace SNEL.WalletPlatform.Application.Notification.Models.Internal;

public class GetNotificationsMessagesInternalAPIInputModel
{
    public string? SortBy { get; set; }
    public bool IsAscending { get; set; }
    public int Index { get; set; } = 1;
    public int Size { get; set; } = 10;


    // filters
    public Guid? ReceiverId { get; set; }
    public LanguageEnum? Language { get; set; }
    public bool? IsRead { get; set; }
}
