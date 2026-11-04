using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;
using SNEL.WalletPlatform.Domain.Lookups;
using System.Runtime.CompilerServices;

namespace SNEL.WalletPlatform.Domain.Payments;

public sealed class PaymentProviderRouterEntity : TrackedEntity<int>
{    
    private PaymentProviderRouterEntity() { }
    private PaymentProviderRouterEntity(
        string countryCode,
        string? companyPrefix,
        string paymentProviderCode,
        string paymentMethodCode)
    {
        CountryCode = countryCode;
        CompanyPrefix = companyPrefix;
        PaymentProviderCode = paymentProviderCode;
        PaymentMethodCode = paymentMethodCode;
    }

    public string CountryCode { get; private set; }
    public string? CompanyPrefix { get; private set; }
    public string PaymentProviderCode { get; private set; }
    public string PaymentMethodCode { get; private set; }
    public static PaymentProviderRouterEntity Create(
        string countryCode,
        string? companyPrefix,
        string paymentProviderCode,
        string paymentMethodCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException("CountryCode cannot be empty", nameof(CountryCode));

        if (string.IsNullOrWhiteSpace(companyPrefix))
            throw new ArgumentException("CompanyPrefix cannot be empty", nameof(CompanyPrefix));

        if (string.IsNullOrWhiteSpace(paymentProviderCode))
            throw new ArgumentException("paymentProviderCode cannot be empty", nameof(PaymentProviderCode));
        
        if (string.IsNullOrWhiteSpace(paymentMethodCode))
            throw new ArgumentException("paymentMethodCode cannot be empty", nameof(PaymentMethodCode));

        var paymentProviderRouter = new PaymentProviderRouterEntity(
            countryCode,
            companyPrefix,
            paymentProviderCode,
            paymentMethodCode);
        return paymentProviderRouter;
    }    

    public void Update(
         string countryCode,
        string? companyPrefix,
        string paymentProviderCode,
        string paymentMethodCode)
    {
        CountryCode = countryCode;
        CompanyPrefix = companyPrefix;
        PaymentProviderCode = paymentProviderCode;
        PaymentMethodCode = paymentMethodCode;
    }                     
}
