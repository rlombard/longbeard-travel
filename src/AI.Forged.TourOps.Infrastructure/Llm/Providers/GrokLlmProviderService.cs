using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces.Llm;
using AI.Forged.TourOps.Application.Models.Llm;
using AI.Forged.TourOps.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AI.Forged.TourOps.Infrastructure.Llm.Providers;

public class GrokLlmProviderService(HttpClient httpClient, IOptions<GrokSettings> settings, IOptions<LlmSettings> llmSettings) : ILlmProviderService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public string ProviderName => "Grok";

    public async Task<LlmProviderResponse> GenerateAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(settings.Value.ApiKey))
        {
            throw new InvalidOperationException("Grok API key is not configured.");
        }

        var endpoint = string.IsNullOrWhiteSpace(settings.Value.BaseUrl)
            ? "https://api.x.ai/v1/chat/completions"
            : $"{settings.Value.BaseUrl!.TrimEnd('/')}/chat/completions";

        using var message = new HttpRequestMessage(HttpMethod.Post, endpoint);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.Value.ApiKey);
        message.Content = new StringContent(JsonSerializer.Serialize(new
        {
            model = request.Model ?? settings.Value.Model ?? llmSettings.Value.DefaultModel,
            temperature = request.Temperature,
            max_tokens = request.MaxTokens,
            messages = new object[]
            {
                new { role = "system", content = request.SystemInstruction },
                new { role = "user", content = request.Prompt }
            }
        }, JsonOptions), Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var root = document.RootElement;
        var choice = root.GetProperty("choices")[0];
        var content = choice.GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
        var model = root.TryGetProperty("model", out var modelElement) ? modelElement.GetString() : request.Model;
        var finishReason = choice.TryGetProperty("finish_reason", out var finishReasonElement) ? finishReasonElement.GetString() : null;

        return new LlmProviderResponse
        {
            Content = content,
            Provider = ProviderName,
            Model = model ?? settings.Value.Model ?? llmSettings.Value.DefaultModel,
            FinishReason = finishReason
        };
    }
}
