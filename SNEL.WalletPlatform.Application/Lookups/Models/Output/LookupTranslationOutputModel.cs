namespace SNEL.WalletPlatform.Application.Lookups.Models.Output;

public sealed record LookupTranslationOutputModel(
    int Id,
    int EntityId,
    string LanguageCode,
    string Name);
