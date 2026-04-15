using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Models.AdminUsers;
using AI.Forged.TourOps.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AI.Forged.TourOps.Infrastructure.Repositories;

public class KeycloakAdminRepository(HttpClient httpClient, IOptions<KeycloakAdminSettings> settings) : IKeycloakAdminRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly HashSet<string> InternalClientIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "account",
        "account-console",
        "admin-cli",
        "broker",
        "realm-management",
        "security-admin-console"
    };

    private AccessTokenEnvelope? cachedToken;

    public async Task<IReadOnlyList<KeycloakAdminUserRecord>> SearchUsersAsync(AdminUserSearchQueryModel query, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, BuildUserSearchPath(query), cancellationToken: cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<List<KeycloakUserResponse>>(JsonOptions, cancellationToken) ?? [];

        return payload
            .Where(x => !string.IsNullOrWhiteSpace(x.Id))
            .Where(x => !query.Enabled.HasValue || x.Enabled == query.Enabled.Value)
            .Select(ToRecord)
            .ToList();
    }

    public async Task<KeycloakAdminUserRecord?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, $"admin/realms/{settings.Value.Realm}/users/{Uri.EscapeDataString(userId)}", throwOnNotFound: false, cancellationToken: cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<KeycloakUserResponse>(JsonOptions, cancellationToken);
        return payload is null ? null : ToRecord(payload);
    }

    public async Task<string> CreateUserAsync(KeycloakAdminCreateUserInput input, CancellationToken cancellationToken = default)
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

        using var response = await SendAsync(HttpMethod.Post, $"admin/realms/{settings.Value.Realm}/users", payload, cancellationToken: cancellationToken);
        if (response.Headers.Location is { Segments.Length: > 0 } location)
        {
            return location.Segments[^1].Trim('/');
        }

        var created = await SearchUsersAsync(new AdminUserSearchQueryModel { SearchTerm = input.Username }, cancellationToken);
        var user = created.FirstOrDefault(x => string.Equals(x.Username, input.Username, StringComparison.OrdinalIgnoreCase));

        return user?.Id ?? throw new InvalidOperationException("Created Keycloak user id could not be resolved.");
    }

    public Task UpdateUserAsync(string userId, KeycloakAdminUpdateUserInput input, CancellationToken cancellationToken = default) =>
        SendWithoutResponseAsync(HttpMethod.Put, $"admin/realms/{settings.Value.Realm}/users/{Uri.EscapeDataString(userId)}", new KeycloakCreateOrUpdateUserRequest
        {
            Username = input.Username,
            Email = input.Email,
            FirstName = input.FirstName,
            LastName = input.LastName,
            Enabled = input.Enabled,
            EmailVerified = input.EmailVerified,
            RequiredActions = input.RequiredActions?.ToList()
        }, cancellationToken);

    public async Task ResetTemporaryPasswordAsync(string userId, string temporaryPassword, CancellationToken cancellationToken = default)
    {
        await SendWithoutResponseAsync(HttpMethod.Put, $"admin/realms/{settings.Value.Realm}/users/{Uri.EscapeDataString(userId)}/reset-password", new KeycloakPasswordCredential
        {
            Type = "password",
            Temporary = true,
            Value = temporaryPassword
        }, cancellationToken);

        var user = await GetUserAsync(userId, cancellationToken) ?? throw new InvalidOperationException("User not found.");
        var requiredActions = user.RequiredActions
            .Append("UPDATE_PASSWORD")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        await SendWithoutResponseAsync(HttpMethod.Put, $"admin/realms/{settings.Value.Realm}/users/{Uri.EscapeDataString(userId)}", new KeycloakCreateOrUpdateUserRequest
        {
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Enabled = user.Enabled,
            EmailVerified = user.EmailVerified,
            RequiredActions = requiredActions
        }, cancellationToken);
    }

    public async Task<KeycloakAdminRoleCatalog> GetAccessCatalogAsync(CancellationToken cancellationToken = default)
    {
        using var realmRoleResponse = await SendAsync(HttpMethod.Get, $"admin/realms/{settings.Value.Realm}/roles", cancellationToken: cancellationToken);
        var realmRoles = await realmRoleResponse.Content.ReadFromJsonAsync<List<KeycloakRoleResponse>>(JsonOptions, cancellationToken) ?? [];

        using var clientResponse = await SendAsync(HttpMethod.Get, $"admin/realms/{settings.Value.Realm}/clients?max=200", cancellationToken: cancellationToken);
        var clients = await clientResponse.Content.ReadFromJsonAsync<List<KeycloakClientResponse>>(JsonOptions, cancellationToken) ?? [];

        var clientRoles = new List<KeycloakAdminClientRoleRecord>();

        foreach (var client in clients.Where(IsAssignableClient))
        {
            using var rolesResponse = await SendAsync(HttpMethod.Get, $"admin/realms/{settings.Value.Realm}/clients/{client.Id}/roles", cancellationToken: cancellationToken);
            var roles = await rolesResponse.Content.ReadFromJsonAsync<List<KeycloakRoleResponse>>(JsonOptions, cancellationToken) ?? [];
            var mappedRoles = roles
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => new KeycloakAdminRoleRecord
                {
                    Name = x.Name!,
                    Description = x.Description
                })
                .ToList();

            if (mappedRoles.Count == 0)
            {
                continue;
            }

            clientRoles.Add(new KeycloakAdminClientRoleRecord
            {
                ClientId = client.ClientId ?? string.Empty,
                ClientInternalId = client.Id ?? string.Empty,
                DisplayName = string.IsNullOrWhiteSpace(client.Name) ? client.ClientId ?? string.Empty : client.Name!,
                Roles = mappedRoles
            });
        }

        return new KeycloakAdminRoleCatalog
        {
            RealmRoles = realmRoles
                .Where(x => !string.IsNullOrWhiteSpace(x.Name) && !x.Name.StartsWith("default-roles-", StringComparison.OrdinalIgnoreCase))
                .Select(x => new KeycloakAdminRoleRecord
                {
                    Name = x.Name!,
                    Description = x.Description
                })
                .ToList(),
            ClientRoles = clientRoles
        };
    }

    public async Task<KeycloakAdminUserRoleAssignments> GetUserRoleAssignmentsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var catalog = await GetAccessCatalogAsync(cancellationToken);

        using var realmResponse = await SendAsync(HttpMethod.Get, $"admin/realms/{settings.Value.Realm}/users/{Uri.EscapeDataString(userId)}/role-mappings/realm", cancellationToken: cancellationToken);
        var realmAssignments = await realmResponse.Content.ReadFromJsonAsync<List<KeycloakRoleResponse>>(JsonOptions, cancellationToken) ?? [];

        var clientAssignments = new List<AdminClientRoleSelectionModel>();

        foreach (var client in catalog.ClientRoles)
        {
            using var response = await SendAsync(
                HttpMethod.Get,
                $"admin/realms/{settings.Value.Realm}/users/{Uri.EscapeDataString(userId)}/role-mappings/clients/{client.ClientInternalId}",
                cancellationToken: cancellationToken);
            var assignedRoles = await response.Content.ReadFromJsonAsync<List<KeycloakRoleResponse>>(JsonOptions, cancellationToken) ?? [];
            if (assignedRoles.Count == 0)
            {
                continue;
            }

            clientAssignments.Add(new AdminClientRoleSelectionModel
            {
                ClientId = client.ClientId,
                RoleNames = assignedRoles
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                    .Select(x => x.Name!)
                    .ToList()
            });
        }

        return new KeycloakAdminUserRoleAssignments
        {
            RealmRoleNames = realmAssignments
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => x.Name!)
                .ToList(),
            ClientRoles = clientAssignments
        };
    }

    public async Task ReplaceUserRoleAssignmentsAsync(string userId, AdminUserRoleUpdateModel model, CancellationToken cancellationToken = default)
    {
        var catalog = await GetAccessCatalogAsync(cancellationToken);
        var existing = await GetUserRoleAssignmentsAsync(userId, cancellationToken);

        var targetRealmRoles = catalog.RealmRoles
            .Where(x => model.RealmRoleNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
            .ToList();
        var existingRealmRoles = catalog.RealmRoles
            .Where(x => existing.RealmRoleNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
            .ToList();

        var realmRolesToAdd = targetRealmRoles.Where(x => !existingRealmRoles.Any(y => string.Equals(y.Name, x.Name, StringComparison.OrdinalIgnoreCase))).ToList();
        var realmRolesToRemove = existingRealmRoles.Where(x => !targetRealmRoles.Any(y => string.Equals(y.Name, x.Name, StringComparison.OrdinalIgnoreCase))).ToList();

        if (realmRolesToAdd.Count > 0)
        {
            await SendWithoutResponseAsync(HttpMethod.Post, $"admin/realms/{settings.Value.Realm}/users/{Uri.EscapeDataString(userId)}/role-mappings/realm", realmRolesToAdd.Select(ToRoleRepresentation).ToList(), cancellationToken);
        }

        if (realmRolesToRemove.Count > 0)
        {
            await SendWithoutResponseAsync(HttpMethod.Delete, $"admin/realms/{settings.Value.Realm}/users/{Uri.EscapeDataString(userId)}/role-mappings/realm", realmRolesToRemove.Select(ToRoleRepresentation).ToList(), cancellationToken);
        }

        foreach (var client in catalog.ClientRoles)
        {
            var target = model.ClientRoles.FirstOrDefault(x => string.Equals(x.ClientId, client.ClientId, StringComparison.OrdinalIgnoreCase));
            var existingClient = existing.ClientRoles.FirstOrDefault(x => string.Equals(x.ClientId, client.ClientId, StringComparison.OrdinalIgnoreCase));

            var targetRoles = client.Roles
                .Where(x => target?.RoleNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase) == true)
                .ToList();
            var existingRoles = client.Roles
                .Where(x => existingClient?.RoleNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase) == true)
                .ToList();

            var rolesToAdd = targetRoles.Where(x => !existingRoles.Any(y => string.Equals(y.Name, x.Name, StringComparison.OrdinalIgnoreCase))).ToList();
            var rolesToRemove = existingRoles.Where(x => !targetRoles.Any(y => string.Equals(y.Name, x.Name, StringComparison.OrdinalIgnoreCase))).ToList();

            if (rolesToAdd.Count > 0)
            {
                await SendWithoutResponseAsync(HttpMethod.Post, $"admin/realms/{settings.Value.Realm}/users/{Uri.EscapeDataString(userId)}/role-mappings/clients/{client.ClientInternalId}", rolesToAdd.Select(ToRoleRepresentation).ToList(), cancellationToken);
            }

            if (rolesToRemove.Count > 0)
            {
                await SendWithoutResponseAsync(HttpMethod.Delete, $"admin/realms/{settings.Value.Realm}/users/{Uri.EscapeDataString(userId)}/role-mappings/clients/{client.ClientInternalId}", rolesToRemove.Select(ToRoleRepresentation).ToList(), cancellationToken);
            }
        }
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

        using var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl($"realms/{settings.Value.Realm}/protocol/openid-connect/token"));
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string?>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = settings.Value.ClientId,
            ["client_secret"] = settings.Value.ClientSecret
        }!);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw await BuildKeycloakExceptionAsync(response, cancellationToken);
        }

        var envelope = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Keycloak admin token response was empty.");

        cachedToken = new AccessTokenEnvelope
        {
            AccessToken = envelope.AccessToken,
            ExpiresAtUtc = DateTime.UtcNow.AddSeconds(envelope.ExpiresIn)
        };

        return cachedToken.AccessToken;
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(settings.Value.BaseUrl))
        {
            throw new InvalidOperationException("Keycloak admin base url is not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.Value.ClientId) || string.IsNullOrWhiteSpace(settings.Value.ClientSecret))
        {
            throw new InvalidOperationException("Keycloak admin client credentials are not configured.");
        }
    }

    private string BuildUrl(string path)
    {
        ValidateSettings();
        return $"{settings.Value.BaseUrl!.TrimEnd('/')}/{path.TrimStart('/')}";
    }

    private static async Task<Exception> BuildKeycloakExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string? errorMessage = null;

        try
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(body))
            {
                using var json = JsonDocument.Parse(body);
                var root = json.RootElement;
                if (root.TryGetProperty("errorMessage", out var errorMessageElement))
                {
                    errorMessage = errorMessageElement.GetString();
                }
                else if (root.TryGetProperty("error", out var errorElement))
                {
                    errorMessage = errorElement.GetString();
                }
            }
        }
        catch
        {
        }

        return response.StatusCode switch
        {
            HttpStatusCode.Conflict => new InvalidOperationException("Keycloak rejected the request because the user or role assignment already exists."),
            HttpStatusCode.NotFound => new InvalidOperationException("Requested Keycloak user or role mapping was not found."),
            HttpStatusCode.Forbidden => new InvalidOperationException("Keycloak admin client is not allowed to perform this operation."),
            HttpStatusCode.Unauthorized => new InvalidOperationException("Keycloak admin client authentication failed."),
            _ => new InvalidOperationException(errorMessage ?? "Keycloak admin request failed.")
        };
    }

    private static bool IsAssignableClient(KeycloakClientResponse client) =>
        !string.IsNullOrWhiteSpace(client.Id)
        && !string.IsNullOrWhiteSpace(client.ClientId)
        && !InternalClientIds.Contains(client.ClientId!)
        && string.Equals(client.Protocol, "openid-connect", StringComparison.OrdinalIgnoreCase);

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

    private static KeycloakRoleRepresentation ToRoleRepresentation(KeycloakAdminRoleRecord role) => new()
    {
        Name = role.Name,
        Description = role.Description
    };

    private string BuildUserSearchPath(AdminUserSearchQueryModel query)
    {
        var queryParts = new List<string> { "max=100" };

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            queryParts.Add($"search={Uri.EscapeDataString(query.SearchTerm.Trim())}");
        }

        return $"admin/realms/{settings.Value.Realm}/users?{string.Join("&", queryParts)}";
    }

    private sealed class AccessTokenEnvelope
    {
        public string AccessToken { get; init; } = string.Empty;
        public DateTime ExpiresAtUtc { get; init; }
    }

    private sealed class KeycloakTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }

    private sealed class KeycloakUserResponse
    {
        public string? Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool? Enabled { get; set; }
        public bool? EmailVerified { get; set; }
        public List<string>? RequiredActions { get; set; }
        public long? CreatedTimestamp { get; set; }
    }

    private sealed class KeycloakRoleResponse
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    private sealed class KeycloakClientResponse
    {
        public string? Id { get; set; }
        public string? ClientId { get; set; }
        public string? Name { get; set; }
        public string? Protocol { get; set; }
    }

    private sealed class KeycloakCreateOrUpdateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
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

    private sealed class KeycloakRoleRepresentation
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
