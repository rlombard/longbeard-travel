using AI.Forged.TourOps.Api.Models;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Models.AdminUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AI.Forged.TourOps.Api.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/admin")]
public class AdminUserController(IAdminUserService adminUserService) : ControllerBase
{
    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<AdminUserListResponse>>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] bool? enabled,
        CancellationToken cancellationToken)
    {
        var users = await adminUserService.SearchUsersAsync(new AdminUserSearchQueryModel
        {
            SearchTerm = search,
            Enabled = enabled
        }, cancellationToken);

        return Ok(users.Select(x => x.ToResponse()).ToList());
    }

    [HttpGet("users/{userId}")]
    public async Task<ActionResult<AdminUserResponse>> GetUser(string userId, CancellationToken cancellationToken)
    {
        var user = await adminUserService.GetUserAsync(userId, cancellationToken);
        return user is null ? NotFound() : Ok(user.ToResponse());
    }

    [HttpPost("users")]
    public async Task<ActionResult<AdminUserCreateResponse>> CreateUser([FromBody] AdminUserCreateRequest request, CancellationToken cancellationToken)
    {
        var result = await adminUserService.CreateUserAsync(request.ToModel(), cancellationToken);
        return CreatedAtAction(nameof(GetUser), new { userId = result.User.Id }, result.ToResponse());
    }

    [HttpPatch("users/{userId}")]
    public async Task<ActionResult<AdminUserResponse>> UpdateUser(string userId, [FromBody] AdminUserUpdateRequest request, CancellationToken cancellationToken)
    {
        var user = await adminUserService.UpdateUserAsync(userId, request.ToModel(), cancellationToken);
        return Ok(user.ToResponse());
    }

    [HttpPost("users/{userId}/reset-password")]
    public async Task<ActionResult<AdminResetPasswordResponse>> ResetPassword(string userId, [FromBody] AdminResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await adminUserService.ResetPasswordAsync(userId, request.ToModel(), cancellationToken);
        return Ok(result.ToResponse());
    }

    [HttpPut("users/{userId}/roles")]
    public async Task<ActionResult<AdminUserResponse>> UpdateRoles(string userId, [FromBody] AdminUserRoleUpdateRequest request, CancellationToken cancellationToken)
    {
        var user = await adminUserService.UpdateRolesAsync(userId, request.ToModel(), cancellationToken);
        return Ok(user.ToResponse());
    }

    [HttpGet("roles")]
    public async Task<ActionResult<AdminAccessCatalogResponse>> GetRoles(CancellationToken cancellationToken)
    {
        var catalog = await adminUserService.GetAccessCatalogAsync(cancellationToken);
        return Ok(catalog.ToResponse());
    }
}
