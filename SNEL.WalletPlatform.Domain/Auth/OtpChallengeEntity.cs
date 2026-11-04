using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Auth;

public sealed class OtpChallengeEntity : TrackedEntity<Guid>
{
    private OtpChallengeEntity()
    {
        MobileNo = string.Empty;
        OtpHash = string.Empty;
    }

    private OtpChallengeEntity(
        string phoneNumber,
        string otpHash,
        OtpPurpose purpose,
        DateTime expiresAt)
    {
        MobileNo = phoneNumber;
        OtpHash = otpHash;
        Purpose = purpose;
        ExpiresAt = expiresAt;
        AttemptCount = 0;
        Status = OtpStatus.Pending;
    }

    public string MobileNo { get; private set; }
    public string OtpHash { get; private set; }
    public int AttemptCount { get; private set; }
    public OtpPurpose Purpose { get; private set; }
    public OtpStatus Status { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? VerifiedAt { get; private set; }

    public static OtpChallengeEntity Create(
        string phoneNumber,
        string otpHash,
        OtpPurpose purpose,
        int expireInMinutes)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Mobile number cannot be empty", nameof(phoneNumber));

        if (string.IsNullOrWhiteSpace(otpHash))
            throw new ArgumentException("OTP hash cannot be empty", nameof(otpHash));

        var expiresAt = DateTime.UtcNow.AddMinutes(expireInMinutes);

        return new OtpChallengeEntity(
            phoneNumber,
            otpHash,
            purpose,
            expiresAt);
    }

    public bool IsExpired(int maxAttempts)
    {
        return DateTime.UtcNow > ExpiresAt
            || AttemptCount >= maxAttempts;
    }

    public bool CanVerify(int maxAttempts)
    {
        return Status == OtpStatus.Pending
            && AttemptCount < maxAttempts;
    }

    public bool CanConsume()
    {
        return Status == OtpStatus.Verified;
    }

    public void IncrementAttempt()
    {
        AttemptCount++;
    }

    public void MarkAsVerified()
    {
        if (Status != OtpStatus.Pending)
            throw new InvalidOperationException($"Cannot verify OTP with status: {Status}");

        VerifiedAt = DateTime.UtcNow;
        Status = OtpStatus.Verified;
    }

    public void MarkAsConsumed()
    {
        if (Status != OtpStatus.Verified)
            throw new InvalidOperationException($"Cannot consume OTP with status: {Status}");

        Status = OtpStatus.Consumed;
    }

    public void MarkAsExpired()
    {
        if (Status != OtpStatus.Pending)
            throw new InvalidOperationException($"Cannot use OTP with status: {Status}");

        Status = OtpStatus.Expired;
    }
}
