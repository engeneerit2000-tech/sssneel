using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

public class GroupedData<T, TAggregationType> where T : class
{
    public int Count { get; set; }
    public TAggregationType Sum { get; set; }
    public TAggregationType Avg { get; set; }
    public TAggregationType Min { get; set; }
    public TAggregationType Max { get; set; }
    public T Data { get; set; }
}
public class TopupRateExcelModel
{
    public int Count { get; set; }
    public decimal Sum { get; set; }
    public decimal Avg { get; set; }
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public string Status { get; set; }
    public string CurrencyCode { get; set; }
    public string? PaymentProviderDescription { get; set; }
    public string? PaymentProviderCode { get; set; }
    public string? PaymentMethodDescription { get; set; }
    public string? PaymentMethodCode { get; set; }
}
public class TopupRateOutputModel : GroupedData<TopupRateBasic, decimal>
{    
}
public class PaymentRateOutputModel : GroupedData<PaymentRateBasic, decimal>
{
}
public class PaymentRateExcelModel 
{
    public int Count { get; set; }
    public decimal Sum { get; set; }
    public decimal Avg { get; set; }
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public string Status { get; set; }
    public string CurrencyCode { get; set; }
    public string? BillerDescription { get; set; }
    public string? BillerCode { get; set; }
    public string? BillingServiceDescription { get; set; }
    public string? BillingServiceCode { get; set; }
}
public class TopupRateBasic
{
    public PaymentStatus Status {get; set;}
    public int CurrencyId { get; set;}
    public string? PaymentProviderDescription { get; set; }
    public long? PaymentProviderId { get; set; }
    public string? PaymentMethodDescription { get; set; }
    public long? PaymentMethodId { get; set; }
}
public class TopupRateExcel
{
    public string Status { get; set; }
    public string CurrencyCode { get; set; }
    public string? PaymentProviderDescription { get; set; }
    public string? PaymentProviderCode { get; set; }
    public string? PaymentMethodDescription { get; set; }
    public string? PaymentMethodCode { get; set; }
}
public class PaymentRateBasic
{
    public PaymentOrderItemStatus Status { get; set; }
    public int? CurrencyId { get; set; }
    public string? BillerDescription { get; set; }
    public long? BillerId { get; set; }
    public string? BillingServiceDescription { get; set; }
    public long? BillingServiceId { get; set; }
}

public class PaymentRateExcel
{
    public string Status { get; set; }
    public string? CurrencyCode { get; set; }
    public string? BillerDescription { get; set; }
    public string? BillerCode { get; set; }
    public string? BillingServiceDescription { get; set; }
    public string? BillingServiceCode { get; set; }
}

public class CompletedTopupBasic
{
    public int CurrencyId { get; set; }
    public string? CurrencyCode { get; set; }
    public string? CurrencyName { get; set; }
    public long? PaymentProviderId { get; set; }
    public string? PaymentProviderCode { get; set; }
    public string? PaymentProviderDescription { get; set; }
}

public class CompletedTopupByCurrencyOutputModel : GroupedData<CompletedTopupBasic, decimal>
{
}

