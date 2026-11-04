using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Application.Common.Enums;

namespace SNEL.WalletPlatform.Infrastructure.Persistence;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULTCONNECTION")
            ?? "Host=localhost;Port=5432;Database=snel_wallet;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options, new DesignTimeCurrentUserContext());
    }

    private sealed class DesignTimeCurrentUserContext : ICurrentUserContext
    {
        public bool IsAuthenticated => false;
        public string? UserId => null;
        public string? UserName => null;
        public string LanguageCode => "en";
        public IReadOnlyCollection<string> Roles => Array.Empty<string>();
        public bool IsInRole(string role) => false;
        public LanguageEnum PreferredLanguage => LanguageEnum.FR;
    }
    
}
