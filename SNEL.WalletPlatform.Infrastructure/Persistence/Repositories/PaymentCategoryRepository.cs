using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class PaymentCategoryRepository(ApplicationDbContext context)
    : LookupRepository<PaymentCategoryEntity,int>(context), IPaymentCategoryRepository
{

}
