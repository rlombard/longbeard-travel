using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Llm;
using AI.Forged.TourOps.Application.Models.Llm;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AI.Forged.TourOps.Infrastructure.Llm;

public class GenericLlmService(
    ILlmProviderResolver providerResolver,
    ILlmAuditLogRepository auditLogRepository,
    IOptions<LlmSettings> settings) : IGenericLlmService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public Task<LlmTextResult> GenerateTextAsync(LlmRequest request, CancellationToken cancellationToken = default) =>
        ExecuteTextRequestAsync(request, cancellationToken);

    public async Task<LlmStructuredResult<T>> GenerateStructuredAsync<T>(LlmRequest request, CancellationToken cancellationToken = default)
    {
        var structuredRequest = new LlmRequest
        {
            Category = request.Category,
            Operation = request.Operation,
            SystemInstruction = request.SystemInstruction,
            Prompt = request.Prompt,
            Model = request.Model,
            Temperature = request.Temperature,
            MaxTokens = request.MaxTokens,
            PreferStructuredOutput = true,
            Metadata = request.Metadata
        };

        var response = await ExecuteProviderRequestAsync(structuredRequest, cancellationToken);
        var parsed = ParseStructured<T>(response.Content);

        await WriteAuditAsync(request, response, response.Content, true, cancellationToken);

        return new LlmStructuredResult<T>
        {
            Data = parsed,
            RawContent = response.Content,
            Provider = response.Provider,
            Model = response.Model
        };
    }

    public Task<LlmTextResult> SummarizeAsync(LlmRequest request, CancellationToken cancellationToken = default) =>
        ExecuteTextRequestAsync(request, cancellationToken);

    public async Task<LlmClassificationResult<T>> ClassifyAsync<T>(LlmRequest request, CancellationToken cancellationToken = default)
    {
        var structured = await GenerateStructuredAsync<LlmClassificationEnvelope<T>>(request, cancellationToken);

        return new LlmClassificationResult<T>
        {
            Label = structured.Data.Label,
            Reason = structured.Data.Reason,
            Confidence = structured.Data.Confidence,
            Provider = structured.Provider,
            Model = structured.Model
        };
    }

    public async Task<LlmDraftResult> DraftReplyAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        var structured = await GenerateStructuredAsync<LlmDraftEnvelope>(request, cancellationToken);

        return new LlmDraftResult
        {
            Subject = structured.Data.Subject,
            Body = structured.Data.Body,
            Provider = structured.Provider,
            Model = structured.Model
        };
    }

    public Task<LlmStructuredResult<T>> ExtractStructuredDataAsync<T>(LlmRequest request, CancellationToken cancellationToken = default) =>
        GenerateStructuredAsync<T>(request, cancellationToken);

    private async Task<LlmTextResult> ExecuteTextRequestAsync(LlmRequest request, CancellationToken cancellationToken)
    {
        var response = await ExecuteProviderRequestAsync(request, cancellationToken);
        await WriteAuditAsync(request, response, null, true, cancellationToken);

        return new LlmTextResult
        {
            Content = response.Content,
            Provider = response.Provider,
            Model = response.Model,
            FinishReason = response.FinishReason
        };
    }

    private async Task<LlmProviderResponse> ExecuteProviderRequestAsync(LlmRequest request, CancellationToken cancellationToken)
    {
        var provider = providerResolver.Resolve(request.Metadata.TryGetValue("provider", out var providerName) ? providerName : null);

        try
        {
            return await provider.GenerateAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            await WriteAuditAsync(request, new LlmProviderResponse
            {
                Provider = provider.ProviderName,
                Model = request.Model ?? settings.Value.DefaultModel,
                Content = ex.Message
            }, null, false, cancellationToken);
            throw;
        }
    }

    private async Task WriteAuditAsync(LlmRequest request, LlmProviderResponse response, string? structuredResult, bool success, CancellationToken cancellationToken)
    {
        var promptSummary = settings.Value.EnablePromptLogging ? request.Prompt[..Math.Min(request.Prompt.Length, 1000)] : $"{request.Category}:{request.Operation}";
        var responseSummary = settings.Value.EnableResponseLogging ? response.Content[..Math.Min(response.Content.Length, 1000)] : response.FinishReason;
        var metadataJson = request.Metadata.Count == 0 ? null : JsonSerializer.Serialize(request.Metadata, JsonOptions);

        await auditLogRepository.AddAsync(new LlmAuditLog
        {
            Id = Guid.NewGuid(),
            Category = request.Category,
            Operation = request.Operation,
            Provider = response.Provider,
            Model = response.Model,
            PromptSummary = promptSummary,
            ResponseSummary = responseSummary,
            StructuredResultJson = structuredResult is null ? null : structuredResult[..Math.Min(structuredResult.Length, 8000)],
            MetadataJson = metadataJson,
            Success = success,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
    }

    private static T ParseStructured<T>(string content)
    {
        var candidate = content.Trim();
        if (candidate.StartsWith("```") && candidate.Contains('\n'))
        {
            candidate = candidate.Split('\n', 2)[1];
            if (candidate.EndsWith("```"))
            {
                candidate = candidate[..^3].Trim();
            }
        }

        return JsonSerializer.Deserialize<T>(candidate, JsonOptions)
            ?? throw new InvalidOperationException("LLM returned an empty structured response.");
    }

    private sealed class LlmClassificationEnvelope<T>
    {
        public T Label { get; set; } = default!;
        public string Reason { get; set; } = string.Empty;
        public decimal Confidence { get; set; }
    }

    private sealed class LlmDraftEnvelope
    {
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
