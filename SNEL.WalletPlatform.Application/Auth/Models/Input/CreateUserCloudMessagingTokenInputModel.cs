
namespace SNEL.WalletPlatform.Application.Auth.Models.Input;

public class CreateUserCloudMessagingTokenInputModel
{
    public required string TokenId { get; set; }
    public int? MobilePlatformId { get; set; }
    public string? DeviceName { get; set; }
}
