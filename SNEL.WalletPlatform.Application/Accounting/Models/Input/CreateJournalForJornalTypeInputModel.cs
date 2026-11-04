using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Accounting.Models.Input;

/// <summary>
/// One entry for bulk account order update.
/// JSON example: <c>{ "account_id": "...", "order": 1 }</c>
/// </summary>
public sealed class CreateJournalForJornalTypeInputModel
{
    [Required]
    public long PartnerId { get; set; }

    [Required]
    public Guid PartnerBankAccountId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public int CurrencyId { get; set; }

    [JsonPropertyName("sourceRef")]
    public string SourceRef { get; set; } = string.Empty;
}
