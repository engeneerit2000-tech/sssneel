using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace SNEL.WalletPlatform.Infrastructure.Auth;

// Maps Keycloak realm roles from the "realm_access" JSON claim into standard ASP.NET role claims.
public sealed class KeycloakRealmRoleClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return Task.FromResult(principal);
        }

        var realmAccessClaim = identity.FindFirst("realm_access");
        if (realmAccessClaim is null || string.IsNullOrWhiteSpace(realmAccessClaim.Value))
        {
            return Task.FromResult(principal);
        }

        try
        {
            using var document = JsonDocument.Parse(realmAccessClaim.Value);

            if (!document.RootElement.TryGetProperty("roles", out var rolesElement) ||
                rolesElement.ValueKind != JsonValueKind.Array)
            {
                return Task.FromResult(principal);
            }

            foreach (var roleNode in rolesElement.EnumerateArray())
            {
                var role = roleNode.GetString();
                if (string.IsNullOrWhiteSpace(role))
                {
                    continue;
                }

                var exists = identity.Claims.Any(c =>
                    c.Type == ClaimTypes.Role &&
                    string.Equals(c.Value, role, StringComparison.OrdinalIgnoreCase));

                if (!exists)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }

            // Make sure NameIdentifier exists for app services that rely on it.
            if (!identity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
            {
                var sub = identity.FindFirst("sub")?.Value;
                if (!string.IsNullOrWhiteSpace(sub))
                {
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, sub));
                }
            }
        }
        catch
        {
            // Non-fatal: invalid claim format should not crash auth pipeline.
        }

        return Task.FromResult(principal);
    }
}
