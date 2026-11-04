using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public sealed class CompletedPrepaidPayBillByDateInputModel
{
    public bool GroupByBiller { get; init; } = true;

    public long? BillerId { get; set; }

    [Required]
    public string CurrencyCode { get; init; } = string.Empty;

    [Required]
    public DateTimeOffset FromDate { get; init; }

    [Required]
    public DateTimeOffset ToDate { get; init; }
}
