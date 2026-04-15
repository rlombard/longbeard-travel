using AI.Forged.TourOps.Api.Models;
using AI.Forged.TourOps.Application.Interfaces.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AI.Forged.TourOps.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/session")]
public sealed class SessionController(ISessionBootstrapService sessionBootstrapService) : ControllerBase
{
    [HttpGet("bootstrap")]
    public async Task<ActionResult<SessionBootstrapResponse>> GetBootstrap(CancellationToken cancellationToken)
    {
        var bootstrap = await sessionBootstrapService.GetBootstrapAsync(cancellationToken);
        return Ok(bootstrap.ToResponse());
    }

    [HttpPost("discover-tenant")]
    public async Task<ActionResult<TenantLoginDiscoveryResponse>> DiscoverTenant([FromBody] DiscoverTenantRequest request, CancellationToken cancellationToken)
    {
        var result = await sessionBootstrapService.DiscoverTenantAsync(request.ToModel(), cancellationToken);
        return Ok(result.ToResponse());
    }
}
