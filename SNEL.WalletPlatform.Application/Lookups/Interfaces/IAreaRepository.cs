namespace SNEL.WalletPlatform.Application.Lookups.Interfaces;

public interface IAreaRepository
{
    Task<bool> ExistsByIdAsync(int areaId);

    /// <summary>
    /// Returns whether an active area with this code exists (matches <c>AreaEntity.Code</c>, case-insensitive).
    /// </summary>
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
}
