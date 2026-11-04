using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class PaymentMethodRepository(ApplicationDbContext context)
    : EntityRepository<PaymentMethodEntity,long>(context), IPaymentMethodRepository
{
    public async Task<IReadOnlyCollection<PaymentMethodEntity>> GetAllWithRelationsScopedByProviderPartnerAsync(
        long? providerPartnerId,
        CancellationToken cancellationToken = default)
    {
        var query = context.Set<PaymentMethodEntity>()
            .Include(x => x.PaymentProvider)
            .ThenInclude(x => x!.Partner)
            .Include(x => x.PaymentMethodCurrencies)
            .AsQueryable();

        if (providerPartnerId.HasValue)
        {
            query = query.Where(x => x.PaymentProvider != null && x.PaymentProvider.PartnerId == providerPartnerId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<PaymentMethodEntity?> GetByCodeAndPspWithRelationsAsync(
        string paymentMethodCode,
        string paymentServiceProviderCode)
    {
        var normalizedPaymentMethodCode = paymentMethodCode.Trim().ToUpperInvariant();
        var normalizedPaymentServiceProviderCode = paymentServiceProviderCode.Trim().ToUpperInvariant();

        return context.Set<PaymentMethodEntity>()
            .Include(x => x.PaymentProvider)
            .ThenInclude(x=>x.Partner)
            .Include(x => x.PaymentMethodCurrencies)
            .FirstOrDefaultAsync(x =>
                x.Code == normalizedPaymentMethodCode &&
                x.PaymentProvider != null &&
                x.PaymentProvider.Code == normalizedPaymentServiceProviderCode);
    }
    public Task<PaymentMethodEntity?> GetByCodeAsync(
        string paymentMethodCode)
    {
        var normalizedPaymentMethodCode = paymentMethodCode.Trim().ToUpperInvariant();
        
        return context.Set<PaymentMethodEntity>()
            .Include(x => x.PaymentMethodCurrencies)
            .FirstOrDefaultAsync(x =>
                x.Code == normalizedPaymentMethodCode);
    }
    public Task<PaymentMethodEntity?> GetByIdWithRelationsAsync(long Id)
    {
        return context.Set<PaymentMethodEntity>()
            .Include(x => x.PaymentProvider)
            .ThenInclude(x => x!.Partner)
            .Include(x => x.PaymentMethodCurrencies)
            .FirstOrDefaultAsync(x => x.Id == Id);
    }
    public async Task<IReadOnlyCollection<PaymentMethodEntity>> GetByProviderIdWithRelationsAsync(long Id, CancellationToken cancellationToken = default)
    {
        return await context.Set<PaymentMethodEntity>()
            .Include(x => x.PaymentProvider)
            .ThenInclude(x => x!.Partner)
            .Include(x => x.PaymentMethodCurrencies)
            .Where(x => x.PaymentProviderId == Id)
            .ToListAsync(cancellationToken);
    }
    public Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return context.Set<PaymentMethodEntity>()
            .AnyAsync(x => x.Code == normalized, cancellationToken);
    }

    public Task<bool> ExistsByCodeExceptIdAsync(string code, long id, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return context.Set<PaymentMethodEntity>()
            .AnyAsync(x => x.Id != id && x.Code == normalized, cancellationToken);
    }
}
