using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Accounting.Models.Input;

/// <summary>
/// One entry for bulk account order update.
/// JSON example: <c>{ "account_id": "...", "order": 1 }</c>
/// </summary>
public sealed class CreateJournalForManualInputModel
{
    [Required]
    public Guid FirstAccountId { get; set; }

    [Required]
    public Guid SecondAccountId { get; set; }

    [Required]
    public bool FirstAccountCredit { get; set; }

    [Required]
    public bool SecondAccountCredit { get; set; }

    [Required]
    public decimal Amount { get; set; }

    public string SourceRef { get; set; } = string.Empty;
}
