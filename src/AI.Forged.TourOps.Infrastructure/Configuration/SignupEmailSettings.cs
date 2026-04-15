namespace AI.Forged.TourOps.Infrastructure.Configuration;

public sealed class SignupEmailSettings
{
    public string Provider { get; init; } = "LogOnly";
    public string FromAddress { get; init; } = "noreply@tourops.local";
    public string FromDisplayName { get; init; } = "AI Forged TourOps";
    public string? SmtpHost { get; init; }
    public int SmtpPort { get; init; } = 587;
    public bool UseSsl { get; init; } = true;
    public string? Username { get; init; }
    public string? Password { get; init; }
}
