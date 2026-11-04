using SNEL.WalletPlatform.Application.Common.Enums;
using SNEL.WalletPlatform.Application.Common;
using System.Text;
using SNEL.WalletPlatform.Application.Notification.Models.Internal;
using SNEL.WalletPlatform.Application.Notification.Models.Input;
using SNEL.WalletPlatform.Domain.Users;
using SNEL.WalletPlatform.Application.Common.Exceptions;
using SNEL.WalletPlatform.Application.Common.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Application.Notification.Models.Output;

namespace SNEL.WalletPlatform.Application.Notification.Services;

public partial class NotificationService
{

    public async Task<Result<PagedResult<PushNotificationOutputModel>>> GetPushNotificationsAsync(PushNotificationQueryInputModel model)
    {
        var currentUserId = await GetCurrentUserEntityIdAsync();

        // determine language
        if (model.LanguageId == null)
        {
            var currentCultureLanguage = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToUpper();
            if (!string.IsNullOrEmpty(currentCultureLanguage))
            {
                LanguageEnum lang = (LanguageEnum)Enum.Parse(typeof(LanguageEnum), currentCultureLanguage);
                model.LanguageId = (int)lang;
            }
            else
            {
                model.LanguageId = (int)LanguageEnum.FR;
            }
        }

        var data = await GetPushNotificationsAsync(currentUserId, model);

        // map data 
        var result = new PagedResult<PushNotificationOutputModel>(
            mapper.Map<List<PushNotificationOutputModel>>(data.Value.Items),
            data.Value.TotalCount
        );

        return Result<PagedResult<PushNotificationOutputModel>>.Success(result);
    }

    public async Task<Result<bool>> MarkReadAsync(Guid notificationId)
    {
        var currentUserId = await GetCurrentUserEntityIdAsync();

        var isRead = await ReadPushNotificationAsync(notificationId, currentUserId);

        return Result<bool>.Success(isRead);
    }



    public async Task<Result<int>> GetUnreadNotificationsCountAsync()
    {
        var currentUserId = await GetCurrentUserEntityIdAsync();

        // channel for push notifications
        var channel = NotificationChannel.Push;

        var count = await GetNewNotificationsCountAsync(currentUserId, channel);

        return Result<int>.Success(count);

    }


    public async Task<Result<bool>> ResetUnreadNotificationCounterAsync()
    {
         var currentUserId = await GetCurrentUserEntityIdAsync();

        // channel for push notifications
        var channel = NotificationChannel.Push;

        bool isReset = await ResetNewNotificationsCountAsync(currentUserId, channel);

        return Result<bool>.Success(isReset);
    }




    #region Private Methods
    private async Task<Guid> GetCurrentUserEntityIdAsync()
    {
        if (string.IsNullOrWhiteSpace(currentUserContext.UserId))
        {
            throw new AppException(
                localizer.GetString(LocalizationKeys.UnAuthorizedDefaultMessage),
                AppHttpStatusCodes.BadRequest);
        }

        var user = await unitOfWork.UserRepository.GetByIamRefAsync(currentUserContext.UserId);
        if (user == null)
        {
            throw new AppException(
                localizer.GetString(LocalizationKeys.CustomerNotFound, currentUserContext.UserId),
                AppHttpStatusCodes.NotFound);
        }

        return user.Id;
    }

    private async Task<List<NotificationReceiverInputModel>> PrepareReceiversDataAsync(List<UserEntity>? ReceiversUsers, List<string>? additionalPhoneNumbers,ParameterDataModel? parameters)
    {
        var receivers = new List<NotificationReceiverInputModel>();

        if (ReceiversUsers != null && ReceiversUsers.Count > 0)
        {
            if (ReceiversUsers.Any(r => r.NotificationPreferences == null || r.NotificationPreferences.Count < 0))
            {
                // load notification preferences
                ReceiversUsers = await unitOfWork.UserRepository.GetByUserIdsWithNotificationsDependenciesAsync(ReceiversUsers.Select(x => x.Id).ToList());

            }



            for (int i = 0; i < ReceiversUsers.Count; i++)
            {
                var userEntity = ReceiversUsers[i];

                try
                {
                    var receiver = new NotificationReceiverInputModel()
                    {
                        Id = userEntity.Id,
                    };

                    // prepare user preferences
                    receiver.Preferences = new ReceiverPreferencesInputModel()
                    {
                        Language = ResolveLanguage(userEntity.Language, userEntity.Id),
                    };

                    if (userEntity.NotificationPreferences != null && userEntity.NotificationPreferences.Any())
                    {
                        receiver.Preferences.ActiveChannels = userEntity.NotificationPreferences.Where(p => p.Active).Select(x => x.NotificationChannel).ToList();
                    }

                    // prepare email data
                    if (!string.IsNullOrEmpty(userEntity.Email))
                    {
                        receiver.EmailNotificationDetails = new EmailNotificationDetailsInputModel() { Emails = new List<string>() { userEntity.Email } };
                    }

                    // prepare sms mobile data
                    if (!string.IsNullOrEmpty(userEntity.MobileNo))
                    {
                        receiver.SMSNotificationDetails = new SMSNotificationDetailsInputModel() { PhoneNumbers = new List<string>() { userEntity.MobileNo } };
                    }

                    // Push tokens are resolved by Notification Service from receiver Id.
                    receivers.Add(receiver);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Failed to prepare notification receiver for user {UserId}. Skipping user.",
                        userEntity.Id);
                }
            }
        }



