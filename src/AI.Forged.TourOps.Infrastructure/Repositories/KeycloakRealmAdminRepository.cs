using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Models.AdminUsers;
using AI.Forged.TourOps.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AI.Forged.TourOps.Infrastructure.Repositories;

public sealed class KeycloakRealmAdminRepository(HttpClient httpClient, IOptions<KeycloakRealmProvisioningAdminSettings> settings) : IKeycloakRealmAdminRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private AccessTokenEnvelope? cachedToken;

    public async Task EnsureRealmAsync(
        string realmName,
        string displayName,
        string clientId,
        string frontendRootUrl,
        IReadOnlyList<string> redirectUris,
        IReadOnlyList<string> webOrigins,
        CancellationToken cancellationToken = default)
    {
        var realmExists = await RealmExistsAsync(realmName, cancellationToken);
        var payload = new RealmRepresentation
        {
            Realm = realmName,
            Enabled = true,
            DisplayName = displayName,
            LoginWithEmailAllowed = true,
            DuplicateEmailsAllowed = false,
            ResetPasswordAllowed = true
        };

        if (!realmExists)
        {
            payload.Clients =
            [
                new ClientRepresentation
                {
                    ClientId = clientId,
                    Name = "TourOps Frontend",
                    PublicClient = true,
                    Protocol = "openid-connect",
                    StandardFlowEnabled = true,
                    DirectAccessGrantsEnabled = false,
                    RootUrl = frontendRootUrl,
                    BaseUrl = frontendRootUrl,
                    RedirectUris = redirectUris.ToList(),
                    WebOrigins = webOrigins.ToList()
                }
            ];
            payload.Roles = new RolesRepresentation
            {
                Realm =
                [
                    new RoleRepresentation { Name = "tenant-admin", Description = "Tenant administrator" },
                    new RoleRepresentation { Name = "operator", Description = "Tenant operator" }
                ]
            };

            await SendWithoutResponseAsync(HttpMethod.Post, "admin/realms", payload, cancellationToken);
            return;
        }

        await SendWithoutResponseAsync(HttpMethod.Put, $"admin/realms/{Uri.EscapeDataString(realmName)}", payload, cancellationToken);
        await EnsureClientAsync(realmName, clientId, frontendRootUrl, redirectUris, webOrigins, cancellationToken);
        await EnsureRealmRoleAsync(realmName, "tenant-admin", "Tenant administrator", cancellationToken);
        await EnsureRealmRoleAsync(realmName, "operator", "Tenant operator", cancellationToken);
    }

    public async Task<KeycloakAdminUserRecord?> FindUserByUsernameAsync(
        string realmName,
        string username,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            $"admin/realms/{Uri.EscapeDataString(realmName)}/users?username={Uri.EscapeDataString(username)}&exact=true",
            cancellationToken: cancellationToken);
        var users = await response.Content.ReadFromJsonAsync<List<KeycloakUserResponse>>(JsonOptions, cancellationToken) ?? [];
        var user = users.FirstOrDefault(x => string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase));
        return user is null ? null : ToRecord(user);
    }

    public async Task<string> CreateUserAsync(
        string realmName,
        KeycloakAdminCreateUserInput input,
        IReadOnlyList<string> realmRoleNames,
        CancellationToken cancellationToken = default)
    {
        var payload = new KeycloakCreateOrUpdateUserRequest
        {
            Username = input.Username,
            Email = input.Email,
            FirstName = input.FirstName,
            LastName = input.LastName,
            Enabled = input.Enabled,
            EmailVerified = input.EmailVerified,
            RequiredActions = input.RequirePasswordChange ? ["UPDATE_PASSWORD"] : [],
            Credentials =
            [
                new KeycloakPasswordCredential
                {
                    Type = "password",
                    Temporary = input.RequirePasswordChange,
                    Value = input.TemporaryPassword
                }
            ]
        };

        using var response = await SendAsync(HttpMethod.Post, $"admin/realms/{Uri.EscapeDataString(realmName)}/users", payload, cancellationToken: cancellationToken);
        var userId = response.Headers.Location is { Segments.Length: > 0 } location
            ? location.Segments[^1].Trim('/')
            : await ResolveUserIdAsync(realmName, input.Username, cancellationToken);

        if (realmRoleNames.Count > 0)
        {
            var roles = await GetRealmRolesAsync(realmName, realmRoleNames, cancellationToken);
            if (roles.Count > 0)
            {
                await SendWithoutResponseAsync(
                    HttpMethod.Post,
                    $"admin/realms/{Uri.EscapeDataString(realmName)}/users/{Uri.EscapeDataString(userId)}/role-mappings/realm",
                    roles,
                    cancellationToken);
            }
        }

        return userId;
    }

    public Task UpdateUserAsync(
        string realmName,
        string userId,
        KeycloakAdminUpdateUserInput input,
        CancellationToken cancellationToken = default) =>
        SendWithoutResponseAsync(
            HttpMethod.Put,
            $"admin/realms/{Uri.EscapeDataString(realmName)}/users/{Uri.EscapeDataString(userId)}",
            new KeycloakCreateOrUpdateUserRequest
            {
                Username = input.Username,
                Email = input.Email,
                FirstName = input.FirstName,
                LastName = input.LastName,
                Enabled = input.Enabled,
                EmailVerified = input.EmailVerified,
                RequiredActions = input.RequiredActions?.ToList()
            },
            cancellationToken);

    public async Task ResetTemporaryPasswordAsync(
        string realmName,
        string userId,
        string temporaryPassword,
        CancellationToken cancellationToken = default)
    {
        await SetPasswordAsync(realmName, userId, temporaryPassword, true, cancellationToken);
    }

    public async Task SetPasswordAsync(
        string realmName,
        string userId,
        string password,
        bool requirePasswordChange,
        CancellationToken cancellationToken = default)
    {
        await SendWithoutResponseAsync(
            HttpMethod.Put,
            $"admin/realms/{Uri.EscapeDataString(realmName)}/users/{Uri.EscapeDataString(userId)}/reset-password",
            new KeycloakPasswordCredential
            {
                Type = "password",
                Temporary = requirePasswordChange,
                Value = password
            },
            cancellationToken);
    }

    public async Task ReplaceRealmRolesAsync(
        string realmName,
        string userId,
        IReadOnlyList<string> realmRoleNames,
        CancellationToken cancellationToken = default)
    {
        var targetRoles = await GetRealmRolesAsync(realmName, realmRoleNames, cancellationToken);
        using var response = await SendAsync(
            HttpMethod.Get,
            $"admin/realms/{Uri.EscapeDataString(realmName)}/users/{Uri.EscapeDataString(userId)}/role-mappings/realm",
            cancellationToken: cancellationToken);
        var existingRoles = await response.Content.ReadFromJsonAsync<List<RoleRepresentation>>(JsonOptions, cancellationToken) ?? [];

        var rolesToAdd = targetRoles
            .Where(x => !existingRoles.Any(y => string.Equals(y.Name, x.Name, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        var rolesToRemove = existingRoles
            .Where(x => !targetRoles.Any(y => string.Equals(y.Name, x.Name, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (rolesToAdd.Count > 0)
        {
            await SendWithoutResponseAsync(
                HttpMethod.Post,
                $"admin/realms/{Uri.EscapeDataString(realmName)}/users/{Uri.EscapeDataString(userId)}/role-mappings/realm",
                rolesToAdd,
                cancellationToken);
        }

        if (rolesToRemove.Count > 0)
        {
            await SendWithoutResponseAsync(
                HttpMethod.Delete,
                $"admin/realms/{Uri.EscapeDataString(realmName)}/users/{Uri.EscapeDataString(userId)}/role-mappings/realm",
                rolesToRemove,
                cancellationToken);
        }
    }

    private async Task<bool> RealmExistsAsync(string realmName, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(HttpMethod.Get, $"admin/realms/{Uri.EscapeDataString(realmName)}", throwOnNotFound: false, cancellationToken: cancellationToken);
        return response.StatusCode != HttpStatusCode.NotFound;
    }

    private async Task EnsureClientAsync(
        string realmName,
        string clientId,
        string frontendRootUrl,
        IReadOnlyList<string> redirectUris,
        IReadOnlyList<string> webOrigins,
        CancellationToken cancellationToken)
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            $"admin/realms/{Uri.EscapeDataString(realmName)}/clients?clientId={Uri.EscapeDataString(clientId)}",
            cancellationToken: cancellationToken);
        var clients = await response.Content.ReadFromJsonAsync<List<ClientLookupResponse>>(JsonOptions, cancellationToken) ?? [];
        if (clients.Count > 0)
        {
            return;
        }

        await SendWithoutResponseAsync(
            HttpMethod.Post,
            $"admin/realms/{Uri.EscapeDataString(realmName)}/clients",
            new ClientRepresentation
            {
                ClientId = clientId,
                Name = "TourOps Frontend",
                PublicClient = true,
                Protocol = "openid-connect",
                StandardFlowEnabled = true,
                DirectAccessGrantsEnabled = false,
                RootUrl = frontendRootUrl,
                BaseUrl = frontendRootUrl,
                RedirectUris = redirectUris.ToList(),
                WebOrigins = webOrigins.ToList()
            },
            cancellationToken);
    }

    private async Task EnsureRealmRoleAsync(string realmName, string roleName, string description, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            $"admin/realms/{Uri.EscapeDataString(realmName)}/roles/{Uri.EscapeDataString(roleName)}",
            throwOnNotFound: false,
            cancellationToken: cancellationToken);

        if (response.StatusCode != HttpStatusCode.NotFound)
        {
            return;
        }

        await SendWithoutResponseAsync(
            HttpMethod.Post,
            $"admin/realms/{Uri.EscapeDataString(realmName)}/roles",
            new RoleRepresentation
            {
                Name = roleName,
                Description = description
            },
            cancellationToken);
    }

    private async Task<string> ResolveUserIdAsync(string realmName, string username, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            $"admin/realms/{Uri.EscapeDataString(realmName)}/users?username={Uri.EscapeDataString(username)}",
            cancellationToken: cancellationToken);
        var users = await response.Content.ReadFromJsonAsync<List<UserLookupResponse>>(JsonOptions, cancellationToken) ?? [];
        var userId = users.FirstOrDefault(x => string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase))?.Id;
        return !string.IsNullOrWhiteSpace(userId)
            ? userId
            : throw new InvalidOperationException("Created Keycloak user id could not be resolved.");
    }

    private async Task<IReadOnlyList<RoleRepresentation>> GetRealmRolesAsync(string realmName, IReadOnlyList<string> roleNames, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(
            HttpMethod.Get,
            $"admin/realms/{Uri.EscapeDataString(realmName)}/roles",
            cancellationToken: cancellationToken);
        var roles = await response.Content.ReadFromJsonAsync<List<RoleRepresentation>>(JsonOptions, cancellationToken) ?? [];
        return roles.Where(x => !string.IsNullOrWhiteSpace(x.Name) && roleNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase)).ToList();
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        object? body = null,
        bool throwOnNotFound = true,
        CancellationToken cancellationToken = default)
    {
        var token = await GetAdminAccessTokenAsync(cancellationToken);
        using var request = new HttpRequestMessage(method, BuildUrl(path));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            if (!throwOnNotFound && response.StatusCode == HttpStatusCode.NotFound)
            {
                return response;
            }

            throw await BuildKeycloakExceptionAsync(response, cancellationToken);
        }

        return response;
    }

    private async Task SendWithoutResponseAsync(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(method, path, body, cancellationToken: cancellationToken);
    }

    private async Task<string> GetAdminAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (cachedToken is not null && cachedToken.ExpiresAtUtc > DateTime.UtcNow.AddSeconds(15))
        {
            return cachedToken.AccessToken;
        }

        ValidateSettings();

        using var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl($"realms/{settings.Value.AdminRealm}/protocol/openid-connect/token"));
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string?>
        {
            ["grant_type"] = "password",
            ["client_id"] = settings.Value.ClientId,
            ["username"] = settings.Value.Username,
            ["password"] = settings.Value.Password
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw await BuildKeycloakExceptionAsync(response, cancellationToken);
        }

        var envelope = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Keycloak admin token response was empty.");

        cachedToken = new AccessTokenEnvelope
        {
            AccessToken = envelope.AccessToken ?? throw new InvalidOperationException("Keycloak admin access token was empty."),
            ExpiresAtUtc = DateTime.UtcNow.AddSeconds(Math.Max(30, envelope.ExpiresIn))
        };

        return cachedToken.AccessToken;
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(settings.Value.BaseUrl))
        {
            throw new InvalidOperationException("Keycloak admin base url is not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.Value.ClientId)
            || string.IsNullOrWhiteSpace(settings.Value.Username)
            || string.IsNullOrWhiteSpace(settings.Value.Password))
        {
            throw new InvalidOperationException("Keycloak realm provisioning admin credentials are not configured.");
        }
    }

    private string BuildUrl(string path) => $"{settings.Value.BaseUrl!.TrimEnd('/')}/{path.TrimStart('/')}";

    private static async Task<Exception> BuildKeycloakExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);

        return response.StatusCode switch
        {
            HttpStatusCode.Conflict => new InvalidOperationException("Keycloak rejected the request because the realm, user, or client already exists."),
            HttpStatusCode.NotFound => new InvalidOperationException("Requested Keycloak realm resource was not found."),
            HttpStatusCode.Forbidden => new InvalidOperationException("Keycloak admin client is not allowed to perform this operation."),
            HttpStatusCode.Unauthorized => new InvalidOperationException("Keycloak admin client authentication failed."),
            _ => new InvalidOperationException(errorMessage ?? "Keycloak admin request failed.")
        };
    }

    private sealed class AccessTokenEnvelope
    {
        public string AccessToken { get; init; } = string.Empty;
        public DateTime ExpiresAtUtc { get; init; }
    }

    private sealed class KeycloakTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; init; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }
    }

    private sealed class RealmRepresentation
    {
        public string Realm { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public bool LoginWithEmailAllowed { get; set; }
        public bool DuplicateEmailsAllowed { get; set; }
        public bool ResetPasswordAllowed { get; set; }
        public List<ClientRepresentation>? Clients { get; set; }
        public RolesRepresentation? Roles { get; set; }
    }

    private sealed class RolesRepresentation
    {
        public List<RoleRepresentation> Realm { get; set; } = [];
    }

    private sealed class ClientRepresentation
    {
        public string ClientId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool PublicClient { get; set; }
        public string Protocol { get; set; } = "openid-connect";
        public bool StandardFlowEnabled { get; set; }
        public bool DirectAccessGrantsEnabled { get; set; }
        public string RootUrl { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public List<string> RedirectUris { get; set; } = [];
        public List<string> WebOrigins { get; set; } = [];
    }

    private sealed class ClientLookupResponse
    {
        public string? Id { get; init; }
        public string? ClientId { get; init; }
    }

    private sealed class UserLookupResponse
    {
        public string? Id { get; init; }
        public string? Username { get; init; }
    }

    private sealed class KeycloakUserResponse
    {
        public string? Id { get; init; }
        public string? Username { get; init; }
        public string? Email { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public bool? Enabled { get; init; }
        public bool? EmailVerified { get; init; }
        public List<string>? RequiredActions { get; init; }
        public long? CreatedTimestamp { get; init; }
    }

    private sealed class RoleRepresentation
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    private sealed class KeycloakCreateOrUpdateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public bool EmailVerified { get; set; }
        public List<string>? RequiredActions { get; set; }
        public List<KeycloakPasswordCredential>? Credentials { get; set; }
    }

    private sealed class KeycloakPasswordCredential
    {
        public string Type { get; set; } = "password";
        public bool Temporary { get; set; }
        public string Value { get; set; } = string.Empty;
    }

    private static KeycloakAdminUserRecord ToRecord(KeycloakUserResponse response) => new()
    {
        Id = response.Id ?? string.Empty,
        Username = response.Username ?? string.Empty,
        Email = response.Email,
        FirstName = response.FirstName ?? string.Empty,
        LastName = response.LastName ?? string.Empty,
        Enabled = response.Enabled ?? false,
        EmailVerified = response.EmailVerified ?? false,
        RequiredActions = response.RequiredActions ?? [],
        CreatedAt = response.CreatedTimestamp.HasValue
            ? DateTimeOffset.FromUnixTimeMilliseconds(response.CreatedTimestamp.Value).UtcDateTime
            : null
    };
}
