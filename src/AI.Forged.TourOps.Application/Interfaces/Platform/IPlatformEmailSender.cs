namespace AI.Forged.TourOps.Application.Interfaces.Platform;

public interface IPlatformEmailSender
{
    Task SendSignupVerificationAsync(string toEmail, string organizationName, string verificationLink, CancellationToken cancellationToken = default);
}
