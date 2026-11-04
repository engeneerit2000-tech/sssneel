using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Partners.Models.Input;
using SNEL.WalletPlatform.Application.Partners.Models.Output;

namespace SNEL.WalletPlatform.Application.Partners.Interfaces;

public interface IPartnerService
{
    Task<Result<PagedResult<PartnerOutputModel>>> GetPartnersAsync(PartnersQueryInputModel model);
    Task<Result<PagedResult<PartnerOutputModel>>> GetVisiblePartnersAsync(RelatedPartnersQueryInputModel model);
}
