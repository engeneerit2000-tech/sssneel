using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Partners;
using SNEL.WalletPlatform.Domain.Users;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext dbContext)
    {
       await dbContext.Database.MigrateAsync();
    }
}
