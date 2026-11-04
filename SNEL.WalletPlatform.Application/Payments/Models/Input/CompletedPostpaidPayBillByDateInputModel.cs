using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public sealed class CompletedPostpaidPayBillByDateInputModel
{
    public bool GroupByBiller { get; init; } = true;

    public long? BillerId { get; set; }

    [Required]
    public string CurrencyCode { get; init; } = string.Empty;

    [Required]
    public DateTimeOffset FromDate { get; init; }

    [Required]
    public DateTimeOffset ToDate { get; init; }

    /// <summary>Filter by center lookup code (e.g. "06-03").</summary>
    public string? CenterCode { get; init; }
}
