using SNEL.WalletPlatform.Application.Payments.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.Payments.Connectors;

public sealed class PspConnectorResolver(IEnumerable<PspConnector> connectors)
    : IPspConnectorResolver
{
    public PspConnector? Resolve(string pspCode)
    {
        if (string.IsNullOrWhiteSpace(pspCode))
        {
            return null;
        }

        var normalized = pspCode.Trim();
        return connectors.FirstOrDefault(x =>
            string.Equals(x.PaymentServiceProviderCode, normalized, StringComparison.OrdinalIgnoreCase));
    }
}