public class CompletedTopupByCurrencyExcelModel
{
    public int Count { get; set; }
    public decimal Sum { get; set; }
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public decimal Avg { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string? CurrencyName { get; set; }
    public string? PaymentProviderCode { get; set; }
    public string? PaymentProviderDescription { get; set; }
}

public sealed class CompletedTopupByDateOutputModel
{
    public DateOnly Date { get; init; }
    public int Count { get; init; }
    public decimal Sum { get; init; }
}

public sealed class CompletedTopupByDateExcelModel
{
    public DateOnly Date { get; set; }
    public int Count { get; set; }
    public decimal Sum { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
}

public sealed class CompletedPrepaidPayBillByDateOutputModel
{
    public DateOnly? Date { get; init; }
    public long? BillerId { get; init; }
    public string? BillerCode { get; init; }
    public string? BillerName { get; init; }
    public int Count { get; init; }
    public decimal Sum { get; init; }
}

public sealed class CompletedPrepaidPayBillByDateRowOutputModel
{
    public long? BillerId { get; init; }
    public string? BillerCode { get; init; }
    public string? BillerName { get; init; }
    public DateOnly PaymentDate { get; init; }
    public string? CurrencyCode { get; init; }
    public string? BillingRefValue { get; init; }
    public string? BillingRefForDisplay { get; init; }
    public decimal Amount { get; init; }
    public decimal? Fees { get; init; }
    public decimal? Tax { get; init; }
    public decimal Total { get; init; }
    public PaymentOrderItemStatus Status { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public string? Description { get; init; }
}

public sealed class CompletedPrepaidPayBillByDateExcelModel
{
    public DateOnly? Date { get; set; }
    public long? BillerId { get; set; }
    public string? BillerCode { get; set; }
    public string? BillerName { get; set; }
    public int Count { get; set; }
    public decimal Sum { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
}

public sealed class CompletedPrepaidPayBillByDateRowExcelModel
{
    public long? BillerId { get; set; }
    public string? BillerCode { get; set; }
    public string? BillerName { get; set; }
    public DateOnly PaymentDate { get; set; }
    public string? CurrencyCode { get; set; }
    public string? BillingRefValue { get; set; }
    public string? BillingRefForDisplay { get; set; }
    public decimal Amount { get; set; }
    public decimal? Fees { get; set; }
    public decimal? Tax { get; set; }
    public decimal Total { get; set; }
    public PaymentOrderItemStatus Status { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? Description { get; set; }
}

public sealed class CompletedPostpaidPayBillByDateOutputModel
{
    public DateOnly? Date { get; init; }
    public long? BillerId { get; init; }
    public string? BillerCode { get; init; }
    public string? BillerName { get; init; }
    public int Count { get; init; }
    public decimal Sum { get; init; }
}

public sealed class CompletedPostpaidPayBillByDateRowOutputModel
{
    public long? BillerId { get; init; }
    public string? BillerCode { get; init; }
    public string? BillerName { get; init; }
    public string CenterCode { get; init; } = string.Empty;
    public string CenterName { get; init; } = string.Empty;
    public DateOnly PaymentDate { get; init; }
    public string? CurrencyCode { get; init; }
    public string? BillingRefValue { get; init; }
    public string? BillingRefForDisplay { get; init; }
    public decimal Amount { get; init; }
    public decimal? Fees { get; init; }
    public decimal? Tax { get; init; }
    public decimal Total { get; init; }
    public PaymentOrderItemStatus Status { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public string? Description { get; init; }
}

public sealed class CompletedPostpaidPayBillByDateExcelModel
{
    public DateOnly? Date { get; set; }
    public long? BillerId { get; set; }
    public string? BillerCode { get; set; }
    public string? BillerName { get; set; }
    public int Count { get; set; }
    public decimal Sum { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
}

public sealed class CompletedPostpaidPayBillByDateRowExcelModel
{
    public long? BillerId { get; set; }
    public string? BillerCode { get; set; }
    public string? BillerName { get; set; }
    public string CenterCode { get; set; } = string.Empty;
    public string CenterName { get; set; } = string.Empty;
    public DateOnly PaymentDate { get; set; }
    public string? CurrencyCode { get; set; }
    public string? BillingRefValue { get; set; }
    public string? BillingRefForDisplay { get; set; }
    public decimal Amount { get; set; }
    public decimal? Fees { get; set; }
    public decimal? Tax { get; set; }
    public decimal Total { get; set; }
    public PaymentOrderItemStatus Status { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? Description { get; set; }
}

public sealed class CompletedPostpaidPayBillByCenterOutputModel
{
    public string? CenterName { get; init; }
    public long? BillerId { get; init; }
    public string? BillerCode { get; init; }
    public string? BillerName { get; init; }
    public int Count { get; init; }
    public decimal Sum { get; init; }
}

public sealed class CompletedPostpaidPayBillByCenterRowOutputModel
{
    public long? BillerId { get; init; }
    public string? BillerCode { get; init; }
    public string? BillerName { get; init; }
    public string CenterCode { get; init; } = string.Empty;
    public string CenterName { get; init; } = string.Empty;
    public DateOnly PaymentDate { get; init; }
    public string? CurrencyCode { get; init; }
    public string? BillingRefValue { get; init; }
    public string? BillingRefForDisplay { get; init; }
    public decimal Amount { get; init; }
    public decimal? Fees { get; init; }
    public decimal? Tax { get; init; }
    public decimal Total { get; init; }
    public PaymentOrderItemStatus Status { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public string? Description { get; init; }
}

public sealed class CompletedPostpaidPayBillByCenterExcelModel
{
    public string? CenterName { get; set; }
    public long? BillerId { get; set; }
    public string? BillerCode { get; set; }
    public string? BillerName { get; set; }
    public int Count { get; set; }
    public decimal Sum { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
}

public sealed class CompletedPostpaidPayBillByCenterRowExcelModel
{
    public long? BillerId { get; set; }
    public string? BillerCode { get; set; }
    public string? BillerName { get; set; }
    public string CenterCode { get; set; } = string.Empty;
    public string CenterName { get; set; } = string.Empty;
    public DateOnly PaymentDate { get; set; }
    public string? CurrencyCode { get; set; }
    public string? BillingRefValue { get; set; }
    public string? BillingRefForDisplay { get; set; }
    public decimal Amount { get; set; }
    public decimal? Fees { get; set; }
    public decimal? Tax { get; set; }
    public decimal Total { get; set; }
    public PaymentOrderItemStatus Status { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? Description { get; set; }
}
