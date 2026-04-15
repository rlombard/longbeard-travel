using System.Security.Claims;
using System.Text.Json;

namespace AI.Forged.TourOps.Api.Auth;

public static class KeycloakRoleClaimEnricher
{
    public static void Enrich(ClaimsIdentity identity)
    {
        AddRealmRoles(identity);
        AddClientRoles(identity);
    }

    private static void AddRealmRoles(ClaimsIdentity identity)
    {
        var realmAccess = identity.FindFirst("realm_access")?.Value;
        if (string.IsNullOrWhiteSpace(realmAccess))
        {
            return;
        }

        using var document = JsonDocument.Parse(realmAccess);
        if (!document.RootElement.TryGetProperty("roles", out var roles) || roles.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var role in roles.EnumerateArray().Select(x => x.GetString()).Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            AddRoleClaim(identity, role!);
        }
    }

    private static void AddClientRoles(ClaimsIdentity identity)
    {
        var resourceAccess = identity.FindFirst("resource_access")?.Value;
        if (string.IsNullOrWhiteSpace(resourceAccess))
        {
            return;
        }

        using var document = JsonDocument.Parse(resourceAccess);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var client in document.RootElement.EnumerateObject())
        {
            if (!client.Value.TryGetProperty("roles", out var roles) || roles.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var role in roles.EnumerateArray().Select(x => x.GetString()).Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                AddRoleClaim(identity, role!);
            }
        }
    }

    private static void AddRoleClaim(ClaimsIdentity identity, string role)
    {
        if (identity.HasClaim(ClaimTypes.Role, role))
        {
            return;
        }

        identity.AddClaim(new Claim(ClaimTypes.Role, role));
    }
}
