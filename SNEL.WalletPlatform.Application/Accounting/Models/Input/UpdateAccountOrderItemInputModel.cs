using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Accounting.Models.Input;

/// <summary>
/// One entry for bulk account order update.
/// JSON example: <c>{ "account_id": "...", "order": 1 }</c>
/// </summary>
public sealed class UpdateAccountOrderItemInputModel
{
    [JsonPropertyName("account_id")]
    public Guid AccountId { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }
}
