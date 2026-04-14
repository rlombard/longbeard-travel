using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces.Llm;
using AI.Forged.TourOps.Application.Models.Llm;
using AI.Forged.TourOps.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AI.Forged.TourOps.Infrastructure.Llm.Providers;

public class AnthropicLlmProviderService(HttpClient httpClient, IOptions<AnthropicSettings> settings) : ILlmProviderService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public string ProviderName => "Anthropic";

    public async Task<LlmProviderResponse> GenerateAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(settings.Value.ApiKey))
        {
            throw new InvalidOperationException("Anthropic API key is not configured.");
        }

        var endpoint = string.IsNullOrWhiteSpace(settings.Value.BaseUrl) ? "https://api.anthropic.com/v1/messages" : $"{settings.Value.BaseUrl!.TrimEnd('/')}/messages";
        using var message = new HttpRequestMessage(HttpMethod.Post, endpoint);
        message.Headers.Add("x-api-key", settings.Value.ApiKey);
        message.Headers.Add("anthropic-version", "2023-06-01");
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        message.Content = new StringContent(JsonSerializer.Serialize(new
        {
            model = request.Model ?? settings.Value.Model,
            max_tokens = request.MaxTokens ?? 1024,
            system = request.SystemInstruction,
            messages = new object[]
            {
                new { role = "user", content = request.Prompt }
            }
        }, JsonOptions), Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var root = document.RootElement;
        var content = root.GetProperty("content")[0].GetProperty("text").GetString() ?? string.Empty;

        return new LlmProviderResponse
        {
            Content = content,
            Provider = ProviderName,
            Model = root.TryGetProperty("model", out var modelElement) ? modelElement.GetString() ?? settings.Value.Model : settings.Value.Model,
            FinishReason = root.TryGetProperty("stop_reason", out var stopReason) ? stopReason.GetString() : null
        };
    }
}
