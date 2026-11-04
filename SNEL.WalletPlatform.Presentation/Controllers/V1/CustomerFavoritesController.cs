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
[Route("api/v1/customer-favorites")]
[Authorize(Roles = IdentityRoleConstants.Customer)]
public sealed class CustomerFavoritesController(ICustomerFavoritesService customerFavoritesService) : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CustomerFavoriteOutputModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CustomerFavoriteOutputModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<CustomerFavoriteOutputModel>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<CustomerFavoriteOutputModel>>> CreateCustomerFavorite(
        [FromBody] CreateCustomerFavoriteInputModel request)
    {
        var result = await customerFavoritesService.CreateCustomerFavoriteAsync(request);
        return FromResult(result);
    }

    [HttpGet("by-types")]
    [ProducesResponseType(typeof(ApiResponse<CustomerFavoritesGroupedOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerFavoritesGroupedOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerFavoritesGroupedOutputModel>>> GetFavoritesByTypeGrouped()
    {
        var result = await customerFavoritesService.GetCustomerFavoritesGroupedByTypeAsync();
        return FromResult(result);
    }

    [HttpPost("subscription/favorite")]
    [ProducesResponseType(typeof(ApiResponse<CustomerFavoriteOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerFavoriteOutputModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CustomerFavoriteOutputModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<CustomerFavoriteOutputModel>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<CustomerFavoriteOutputModel>>> SetSubscriptionFavorite(
        [FromBody] SetSubscriptionFavoriteInputModel request)
    {
        var result = await customerFavoritesService.SetSubscriptionFavoriteAsync(request);
        return FromResult(result);
    }
}

