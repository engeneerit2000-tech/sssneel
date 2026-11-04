using System.ComponentModel.DataAnnotations;
using SNEL.WalletPlatform.Application.Common.Models.Input;

namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public sealed class CompletedTopupByCurrencyRowsListInputModel : BaseQueryInputModel
{
    public bool GroupByProvider { get; init; } = true;

    public long? PaymentProviderId { get; set; }

    [Required]
    public string CurrencyCode { get; init; } = string.Empty;
}

public sealed class CompletedTopupByDateRowsListInputModel : BaseQueryInputModel
{
    [Required]
    public string CurrencyCode { get; init; } = string.Empty;

    [Required]
    public DateTimeOffset FromDate { get; init; }

    [Required]
    public DateTimeOffset ToDate { get; init; }

    public long? PaymentProviderId { get; set; }
}

public sealed class CompletedPrepaidPayBillByDateRowsListInputModel : BaseQueryInputModel
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

public sealed class CompletedPostpaidPayBillByDateRowsListInputModel : BaseQueryInputModel
{
    public bool GroupByBiller { get; init; } = true;

    public long? BillerId { get; set; }

    [Required]
    public string CurrencyCode { get; init; } = string.Empty;

    [Required]
    public DateTimeOffset FromDate { get; init; }

    [Required]
    public DateTimeOffset ToDate { get; init; }

    public string? CenterCode { get; init; }
}

public sealed class CompletedPostpaidPayBillByCenterRowsListInputModel : BaseQueryInputModel
{
    public bool GroupByBiller { get; init; } = true;

    public long? BillerId { get; set; }

    [Required]
    public string CurrencyCode { get; init; } = string.Empty;

    [Required]
    public DateTimeOffset FromDate { get; init; }

    [Required]
    public DateTimeOffset ToDate { get; init; }

    public string? CenterCode { get; init; }
}


