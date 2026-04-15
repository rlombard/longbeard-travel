using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.Forged.TourOps.Infrastructure.Email;

public sealed class LogOnlyPlatformEmailSender(
    ILogger<LogOnlyPlatformEmailSender> logger,
    IOptions<SignupEmailSettings> signupEmailSettings) : IPlatformEmailSender
{
    public Task SendSignupVerificationAsync(string toEmail, string organizationName, string verificationLink, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Signup verification email queued. To={ToEmail} Org={OrganizationName} From={FromAddress} Link={VerificationLink}",
            toEmail,
            organizationName,
            signupEmailSettings.Value.FromAddress,
            verificationLink);
        return Task.CompletedTask;
    }
}
