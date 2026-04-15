namespace AI.Forged.TourOps.Infrastructure.Configuration;

public sealed class MicrosoftEmailProviderSettings
{
    public bool Enabled { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string RedirectUri { get; set; } = "http://localhost:5001/api/email-integrations/oauth/callback/microsoft365";
    public string BaseUrl { get; set; } = "https://graph.microsoft.com/v1.0";
    public string Scope { get; set; } = "offline_access Mail.Read Mail.Send User.Read";
    public string DefaultTenantId { get; set; } = "common";
}
