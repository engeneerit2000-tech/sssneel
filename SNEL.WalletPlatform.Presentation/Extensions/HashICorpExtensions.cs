using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

namespace SNEL.WalletPlatform.Presentation.Extensions;

public static class HashICorpExtensions
{
    public static async Task<WebApplicationBuilder> AddHashICorp(
        this WebApplicationBuilder builder)
    {
        var env = builder.Environment.EnvironmentName;
        var enabled = builder.Configuration.GetValue<bool>("Vault:Enabled");

        if (!enabled)
        {
            return builder;
        }

        var address = builder.Configuration["Vault:Address"];
        var token = builder.Configuration["Vault:Token"];
        var mountPoint = builder.Configuration["Vault:MountPoint"] ?? "secret";
        var basePath = builder.Configuration["Vault:BasePath"] ?? "snel-wallet";

        if (string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Vault address or token is missing.");

        var authMethod = new TokenAuthMethodInfo(token);

        var vaultClient = new VaultClient(
            new VaultClientSettings(address, authMethod));

        var secretPath = $"{basePath}/{env.ToLower()}";

        var secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(
            path: secretPath,
            mountPoint: mountPoint);

        if (secret?.Data?.Data is null || secret.Data.Data.Count == 0)
        {
            throw new InvalidOperationException(
                $"No Vault secrets were found at " +
                $"'{mountPoint}/data/{secretPath}'.");
        }

        var configurationValues = secret.Data.Data.ToDictionary(
            item => item.Key.Replace("__", ":"),
            item => ConvertSecretValueToString(item.Value),
            StringComparer.OrdinalIgnoreCase);

        // Vault values override the original appsettings values.
        builder.Configuration.AddInMemoryCollection(configurationValues);

        return builder;
    }
    public static async Task<WebApplicationBuilder> AddHashICorp_Keycloak(
        this WebApplicationBuilder builder)
    {
        var enabled = builder.Configuration.GetValue<bool>("Vault:Enabled");

        if (!enabled)
        {
            return builder;
        }

        var settings = ReadSettings(builder.Configuration);

        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Step 1: Authenticate the application against Keycloak.
        var keycloakAccessToken = await GetKeycloakAccessTokenAsync(
            httpClient,
            settings);

        // Step 2: Exchange the Keycloak JWT for a Vault token.
        var vaultToken = await LoginToVaultWithJwtAsync(
            httpClient,
            settings,
            keycloakAccessToken);

        // Step 3: Use the Vault token to read the application's secrets.
        var vaultClient = CreateVaultClient(
            settings.VaultAddress,
            vaultToken);

        var environmentName =
            builder.Environment.EnvironmentName.ToLowerInvariant();

        var secretPath =
            $"{settings.VaultBasePath}/{environmentName}";

        var secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(
            path: secretPath,
            mountPoint: settings.VaultMountPoint);

        if (secret?.Data?.Data is null || secret.Data.Data.Count == 0)
        {
            throw new InvalidOperationException(
                $"No Vault secrets were found at " +
                $"'{settings.VaultMountPoint}/data/{secretPath}'.");
        }

        var configurationValues = secret.Data.Data.ToDictionary(
            item => item.Key.Replace("__", ":"),
            item => ConvertSecretValueToString(item.Value),
            StringComparer.OrdinalIgnoreCase);

        // Vault values override the original appsettings values.
        builder.Configuration.AddInMemoryCollection(configurationValues);

        return builder;
    }

    private static VaultSettings ReadSettings(
        IConfiguration configuration)
    {
        var vaultAddress = GetRequiredValue(
            configuration,
            "Vault:Address");

        var vaultMountPoint =
            configuration["Vault:MountPoint"] ?? "secret";

        var vaultBasePath =
            configuration["Vault:BasePath"] ?? "snel-wallet";

        var keycloakBaseUrl = GetRequiredValue(
            configuration,
            "Keycloak:auth-server-url");

        var keycloakRealm = GetRequiredValue(
            configuration,
            "Keycloak:realm");

        var grantType =
            configuration["Keycloak:Vault:grant_type"]
            ?? "client_credentials";

        var keycloakClientId = GetRequiredValue(
            configuration,
            "Keycloak:Vault:ClientId");

        var keycloakClientSecret = GetRequiredValue(
            configuration,
            "Keycloak:Vault:ClientSecret");

        var vaultRole = GetRequiredValue(
            configuration,
            "Keycloak:Vault:VaultRole");

        return new VaultSettings(
            VaultAddress: vaultAddress.TrimEnd('/'),
            VaultMountPoint: vaultMountPoint.Trim('/'),
            VaultBasePath: vaultBasePath.Trim('/'),
            KeycloakBaseUrl: keycloakBaseUrl.TrimEnd('/'),
            KeycloakRealm: keycloakRealm,
            KeycloakGrantType: grantType,
            KeycloakClientId: keycloakClientId,
            KeycloakClientSecret: keycloakClientSecret,
            VaultRole: vaultRole);
    }

    private static async Task<string> GetKeycloakAccessTokenAsync(
        HttpClient httpClient,
        VaultSettings settings)
    {
        var tokenEndpoint =
            $"{settings.KeycloakBaseUrl}/realms/" +
            $"{Uri.EscapeDataString(settings.KeycloakRealm)}/" +
            "protocol/openid-connect/token";

        using var requestContent = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = settings.KeycloakGrantType,
                ["client_id"] = settings.KeycloakClientId,
                ["client_secret"] = settings.KeycloakClientSecret
            });

