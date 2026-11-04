namespace SNEL.WalletPlatform.Application.BillingServices.Interfaces;

public interface IBillerConnectorResolver
{
    BillerConnector? Resolve(string billerCode);
}
