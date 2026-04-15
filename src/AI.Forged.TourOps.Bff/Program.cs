using AI.Forged.TourOps.Bff.Configuration;
using AI.Forged.TourOps.Bff.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<BffSettings>()
    .Bind(builder.Configuration.GetSection("Bff"))
    .ValidateDataAnnotations()
    .Validate(
        settings => settings.AllowedPathPrefixes.Count > 0
                    && settings.AllowedPathPrefixes.All(prefix => !string.IsNullOrWhiteSpace(prefix)),
        "Bff:AllowedPathPrefixes must contain at least one value.")
    .ValidateOnStart();

builder.Services.AddHttpClient<IApiProxyService, ApiProxyService>((serviceProvider, httpClient) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<BffSettings>>().Value;
    httpClient.BaseAddress = new Uri($"{settings.ApiBaseUrl.TrimEnd('/')}/");
    httpClient.Timeout = Timeout.InfiniteTimeSpan;
});

var app = builder.Build();

app.MapGet("/health", (IOptions<BffSettings> settings) => Results.Ok(new
{
    status = "ok",
    upstream = settings.Value.ApiBaseUrl
}));

app.MapGet("/bff/health", () => Results.Ok(new { status = "ok" }));

app.MapMethods("/bff/api/{**path}", ["GET", "POST", "PUT", "PATCH", "DELETE", "HEAD"], async (
    HttpContext context,
    string? path,
    IOptions<BffSettings> settings,
    IApiProxyService proxyService,
    ILoggerFactory loggerFactory,
    CancellationToken cancellationToken) =>
{
    var normalizedPath = (path ?? string.Empty).Trim('/');
    var firstSegment = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
    var logger = loggerFactory.CreateLogger("BffRoutes");

    if (string.IsNullOrWhiteSpace(firstSegment)
        || !settings.Value.AllowedPathPrefixes.Contains(firstSegment, StringComparer.OrdinalIgnoreCase))
    {
        logger.LogWarning("Rejected BFF route for unsupported prefix: {Path}", normalizedPath);
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsJsonAsync(new { error = "Route not exposed by BFF." }, cancellationToken);
        return;
    }

    logger.LogInformation("Proxying {Method} /bff/api/{Path}", context.Request.Method, normalizedPath);
    await proxyService.ForwardAsync(context, normalizedPath, cancellationToken);
});

await app.RunAsync();
