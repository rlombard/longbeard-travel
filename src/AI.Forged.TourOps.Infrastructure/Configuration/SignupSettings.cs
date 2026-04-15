namespace AI.Forged.TourOps.Infrastructure.Configuration;

public sealed class SignupSettings
{
    public bool Enabled { get; init; }
    public bool AllowInStandalone { get; init; }
    public int SessionExpirationHours { get; init; } = 48;
    public int VerificationTokenMinutes { get; init; } = 30;
    public int VerificationResendSeconds { get; init; } = 60;
    public bool ExposeDebugTokens { get; init; }
    public string PublicSignupUrl { get; init; } = "http://localhost:3000/signup";
}
