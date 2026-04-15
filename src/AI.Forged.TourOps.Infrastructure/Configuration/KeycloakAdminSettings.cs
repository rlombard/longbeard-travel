namespace AI.Forged.TourOps.Infrastructure.Configuration;

public sealed class KeycloakAdminSettings
{
    public string? BaseUrl { get; set; }
    public string Realm { get; set; } = "tourops";
    public string ClientId { get; set; } = "tourops-admin-api";
    public string? ClientSecret { get; set; }
}
