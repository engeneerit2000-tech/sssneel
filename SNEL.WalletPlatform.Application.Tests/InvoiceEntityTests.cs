using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Invoicing;

namespace SNEL.WalletPlatform.Application.Tests;

public sealed class InvoiceEntityTests
{
    [Fact]
    public void ApplyPayment_UpdatesPaidOutstandingAndStatus()
    {
        var invoice = InvoiceEntity.Create(
            billingServiceId: 1,
            billingRef: "06-123456",
            originInvoiceId: null,
            invoiceRef: "INV-1",
            amountOutstanding: 150m,
            amountPaid: 0m,
            amountTotal: 150m,
            currencyId: 1,
            issueDate: DateTimeOffset.UtcNow.AddDays(-1),
            dueDate: DateTimeOffset.UtcNow.AddDays(10),
            status: InvoiceStatus.Issued.ToString(),
            invoiceInfo: null,
            invoiceUpdateDate: null);

        invoice.ApplyPayment(25m);

        Assert.Equal(25m, invoice.AmountPaid);
        Assert.Equal(125m, invoice.AmountOutstanding);
        Assert.Equal(InvoiceStatus.PartiallyPaid.ToString(), invoice.Status);
    }
}
