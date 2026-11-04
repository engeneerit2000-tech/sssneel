using SNEL.WalletPlatform.Application.Auth.Models.Input;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Notification.Interfaces;

namespace SNEL.WalletPlatform.Application.Auth.Services;

public partial class AuthService
{
    public async Task<Result<bool>> CreateCloudMessagingTokenAsync(CreateUserCloudMessagingTokenInputModel model)
    {
        if (string.IsNullOrWhiteSpace(currentUserContext.UserId))
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.UnAuthorizedDefaultMessage],
                AppHttpStatusCodes.BadRequest);
        }

        var user = await unitOfWork.UserRepository.GetByIamRefAsync(currentUserContext.UserId);
        if (user is null)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.CustomerNotFound, currentUserContext.UserId],
                AppHttpStatusCodes.NotFound);
        }

        var result = await notificationManager.CreateCloudMessagingTokenAsync(
            user.Id,
            model.TokenId,
            model.MobilePlatformId,
            model.DeviceName);

        if (!result.IsSuccess)
        {
            return Result<bool>.Failure(
                result.Message ?? localizer[LocalizationKeys.InternalServerErrorDefaultMessage],
                result.StatusCode);
        }

        return Result<bool>.Success(
            true,
            localizer[LocalizationKeys.CloudTokenCreateSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<bool>> DeleteCloudMessagingTokenAsync(DeleteUserCloudMessagingTokenInputModel model)
    {
        if (string.IsNullOrWhiteSpace(currentUserContext.UserId))
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.UnAuthorizedDefaultMessage],
                AppHttpStatusCodes.BadRequest);
        }

        var user = await unitOfWork.UserRepository.GetByIamRefAsync(currentUserContext.UserId);
        if (user is null)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.CustomerNotFound, currentUserContext.UserId],
                AppHttpStatusCodes.NotFound);
        }

        var result = await notificationManager.DeleteCloudMessagingTokenAsync(user.Id, model.TokenId);
        if (!result.IsSuccess)
        {
            return Result<bool>.Failure(
                result.Message ?? localizer[LocalizationKeys.CloudTokenNotFound],
                result.StatusCode);
        }

        return Result<bool>.Success(true);
    }
}
