using AI.Forged.TourOps.Application.Interfaces.Llm;
using AI.Forged.TourOps.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AI.Forged.TourOps.Infrastructure.Llm;

public class LlmProviderResolver(IEnumerable<ILlmProviderService> providers, IOptions<LlmSettings> settings) : ILlmProviderResolver
{
    public ILlmProviderService Resolve(string? providerName = null)
    {
        var targetProvider = string.IsNullOrWhiteSpace(providerName) ? settings.Value.DefaultProvider : providerName;
        var provider = providers.FirstOrDefault(x => string.Equals(x.ProviderName, targetProvider, StringComparison.OrdinalIgnoreCase));

        return provider ?? throw new InvalidOperationException($"LLM provider '{targetProvider}' is not registered.");
    }
}
