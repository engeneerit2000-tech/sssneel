namespace SNEL.WalletPlatform.Application.BillingServices.Interfaces;

public interface ISnelBootstrapService
{
    Task EnsureInitializedAsync(CancellationToken ct = default);
}
