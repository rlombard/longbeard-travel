using System.Security.Claims;
using AI.Forged.TourOps.Api.Auth;
using Xunit;

namespace AI.Forged.TourOps.Application.Tests;

public class KeycloakRoleClaimEnricherTests
{
    [Fact]
    public void Enrich_AddsRealmAndClientRolesAsRoleClaims()
    {
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim("realm_access", """{"roles":["admin","operator"]}"""));
        identity.AddClaim(new Claim("resource_access", """{"frontend":{"roles":["trip-builder"]}}"""));

        KeycloakRoleClaimEnricher.Enrich(identity);

        var roleClaims = identity.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList();
        Assert.Contains("admin", roleClaims);
        Assert.Contains("operator", roleClaims);
        Assert.Contains("trip-builder", roleClaims);
    }
}
