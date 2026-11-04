namespace SNEL.WalletPlatform.Domain.Auth;

public enum OtpStatus
{
    Pending = 1,
    Verified = 2,
    Expired = 3,
    Consumed = 4
}
