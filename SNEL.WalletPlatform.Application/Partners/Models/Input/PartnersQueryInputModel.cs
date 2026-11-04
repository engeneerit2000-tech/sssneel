using SNEL.WalletPlatform.Application.Common.Models.Input;

namespace SNEL.WalletPlatform.Application.Partners.Models.Input;

public sealed class PartnersQueryInputModel : BaseQueryInputModel
{
    public int? PartnerTypeId { get; set; }
}