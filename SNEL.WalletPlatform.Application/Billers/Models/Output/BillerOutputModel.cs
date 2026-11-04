using SNEL.WalletPlatform.Application.Accounting.Models.Output;

namespace SNEL.WalletPlatform.Application.Billers.Models.Output;

public sealed class BillerOutputModel
{
    public BillerOutputModel() { }

    public BillerOutputModel(
        long id,
        long partnerId,
        string code,
        string name,
        IReadOnlyCollection<AccountOutputModel> accounts,
        bool applyAfterPay,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        string createdBy,
        string updatedBy)
    {
        Id = id;
        PartnerId = partnerId;
        Code = code;
        Name = name;
        Accounts = accounts;
        ApplyAfterPay = applyAfterPay;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        CreatedBy = createdBy;
        UpdatedBy = updatedBy;
    }

    public long Id { get; set; }
    public long PartnerId { get; set; }
    public PartnerInfoOutputModel? Partner { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public IReadOnlyCollection<AccountOutputModel> Accounts { get; set; } = [];
    public bool ApplyAfterPay { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}
