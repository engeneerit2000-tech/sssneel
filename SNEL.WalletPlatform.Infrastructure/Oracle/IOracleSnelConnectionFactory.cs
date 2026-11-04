using SNEL.WalletPlatform.Application.Billers.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.Oracle;

/// <summary>
/// Factory for creating Oracle connections to the SNEL-DEV database (used by jobs such as SyncGcoEnCoursFacturesJob).
/// </summary>
public interface IOracleSnelConnectionFactory
{
    /// <summary>
    /// Creates a new Oracle connection. Caller is responsible for disposing it.
    /// </summary>
    System.Data.IDbConnection CreateConnection(string area);
}

