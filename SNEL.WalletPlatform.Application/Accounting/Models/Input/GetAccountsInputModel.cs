using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Accounting.Models.Input;

/// <summary>
/// One entry for bulk account order update.
/// JSON example: <c>{ "account_id": "...", "order": 1 }</c>
/// </summary>
public sealed class GetAccountsInputModel
{
    [Required]
    public long PartnerId { get; set; }

    [Required]
    public int CurrencyId { get; set; }

    [Required]
    public int AccountTypeId { get; set; }    
}
