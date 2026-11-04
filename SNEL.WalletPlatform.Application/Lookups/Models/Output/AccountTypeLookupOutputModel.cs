namespace SNEL.WalletPlatform.Application.Lookups.Models.Output;

public sealed record AccountTypeLookupOutputModel(int Id, string Code, string NameEn, string NameFr, int Order, bool Active)
    : LookupItemOutputModel(Id, Code, NameEn, NameFr);
