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
[Route("api/v1/customer-subscriptions")]
[Authorize(Roles = IdentityRoleConstants.Customer)]
public sealed class CustomerSubscriptionController(ICustomerSubscriptionService customerSubscriptionService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CustomerSubscriptionOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<CustomerSubscriptionOutputModel>>>> GetCustomerSubscriptions(
        [FromQuery] CustomerSubscriptionsQueryInputModel query)
    {
        var result = await customerSubscriptionService.GetCustomerSubscriptionsAsync(query);
        return FromResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerSubscriptionOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerSubscriptionOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerSubscriptionOutputModel>>> GetCustomerSubscriptionById(
        Guid id)
    {
        var result = await customerSubscriptionService.GetCustomerSubscriptionByIdAsync(id);
        return FromResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CustomerSubscriptionOutputModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CustomerSubscriptionOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CustomerSubscriptionOutputModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<CustomerSubscriptionOutputModel>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<CustomerSubscriptionOutputModel>>> CreateCustomerSubscription(
        [FromBody] CreateCustomerSubscriptionInputModel request)
    {
        var result = await customerSubscriptionService.CreateCustomerSubscriptionAsync(request);
        if (!result.IsSuccess)
        {
            return FromResult(result);
        }

        return CreatedAtAction(
            nameof(GetCustomerSubscriptionById),
            new { id = result.Value!.Id },
            new ApiResponse<CustomerSubscriptionOutputModel>(result.StatusCode, result.Message, result.Value));
    }

    [HttpPost("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerSubscriptionOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerSubscriptionOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CustomerSubscriptionOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerSubscriptionOutputModel>>> UpdateCustomerSubscription(
        Guid id,
        [FromBody] UpdateCustomerSubscriptionInputModel request)
    {
        var result = await customerSubscriptionService.UpdateCustomerSubscriptionAsync(id, request);
        return FromResult(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCustomerSubscription(
        Guid id)
    {
        var result = await customerSubscriptionService.DeleteAsync(id);
        return FromResult(result);
    }

    [HttpPost("info")]
    [ProducesResponseType(typeof(ApiResponse<CustomerSubscriptionInfoOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerSubscriptionInfoOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerSubscriptionInfoOutputModel>>> GetCustomerSubscriptionInfo(
        [FromBody] CreateCustomerSubscriptionInputModel request)
    {
        var result = await customerSubscriptionService.GetCustomerSubscriptionInfoAsync(request);
        return FromResult(result);
    }
}