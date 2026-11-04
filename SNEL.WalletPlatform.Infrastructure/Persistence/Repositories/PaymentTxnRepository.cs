using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class PaymentTxnRepository(ApplicationDbContext context)
    : EntityRepository<PaymentTxnEntity, Guid>(context), IPaymentTxnRepository
{
    public async Task<IReadOnlyList<PaymentTxnEntity>> GetByPaymentIdAsync(
        Guid paymentId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await Context.PaymentTxns
            .AsNoTracking()
            .Where(t => t.PaymentId == paymentId)
            .OrderByDescending(t => t.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountByPaymentIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        return Context.PaymentTxns
            .AsNoTracking()
            .Where(t => t.PaymentId == paymentId)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentTxnEntity>> GetByPaymentIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        return await Context.PaymentTxns
            .AsNoTracking()
            .Where(t => t.PaymentId == paymentId)
            .OrderBy(t => t.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
