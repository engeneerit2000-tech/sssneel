namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

public interface IPspConnectorResolver
{
    PspConnector? Resolve(string pspCode);
}
