using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using SNEL.WalletPlatform.Application.Auth.Interfaces;
using SNEL.WalletPlatform.Application.Auth.Models.Options;
using SNEL.WalletPlatform.Application.Auth.Models.Output;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Domain.Auth;
using System.Security.Cryptography;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Application.Notification.Enums;
using SNEL.WalletPlatform.Application.Notification.Models.Internal;
using SNEL.WalletPlatform.Application.Notification.Services;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Users;

namespace SNEL.WalletPlatform.Application.Auth.Services;

public sealed class OtpChallengeService(
    IUnitOfWork unitOfWork,
    IOptions<OtpConfigurationOptions> otpConfiguration,
    IStringLocalizer<SharedResource> localizer,
    NotificationService notificationService,
    ICurrentUserContext currentUserContext
    ) : IOtpChallengeService
    
{
    public async Task<Result<OtpChallengeOutputModel>> GenerateAndSendOtpAsync(
        string phoneNumber,
        OtpPurpose purpose)
    {
        var pendingOtp = await unitOfWork.OtpChallengeRepository.GetPendingOtpAsync(phoneNumber, purpose);
        if (pendingOtp != null)
        {
            if (!pendingOtp.IsExpired(otpConfiguration.Value.MaxAttempts))
            {
                return Result<OtpChallengeOutputModel>.Success(
                    new OtpChallengeOutputModel() { ExpiresAt = pendingOtp.ExpiresAt, Id = pendingOtp.Id },
                        localizer[LocalizationKeys.OtpChallengeGeneratedSuccess],
                    AppHttpStatusCodes.Ok);
            }

            pendingOtp.MarkAsExpired();
        }

        var otpCode = GenerateRandomOtp(otpConfiguration.Value.OtpLength);
        var otpHash = HashOtp(otpCode);
        
        // Send Notification
        var notificationModel = new NotificationDetailsModel()
        {
            IsTransactional = true,
            NotificationCode = NotificationCodeEnum.OTP,
            NotificationType = NotificationTypeEnum.OTP,
            Params =
            [
                new ParameterDataModel
                {
                    Language = currentUserContext.PreferredLanguage,
                    Params = 
                    [
                        otpCode,
                        otpConfiguration.Value.ExpireInMinutes.ToString()
                    ]
                }
            ],
            SenderId = null,
            OverrideReceiverChannelsPreferences = false,
            RequestedChannels = notificationService.GetTargettedChannels(
                new List<NotificationChannel> { NotificationChannel.SMS },
                isTransactional: true),
            Receivers = null, // no persisted user yet
            AdditionalPhoneNumbers = new List<string> { phoneNumber }
            
        };
        
        bool IsSMSSent =  await notificationService.SendNotificationsAsync(new List<NotificationDetailsModel>() { notificationModel });
        // bool IsSMSSent = true;

        if (!IsSMSSent)
        {
            return Result<OtpChallengeOutputModel>.Failure(
                localizer[LocalizationKeys.CouldNotSendOTPRightNow],
                AppHttpStatusCodes.Failed);
        }

        var otpChallenge = OtpChallengeEntity.Create(
            phoneNumber,
            otpHash,
            purpose,
            otpConfiguration.Value.ExpireInMinutes);

        await unitOfWork.OtpChallengeRepository.AddAsync(otpChallenge);

        return Result<OtpChallengeOutputModel>.Success(
            new OtpChallengeOutputModel() { ExpiresAt = otpChallenge.ExpiresAt, Id = otpChallenge.Id },
            localizer[LocalizationKeys.OtpChallengeGeneratedSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<bool>> VerifyOtpAsync(
        Guid otpChallengeId,
        string otpCode,
        OtpPurpose purpose)
    {
        var pendingOtp = await unitOfWork.OtpChallengeRepository.GetByIdAsync(otpChallengeId);
        if (pendingOtp == null || pendingOtp.Purpose != purpose)
        {
            return Result<bool>.Failure(
              localizer[LocalizationKeys.OtpChallengeNotFound],
              AppHttpStatusCodes.NotFound);
        }

        if (pendingOtp.IsExpired(otpConfiguration.Value.MaxAttempts))
        {
            pendingOtp.MarkAsExpired();

            return Result<bool>.Failure(
                localizer[LocalizationKeys.OtpChallengeExpired],
                AppHttpStatusCodes.BadRequest);
        }

        if (!pendingOtp.CanVerify(otpConfiguration.Value.MaxAttempts))
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.OtpChallengeCannontBeVerified],
                AppHttpStatusCodes.BadRequest);
        }

        pendingOtp.IncrementAttempt();

        if (otpCode != otpConfiguration.Value.DefaultOtp 
            && !VerifyOtpHash(otpCode, pendingOtp.OtpHash))
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.OtpChallengeVerificationFailed],
                AppHttpStatusCodes.BadRequest);
        }

        pendingOtp.MarkAsVerified();

        return Result<bool>.Success(true, AppHttpStatusCodes.Ok);
    }

    public async Task<Result<string>> ConsumeOtpAsync(Guid otpChallengeId)
    {
        var otpChallenge = await unitOfWork.OtpChallengeRepository.GetByIdAsync(otpChallengeId);
        if (otpChallenge == null)
        {
            return Result<string>.Failure(
              localizer[LocalizationKeys.OtpChallengeNotFound],
              AppHttpStatusCodes.NotFound);
        }

        if (!otpChallenge.CanConsume())
        {
            return Result<string>.Failure(
                localizer[LocalizationKeys.OtpChallengeCannontBeConsumed],
                AppHttpStatusCodes.BadRequest);
        }

        otpChallenge.MarkAsConsumed();
        return Result<string>.Success(otpChallenge.MobileNo, AppHttpStatusCodes.Ok);
    }

    private string GenerateRandomOtp(int length)
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var random = BitConverter.ToInt32(bytes, 0) & int.MaxValue;
            return (random % (int)Math.Pow(10, length)).ToString($"D{length}");
        }
    }

    private string HashOtp(string otpCode)
    {
        return BCrypt.Net.BCrypt.HashPassword(otpCode);
    }

    private bool VerifyOtpHash(string otpCode, string otpHash)
    {
        return BCrypt.Net.BCrypt.Verify(otpCode, otpHash);
    }
}
