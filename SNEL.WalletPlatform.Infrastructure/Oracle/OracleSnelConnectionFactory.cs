using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using SNEL.WalletPlatform.Application.Billers.Interfaces;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using System.Threading.Tasks;

namespace SNEL.WalletPlatform.Infrastructure.Oracle;

/// <summary>
/// Creates Oracle connections using the ConnectionStrings:OracleSnelConnection connection string.
/// Configure with: User Id=...;Password=...;Data Source=host:port/service_name;DBA Privilege=SYSDBA
/// </summary>
public sealed class OracleSnelConnectionFactory : IOracleSnelConnectionFactory
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;    

    public OracleSnelConnectionFactory(IConfiguration configuration, IServiceScopeFactory scopeFactory)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
    }

    public System.Data.IDbConnection CreateConnection(string area)
    {
        var scope = _scopeFactory.CreateScope();
        var _configRepository = scope.ServiceProvider.GetRequiredService<ISnelConfigService>();
        var configValue = _configRepository.GetConnectionStringAsync(area).GetAwaiter().GetResult();
        //var configValue = config.FirstOrDefault(x => x.Name.StartsWith(area) && x.BillerName == "ConnectionString");
        if (configValue is null || string.IsNullOrWhiteSpace(configValue))
        {
            throw new InvalidOperationException(
                "Connection string 'OracleSnelConnection' is not configured. Add it under ConnectionStrings:OracleSnelConnection.");
        }
        //var cs = _configuration.GetConnectionString("OracleSnelConnection");
        //if (string.IsNullOrWhiteSpace(cs))
        //    throw new InvalidOperationException(
        //        "Connection string 'OracleSnelConnection' is not configured. Add it under ConnectionStrings:OracleSnelConnection.");
        return new OracleConnection(configValue);
    }
}