        using var response = await httpClient.PostAsync(
            tokenEndpoint,
            requestContent);

        var responseContent =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Keycloak authentication failed. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.ReasonPhrase}. " +
                $"Response: {responseContent}");
        }

        using var jsonDocument =
            JsonDocument.Parse(responseContent);

        if (!jsonDocument.RootElement.TryGetProperty(
                "access_token",
                out var accessTokenElement))
        {
            throw new InvalidOperationException(
                "Keycloak response did not contain an access_token.");
        }

        var accessToken = accessTokenElement.GetString();

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException(
                "Keycloak returned an empty access token.");
        }

        return accessToken;
    }

    private static async Task<string> LoginToVaultWithJwtAsync(
        HttpClient httpClient,
        VaultSettings settings,
        string keycloakAccessToken)
    {
        var vaultLoginEndpoint =
            $"{settings.VaultAddress}/v1/auth/jwt/login";

        var loginRequest = new
        {
            role = settings.VaultRole,
            jwt = keycloakAccessToken
        };

        var requestJson =
            JsonSerializer.Serialize(loginRequest);

        using var requestContent = new StringContent(
            requestJson,
            Encoding.UTF8,
            "application/json");

        using var response = await httpClient.PostAsync(
            vaultLoginEndpoint,
            requestContent);

        var responseContent =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Vault JWT authentication failed. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.ReasonPhrase}. " +
                $"Response: {responseContent}");
        }

        using var jsonDocument =
            JsonDocument.Parse(responseContent);

        if (!jsonDocument.RootElement.TryGetProperty(
                "auth",
                out var authElement))
        {
            throw new InvalidOperationException(
                "Vault login response did not contain an auth object.");
        }

        if (!authElement.TryGetProperty(
                "client_token",
                out var clientTokenElement))
        {
            throw new InvalidOperationException(
                "Vault login response did not contain a client_token.");
        }

        var vaultToken = clientTokenElement.GetString();

        if (string.IsNullOrWhiteSpace(vaultToken))
        {
            throw new InvalidOperationException(
                "Vault returned an empty client token.");
        }

        return vaultToken;
    }

    private static VaultClient CreateVaultClient(
        string vaultAddress,
        string vaultToken)
    {
        var authMethod = new TokenAuthMethodInfo(vaultToken);

        var clientSettings = new VaultClientSettings(
            vaultAddress,
            authMethod);

        return new VaultClient(clientSettings);
    }

    private static string? ConvertSecretValueToString(object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.String => jsonElement.GetString(),
                JsonValueKind.Null => null,
                _ => jsonElement.ToString()
            };
        }

        return Convert.ToString(
            value,
            System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string GetRequiredValue(
        IConfiguration configuration,
        string key)
    {
        var value = configuration[key];

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Required configuration value '{key}' is missing.");
        }

        return value;
    }

    private sealed record VaultSettings(
        string VaultAddress,
        string VaultMountPoint,
        string VaultBasePath,
        string KeycloakBaseUrl,
        string KeycloakRealm,
        string KeycloakGrantType,
        string KeycloakClientId,
        string KeycloakClientSecret,
        string VaultRole);
}