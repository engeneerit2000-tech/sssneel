using AutoMapper;
using SNEL.WalletPlatform.Application.Notification.Models.Internal;
using SNEL.WalletPlatform.Application.Notification.Models.Output;

namespace SNEL.WalletPlatform.Application.Notification.Mapper;

public class NotificationMapper : Profile
{
    public NotificationMapper()
    {

        CreateMap<ParameterDataModel, ParameterInputModel>();

        CreateMap<NotificationMessageInternalAPIOutputModel, PushNotificationOutputModel>();
    }
}
