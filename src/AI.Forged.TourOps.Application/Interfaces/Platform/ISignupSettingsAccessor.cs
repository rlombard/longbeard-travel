namespace AI.Forged.TourOps.Application.Interfaces.Platform;

public interface ISignupSettingsAccessor
{
    bool IsEnabled { get; }
    bool AllowInStandalone { get; }
    int SessionExpirationHours { get; }
    int VerificationTokenMinutes { get; }
    int VerificationResendSeconds { get; }
    bool ExposeDebugTokens { get; }
    bool AllowTestPaymentConfirmation { get; }
    string PublicSignupUrl { get; }
    string BillingSupportEmail { get; }
}
