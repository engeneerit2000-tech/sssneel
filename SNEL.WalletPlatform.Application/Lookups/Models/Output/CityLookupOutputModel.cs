namespace SNEL.WalletPlatform.Application.Lookups.Models.Output;

public sealed record CityLookupOutputModel(int Id, int RegionId, string Code, string NameEn, string NameFr)
    : LookupItemOutputModel(Id, Code, NameEn, NameFr);
