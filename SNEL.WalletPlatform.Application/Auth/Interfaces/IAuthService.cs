using SNEL.WalletPlatform.Application.Auth.Models.Input;
using SNEL.WalletPlatform.Application.Auth.Models.Output;
using SNEL.WalletPlatform.Application.Common;

namespace SNEL.WalletPlatform.Application.Auth.Interfaces;

public interface IAuthService
{
    Task<Result<bool>> ChangeUserPassowrdAsync(ChangeUserPasswordInputModel model);
    Task<Result<OtpChallengeOutputModel>> GenerateChangeMobileNoOtpAsync(GenerateMobileNoOtpInputModel model);
    Task<Result<OtpChallengeOutputModel>> GenerateForgotPasswordOtpAsync(GenerateForgotPasswordOtpInputModel model);
    Task<Result<bool>> VerifyChangeMobileNoOtpAsync(VerifyOtpInputModel model);
    Task<Result<bool>> VerifyForgotPasswordOtpAsync(VerifyOtpInputModel model);
    Task<Result<bool>> CompleteForgotPasswordAsync(CompleteForgotPasswordInputModel model);
    Task<Result<bool>> VerifyRegistrationOtpAsync(VerifyOtpInputModel model);
    Task<Result<CompleteRegistrationOutputModel>> CompleteRegistrationAsync(CompleteRegistrationInputModel model);
    Task<Result<OtpChallengeOutputModel>> GenerateRegistrationOtpAsync(GenerateRegistrationOtpInputModel model);
    Task<Result<bool>> CreateCloudMessagingTokenAsync(CreateUserCloudMessagingTokenInputModel model);
    Task<Result<bool>> DeleteCloudMessagingTokenAsync(DeleteUserCloudMessagingTokenInputModel model);
    Task<Result<bool>> DeleteAsync();
}
