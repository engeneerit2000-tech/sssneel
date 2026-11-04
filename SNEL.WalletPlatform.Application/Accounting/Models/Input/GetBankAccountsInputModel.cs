using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Accounting.Models.Input;

/// <summary>
/// One entry for bulk account order update.
/// JSON example: <c>{ "account_id": "...", "order": 1 }</c>
/// </summary>
public sealed class GetBankAccountsInputModel
{
    [Required]
    public long PartnerId { get; set; }

    [Required]
    public int CurrencyId { get; set; }

    [Required]
    public bool IncludeDefaultAccount { get; set; } = false;

}
