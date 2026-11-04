using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Payments.Models.Input;
using SNEL.WalletPlatform.Application.Payments.Models.Output;

namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

public interface IPaymentMethodService
{
    Task<Result<IReadOnlyCollection<PaymentMethodOutputModel>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<PaymentMethodOutputModel>> CreateAsync(CreatePaymentMethodInputModel model, CancellationToken cancellationToken = default);
    Task<Result<PaymentMethodOutputModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Result<PaymentMethodOutputModel>> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Result<List<PaymentMethodOutputModel>>> GetByProviderIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Result<PaymentMethodOutputModel>> UpdateAsync(long id, UpdatePaymentMethodInputModel model, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
