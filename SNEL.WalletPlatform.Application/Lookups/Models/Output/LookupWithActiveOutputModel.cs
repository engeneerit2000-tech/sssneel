namespace SNEL.WalletPlatform.Application.Lookups.Models.Output;

public record LookupWithActiveOutputModel(int Id, string Code, string NameEn, string NameFr, bool IsActive)
    : LookupItemOutputModel(Id, Code, NameEn, NameFr);
