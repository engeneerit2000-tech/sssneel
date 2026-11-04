namespace SNEL.WalletPlatform.Application.Lookups.Models.Output;

public sealed record AreaLookupOutputModel(int Id, int CityId, string Code, string NameEn, string NameFr)
    : LookupItemOutputModel(Id, Code, NameEn, NameFr);
