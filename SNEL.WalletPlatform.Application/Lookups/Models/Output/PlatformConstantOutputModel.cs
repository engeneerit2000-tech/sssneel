namespace SNEL.WalletPlatform.Application.Lookups.Models.Output;

public sealed record PlatformConstantTranslationOutputModel(
    int Id,
    string LanguageCode,
    string Name);

public sealed record PlatformConstantOutputModel(
    int Id,
    string Code,
    string Name,
    string Group,
    string Value,
    int Order,
    bool Active,
    IReadOnlyList<PlatformConstantTranslationOutputModel> Translations);
