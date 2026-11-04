using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Models.Input;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Presentation.Common;

namespace SNEL.WalletPlatform.Presentation.Controllers.V1;

[Route("api/v1/payment-providers")]
[Authorize]
public sealed class PaymentProviderController(IPaymentProviderService paymentProviderService) : Controllers.ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.PspAdmin},{IdentityRoleConstants.Customer}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<PaymentProviderOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<PaymentProviderOutputModel>>>> GetPaymentProviders(
        CancellationToken cancellationToken = default)
    {
        var result = await paymentProviderService.GetAllAsync(cancellationToken);
        return FromResult(result);
    }

    [HttpPost]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentProviderOutputModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<PaymentProviderOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PaymentProviderOutputModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<PaymentProviderOutputModel>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<PaymentProviderOutputModel>>> CreatePaymentProvider(
        [FromBody] CreatePaymentProviderInputModel request,
        CancellationToken cancellationToken = default)
    {
        var result = await paymentProviderService.CreatePaymentProviderAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return FromResult(result);
        }

        return CreatedAtAction(
            nameof(GetPaymentProviderById),
            new { id = result.Value!.Id },
            new ApiResponse<PaymentProviderOutputModel>(AppHttpStatusCodes.Created, result.Message, result.Value));
    }

    [HttpGet("{id:long}")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.PspAdmin},{IdentityRoleConstants.Customer}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentProviderOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentProviderOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentProviderOutputModel>>> GetPaymentProviderById(
        long id,
        CancellationToken cancellationToken = default)
    {
        var result = await paymentProviderService.GetByIdAsync(id, cancellationToken);
        return FromResult(result);
    }

    [HttpPost("{id:long}")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentProviderOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentProviderOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PaymentProviderOutputModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<PaymentProviderOutputModel>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<PaymentProviderOutputModel>>> UpdatePaymentProvider(
        long id,
        [FromBody] UpdatePaymentProviderInputModel request,
        CancellationToken cancellationToken = default)
    {
        var result = await paymentProviderService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeletePaymentProvider(
        long id,
        CancellationToken cancellationToken = default)
    {
        var result = await paymentProviderService.DeleteAsync(id, cancellationToken);
        return FromResult(result);
    }
}