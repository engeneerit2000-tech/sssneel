using SNEL.WalletPlatform.Application.Accounting.Models.Output;

namespace SNEL.WalletPlatform.Application.Billers.Models.Output;

public sealed class AddBillerOutputModel
{
    public BillerInfoOutputModel Biller { get; set; } = null!;
    public PartnerInfoOutputModel Partner { get; set; } = null!;
    public IReadOnlyCollection<AccountOutputModel> Accounts { get; set; } = null!;
}

public sealed class BillerInfoOutputModel
{
    public long Id { get; set; }
    public long PartnerId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class PartnerInfoOutputModel
{
    public long Id { get; set; }
    public int PartnerTypeId { get; set; }
    public string LegalName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public bool Active { get; set; }
}
