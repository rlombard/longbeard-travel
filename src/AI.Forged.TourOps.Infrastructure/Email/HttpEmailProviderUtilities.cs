using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AI.Forged.TourOps.Application.Models.EmailIntegrations;

namespace AI.Forged.TourOps.Infrastructure.Email;

internal static class HttpEmailProviderUtilities
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<JsonDocument> SendJsonAsync(HttpClient httpClient, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(BuildErrorMessage(response.StatusCode, content));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return JsonDocument.Parse("{}");
        }

        return JsonDocument.Parse(content);
    }

    public static string ToBase64Url(ReadOnlySpan<byte> bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static string ToBase64Url(string value) => ToBase64Url(Encoding.UTF8.GetBytes(value));

    public static void ApplyBearer(HttpRequestMessage request, string accessToken, string? tokenType = null)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue(tokenType ?? "Bearer", accessToken);
    }

    public static string BuildErrorMessage(HttpStatusCode statusCode, string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return $"Provider request failed with status {(int)statusCode}.";
        }

        try
        {
            using var json = JsonDocument.Parse(body);
            var root = json.RootElement;
            if (root.TryGetProperty("error_description", out var description))
            {
                return description.GetString() ?? $"Provider request failed with status {(int)statusCode}.";
            }

            if (root.TryGetProperty("error", out var error))
            {
                return error.ValueKind == JsonValueKind.String
                    ? error.GetString() ?? $"Provider request failed with status {(int)statusCode}."
                    : error.ToString();
            }

            if (root.TryGetProperty("message", out var message))
            {
                return message.GetString() ?? $"Provider request failed with status {(int)statusCode}.";
            }
        }
        catch
        {
        }

        return body.Length <= 500 ? body : body[..500];
    }

    public static T Deserialize<T>(string json) where T : class =>
        JsonSerializer.Deserialize<T>(json, JsonOptions)
        ?? throw new InvalidOperationException("Provider response could not be parsed.");

    public static StringContent JsonBody(object payload) =>
        new(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

    public static DateTime? ReadExpiresAt(JsonElement root, string expiresFieldName = "expires_in")
    {
        if (!root.TryGetProperty(expiresFieldName, out var expiresInElement) || expiresInElement.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        var expiresInSeconds = expiresInElement.GetInt32();
        return DateTime.UtcNow.AddSeconds(expiresInSeconds);
    }
}
