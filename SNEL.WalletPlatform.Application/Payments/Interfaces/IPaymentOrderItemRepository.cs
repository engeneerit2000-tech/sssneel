using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Application.Payments.Models.Input;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

public interface IPaymentOrderItemRepository : IEntityRepository<PaymentOrderItemEntity, Guid>
{
    Task<IReadOnlyCollection<PaymentOrderItemEntity>> GetByInvoicesAndTypeAndStatusAsync(
        IReadOnlyCollection<Guid> invoiceIds,
        OrderItemType itemType,
        PaymentOrderItemStatus status,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, decimal>> GetAmountTotalsByInvoicesAndTypeAndStatusAsync(
        IReadOnlyCollection<Guid> invoiceIds,
        OrderItemType itemType,
        PaymentOrderItemStatus status,
        CancellationToken cancellationToken = default);
    Task<List<PaymentOrderItemEntity>> GetByInvoiceIDAndStatusAsync(Guid InvoiceID, PaymentOrderItemStatus status);
    Task<List<PaymentOrderItemEntity>> GetByInvoicesIDsAndStatusAsync(List<Guid> InvoicesIDs, PaymentOrderItemStatus status);
    Task<List<PaymentOrderItemEntity>> GetPaidPaymentOrderItemsForApplyAsync();
    Task<List<PaymentOrderItemEntity>> GetPaidPaymentOrderItemsForBillingServiceCodeForApplyAsync(string billingServiceCode);
    Task<List<PaymentOrderItemEntity>> GetByPaymentIdForApplyAsync(Guid paymentId);
    Task<ICollection<PaymentOrderItemEntity>> GetByPaymentOrderIdAsync(Guid paymentOrderId);
    Task<IReadOnlyList<PaymentOrderItemEntity>> GetPaymentRateListAsync(
        PaymentRateListInputModel model,
        CancellationToken cancellationToken = default);
    Task<int> CountPaymentRateListAsync(
        PaymentRateListInputModel model,
        CancellationToken cancellationToken = default);
}
