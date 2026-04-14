namespace AI.Forged.TourOps.Application.Interfaces.Llm;

public interface ILlmProviderResolver
{
    ILlmProviderService Resolve(string? providerName = null);
}
