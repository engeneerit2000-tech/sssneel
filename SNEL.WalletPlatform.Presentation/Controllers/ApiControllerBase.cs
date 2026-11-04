using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Presentation.Common;

namespace SNEL.WalletPlatform.Presentation.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult<ApiResponse<T>> FromResult<T>(Result<T> result)
    {
        return StatusCode((int)result.StatusCode, new ApiResponse<T>(result.StatusCode, result.Message, result.Value));
    }
}
