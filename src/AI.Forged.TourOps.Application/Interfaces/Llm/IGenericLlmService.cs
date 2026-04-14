using AI.Forged.TourOps.Application.Models.Llm;

namespace AI.Forged.TourOps.Application.Interfaces.Llm;

public interface IGenericLlmService
{
    Task<LlmTextResult> GenerateTextAsync(LlmRequest request, CancellationToken cancellationToken = default);
    Task<LlmStructuredResult<T>> GenerateStructuredAsync<T>(LlmRequest request, CancellationToken cancellationToken = default);
    Task<LlmTextResult> SummarizeAsync(LlmRequest request, CancellationToken cancellationToken = default);
    Task<LlmClassificationResult<T>> ClassifyAsync<T>(LlmRequest request, CancellationToken cancellationToken = default);
    Task<LlmDraftResult> DraftReplyAsync(LlmRequest request, CancellationToken cancellationToken = default);
    Task<LlmStructuredResult<T>> ExtractStructuredDataAsync<T>(LlmRequest request, CancellationToken cancellationToken = default);
}
