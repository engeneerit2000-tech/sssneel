using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Invoices.Models.Input;
using SNEL.WalletPlatform.Application.Invoices.Models.Output;

namespace SNEL.WalletPlatform.Application.Invoices.Interfaces;

public interface IInvoiceService
{
    Task<Result<PagedResult<InvoiceOutputModel>>> GetCustomerSubscriptionInvoicesAsync(InvoicesQueryInputModel model);
    Task<Result<byte[]>> GetInvoicePdfAsync(Guid invoiceId, CancellationToken cancellationToken);
    Task<Result<string>> GetInvoiceHtmlPreviewAsync(Guid invoiceId, CancellationToken cancellationToken);
}
