using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Models.Input;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Presentation.Common;

namespace SNEL.WalletPlatform.Presentation.Controllers.V1;

[Authorize]
[Route("api/v1/payment-methods")]
public sealed class PaymentMethodController(IPaymentMethodService paymentMethodService) : Controllers.ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.PspAdmin},{IdentityRoleConstants.Customer}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<PaymentMethodOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<PaymentMethodOutputModel>>>> GetPaymentMethods(
        CancellationToken cancellationToken = default)
    {
        var result = await paymentMethodService.GetAllAsync(cancellationToken);
        return FromResult(result);
    }

    [HttpPost]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.PspAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodOutputModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodOutputModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodOutputModel>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<PaymentMethodOutputModel>>> CreatePaymentMethod(
        [FromBody] CreatePaymentMethodInputModel request,
        CancellationToken cancellationToken = default)
    {
        var result = await paymentMethodService.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return FromResult(result);
        }

        return CreatedAtAction(
            nameof(GetPaymentMethodById),
            new { id = result.Value!.Id },
            new ApiResponse<PaymentMethodOutputModel>(AppHttpStatusCodes.Created, result.Message, result.Value));
    }

    [HttpGet("{id:long}")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.PspAdmin},{IdentityRoleConstants.Customer}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentMethodOutputModel>>> GetPaymentMethodById(
        long id,
        CancellationToken cancellationToken = default)
    {
        var result = await paymentMethodService.GetByIdAsync(id, cancellationToken);
        return FromResult(result);
    }

    [HttpPost("{id:long}")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.PspAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodOutputModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodOutputModel>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<PaymentMethodOutputModel>>> UpdatePaymentMethod(
        long id,
        [FromBody] UpdatePaymentMethodInputModel request,
        CancellationToken cancellationToken = default)
    {
        var result = await paymentMethodService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.PspAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeletePaymentMethod(
        long id,
        CancellationToken cancellationToken = default)
    {
        var result = await paymentMethodService.DeleteAsync(id, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("by-payment-provider/{id:long}")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.PspAdmin},{IdentityRoleConstants.Customer}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentMethodOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<PaymentMethodOutputModel>>>> GetPaymentMethodByProviderId(
        long id,
        CancellationToken cancellationToken = default)
    {
        var result = await paymentMethodService.GetByProviderIdAsync(id, cancellationToken);
        return FromResult(result);
    }
}
