using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SNEL.WalletPlatform.Infrastructure.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace SNEL.WalletPlatform.Presentation.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddSnelAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var keycloak = configuration.GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>() ?? new KeycloakOptions();

        if (string.IsNullOrWhiteSpace(keycloak.BaseUrl) || string.IsNullOrWhiteSpace(keycloak.Realm))
        {
            services.AddAuthorization();
            return services;
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "Default";
            options.DefaultChallengeScheme = "Default";
        })
        .AddPolicyScheme("Default", "Basic or JWT", options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                if (context.Request.Path.Value?.EndsWith("/api/v1/webhooks/easypay") == true)
                {
                    return "Basic";
                }

                return JwtBearerDefaults.AuthenticationScheme;
            };
        })
        .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(
            "Basic",
            null)
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Authority = keycloak.Authority;
            options.RequireHttpsMetadata = keycloak.RequireHttpsMetadata;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = keycloak.ValidateAudience,
                ValidAudience = string.IsNullOrWhiteSpace(keycloak.ApiClientId)
                    ? null
                    : keycloak.ApiClientId,
                NameClaimType = "preferred_username",
                RoleClaimType = ClaimTypes.Role
            };
        });

        services.AddAuthorization();

        return services;
    }

    public class BasicAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header"));

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(
                    Request.Headers["Authorization"]);

                var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);

                var username = credentials[0];
                var password = credentials[1];

                if (username != "test" || password != "test")
                    return Task.FromResult(AuthenticateResult.Fail("Invalid credentials"));

                var claims = new[]
                {
                new Claim(ClaimTypes.Name, username)
            };

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization header"));
            }
        }
    }
}
