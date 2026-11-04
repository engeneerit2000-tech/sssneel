using System.Diagnostics.Contracts;

namespace SNEL.WalletPlatform.Application.Lookups.Models.Output;

public sealed class CurrencyOutputModel
{
    public CurrencyOutputModel() { }

    public CurrencyOutputModel(
        int id,
        string code,
        string abbreviation,
        string name)
    {
        Id = id;
        Code = code;
        Abbreviation = abbreviation;
        Name = name;
    }

    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
