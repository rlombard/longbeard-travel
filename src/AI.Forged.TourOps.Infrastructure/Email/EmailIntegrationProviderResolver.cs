using AI.Forged.TourOps.Application.Interfaces.Email;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Infrastructure.Email;

public sealed class EmailIntegrationProviderResolver(IEnumerable<IEmailIntegrationProvider> providers) : IEmailIntegrationProviderResolver
{
    public IEmailIntegrationProvider GetRequiredProvider(EmailIntegrationProviderType providerType) =>
        providers.FirstOrDefault(x => x.ProviderType == providerType)
        ?? throw new InvalidOperationException($"Email integration provider '{providerType}' is not registered.");
}
