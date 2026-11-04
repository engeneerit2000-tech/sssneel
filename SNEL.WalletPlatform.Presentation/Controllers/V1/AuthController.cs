using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.Auth.Interfaces;
using SNEL.WalletPlatform.Application.Auth.Models.Input;
using SNEL.WalletPlatform.Application.Auth.Models.Output;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Presentation.Common;

namespace SNEL.WalletPlatform.Presentation.Controllers.V1;

[Route("api/v1/auth")]
public sealed class AuthController(IAuthService authService) : Controllers.ApiControllerBase
{
    #region Registration
    [AllowAnonymous]
    [HttpPost("registration/generate-otp")]
    [ProducesResponseType(typeof(ApiResponse<OtpChallengeOutputModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<OtpChallengeOutputModel>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<OtpChallengeOutputModel>>> GenerateRegistrationOtp(
        [FromBody] GenerateRegistrationOtpInputModel request)
    {
        var result = await authService.GenerateRegistrationOtpAsync(request);
        return FromResult(result);
    }

    [AllowAnonymous]
    [HttpPost("registration/verify-otp")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> VerifyRegistrationOtp(
    [FromBody] VerifyOtpInputModel request)
    {
        var result = await authService.VerifyRegistrationOtpAsync(request);
        return FromResult(result);
    }

    [AllowAnonymous]
    [HttpPost("registration/complete")]
    [ProducesResponseType(typeof(ApiResponse<CompleteRegistrationOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CompleteRegistrationOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CompleteRegistrationOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CompleteRegistrationOutputModel>>> CompleteRegistration(
        [FromBody] CompleteRegistrationInputModel request)
    {
        var result = await authService.CompleteRegistrationAsync(request);
        return FromResult(result);
    }

    #endregion

    #region Change Password
    [Authorize(Roles = IdentityRoleConstants.Customer)]
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword(
        [FromBody] ChangeUserPasswordInputModel request)
    {
        var result = await authService.ChangeUserPassowrdAsync(request);
        return FromResult(result);
    }
    
    #endregion
    
    #region Forgot Password
    [AllowAnonymous]
    [HttpPost("forgot-password/generate-otp")]
    [ProducesResponseType(typeof(ApiResponse<OtpChallengeOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OtpChallengeOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OtpChallengeOutputModel>>> GenerateForgotPasswordOtp(
        [FromBody] GenerateForgotPasswordOtpInputModel request)
    {
        var result = await authService.GenerateForgotPasswordOtpAsync(request);
        return FromResult(result);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password/verify-otp")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> VerifyForgotPasswordOtp(
        [FromBody] VerifyOtpInputModel request)
    {
        var result = await authService.VerifyForgotPasswordOtpAsync(request);
        return FromResult(result);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password/complete")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> CompleteForgotPassword(
        [FromBody] CompleteForgotPasswordInputModel request)
    {
        var result = await authService.CompleteForgotPasswordAsync(request);
        return FromResult(result);
    }

    #endregion

    #region Change Mobile Number
    [Authorize(Roles = IdentityRoleConstants.Customer)]
    [HttpPost("change-mobile/generate-otp")]
    [ProducesResponseType(typeof(ApiResponse<OtpChallengeOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OtpChallengeOutputModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<OtpChallengeOutputModel>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<OtpChallengeOutputModel>>> GenerateChangeMobileNoOtp(
        [FromBody] GenerateMobileNoOtpInputModel request)
    {
        var result = await authService.GenerateChangeMobileNoOtpAsync(request);
        return FromResult(result);
    }

    [Authorize(Roles = IdentityRoleConstants.Customer)]
    [HttpPost("change-mobile/verify-and-complete-otp")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> VerifyChangeMobileNoOtp(
        [FromBody] VerifyOtpInputModel request)
    {
        var result = await authService.VerifyChangeMobileNoOtpAsync(request);
        return FromResult(result);
    }
    #endregion

    [Authorize(Roles = IdentityRoleConstants.Customer)]
    [HttpDelete]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete()
    {
        var result = await authService.DeleteAsync();
        return FromResult(result);
    }

    #region Cloud Messaging Tokens
    [HttpPost("cm-token")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<bool>>> CreateCloudMessagingToken([FromBody] CreateUserCloudMessagingTokenInputModel model)
    {
        var result = await authService.CreateCloudMessagingTokenAsync(model);
        return FromResult(result);
    }

    [HttpDelete("cm-token")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCloudMessagingToken([FromBody] DeleteUserCloudMessagingTokenInputModel model)
    {
        var result = await authService.DeleteCloudMessagingTokenAsync(model);
        return FromResult(result);
    }

    #endregion
}
