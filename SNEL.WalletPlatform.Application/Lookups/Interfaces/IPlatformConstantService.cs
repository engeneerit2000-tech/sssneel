using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Lookups.Models.Output;

namespace SNEL.WalletPlatform.Application.Lookups.Interfaces;

public interface IPlatformConstantService
{
    Task<Result<string>> GetLatestMobileVersionAsync();
    Task<Result<IReadOnlyList<PlatformConstantOutputModel>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<(string PaymentServiceProviderCode, string PaymentMethodCode, bool requirePhoneNumber)>> GetDefaultPaymentMethodCodesAsync(
        CancellationToken cancellationToken = default);
}
