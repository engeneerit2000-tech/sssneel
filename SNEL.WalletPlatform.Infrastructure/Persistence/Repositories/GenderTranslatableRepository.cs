using SNEL.WalletPlatform.Application.Lookups.Interfaces;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class GenderTranslatableRepository(ApplicationDbContext context)
    : TranslatableLookupRepository<GenderEntity, GenderTranslationEntity, int>(context), IGenderTranslatableRepository
{
}
