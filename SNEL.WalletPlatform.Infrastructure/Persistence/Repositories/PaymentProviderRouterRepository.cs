using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class PaymentProviderRouterRepository(ApplicationDbContext context)
    : BaseRepository<PaymentProviderRouterEntity, int>(context), IPaymentProviderRouterRepository
{
    public Task<PaymentProviderRouterEntity> GetByPrefix(string countryCode, string prefix, CancellationToken cancellationToken = default)
    {
        return Context.Set<PaymentProviderRouterEntity>().FirstOrDefaultAsync(x => x.CountryCode == countryCode && x.CompanyPrefix == prefix, cancellationToken)!;
    }

    public Task<bool> IsPhoneMatchingProvider(string countryCode, string prefix, string paymentProviderCode, CancellationToken cancellationToken = default)
    {
        return Context.Set<PaymentProviderRouterEntity>().AnyAsync(
            x => x.CountryCode == countryCode
                 && x.CompanyPrefix == prefix
                 && x.PaymentProviderCode == paymentProviderCode,
            cancellationToken);
    }
}
