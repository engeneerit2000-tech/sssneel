using AutoMapper;
using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.Auth.Interfaces;
using SNEL.WalletPlatform.Application.Auth.Models.Input;
using SNEL.WalletPlatform.Application.Auth.Models.Output;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Customers.Interfaces;
using SNEL.WalletPlatform.Application.Customers.Models.Input;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Application.Interfaces.IAM;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Notification.Enums;
using SNEL.WalletPlatform.Application.Notification.Models.Internal;
using SNEL.WalletPlatform.Application.Notification.Interfaces;
using SNEL.WalletPlatform.Application.Notification.Services;
using SNEL.WalletPlatform.Domain.Auth;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Users;

namespace SNEL.WalletPlatform.Application.Auth.Services;

public sealed partial class AuthService(
    IUnitOfWork unitOfWork,
    IIdentityUserManager identityUserManager,
    IOtpChallengeService otpChallengeService,
    ICurrentUserContext currentUserContext,
    ICustomerService customerService,
    NotificationService notificationService,
    INotificationManager notificationManager,
    IStringLocalizer<SharedResource> localizer,
    IMapper mapper) : IAuthService
{
    #region Registration

    public async Task<Result<OtpChallengeOutputModel>> GenerateRegistrationOtpAsync(
        GenerateRegistrationOtpInputModel model)
    {
        if (await unitOfWork.UserRepository.ExistsByMobileNoAsync(model.MobileNo))
        {
            var normalizedPhone = model.MobileNo.Trim();
            return Result<OtpChallengeOutputModel>.Failure(
                localizer[LocalizationKeys.UserPhoneNumberAlreadyExists, normalizedPhone],
                AppHttpStatusCodes.Conflict);
        }

        var otpResult = await otpChallengeService.GenerateAndSendOtpAsync(
            model.MobileNo,
            OtpPurpose.Registration);

        if (!otpResult.IsSuccess)
        {
            return otpResult;
        }
        
        await unitOfWork.CompleteAsync();

        return otpResult;
    }

    public async Task<Result<bool>> VerifyRegistrationOtpAsync(VerifyOtpInputModel model)
    {
        var verifyResult = await otpChallengeService.VerifyOtpAsync(
            model.OtpChallengeId,
            model.OtpCode,
            OtpPurpose.Registration);

        await unitOfWork.CompleteAsync();

        return verifyResult;
    }

    public async Task<Result<CompleteRegistrationOutputModel>> CompleteRegistrationAsync(
        CompleteRegistrationInputModel model)
    {
        var consumeResult = await otpChallengeService.ConsumeOtpAsync(model.OtpChallengeId);
        if (!consumeResult.IsSuccess)
        {
            return Result<CompleteRegistrationOutputModel>.Failure(
                consumeResult.Message!,
                consumeResult.StatusCode);
        }

        var createCustomerModel = mapper.Map<CreateCustomerInputModel>(model);
        createCustomerModel.MobileNo = consumeResult.Value!;
        var customerResult = await customerService.CreateCustomerAsync(createCustomerModel);
        if (!customerResult.IsSuccess)
        {
            return Result<CompleteRegistrationOutputModel>.Failure(
                customerResult.Message!,
                customerResult.StatusCode);
        }

        return Result<CompleteRegistrationOutputModel>.Success(
            new CompleteRegistrationOutputModel(
                customerResult.Value!.Id,
                customerResult.Value.UserId,
                customerResult.Value.Username,
                customerResult.Value.Status),
            localizer[LocalizationKeys.AuthAccountCreatedSuccessfully],
            AppHttpStatusCodes.Created);
    }

    #endregion

    #region Change Password

    public async Task<Result<bool>> ChangeUserPassowrdAsync(ChangeUserPasswordInputModel model)
    {
        var user =
            await unitOfWork.UserRepository.GetByIamRefWithNotificationsDependenciesAsync(currentUserContext.UserId!);

        if (!await identityUserManager.ValidatePasswordAsync(user!.MobileNo!, model.OldPassword))
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.UserInvalidPassword],
                AppHttpStatusCodes.BadRequest);
        }

        await identityUserManager.SetPasswordAsync(currentUserContext.UserId!, model.NewPassword);

        // Send Notification
        var notificationModel = new NotificationDetailsModel()
        {
            IsTransactional = false,
            NotificationCode = NotificationCodeEnum.PROFILE_PASSWORD_CHANGED,
            NotificationType = NotificationTypeEnum.PROFILE,
            Params = [new ParameterDataModel() { Language = currentUserContext.PreferredLanguage}],
            SenderId = null,
            OverrideReceiverChannelsPreferences = false,
            RequestedChannels = notificationService.GetTargettedChannels(new List<NotificationChannel>()
                { NotificationChannel.Push }),
            Receivers = [user],
        };


        await notificationService.SendNotificationsAsync(new List<NotificationDetailsModel>() { notificationModel });
        return Result<bool>.Success(
            true,
            localizer[LocalizationKeys.AuthPasswordUpdatedSuccessfully],
            AppHttpStatusCodes.Ok);
    }

    #endregion

    #region Forget Password

    public async Task<Result<OtpChallengeOutputModel>> GenerateForgotPasswordOtpAsync(
        GenerateForgotPasswordOtpInputModel model)
    {
        var user = await unitOfWork.UserRepository.GetByMobileNoAsync(model.MobileNo);
        if (user == null)
        {
            return Result<OtpChallengeOutputModel>.Failure(
                localizer[LocalizationKeys.CustomerNotFound, model.MobileNo.Trim()],
                AppHttpStatusCodes.NotFound);
        }

        var otpResult = await otpChallengeService.GenerateAndSendOtpAsync(
            model.MobileNo,
            OtpPurpose.ForgotPassword);

        if (!otpResult.IsSuccess)
        {
            return otpResult;
        }

        await unitOfWork.CompleteAsync();

        return otpResult;
    }

    public async Task<Result<bool>> VerifyForgotPasswordOtpAsync(VerifyOtpInputModel model)
    {
        var verifyResult = await otpChallengeService.VerifyOtpAsync(
            model.OtpChallengeId,
            model.OtpCode,
            OtpPurpose.ForgotPassword);

        await unitOfWork.CompleteAsync();

        return verifyResult;
    }

    public async Task<Result<bool>> CompleteForgotPasswordAsync(CompleteForgotPasswordInputModel model)
    {
        var consumeResult = await otpChallengeService.ConsumeOtpAsync(model.OtpChallengeId);
        if (!consumeResult.IsSuccess)
        {
            return Result<bool>.Failure(
                consumeResult.Message!,
                consumeResult.StatusCode);
        }

        var user = await unitOfWork.UserRepository.GetByMobileNoAsync(consumeResult.Value!);
        if (user == null || string.IsNullOrEmpty(user.IamRef))
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.CustomerNotFound, consumeResult.Value!.Trim()],
                AppHttpStatusCodes.NotFound);
        }

        await identityUserManager.SetPasswordAsync(user.IamRef, model.NewPassword);
        await unitOfWork.CompleteAsync();

        return Result<bool>.Success(
            true,
            localizer[LocalizationKeys.AuthPasswordUpdatedSuccessfully],
            AppHttpStatusCodes.Ok);
    }

    #endregion

    #region Change MobileNo

    public async Task<Result<OtpChallengeOutputModel>> GenerateChangeMobileNoOtpAsync(
        GenerateMobileNoOtpInputModel model)
    {
        if (await unitOfWork.UserRepository.ExistsByMobileNoAsync(model.NewMobileNo))
        {
            var normalizedPhone = model.NewMobileNo.Trim();
            return Result<OtpChallengeOutputModel>.Failure(
                localizer[LocalizationKeys.UserPhoneNumberAlreadyExists, normalizedPhone],
                AppHttpStatusCodes.Conflict);
        }

        var otpResult = await otpChallengeService.GenerateAndSendOtpAsync(
            model.NewMobileNo,
            OtpPurpose.ChangeMobileNo);

        if (!otpResult.IsSuccess)
        {
            return otpResult;
        }


        await unitOfWork.CompleteAsync();

        return otpResult;
    }

    public async Task<Result<bool>> VerifyChangeMobileNoOtpAsync(VerifyOtpInputModel model)
    {
        var verifyResult = await otpChallengeService.VerifyOtpAsync(
            model.OtpChallengeId,
            model.OtpCode,
            OtpPurpose.ChangeMobileNo);

        var user = await unitOfWork.UserRepository.GetByIamRefWithCustomerAndSubscriptionsAsync(currentUserContext.UserId!);
        if (user == null || string.IsNullOrEmpty(user.IamRef))
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.CustomerNotFound, currentUserContext.UserId!],
                AppHttpStatusCodes.NotFound);
        }

        var consumeResult = await otpChallengeService.ConsumeOtpAsync(model.OtpChallengeId);
        if (!consumeResult.IsSuccess)
        {
            return Result<bool>.Failure(
                consumeResult.Message!,
                consumeResult.StatusCode);
        }

        if (await unitOfWork.UserRepository.ExistsByMobileNoAsync(consumeResult.Value!))
        {
            var normalizedPhone = consumeResult.Value!.Trim();
            return Result<bool>.Failure(
                localizer[LocalizationKeys.UserPhoneNumberAlreadyExists, normalizedPhone],
                AppHttpStatusCodes.Conflict);
        }

        var oldMobileNo = user.MobileNo!;
        try
        {
            var updateUserModel = new UpdateIdentityUserRequest(
                user.IamRef,
                consumeResult.Value!,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Status == UserStatus.Active);
            await identityUserManager.UpdateUserAsync(updateUserModel);

            user.UpdateMobileNo(consumeResult.Value!, consumeResult.Value!);
            unitOfWork.UserRepository.Update(user);

            await unitOfWork.CompleteAsync();
            
            // send Notification 
            
            var notificationModel = new NotificationDetailsModel()
            {
                IsTransactional =  false,
                NotificationCode = NotificationCodeEnum.PROFILE_CHANGE_MOBILE_NUMBER,
                NotificationType = NotificationTypeEnum.PROFILE,
                Params = [new ParameterDataModel() { Language = Common.Enums.LanguageEnum.FR, Params = [] }],
                SenderId = null,
                OverrideReceiverChannelsPreferences = false,
                RequestedChannels = notificationService.GetTargettedChannels(new List<NotificationChannel>() { NotificationChannel.Push }),
                Receivers = [user]
            };
        
            await notificationService.SendNotificationsAsync(new List<NotificationDetailsModel>() { notificationModel });
        }
        catch (Exception)
        {
            var updateUserModel = new UpdateIdentityUserRequest(
                user.IamRef,
                oldMobileNo,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Status == UserStatus.Active);
            await identityUserManager.UpdateUserAsync(updateUserModel);

            throw;
        }

        return Result<bool>.Success(
            true,
            localizer[LocalizationKeys.AuthMobileUpdatedSuccessfully],
            AppHttpStatusCodes.Ok);
    }

    #endregion

    public async Task<Result<bool>> DeleteAsync()
    {
        var user = await unitOfWork.UserRepository.GetByIamRefAsync(currentUserContext.UserId!);
        if (user == null || string.IsNullOrEmpty(user.IamRef))
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.CustomerNotFound, currentUserContext.UserId!],
                AppHttpStatusCodes.NotFound);
        }

        if (user.PartnerId is null)
        {
            return Result<bool>.Failure(localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.NotFound);
        }

        var customer = await unitOfWork.CustomerRepository.GetByPartnerIdAsync(user.PartnerId.Value);
        if (customer is null)
        {
            return Result<bool>.Failure(localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.NotFound);
        }

        var partner = await unitOfWork.PartnerRepository.GetByIdAsync(customer.PartnerId);
        if (partner is null)
        {
            return Result<bool>.Failure(localizer[LocalizationKeys.PartnerNotFound], AppHttpStatusCodes.NotFound);
        }

        try
        {
            user.ChangeStatus(UserStatus.Inactive);
            customer.ChangeStatus(CustomerStatus.Inactive);
            partner.Deactivate();

            unitOfWork.UserRepository.Update(user);
            unitOfWork.CustomerRepository.Update(customer);
            unitOfWork.PartnerRepository.Update(partner);

            await unitOfWork.CompleteAsync();

            await identityUserManager.DeleteUserAsync(user.IamRef);
        }
        catch (Exception)
        {
            user.ChangeStatus(UserStatus.Active);
            customer.ChangeStatus(CustomerStatus.Active);
            partner.Activate();
            await unitOfWork.CompleteAsync();

            throw;
        }

        return Result<bool>.Success(
            true,
            localizer[LocalizationKeys.AuthDeleteSuccess],
            AppHttpStatusCodes.Ok);
    }

    private IReadOnlyCollection<NotificationChannel> ResolveNotificationChannels(
        IReadOnlyCollection<int>? channels)
    {
        if (channels is null || channels.Count == 0)
        {
            return [NotificationChannel.Push, NotificationChannel.SMS, NotificationChannel.Email];
        }

        return channels
            .Distinct()
            .Select(value => Enum.IsDefined(typeof(NotificationChannel), value)
                ? (NotificationChannel)value
                : throw new ArgumentException(localizer[LocalizationKeys.NotificationChannelInvalid, value]))
            .ToArray();
    }
}