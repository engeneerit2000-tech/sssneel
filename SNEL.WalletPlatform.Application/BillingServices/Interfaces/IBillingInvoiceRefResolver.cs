using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Domain.Billing;

namespace SNEL.WalletPlatform.Application.BillingServices.Interfaces;

public interface IBillingInvoiceRefResolver
{
    /// <summary>
    /// Resolves an invoice by its globally unique <see cref="InvoiceEntity.InvoiceRef"/>.
    /// </summary>
    /// <param name="requireOutstanding">When true, fails if the invoice has no amount outstanding.</param>
    Task<Result<InvoiceEntity>> ResolveInvoiceByRefAsync(
        string invoiceRef,
        bool requireOutstanding,
        CancellationToken cancellationToken = default);
}
