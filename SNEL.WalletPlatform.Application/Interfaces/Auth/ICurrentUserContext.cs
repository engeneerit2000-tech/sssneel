using SNEL.WalletPlatform.Application.Common.Enums;

namespace SNEL.WalletPlatform.Application.Interfaces.Auth;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }
    string? UserId { get; }
    string? UserName { get; }
    string LanguageCode { get; }
    LanguageEnum PreferredLanguage { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsInRole(string role);
}
