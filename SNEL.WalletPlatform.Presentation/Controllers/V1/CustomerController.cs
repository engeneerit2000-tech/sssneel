using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Customers.Interfaces;
using SNEL.WalletPlatform.Application.Customers.Models.Input;
using SNEL.WalletPlatform.Application.Customers.Models.Output;
using SNEL.WalletPlatform.Presentation.Common;

namespace SNEL.WalletPlatform.Presentation.Controllers.V1;

[Authorize]
[Route("api/v1/customers")]
public sealed class CustomerController(ICustomerService customerService) : Controllers.ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CustomerOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<CustomerOutputModel>>>> GetCustomers(
        [FromQuery] CustomersQueryInputModel query)
    {
        var result = await customerService.GetCustomersAsync(query);
        return FromResult(result);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<CustomerOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerOutputModel>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<CustomerOutputModel>>> GetMyProfile()
    {
        var result = await customerService.GetMyProfileAsync();
        return FromResult(result);
    }

    [HttpPost("me")]
    [Authorize(Roles = $"{IdentityRoleConstants.Customer}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerOutputModel>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<CustomerOutputModel>>> UpdateMyProfile(
        [FromBody] UpdateMyCustomerProfileInputModel request)
    {
        var result = await customerService.UpdateMyProfileAsync(request);
        return FromResult(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.Customer}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerOutputModel>>> GetCustomerById(
        Guid id)
    {
        var result = await customerService.GetCustomerByIdAsync(id);
        return FromResult(result);
    }

    [HttpPost]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerOutputModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CustomerOutputModel>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<CustomerOutputModel>>> CreateCustomer(
        [FromBody] CreateCustomerInputModel request)
    {
        var result = await customerService.CreateCustomerAsync(request);
        if (!result.IsSuccess)
        {
            return FromResult(result);
        }

        return CreatedAtAction(
            nameof(GetCustomerById),
            new { id = result.Value!.Id },
            new ApiResponse<CustomerOutputModel>(result.StatusCode, result.Message, result.Value));
    }

    [HttpPost("{id:guid}")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerOutputModel>>> UpdateCustomer(
        Guid id,
        [FromBody] UpdateCustomerInputModel request)
    {
        var result = await customerService.UpdateCustomerAsync(id, request);
        return FromResult(result);
    }

    [HttpPost("{id:guid}/status")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.Customer}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateCustomerStatus(
        Guid id,
        [FromBody] UpdateCustomerStatusInputModel request)
    {
        var result = await customerService.UpdateCustomerStatusAsync(id, request);
        return FromResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCustomer(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await customerService.DeleteCustomerAsync(id, cancellationToken);
        return FromResult(result);
    }
}
