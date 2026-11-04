using SNEL.WalletPlatform.Application.Common.Models.Input;
using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Application.Payments.Models.Input;
using SNEL.WalletPlatform.Application.Payments.Models.Internal;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

public interface IPaymentRepository : IEntityRepository<PaymentEntity, Guid>
{
    Task<PaymentAccessContextInternalModel?> GetPaymentAccessContextAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);
    Task<PaymentEntity> GetByPaymentRefWithTransactionsAsync(string paymentTransactionReference);
    Task<PaymentEntity?> GetByIdWithApplyContextAsync(Guid paymentId);
    Task<PaymentEntity?> GetByIdWithHistoryContextAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);
    Task<PaymentEntity?> GetByIdWithPspContextAsync(Guid paymentId);
    Task<PaymentEntity?> GetByIdWithPrintContextAsync(Guid paymentId);
    Task<IReadOnlyList<PaymentEntity>> GetByIdsWithApplyContextAsync(IReadOnlyCollection<Guid> paymentIds);
    Task<IReadOnlyList<PaymentEntity>> GetCompletedPaymentsWithApplyContextAsync();
    Task<PaymentEntity?> GetByPaymentRefWithApplyContextAsync(string paymentRef);
    Task<PaymentEntity?> GetByExternalRefWithApplyContextAsync(string externalRef);
    Task<IReadOnlyList<PaymentEntity>> GetPaymentHistoryByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentEntity>> GetTopUpPaymentHistoryByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentEntity>> GetPaymentHistoryByProviderPartnerTypeAsync(string partnerTypeCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentEntity>> GetTopUpPaymentHistoryByProviderPartnerTypeAsync(string partnerTypeCode, CancellationToken cancellationToken = default);
    Task<string?> GetProviderPartnerTypeCodeAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentEntity>> GetTopupRateListAsync(
        TopupRateListInputModel model,
        CancellationToken cancellationToken = default);
    Task<int> CountTopupRateListAsync(
        TopupRateListInputModel model,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentEntity>> GetCompletedTopupReportRowsPageAsync(
        IQueryable<PaymentEntity> query,
        string? sortBy,
        bool isAscending,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<int> CountCompletedTopupReportRowsAsync(
        IQueryable<PaymentEntity> query,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentEntity>> GetPaymentHistoryByCustomerIdAsync(
        PaymentHistoryQueryInternalModel model,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentEntity>> GetPaymentHistoryByPartnerIdAsync(
        PaymentHistoryQueryInternalModel model,
        long partnerId,
        CancellationToken cancellationToken = default);
    Task<int> CountPaymentHistoryByCustomerIdAsync(
        PaymentHistoryQueryInternalModel model,
        CancellationToken cancellationToken = default);
    Task<int> CountPaymentHistoryByPartnerIdAsync(
        PaymentHistoryQueryInternalModel model,
        long partnerId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentEntity>> GetPaymentHistoryByProviderPartnerIdAsync(
        PaymentHistoryQueryInternalModel model,
        long partnerId,
        CancellationToken cancellationToken = default);
    Task<int> CountPaymentHistoryByProviderPartnerIdAsync(
        PaymentHistoryQueryInternalModel model,
        long partnerId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentEntity>> GetPaymentHistoryByScopeAsync(
        PaymentHistoryQueryInternalModel model,
        long? providerPartnerId,
        CancellationToken cancellationToken = default);
    Task<int> CountPaymentHistoryByScopeAsync(
        PaymentHistoryQueryInternalModel model,
        long? providerPartnerId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentEntity>> GetTopUpPaymentHistoryByCustomerIdAsync(
        TopUpPaymentHistoryQueryInternalModel model,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentEntity>> GetTopUpPaymentHistoryByPartnerIdAsync(
        TopUpPaymentHistoryQueryInternalModel model,
        long partnerId,
        CancellationToken cancellationToken = default);
    Task<int> CountTopUpPaymentHistoryByCustomerIdAsync(
        TopUpPaymentHistoryQueryInternalModel model,
        CancellationToken cancellationToken = default);
    Task<int> CountTopUpPaymentHistoryByPartnerIdAsync(
        TopUpPaymentHistoryQueryInternalModel model,
        long partnerId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentEntity>> GetTopUpPaymentHistoryByProviderPartnerIdAsync(
        TopUpPaymentHistoryQueryInternalModel model,
        long partnerId,
        CancellationToken cancellationToken = default);
    Task<int> CountTopUpPaymentHistoryByProviderPartnerIdAsync(
        TopUpPaymentHistoryQueryInternalModel model,
        long partnerId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentEntity>> GetTopUpPaymentHistoryByScopeAsync(
        TopUpPaymentHistoryQueryInternalModel model,
        long? providerPartnerId,
        CancellationToken cancellationToken = default);
    Task<int> CountTopUpPaymentHistoryByScopeAsync(
        TopUpPaymentHistoryQueryInternalModel model,
        long? providerPartnerId,
        CancellationToken cancellationToken = default);
    Task<PaymentEntity?> GetByIdWithConfirmContextAsync(Guid paymentId);
    Task<IReadOnlyList<Guid>> GetCanBeExpiredPaymentIdsCreatedBeforeUtcAsync(
        DateTimeOffset cutoffUtc,
        int take,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetInPaymentIdsByStatusesAsync(
        IReadOnlyCollection<PaymentStatus> statuses,
        DateTimeOffset createdBeforeUtc,
        int take,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentEntity>> GetPaymentsForReviewAsync(
        PaymentReviewQueryInputModel model,
        CancellationToken cancellationToken = default);

    Task<int> CountPaymentsForReviewAsync(
        PaymentReviewQueryInputModel model,
        CancellationToken cancellationToken = default);
}
