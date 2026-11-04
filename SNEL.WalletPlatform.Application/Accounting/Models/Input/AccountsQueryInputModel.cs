using SNEL.WalletPlatform.Application.Common.Models.Input;

namespace SNEL.WalletPlatform.Application.Accounting.Models.Input;

public sealed class AccountsQueryInputModel : BaseQueryInputModel
{
    public long? PartnerId { get; set; }
}