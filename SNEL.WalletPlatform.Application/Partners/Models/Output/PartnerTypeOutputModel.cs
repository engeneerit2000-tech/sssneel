namespace SNEL.WalletPlatform.Application.Partners.Models.Output;

public sealed class PartnerTypeOutputModel
{
    public PartnerTypeOutputModel() { }

    public PartnerTypeOutputModel(
        int id,
        string code,
        string name)
    {
        Id = id;
        Code = code;
        Name = name;
    }

    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
