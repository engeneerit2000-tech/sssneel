using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Infrastructure.BillingServices;

public sealed class SnelPaymentTypeResolver : ISnelPaymentTypeResolver
{
    public string Resolve(PaymentOrderItemEntity item)
    {
        var invoice = item.Invoice;
        if (invoice is null)
            return "RGI";

        var paidItems = invoice.PaymentOrderItems?
            .Where(poi =>
                poi.Status is PaymentOrderItemStatus.Paid &&
                poi.InvoiceId == invoice.Id)
            .OrderBy(ResolvePaymentSortKey)
            .ThenBy(poi => poi.CreatedAtUtc)
            .ThenBy(poi => poi.Id)
            .ToList();

        if (paidItems is null || paidItems.Count == 0)
        {
            return invoice.AmountPaid + item.Amount >= invoice.AmountTotal
                ? "RGT"
                : "RGI";
        }

        decimal runningPaid = invoice.AmountPaid;

        foreach (var paidItem in paidItems)
        {
            runningPaid += paidItem.Amount;

            if (paidItem.Id != item.Id)
                continue;

            return runningPaid >= invoice.AmountTotal
                ? "RGT"
                : "RGI";
        }

        return invoice.AmountPaid + item.Amount >= invoice.AmountTotal
            ? "RGT"
            : "RGI";
    }

    private static DateTimeOffset ResolvePaymentSortKey(PaymentOrderItemEntity item)
    {
        var paymentCreatedAtUtc = item.PaymentOrder?.Payments
            .OrderBy(payment => payment.CreatedAtUtc)
            .Select(payment => (DateTimeOffset?)payment.CreatedAtUtc)
            .FirstOrDefault();

        return paymentCreatedAtUtc
            ?? item.PaymentOrder?.CreatedAtUtc
            ?? item.CreatedAtUtc;
    }
}
