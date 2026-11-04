using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Payments.Models.Input;
using SNEL.WalletPlatform.Application.Payments.Models.Output;

namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

public interface IPaymentProviderService
{
    Task<Result<IReadOnlyCollection<PaymentProviderOutputModel>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<PaymentProviderOutputModel>> CreatePaymentProviderAsync(
        CreatePaymentProviderInputModel model,
        CancellationToken cancellationToken = default);

    Task<Result<PaymentProviderOutputModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Result<PaymentProviderOutputModel>> GetByPartnerIdAsync(long partnerId, CancellationToken cancellationToken = default);
    Task<Result<PaymentProviderOutputModel>> UpdateAsync(
        long id,
        UpdatePaymentProviderInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
