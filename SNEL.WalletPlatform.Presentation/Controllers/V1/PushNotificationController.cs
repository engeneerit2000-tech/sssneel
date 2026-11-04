using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Notification.Models.Input;
using SNEL.WalletPlatform.Application.Notification.Models.Output;
using SNEL.WalletPlatform.Application.Notification.Services;
using SNEL.WalletPlatform.Presentation.Common;

namespace SNEL.WalletPlatform.Presentation.Controllers.V1;


[Route("api/v1/push-notifications")]
[ApiController]
[Authorize]
public sealed class PushNotificationController(NotificationService notificationService) : Controllers.ApiControllerBase
{

    [HttpPost("{NotificationId}/mark-read")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<bool>>> MarkRead(Guid NotificationId)
    {
        var result = await notificationService.MarkReadAsync(NotificationId);
        return FromResult(result);
    }



    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PushNotificationOutputModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PagedResult<PushNotificationOutputModel>>>> GetPushNotifications([FromQuery] PushNotificationQueryInputModel model)
    {
        var result = await notificationService.GetPushNotificationsAsync(model);
        return FromResult(result);
    }


    [HttpGet("unread-count")]
    [Authorize]
    [ProducesResponseType(typeof(ActionResult<ApiResponse<int>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ActionResult<Result<int>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ActionResult<Result<int>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> GetNotSeenNotificationsCount()
    {
        var result = await notificationService.GetUnreadNotificationsCountAsync();
        return FromResult(result);
    }

    [HttpPost("reset-unread-count")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> ResetNotSeenNotificationsCount()
    {
        var result = await notificationService.ResetUnreadNotificationCounterAsync();
        return FromResult( result);
    }
}
