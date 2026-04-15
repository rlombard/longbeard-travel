using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AI.Forged.TourOps.Infrastructure.Platform;

public sealed class SignupSettingsAccessor(
    IOptions<SignupSettings> signupSettings,
    IOptions<SignupBillingSettings> billingSettings) : ISignupSettingsAccessor
{
    public bool IsEnabled => signupSettings.Value.Enabled;
    public bool AllowInStandalone => signupSettings.Value.AllowInStandalone;
    public int SessionExpirationHours => signupSettings.Value.SessionExpirationHours;
    public int VerificationTokenMinutes => signupSettings.Value.VerificationTokenMinutes;
    public int VerificationResendSeconds => signupSettings.Value.VerificationResendSeconds;
    public bool ExposeDebugTokens => signupSettings.Value.ExposeDebugTokens;
    public bool AllowTestPaymentConfirmation => billingSettings.Value.AllowTestPaymentConfirmation;
    public string PublicSignupUrl => signupSettings.Value.PublicSignupUrl;
    public string BillingSupportEmail => billingSettings.Value.SupportEmail;
}
