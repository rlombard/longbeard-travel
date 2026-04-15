namespace AI.Forged.TourOps.Bff.Services;

public interface IApiProxyService
{
    Task ForwardAsync(HttpContext context, string upstreamPath, CancellationToken cancellationToken);
}
