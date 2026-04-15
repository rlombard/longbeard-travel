namespace AI.Forged.TourOps.Infrastructure.Configuration;

public sealed class SendGridEmailProviderSettings
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = "https://api.sendgrid.com/v3";
    public string? InboundWebhookSigningSecret { get; set; }
}
