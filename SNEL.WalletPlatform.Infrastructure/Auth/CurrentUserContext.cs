using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Application.Common.Enums;

namespace SNEL.WalletPlatform.Infrastructure.Auth;

public sealed class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    private const string DefaultLanguageCode = "en";

    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public string? UserId =>
        User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User?.FindFirstValue("sub");

    public string? UserName =>
        User?.Identity?.Name ??
        User?.FindFirstValue("preferred_username");

    public string LanguageCode
    {
        get
        {
            var acceptLanguage = httpContextAccessor.HttpContext?.Request.Headers.AcceptLanguage.ToString();
            if (string.IsNullOrWhiteSpace(acceptLanguage))
            {
                return DefaultLanguageCode;
            }

            var candidates = acceptLanguage.Split(',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var candidate in candidates)
            {
                var languagePart = candidate.Split(';', 2, StringSplitOptions.TrimEntries)[0];
                if (string.IsNullOrWhiteSpace(languagePart))
                {
                    continue;
                }

                var normalized = languagePart.ToLowerInvariant();
                var dashIndex = normalized.IndexOf('-');
                if (dashIndex > 0)
                {
                    normalized = normalized[..dashIndex];
                }

                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    return normalized;
                }
            }

            return DefaultLanguageCode;
        }
    }

    public LanguageEnum PreferredLanguage => MapLanguageCodeToEnum(LanguageCode);

    private static LanguageEnum MapLanguageCodeToEnum(string languageCode)
    {
        return languageCode.ToLowerInvariant() switch
        {
            "en" => LanguageEnum.EN,
            "fr" => LanguageEnum.FR,
            _ => LanguageEnum.EN
        };
    }

    public IReadOnlyCollection<string> Roles =>
        User?.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "roles" || c.Type == "role")
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()
        ?? Array.Empty<string>();

    public bool IsInRole(string role) =>
        Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
}