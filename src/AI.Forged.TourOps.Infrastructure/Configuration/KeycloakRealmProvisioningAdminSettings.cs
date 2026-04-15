namespace AI.Forged.TourOps.Infrastructure.Configuration;

public sealed class KeycloakRealmProvisioningAdminSettings
{
    public string? BaseUrl { get; set; }
    public string AdminRealm { get; set; } = "master";
    public string ClientId { get; set; } = "admin-cli";
    public string? Username { get; set; }
    public string? Password { get; set; }
}
