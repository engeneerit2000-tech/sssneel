using SNEL.WalletPlatform.Application.Common.Models.Input;

namespace SNEL.WalletPlatform.Application.Accounting.Models.Input;

public sealed class JournalLinesQueryInputModel : BaseQueryInputModel
{
    public Guid AccountId { get; set; }
    public int? JournalLineTypeId { get; set; }
}