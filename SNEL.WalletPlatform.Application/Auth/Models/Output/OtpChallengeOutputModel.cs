namespace SNEL.WalletPlatform.Application.Auth.Models.Output;

public sealed class OtpChallengeOutputModel
{
    public Guid Id { get; set; }
    public DateTime ExpiresAt { get; set; }
}
