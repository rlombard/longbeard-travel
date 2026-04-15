using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace AI.Forged.TourOps.Api.Auth;

public sealed class KeycloakMultiRealmSecurityKeyProvider(string internalBaseUrl, string publicBaseUrl, string managementRealm, string tenantRealmPrefix)
{
    private readonly HttpClient httpClient = new();
    private readonly ConcurrentDictionary<string, IReadOnlyList<SecurityKey>> signingKeyCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string internalBaseUrl = internalBaseUrl.TrimEnd('/');
    private readonly string publicBaseUrl = publicBaseUrl.TrimEnd('/');
    private readonly string managementRealm = managementRealm;
    private readonly string tenantRealmPrefix = tenantRealmPrefix;

    public string ValidateIssuer(string issuer)
    {
        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new SecurityTokenInvalidIssuerException("Token issuer was empty.");
        }

        var normalized = issuer.TrimEnd('/');
        if (IsManagementIssuer(normalized) || IsTenantIssuer(normalized))
        {
            return normalized;
        }

        throw new SecurityTokenInvalidIssuerException($"Token issuer '{issuer}' is not allowed.");
    }

    public IEnumerable<SecurityKey> ResolveSigningKeys(string token)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var issuer = ValidateIssuer(jwt.Issuer);
        return signingKeyCache.GetOrAdd(issuer, LoadSigningKeys);
    }

    private bool IsManagementIssuer(string issuer) =>
        string.Equals(issuer, $"{publicBaseUrl}/realms/{managementRealm}", StringComparison.OrdinalIgnoreCase)
        || string.Equals(issuer, $"{internalBaseUrl}/realms/{managementRealm}", StringComparison.OrdinalIgnoreCase);

    private bool IsTenantIssuer(string issuer)
    {
        var publicPrefix = $"{publicBaseUrl}/realms/{tenantRealmPrefix}";
        var internalPrefix = $"{internalBaseUrl}/realms/{tenantRealmPrefix}";
        return issuer.StartsWith(publicPrefix, StringComparison.OrdinalIgnoreCase)
            || issuer.StartsWith(internalPrefix, StringComparison.OrdinalIgnoreCase);
    }

    private IReadOnlyList<SecurityKey> LoadSigningKeys(string issuer)
    {
        var effectiveIssuer = issuer.StartsWith(publicBaseUrl, StringComparison.OrdinalIgnoreCase)
            ? $"{internalBaseUrl}{issuer[publicBaseUrl.Length..]}"
            : issuer;
        var certsUrl = $"{effectiveIssuer.TrimEnd('/')}/protocol/openid-connect/certs";
        var json = httpClient.GetStringAsync(certsUrl).GetAwaiter().GetResult();
        var payload = JsonSerializer.Deserialize<JwksDocument>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException("Keycloak JWKS response was empty.");

        return payload.Keys
            .Where(x => !string.IsNullOrWhiteSpace(x.Kid) && !string.IsNullOrWhiteSpace(x.N) && !string.IsNullOrWhiteSpace(x.E))
            .Select(x => new JsonWebKey
            {
                Kid = x.Kid,
                Kty = x.Kty,
                Use = x.Use,
                N = x.N,
                E = x.E
            })
            .Cast<SecurityKey>()
            .ToList();
    }

    private sealed class JwksDocument
    {
        public List<JwksKey> Keys { get; init; } = [];
    }

    private sealed class JwksKey
    {
        public string? Kid { get; init; }
        public string? Kty { get; init; }
        public string? Use { get; init; }
        public string? N { get; init; }
        public string? E { get; init; }
        public IList<string>? X5c { get; init; }
    }
}
