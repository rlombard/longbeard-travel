namespace AI.Forged.TourOps.Infrastructure.Configuration;

public sealed class SignupBillingSettings
{
    public string Provider { get; init; } = "Manual";
    public string SupportEmail { get; init; } = "sales@tourops.local";
    public bool AllowTestPaymentConfirmation { get; init; }
}
