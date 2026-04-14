using AI.Forged.TourOps.Application.Models.Llm;

namespace AI.Forged.TourOps.Application.Interfaces.Llm;

public interface ILlmProviderService
{
    string ProviderName { get; }
    Task<LlmProviderResponse> GenerateAsync(LlmRequest request, CancellationToken cancellationToken = default);
}
