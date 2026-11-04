using SNEL.WalletPlatform.Application.Auth.Models.Output;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Domain.Auth;

namespace SNEL.WalletPlatform.Application.Auth.Interfaces;

public interface IOtpChallengeService
{
    Task<Result<string>> ConsumeOtpAsync(Guid otpChallengeId);
    Task<Result<OtpChallengeOutputModel>> GenerateAndSendOtpAsync(string phoneNumber, OtpPurpose purpose);
    Task<Result<bool>> VerifyOtpAsync(Guid otpChallengeId, string otpCode, OtpPurpose purpose);
    
}
