using System.Text;
using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces.Llm;
using AI.Forged.TourOps.Application.Models.Llm;
using AI.Forged.TourOps.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AI.Forged.TourOps.Infrastructure.Llm.Providers;

public class AzureOpenAiLlmProviderService(HttpClient httpClient, IOptions<AzureOpenAiSettings> settings) : ILlmProviderService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public string ProviderName => "AzureOpenAI";

    public async Task<LlmProviderResponse> GenerateAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(settings.Value.Endpoint) || string.IsNullOrWhiteSpace(settings.Value.ApiKey) || string.IsNullOrWhiteSpace(settings.Value.DeploymentName))
        {
            throw new InvalidOperationException("Azure OpenAI is not fully configured.");
        }

        var endpoint = $"{settings.Value.Endpoint!.TrimEnd('/')}/openai/deployments/{settings.Value.DeploymentName}/chat/completions?api-version={settings.Value.ApiVersion}";
        using var message = new HttpRequestMessage(HttpMethod.Post, endpoint);
        message.Headers.Add("api-key", settings.Value.ApiKey);
        message.Content = new StringContent(JsonSerializer.Serialize(new
        {
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
        var choice = document.RootElement.GetProperty("choices")[0];
        var content = choice.GetProperty("message").GetProperty("content").GetString() ?? string.Empty;

        return new LlmProviderResponse
        {
            Content = content,
            Provider = ProviderName,
            Model = settings.Value.DeploymentName!,
            FinishReason = choice.TryGetProperty("finish_reason", out var finishReason) ? finishReason.GetString() : null
        };
    }
}
