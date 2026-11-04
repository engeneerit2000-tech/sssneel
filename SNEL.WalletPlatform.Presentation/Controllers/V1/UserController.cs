using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Users.Interfaces;
using SNEL.WalletPlatform.Application.Users.Models.Input;
using SNEL.WalletPlatform.Application.Users.Models.Output;
using SNEL.WalletPlatform.Presentation.Common;

namespace SNEL.WalletPlatform.Presentation.Controllers.V1;

[Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin},{IdentityRoleConstants.PspAdmin}")]
[Route("api/v1/users")]
public sealed class UserController(IUserManagementService userManagementService) : Controllers.ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ManagedUserOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ManagedUserOutputModel>>>> GetUsers(
        [FromQuery] UsersQueryInputModel query,
        CancellationToken cancellationToken)
    {
        var result = await userManagementService.GetUsersAsync(query, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ManagedUserOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ManagedUserOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ManagedUserOutputModel>>> GetUserById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await userManagementService.GetUserByIdAsync(id, cancellationToken);
        return FromResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ManagedUserOutputModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ManagedUserOutputModel>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<ManagedUserOutputModel>>> CreateUser(
        [FromBody] CreateManagedUserInputModel request,
        CancellationToken cancellationToken)
    {
        var result = await userManagementService.CreateUserAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return FromResult(result);
        }

        return CreatedAtAction(
            nameof(GetUserById),
            new { id = result.Value!.Id },
            new ApiResponse<ManagedUserOutputModel>(result.StatusCode, result.Message, result.Value));
    }

    [HttpPost("{id:guid}/password")]
    [ProducesResponseType(typeof(ApiResponse<ManagedUserOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ManagedUserOutputModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ManagedUserOutputModel>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<ManagedUserOutputModel>>> ChangeUserPassword(
        Guid id,
        [FromBody] ChangeManagedUserPasswordInputModel request,
        CancellationToken cancellationToken)
    {
        var result = await userManagementService.ChangeUserPasswordAsync(id, request, cancellationToken);
        return FromResult(result);
    }

    [HttpPost("{id:guid}/customer-mobile")]
    [ProducesResponseType(typeof(ApiResponse<ManagedUserOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ManagedUserOutputModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ManagedUserOutputModel>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ManagedUserOutputModel>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<ManagedUserOutputModel>>> ChangeCustomerMobile(
        Guid id,
        [FromBody] ChangeCustomerMobileInputModel request,
        CancellationToken cancellationToken)
    {
        var result = await userManagementService.ChangeCustomerMobileAsync(id, request, cancellationToken);
        return FromResult(result);
    }

    [HttpPost("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ManagedUserOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ManagedUserOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ManagedUserOutputModel>>> UpdateUser(
        Guid id,
        [FromBody] UpdateManagedUserInputModel request,
        CancellationToken cancellationToken)
    {
        var result = await userManagementService.UpdateUserAsync(id, request, cancellationToken);
        return FromResult(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await userManagementService.DeleteUserAsync(id, cancellationToken);
        return FromResult(result);
    }
}
