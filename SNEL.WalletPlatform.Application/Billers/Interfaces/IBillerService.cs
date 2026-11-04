using SNEL.WalletPlatform.Application.Billers.Models.Input;
using SNEL.WalletPlatform.Application.Billers.Models.Output;
using SNEL.WalletPlatform.Application.Common;

namespace SNEL.WalletPlatform.Application.Billers.Interfaces;

public interface IBillerService
{
    Task<Result<AddBillerOutputModel>> AddBillerAsync(AddBillerInputModel model, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyCollection<BillerOutputModel>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<BillerOutputModel>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Result<BillerOutputModel>> GetByPartnerIdAsync(long partnerId, CancellationToken cancellationToken = default);
    Task<Result<BillerOutputModel>> UpdateAsync(long id, UpdateBillerInputModel model, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
