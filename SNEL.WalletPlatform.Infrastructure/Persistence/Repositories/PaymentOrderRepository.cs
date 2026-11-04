using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class PaymentOrderRepository(ApplicationDbContext context)
    : EntityRepository<PaymentOrderEntity,Guid>(context), IPaymentOrderRepository
{
    public async Task<IReadOnlyList<PaymentOrderEntity>> GetPaymentHistoryByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await Context.PaymentOrders
            .AsNoTracking()
            .Include(po => po.Items)
            .Include(po => po.Payments).ThenInclude(p => p.Currency)
            .Include(po => po.Payments).ThenInclude(p => p.PaymentMethod).ThenInclude(pm => pm!.PaymentProvider).ThenInclude(pp => pp.Partner)
            .Include(po => po.Currency)
            .Where(po => po.CustomerId == customerId
                && po.Items.Any(i => i.ItemType != OrderItemType.Topup))
            .OrderByDescending(po => po.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
    public async Task<PaymentOrderEntity> GetByIdWithItemsAndCustomerAsync(Guid id, CancellationToken ct)
    {
        return await Context.PaymentOrders
            .Include(x => x!.Items).ThenInclude(x => x.BillingService).ThenInclude(x => x!.Biller).ThenInclude(b => b.Partner)
            .Include(x => x!.Items).ThenInclude(x => x.BillingService).ThenInclude(x => x!.Translations)
            .Include(x => x!.Items).ThenInclude(x => x.BillingService).ThenInclude(x => x!.Currencies)
            .Include(x => x!.Items).ThenInclude(x => x.Invoice).ThenInclude(x => x!.BillingService).ThenInclude(x => x!.Biller).ThenInclude(b => b.Partner)
            .Include(x => x!.Items).ThenInclude(x => x.Invoice).ThenInclude(x => x!.BillingService).ThenInclude(x => x!.Translations)
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }
}
