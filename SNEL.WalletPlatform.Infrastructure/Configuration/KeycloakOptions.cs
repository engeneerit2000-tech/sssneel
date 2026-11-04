using System.Text.Encodings.Web;

namespace SNEL.WalletPlatform.Infrastructure.Configuration;

public sealed class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    public string BaseUrl { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public string MobileClientId { get; set; } = string.Empty;
    public string ApiClientId { get; set; } = string.Empty;
    public string? ApiClientSecret { get; set; }
    public bool RequireHttpsMetadata { get; set; } = true;
    public bool ValidateAudience { get; set; } = false;

    public KeycloakAdminOptions Admin { get; set; } = new();

    public string Authority => $"{BaseUrl.TrimEnd('/')}/realms/{Realm}";
    public string TokenEndpoint => $"{Authority}/protocol/openid-connect/token";
    public string AdminBaseUrl => $"{BaseUrl.TrimEnd('/')}/admin/realms/{Realm}";    
}

public sealed class KeycloakAdminOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string? ClientSecret { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}
