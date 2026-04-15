using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Infrastructure.Email;

public sealed class SmtpDirectEmailIntegrationProvider : MailKitEmailIntegrationProviderBase
{
    public override EmailIntegrationProviderType ProviderType => EmailIntegrationProviderType.SmtpDirect;
}
