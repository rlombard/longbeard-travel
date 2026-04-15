using AI.Forged.TourOps.Bff.Configuration;
using Microsoft.Extensions.Options;

namespace AI.Forged.TourOps.Bff.Services;

public sealed class ApiProxyService(
    HttpClient httpClient,
    IOptions<BffSettings> settings,
    ILogger<ApiProxyService> logger) : IApiProxyService
{
    private static readonly HashSet<string> HopByHopHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "connection",
        "content-length",
        "host",
        "keep-alive",
        "proxy-authenticate",
        "proxy-authorization",
        "te",
        "trailer",
        "transfer-encoding",
        "upgrade"
    };

    public async Task ForwardAsync(HttpContext context, string upstreamPath, CancellationToken cancellationToken)
    {
        using var request = BuildUpstreamRequest(context, upstreamPath);
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(settings.Value.UpstreamTimeoutSeconds));

        try
        {
            using var response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                timeoutCts.Token);

            await CopyUpstreamResponseAsync(context, response, timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("BFF upstream timeout for {Method} {Path}", context.Request.Method, upstreamPath);
            context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
            await context.Response.WriteAsJsonAsync(new { error = "Upstream API timed out." }, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "BFF upstream request failed for {Method} {Path}", context.Request.Method, upstreamPath);
            context.Response.StatusCode = StatusCodes.Status502BadGateway;
            await context.Response.WriteAsJsonAsync(new { error = "Upstream API request failed." }, cancellationToken);
        }
    }

    private HttpRequestMessage BuildUpstreamRequest(HttpContext context, string upstreamPath)
    {
        var target = BuildTargetPath(upstreamPath, context.Request.QueryString.Value);
        var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), target);

        if (HttpMethods.IsPost(context.Request.Method)
            || HttpMethods.IsPut(context.Request.Method)
            || HttpMethods.IsPatch(context.Request.Method))
        {
            request.Content = new StreamContent(context.Request.Body);
        }

        CopyRequestHeaders(context, request);
        AddForwardHeaders(context, request);

        return request;
    }

    private static string BuildTargetPath(string upstreamPath, string? queryString)
    {
        var path = string.IsNullOrWhiteSpace(upstreamPath) ? string.Empty : upstreamPath.TrimStart('/');
        return string.IsNullOrWhiteSpace(queryString) ? path : $"{path}{queryString}";
    }

    private static void CopyRequestHeaders(HttpContext context, HttpRequestMessage request)
    {
        foreach (var header in context.Request.Headers)
        {
            if (HopByHopHeaders.Contains(header.Key))
            {
                continue;
            }

            if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
            {
                request.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }
    }

    private static void AddForwardHeaders(HttpContext context, HttpRequestMessage request)
    {
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", context.Request.Scheme);
        request.Headers.TryAddWithoutValidation("X-Forwarded-Host", context.Request.Host.Value);

        if (context.Connection.RemoteIpAddress is not null)
        {
            request.Headers.TryAddWithoutValidation("X-Forwarded-For", context.Connection.RemoteIpAddress.ToString());
        }
    }

    private static async Task CopyUpstreamResponseAsync(HttpContext context, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        context.Response.StatusCode = (int)response.StatusCode;

        foreach (var header in response.Headers)
        {
            if (HopByHopHeaders.Contains(header.Key))
            {
                continue;
            }

            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        foreach (var header in response.Content.Headers)
        {
            if (HopByHopHeaders.Contains(header.Key))
            {
                continue;
            }

            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        context.Response.Headers.Remove("transfer-encoding");
        await response.Content.CopyToAsync(context.Response.Body, cancellationToken);
    }
}
