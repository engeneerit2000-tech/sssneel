using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Notifications.Models.Input;
using SNEL.WalletPlatform.Application.Notifications.Models.Output;

namespace SNEL.WalletPlatform.Application.Notifications.Interfaces;

public interface ICampaignService
{
    Task<Result<IReadOnlyCollection<CampaignOutputModel>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<CampaignOutputModel>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CampaignOutputModel>> CreateAsync(CreateCampaignInputModel model, CancellationToken cancellationToken = default);
    Task<Result<CampaignOutputModel>> UpdateAsync(Guid id, UpdateCampaignInputModel model, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
