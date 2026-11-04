using AutoMapper;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Exceptions;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Notification.Configuration;
using SNEL.WalletPlatform.Application.Notification.Interfaces;
using SNEL.WalletPlatform.Application.Notification.Models.Internal;
using SNEL.WalletPlatform.Domain.Customers;
using System.Text;
using System.Text.Json;
using SNEL.WalletPlatform.Application.Notification.Enums;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Notification.Services;

public partial class NotificationService(
    INotificationManager notificationProvider,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IStringLocalizer<SharedResource> localizer,
    ILogger<NotificationService> logger,
    ICurrentUserContext currentUserContext,
    IOptions<NotificationConfigurations> notificationSettings)
{
    public async Task<bool> SendNotificationsAsync(List<NotificationDetailsModel> notificationsDetails)
    {
        try
        {
            // 1. map the data
            var apiInputModel = new SendNotificationInternalAPIInputModel()
            {
                Notifications = (await Task.WhenAll(notificationsDetails.Select(async x => new NotificationDataModel()
                {
                    IsTransactional = x.IsTransactional,
                    NotificationCode = x.NotificationCode,
                    NotificationType = x.NotificationType,
                    Parameters = mapper.Map<List<ParameterInputModel>>(x.Params),
                    SenderId = x.SenderId,
                    ReferenceType = x.ReferenceType,
                    ReferenceId = x.ReferenceId,
                    OverrideChannelsPreferences = x.OverrideReceiverChannelsPreferences,
                    RequestedChannels = x.RequestedChannels,
                    Receivers = await PrepareReceiversDataAsync(x.Receivers, x.AdditionalPhoneNumbers,
                        x.Params?.First()),
                    SendingAt = x.SendingAt,
                    SaveInAppNotification = x.SaveInAppNotification,
                    PushMessageMode = x.PushMessageMode,
                    PushData = x.PushData
                }).ToList())).ToList()
            };


            // 2. send
            logger.LogInformation(
                "Notification service (third-party) request: SendNotifictionAsync. Payload: {Payload}",
                JsonSerializer.Serialize(apiInputModel));
            var result = await notificationProvider.SendNotifictionAsync(apiInputModel);
            logger.LogInformation(
                "Notification service (third-party) response: SendNotifictionAsync. Value={Value}, Message={Message}, StatusCode={StatusCode}",
                result?.Value,
                result?.Message,
                result?.StatusCode);
            if (result == null || result.Value == false)
            {
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while sending notification");
            //throw new AppException(localizer.GetString(LocalizationKeys.InternalServerErrorDefaultMessage),
            //    AppHttpStatusCodes.Failed);
            return false;
        }
    }


    public List<NotificationChannel> GetTargettedChannels(List<NotificationChannel> requestedChannels,
        bool isTransactional = false)
    {
        var notificationsSettings = notificationSettings.Value;
        var targettedChannels = new List<NotificationChannel>();

        if (requestedChannels.Contains(NotificationChannel.Email))
        {
            if (notificationsSettings.EmailIsEnabled)
            {
                targettedChannels.Add(NotificationChannel.Email);
            }
            else if (isTransactional)
            {
                logger.LogCritical(
                    "Email channel is not enabled, but a transactional notification was requested. Requested channels: {RequestedChannels}",
                    string.Join(", ", requestedChannels));
            }
        }

        if (requestedChannels.Contains(NotificationChannel.SMS))
        {
            if (notificationsSettings.SMSIsEnabled)
            {
                targettedChannels.Add(NotificationChannel.SMS);
            }
            else if (isTransactional)
            {
                logger.LogCritical(
                    "SMS channel is not enabled, but a transactional notification was requested. Requested channels: {RequestedChannels}",
                    string.Join(", ", requestedChannels));
            }
        }


        if (requestedChannels.Contains(NotificationChannel.Push))
        {
            if (notificationsSettings.PushIsEnabled)
            {
                targettedChannels.Add(NotificationChannel.Push);
            }
            else if (isTransactional)
            {
                logger.LogCritical(
                    "Push channel is not enabled, but a transactional notification was requested. Requested channels: {RequestedChannels}",
                    string.Join(", ", requestedChannels));
            }
        }


        return targettedChannels;
    }
}