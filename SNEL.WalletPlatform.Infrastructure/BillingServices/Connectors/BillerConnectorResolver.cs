using SNEL.WalletPlatform.Application.BillingServices.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.BillingServices.Connectors;

public sealed class BillerConnectorResolver(IEnumerable<BillerConnector> connectors) : IBillerConnectorResolver
{
    public BillerConnector? Resolve(string billerCode)
    {
        if (string.IsNullOrWhiteSpace(billerCode))
        {
            return null;
        }

        var normalized = billerCode.Trim();
        return connectors.FirstOrDefault(x => string.Equals(x.BillerCode, normalized, StringComparison.OrdinalIgnoreCase));
    }
}
