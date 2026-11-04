namespace SNEL.WalletPlatform.Application.Accounting.Models.Output;

public sealed class AccountTypeOutputModel
{
    public AccountTypeOutputModel() { }

    public AccountTypeOutputModel(
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
