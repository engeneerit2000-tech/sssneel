namespace SNEL.WalletPlatform.Application.Lookups.Models.Output;

public sealed record CurrencyLookupOutputModel(int Id, string Code, int Iso4217, int MinorUnits, string NameEn, string NameFr, bool IsActive)
    : LookupWithActiveOutputModel(Id, Code, NameEn, NameFr, IsActive);
