using SNEL.WalletPlatform.Application.Common;

namespace SNEL.WalletPlatform.Application.Accounting.Interfaces;

public interface IPlatformInvoiceService
{
    Task<Result<byte[]>> GetPlatformInvoicePdfAsync(
        long platformInvoiceId,
        string? acceptLanguage,
        CancellationToken cancellationToken = default);
}
