using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Interfaces.IAM;
using SNEL.WalletPlatform.Infrastructure.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Infrastructure.IAM;

public sealed class KeycloakIdentityUserManager(
    IHttpClientFactory httpClientFactory,
    IOptions<KeycloakOptions> keycloakOptionsAccessor,
    ILogger<KeycloakIdentityUserManager> logger) : IIdentityUserManager
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly KeycloakOptions keycloakOptions = keycloakOptionsAccessor.Value;

    public async Task<IdentityUserModel?> FindByUsernameAsync(string username)
    {
        var users = await SearchUsersAsync($"username={Uri.EscapeDataString(username)}&exact=true");
        return users.FirstOrDefault(x => string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IdentityUserModel?> FindByEmailAsync(string email)
    {
        var users = await SearchUsersAsync($"email={Uri.EscapeDataString(email)}&exact=true");
        return users.FirstOrDefault(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IdentityUserModel> CreateUserAsync(CreateIdentityUserRequest request)
    {
        var createUrl = $"{keycloakOptions.AdminBaseUrl}/users";
        logger.LogInformation(
            "Keycloak request: POST {Url}. Body: username={Username}, email={Email}, firstName={FirstName}, lastName={LastName}, enabled={Enabled}",
            createUrl,
            request.Username,
            request.Email,
            request.FirstName,
            request.LastName,
            request.Enabled);

        var client = await CreateAdminClientAsync();

        var createBody = new
        {
            username = request.Username,
            email = request.Email,
            firstName = request.FirstName,
            lastName = request.LastName,
            enabled = request.Enabled,
            emailVerified = true
        };

        var createResponse = await client.PostAsJsonAsync(createUrl, createBody);
        var createErrorBody = createResponse.IsSuccessStatusCode
            ? null
            : await createResponse.Content.ReadAsStringAsync();
        logger.LogInformation(
            "Keycloak response: POST {Url} -> StatusCode={StatusCode}, Body: {ErrorBody}",
            createUrl,
            (int)createResponse.StatusCode,
            createErrorBody ?? "(success)");

        if (createResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            // User already exists in Keycloak (e.g. DB was recreated but IdP was not). Reuse the existing id.
            var existing = await FindByUsernameAsync(request.Username);
            if (existing is not null)
            {
                return existing;
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                existing = await FindByEmailAsync(request.Email);
                if (existing is not null)
                {
                    return existing;
                }
            }

            throw new InvalidOperationException(
                $"Keycloak returned conflict for user '{request.Username}' but the user could not be resolved by username or email.");
        }

        if (!createResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Keycloak user creation failed. Status: {createResponse.StatusCode}, Error: {createErrorBody ?? string.Empty}");
        }

        var location = createResponse.Headers.Location?.ToString();
        var userId = ExtractUserIdFromLocation(location)
                     ?? (await FindByUsernameAsync(request.Username))?.Id;

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("Keycloak user was created but user ID could not be resolved.");
        }

        await SetPasswordAsync(userId, request.Password);

        return new IdentityUserModel(userId, request.Username, request.Email);
    }

    public async Task UpdateUserAsync(UpdateIdentityUserRequest request)
    {
        var updateUrl = $"{keycloakOptions.AdminBaseUrl}/users/{request.Id}";
        logger.LogInformation(
            "Keycloak request: PUT {Url}. Body: id={Id}, username={Username}, email={Email}, firstName={FirstName}, lastName={LastName}, enabled={Enabled}",
            updateUrl,
            request.Id,
            request.Username,
            request.Email,
            request.FirstName,
            request.LastName,
            request.Enabled);

        var client = await CreateAdminClientAsync();

        var updateBody = new
        {
            id = request.Id,
            username = request.Username,
            email = request.Email,
            firstName = request.FirstName,
            lastName = request.LastName,
            enabled = request.Enabled,
            emailVerified = true
        };

        var updateResponse = await client.PutAsJsonAsync(
            updateUrl,
            updateBody);
        string? updateErrorContent = null;
        if (!updateResponse.IsSuccessStatusCode)
        {
            updateErrorContent = await updateResponse.Content.ReadAsStringAsync();
            logger.LogInformation(
                "Keycloak response: PUT {Url} -> StatusCode={StatusCode}, Error: {ErrorContent}",
                updateUrl,
                (int)updateResponse.StatusCode,
                updateErrorContent);
        }
        else
        {
            logger.LogInformation("Keycloak response: PUT {Url} -> StatusCode={StatusCode}", updateUrl, (int)updateResponse.StatusCode);
        }

        if (updateResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            throw new InvalidOperationException($"Keycloak user with username '{request.Username}' already exists.");
        }

        if (!updateResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Keycloak update failed. Status: {updateResponse.StatusCode}, Error: {updateErrorContent ?? string.Empty}");
        }

        updateResponse.EnsureSuccessStatusCode();
    }

    public async Task AssignRealmRolesAsync(string identityUserId, IReadOnlyCollection<string> roles)
    {
        if (roles.Count == 0)
        {
            return;
        }
        var client = await CreateAdminClientAsync();
        var roleRepresentations = new List<object>();

        foreach (var roleName in roles)
        {
            var getRoleUrl = $"{keycloakOptions.AdminBaseUrl}/roles/{Uri.EscapeDataString(roleName)}";
            logger.LogInformation("Keycloak request: GET {Url}", getRoleUrl);
            var response = await client.GetAsync(getRoleUrl);
            var getRoleError = response.IsSuccessStatusCode ? null : await response.Content.ReadAsStringAsync();
            logger.LogInformation(
                "Keycloak response: GET {Url} -> StatusCode={StatusCode}, Body: {Body}",
                getRoleUrl,
                (int)response.StatusCode,
                getRoleError ?? "(success)");

            response.EnsureSuccessStatusCode();

            var role = await response.Content.ReadFromJsonAsync<KeycloakRoleRepresentation>(JsonOptions)
                       ?? throw new InvalidOperationException($"Keycloak role '{roleName}' could not be read.");

            roleRepresentations.Add(new { id = role.Id, name = role.Name });
        }

        var assignUrl = $"{keycloakOptions.AdminBaseUrl}/users/{identityUserId}/role-mappings/realm";
        logger.LogInformation("Keycloak request: POST {Url}. RoleCount={Count}", assignUrl, roleRepresentations.Count);
        var assignResponse = await client.PostAsJsonAsync(
            assignUrl,
            roleRepresentations);
        var assignError = assignResponse.IsSuccessStatusCode ? null : await assignResponse.Content.ReadAsStringAsync();
        logger.LogInformation(
            "Keycloak response: POST {Url} -> StatusCode={StatusCode}, Body: {Body}",
            assignUrl,
            (int)assignResponse.StatusCode,
            assignError ?? "(success)");

        assignResponse.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyCollection<string>> GetRealmRolesAsync(string identityUserId)
    {
        var getRolesUrl = $"{keycloakOptions.AdminBaseUrl}/users/{identityUserId}/role-mappings/realm";
        logger.LogInformation("Keycloak request: GET {Url}", getRolesUrl);
        var client = await CreateAdminClientAsync();
        var response = await client.GetAsync(getRolesUrl);
        var getRolesError = response.IsSuccessStatusCode ? null : await response.Content.ReadAsStringAsync();
        logger.LogInformation(
            "Keycloak response: GET {Url} -> StatusCode={StatusCode}, for Realm {realm}, Body: {Body}",
            getRolesUrl,
            (int)response.StatusCode,
            keycloakOptions.Realm,
            getRolesError ?? "(success)");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogWarning(
                "Keycloak: user {UserId} not found in realm {realm} when loading realm role-mappings; returning no roles.",
                identityUserId,
                keycloakOptions.Realm);            
        }

        response.EnsureSuccessStatusCode();

        var roles = await response.Content.ReadFromJsonAsync<List<KeycloakRoleRepresentation>>(JsonOptions) ?? [];
        logger.LogInformation("Keycloak: resolved realm role count {Count} for user {UserId}.", roles.Count, identityUserId);
        return roles
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .ToArray();
    }

    public async Task ReplaceRealmRolesAsync(string identityUserId, IReadOnlyCollection<string> roles)
    {
        var client = await CreateAdminClientAsync();
        var existingRoleNames = await GetRealmRolesAsync(identityUserId);
        if (existingRoleNames.Count > 0)
        {
            var existingRoleRepresentations = new List<object>();
            foreach (var roleName in existingRoleNames)
            {
                var getExistingRoleUrl = $"{keycloakOptions.AdminBaseUrl}/roles/{Uri.EscapeDataString(roleName)}";
                logger.LogInformation("Keycloak request: GET {Url}", getExistingRoleUrl);
                var response = await client.GetAsync(getExistingRoleUrl);
                var getExistingError = response.IsSuccessStatusCode ? null : await response.Content.ReadAsStringAsync();
                logger.LogInformation(
                    "Keycloak response: GET {Url} -> StatusCode={StatusCode}, Body: {Body}",
                    getExistingRoleUrl,
                    (int)response.StatusCode,
                    getExistingError ?? "(success)");
                response.EnsureSuccessStatusCode();

                var role = await response.Content.ReadFromJsonAsync<KeycloakRoleRepresentation>(JsonOptions)
                           ?? throw new InvalidOperationException($"Keycloak role '{roleName}' could not be read.");
                existingRoleRepresentations.Add(new { id = role.Id, name = role.Name });
            }

            var removeUrl = $"{keycloakOptions.AdminBaseUrl}/users/{identityUserId}/role-mappings/realm";
            logger.LogInformation("Keycloak request: DELETE {Url}. RoleCount={Count}", removeUrl, existingRoleRepresentations.Count);
            var removeResponse = await client.SendAsync(new HttpRequestMessage(
                HttpMethod.Delete,
                removeUrl)
            {
                Content = JsonContent.Create(existingRoleRepresentations)
            });
            var removeError = removeResponse.IsSuccessStatusCode ? null : await removeResponse.Content.ReadAsStringAsync();
            logger.LogInformation(
                "Keycloak response: DELETE {Url} -> StatusCode={StatusCode}, Body: {Body}",
                removeUrl,
                (int)removeResponse.StatusCode,
                removeError ?? "(success)");
            removeResponse.EnsureSuccessStatusCode();
        }

        if (roles.Count > 0)
        {
            await AssignRealmRolesAsync(identityUserId, roles);
        }
    }

    public async Task DeleteUserAsync(string identityUserId)
    {
        var deleteUrl = $"{keycloakOptions.AdminBaseUrl}/users/{identityUserId}";
        logger.LogInformation("Keycloak request: DELETE {Url}", deleteUrl);
        var client = await CreateAdminClientAsync();
        var response = await client.DeleteAsync(deleteUrl);
        var deleteError = response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound
            ? null
            : await response.Content.ReadAsStringAsync();
        logger.LogInformation(
            "Keycloak response: DELETE {Url} -> StatusCode={StatusCode}, for Realm {realm}, Body: {Body}",
            deleteUrl,
            (int)response.StatusCode,
            keycloakOptions.Realm,
            deleteError ?? (response.IsSuccessStatusCode ? "(success)" : "(not found)"));

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {            
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    private async Task<List<IdentityUserModel>> SearchUsersAsync(string queryString)
    {
        var searchUrl = $"{keycloakOptions.AdminBaseUrl}/users?enabled=true&{queryString}";
        logger.LogInformation("Keycloak request: GET {Url}", searchUrl);
        var client = await CreateAdminClientAsync();
        var response = await client.GetAsync(searchUrl);
        var searchError = response.IsSuccessStatusCode ? null : await response.Content.ReadAsStringAsync();
        logger.LogInformation(
            "Keycloak response: GET users search -> StatusCode={StatusCode}, Body: {Body}",
            (int)response.StatusCode,
            searchError ?? "(success)");
        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<KeycloakUserRepresentation>>(JsonOptions) ?? [];
        logger.LogInformation("Keycloak: search returned {Count} user(s) before filter.", users.Count);
        
        return users
            .Where(x => !string.IsNullOrWhiteSpace(x.Id) && !string.IsNullOrWhiteSpace(x.Username))
            .Select(x => new IdentityUserModel(x.Id!, x.Username!, x.Email))
            .ToList();
    }

    private async Task<HttpClient> CreateAdminClientAsync()
    {
        var token = await GetAdminAccessTokenAsync();
        var client = httpClientFactory.CreateClient(KeycloakConstants.KeycloakAdminClient);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<string> GetAdminAccessTokenAsync()
    {
        if (string.IsNullOrWhiteSpace(keycloakOptions.Admin.ClientId) ||
            string.IsNullOrWhiteSpace(keycloakOptions.Admin.ClientSecret))
        {
            throw new InvalidOperationException("Keycloak admin client credentials are not configured.");
        }

        var client = httpClientFactory.CreateClient(KeycloakConstants.KeycloakAdminClient);

        var form = new Dictionary<string, string>
        {
            ["client_id"] = keycloakOptions.Admin.ClientId,
            ["client_secret"] = keycloakOptions.Admin.ClientSecret,
            ["grant_type"] = "client_credentials"
        };

        logger.LogInformation(
            "Keycloak request: POST token endpoint. Url={TokenEndpoint}, client_id={ClientId}, grant_type=client_credentials",
            keycloakOptions.TokenEndpoint,
            keycloakOptions.Admin.ClientId);
        var response = await client.PostAsync(
            keycloakOptions.TokenEndpoint,
            new FormUrlEncodedContent(form));
        var tokenError = response.IsSuccessStatusCode ? null : await response.Content.ReadAsStringAsync();
        logger.LogInformation(
            "Keycloak response: token endpoint -> StatusCode={StatusCode}, Body: {Body}",
            (int)response.StatusCode,
            tokenError ?? "(success, token omitted)");

        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(JsonOptions)
                            ?? throw new InvalidOperationException("Keycloak token response is empty.");

        if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException("Keycloak admin access token is missing.");
        }

        logger.LogInformation("Keycloak: admin access token length {TokenLength} characters.", tokenResponse.AccessToken.Length);
        return tokenResponse.AccessToken;
    }

    public async Task SetPasswordAsync(string userId, string password)
    {
        var adminBaseUrl = keycloakOptions.AdminBaseUrl;
        var resetUrl = $"{adminBaseUrl}/users/{userId}/reset-password";
        logger.LogInformation("Keycloak request: PUT {Url} (password reset, value omitted).", resetUrl);
        var client = await CreateAdminClientAsync();

        var payload = new
        {
            type = "password",
            value = password,
            temporary = false
        };

        var response = await client.PutAsJsonAsync(
            resetUrl,
            payload);
        var resetError = response.IsSuccessStatusCode ? null : await response.Content.ReadAsStringAsync();
        logger.LogInformation(
            "Keycloak response: PUT reset-password -> StatusCode={StatusCode}, Body: {Body}",
            (int)response.StatusCode,
            resetError ?? "(success)");
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> ValidatePasswordAsync(string username, string password)
    {
        var tokenUrl = $"{keycloakOptions.BaseUrl}/realms/{keycloakOptions.Realm}/protocol/openid-connect/token";
        try
        {
            using var client = new HttpClient();

            var tokenPayload = new Dictionary<string, string>
            {
                ["client_id"] = keycloakOptions.MobileClientId,
                ["grant_type"] = "password",
                ["username"] = username,
                ["password"] = password
            };

            logger.LogInformation(
                "Keycloak request: POST {Url}, grant_type=password, client_id={ClientId}, username={Username} (password omitted).",
                tokenUrl,
                keycloakOptions.MobileClientId,
                username);
            var response = await client.PostAsync(
                tokenUrl,
                new FormUrlEncodedContent(tokenPayload));
            var validateError = response.IsSuccessStatusCode ? null : await response.Content.ReadAsStringAsync();
            logger.LogInformation(
                "Keycloak response: password grant -> StatusCode={StatusCode}, Body: {Body}",
                (int)response.StatusCode,
                validateError ?? "(success, token omitted)");

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Keycloak password grant failed for user {Username}.", username);
            return false;
        }
    }

    private static string? ExtractUserIdFromLocation(string? location)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            return null;
        }

        var parts = location.TrimEnd('/').Split('/');
        return parts.Length == 0 ? null : parts[^1];
    }

    private sealed class KeycloakTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = string.Empty;
    }

    private sealed class KeycloakUserRepresentation
    {
        public string? Id { get; init; }
        public string? Username { get; init; }
        public string? Email { get; init; }
    }

    private sealed class KeycloakRoleRepresentation
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
    }
}
