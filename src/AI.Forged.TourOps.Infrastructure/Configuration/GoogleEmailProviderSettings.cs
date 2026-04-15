namespace AI.Forged.TourOps.Infrastructure.Configuration;

public sealed class GoogleEmailProviderSettings
{
    public bool Enabled { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string RedirectUri { get; set; } = "http://localhost:5001/api/email-integrations/oauth/callback/gmail";
    public string Scope { get; set; } = "openid email https://www.googleapis.com/auth/gmail.readonly https://www.googleapis.com/auth/gmail.send";
    public string TokenUrl { get; set; } = "https://oauth2.googleapis.com/token";
    public string AuthorizationUrl { get; set; } = "https://accounts.google.com/o/oauth2/v2/auth";
    public string GmailApiBaseUrl { get; set; } = "https://gmail.googleapis.com/gmail/v1";
}
