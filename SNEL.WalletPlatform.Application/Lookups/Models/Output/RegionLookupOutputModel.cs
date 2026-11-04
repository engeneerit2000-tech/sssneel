namespace SNEL.WalletPlatform.Application.Lookups.Models.Output;

public sealed record RegionLookupOutputModel(int Id, int CountryId, string Code, string NameEn, string NameFr)
    : LookupItemOutputModel(Id, Code, NameEn, NameFr);
