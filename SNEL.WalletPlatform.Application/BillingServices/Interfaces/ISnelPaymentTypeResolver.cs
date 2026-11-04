using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.BillingServices.Interfaces;

public interface ISnelPaymentTypeResolver
{
    string Resolve(PaymentOrderItemEntity item);
}
