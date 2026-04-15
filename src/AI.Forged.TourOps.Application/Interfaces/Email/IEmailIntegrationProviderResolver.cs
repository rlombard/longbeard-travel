using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Interfaces.Email;

public interface IEmailIntegrationProviderResolver
{
    IEmailIntegrationProvider GetRequiredProvider(EmailIntegrationProviderType providerType);
}
