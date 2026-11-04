using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.Accounting.Interfaces;
using SNEL.WalletPlatform.Application.Accounting.Models.Input;
using SNEL.WalletPlatform.Application.Accounting.Models.Output;
using SNEL.WalletPlatform.Application.Billers.Models.Output;
using SNEL.WalletPlatform.Application.Billers.Services;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Presentation.Common;

namespace SNEL.WalletPlatform.Presentation.Controllers.V1;

[Authorize]
[Route("api/v1/accounts")]
public sealed class AccountController(IAccountingService accountingService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AccountOutputModel>>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{IdentityRoleConstants.Customer},{IdentityRoleConstants.PlatformAdmin}")]
    public async Task<ActionResult<ApiResponse<PagedResult<AccountOutputModel>>>> GetCustomerSubscriptions(
        [FromQuery] AccountsQueryInputModel query)
    {
        var result = await accountingService.GetCustomerAccountsAsync(query);
        return FromResult(result);
    }
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AccountOutputModel>>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{IdentityRoleConstants.Customer},{IdentityRoleConstants.PlatformAdmin}")]
    public async Task<ActionResult<ApiResponse<PagedResult<AccountOutputModel>>>> GetAllAccounts(
        [FromQuery] AccountsQueryInputModel query)
    {
        var result = await accountingService.GetAccountsAsync(query);
        return FromResult(result);
    }
    /// <summary>
    /// Body: <c>[{ "account_id": "guid", "order": 1 }, ...]</c>
    /// </summary>
    [HttpPost("order")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{IdentityRoleConstants.Customer},{IdentityRoleConstants.PlatformAdmin}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateAccountsOrder(
        [FromBody] IReadOnlyList<UpdateAccountOrderItemInputModel> items)
    {
        var result = await accountingService.UpdateCustomerAccountsOrderAsync(items);
        return FromResult(result);
    }

    [HttpPost("payment-voucher")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin}")]
    public async Task<ActionResult<ApiResponse<bool>>> CreateJournalForPaymentVoucher(
        [FromBody] CreateJournalForJornalTypeInputModel model)
    {
        var result = await accountingService.CreateJournalForPaymentVoucher(model);
        return FromResult(result);
    }

    [HttpPost("recipt-voucher")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin}")]
    public async Task<ActionResult<ApiResponse<bool>>> CreateJournalForReciptVoucher(
        [FromBody] CreateJournalForJornalTypeInputModel model)
    {
        var result = await accountingService.CreateJournalForReciptVoucher(model);
        return FromResult(result);
    }

    [HttpPost("credit-note")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin}")]
    public async Task<ActionResult<ApiResponse<bool>>> CreateJournalForCreditNote(
        [FromBody] CreateJournalForJornalTypeInputModel model)
    {
        var result = await accountingService.CreateJournalForCreditNote(model);
        return FromResult(result);
    }
    [HttpPost("debit-note")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin}")]
    public async Task<ActionResult<ApiResponse<bool>>> CreateJournalForDebitNote(
        [FromBody] CreateJournalForJornalTypeInputModel model)
    {
        var result = await accountingService.CreateJournalForDebitNote(model);
        return FromResult(result);
    }
    [HttpPost("manual")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin}")]
    public async Task<ActionResult<ApiResponse<bool>>> CreateJournalForManualJournal(
        [FromBody] CreateJournalForManualInputModel model)
    {
        var result = await accountingService.CreateJournalForManualJournal(model);
        return FromResult(result);
    }

    [HttpGet("Bank-accounts-by-partner")]
    [ProducesResponseType(typeof(ApiResponse<AccountOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AccountOutputModel>), StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{IdentityRoleConstants.BillerAdmin},{IdentityRoleConstants.PspAdmin},{IdentityRoleConstants.PlatformAdmin}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AccountOutputModel>>>> GetByPartnerId(
        [FromQuery] GetBankAccountsInputModel model)
    {
        var result = await accountingService.GetAllBankAccountsForPartnerAsync(model);
        return FromResult(result);
    }
}
