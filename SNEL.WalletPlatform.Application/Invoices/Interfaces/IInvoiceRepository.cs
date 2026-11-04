using SNEL.WalletPlatform.Application.Accounting.Models.Input;
using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Application.Invoices.Models.Input;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Billing;

namespace SNEL.WalletPlatform.Application.Invoices.Interfaces;

public interface IInvoiceRepository : IEntityRepository<InvoiceEntity,Guid>
{
    Task<InvoiceEntity?> GetByIdWithBillingServiceAsync(Guid id);
    Task<IReadOnlyCollection<InvoiceEntity>> GetByBillingServiceIdAndBillingRefAsync(
        long billingServiceId,
        string billingRef,
        bool unpaidOnly = true,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InvoiceEntity>> GetInquiryPageAsync(
        InvoiceInquiryQueryInternalModel model,
        CancellationToken cancellationToken = default);
    Task<(
        int TotalCount,
        IReadOnlyDictionary<string, decimal> TotalOutstanding)> GetInquirySummaryAsync(
        InvoiceInquiryQueryInternalModel model,
        CancellationToken cancellationToken = default);
    Task<InvoiceEntity?> GetByInvoiceRefAsync(string invoiceRef);
    Task<InvoiceEntity?> GetByInvoiceRefWithBillingServiceAsync(string invoiceRef, CancellationToken cancellationToken = default);
    Task<bool> ExistsByInvoiceRefAsync(string invoiceRef);
    Task<int> CountAsync(InvoicesQueryInternalModel normalizedQuery);
    Task<IReadOnlyCollection<InvoiceEntity>> GetPagedAsync(InvoicesQueryInternalModel normalizedQuery);
    Task<List<InvoiceEntity>> GetByIDsWithBillingServiceWithBillarPartnerAsync(List<Guid> IDs);
    Task<IReadOnlyCollection<InvoiceEntity>> GetCustomerOpenInvoicesAsync(string billingRef);
}
