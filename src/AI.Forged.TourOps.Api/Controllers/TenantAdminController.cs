using AI.Forged.TourOps.Api.Models;
using AI.Forged.TourOps.Application.Interfaces.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AI.Forged.TourOps.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tenant-admin")]
public sealed class TenantAdminController(ITenantAdminService tenantAdminService) : ControllerBase
{
    [HttpGet("workspace")]
    public async Task<ActionResult<TenantAdminWorkspaceResponse>> GetWorkspace(CancellationToken cancellationToken)
    {
        var workspace = await tenantAdminService.GetWorkspaceAsync(cancellationToken);
        return Ok(workspace.ToResponse());
    }

    [HttpPut("workspace")]
    public async Task<ActionResult<TenantAdminWorkspaceResponse>> UpdateWorkspace([FromBody] UpdateTenantWorkspaceProfileRequest request, CancellationToken cancellationToken)
    {
        var workspace = await tenantAdminService.UpdateWorkspaceProfileAsync(request.ToModel(), cancellationToken);
        return Ok(workspace.ToResponse());
    }

    [HttpPut("config")]
    public async Task<ActionResult<TenantConfigEntryResponse>> UpsertConfig([FromBody] UpsertTenantConfigRequest request, CancellationToken cancellationToken)
    {
        var config = await tenantAdminService.UpsertConfigAsync(request.ToModel(), cancellationToken);
        return Ok(config.ToResponse());
    }

    [HttpPost("users")]
    public async Task<ActionResult<TenantWorkspaceUserCreateResponse>> CreateUser([FromBody] CreateTenantWorkspaceUserRequest request, CancellationToken cancellationToken)
    {
        var user = await tenantAdminService.CreateUserAsync(request.ToModel(), cancellationToken);
        return Ok(user.ToResponse());
    }

    [HttpPut("users/{userId}")]
    public async Task<ActionResult<TenantWorkspaceUserResponse>> UpdateUser(string userId, [FromBody] UpdateTenantWorkspaceUserRequest request, CancellationToken cancellationToken)
    {
        var user = await tenantAdminService.UpdateUserAsync(userId, request.ToModel(), cancellationToken);
        return Ok(user.ToResponse());
    }

    [HttpPost("users/{userId}/reset-password")]
    public async Task<ActionResult<TenantWorkspaceUserPasswordResetResponse>> ResetPassword(string userId, [FromBody] ResetTenantWorkspaceUserPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await tenantAdminService.ResetPasswordAsync(userId, request.ToModel(), cancellationToken);
        return Ok(result.ToResponse());
    }
}