        // anonymous users - send sms to the additional phone numbers. Exmple: OTP
        if (additionalPhoneNumbers != null && additionalPhoneNumbers.Count > 0)
        {
            for (int i = 0; i < additionalPhoneNumbers.Count(); i++)
            {
                var phoneNumber = additionalPhoneNumbers[i];

                var receiver = new NotificationReceiverInputModel()
                {
                    Id = null,
                    EmailNotificationDetails = null,
                    Preferences = new ReceiverPreferencesInputModel(){ Language = parameters.Language },
                    PushNotificationDetails = null,
                    SMSNotificationDetails = new SMSNotificationDetailsInputModel() { PhoneNumbers = new List<string>() { phoneNumber } }
                };

                receivers.Add(receiver);
            }

        }


        return receivers;
    }

    private LanguageEnum ResolveLanguage(string? language, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return LanguageEnum.EN;
        }

        if (Enum.TryParse(typeof(LanguageEnum), language.Trim().ToUpperInvariant(), out var parsed)
            && parsed is LanguageEnum languageEnum)
        {
            return languageEnum;
        }

        logger.LogWarning(
            "Invalid language '{Language}' for user {UserId}. Falling back to {FallbackLanguage}.",
            language,
            userId,
            LanguageEnum.EN);

        return LanguageEnum.EN;
    }

    private async Task<Result<PagedResult<NotificationMessageInternalAPIOutputModel>>> GetPushNotificationsAsync(Guid userId, PushNotificationQueryInputModel inputModel)
    {

        var apiInputModel = new GetNotificationsMessagesInternalAPIInputModel()
        {
            Index = inputModel.PageNumber,
            Size = inputModel.PageSize,
            IsAscending = inputModel.IsAscending,
            SortBy = inputModel.SortBy,

            Language = inputModel.LanguageId == null ? LanguageEnum.FR : (LanguageEnum)inputModel.LanguageId,
            IsRead = inputModel.IsRead,
            ReceiverId = userId
        };


        var result = await notificationProvider.GetNotificationMessagesAsync(apiInputModel);

        if (result == null)
        {
            throw new AppException(localizer.GetString(LocalizationKeys.InternalServerErrorDefaultMessage), AppHttpStatusCodes.Failed);
        }


        return result;

    }

    private async Task<bool> ReadPushNotificationAsync(Guid notificaitonId, Guid userId)
    {

        var result = await notificationProvider.ReadNotificationAsync(notificaitonId, userId);
        if (result == null || result.Value == false)
        {
            return false;
        }


        return true;
    }
    private async Task<int> GetNewNotificationsCountAsync(Guid userId, NotificationChannel channel)
    {
        var result = await notificationProvider.GetNewNotificationsCountAsync(userId, channel);
        if (result == null)
        {
            throw new AppException(localizer.GetString(LocalizationKeys.InternalServerErrorDefaultMessage), AppHttpStatusCodes.Failed);
        }


        return result.Value;
    }


    private async Task<bool> ResetNewNotificationsCountAsync(Guid userId, NotificationChannel channel)
    {
        var result = await notificationProvider.ResetNewNotificationsCountAsync(userId, channel);
        if (result == null || result.Value == false)
        {
            throw new AppException(localizer.GetString(LocalizationKeys.InternalServerErrorDefaultMessage), AppHttpStatusCodes.Failed);
        }


        return result.Value;
    }
    #endregion
}
