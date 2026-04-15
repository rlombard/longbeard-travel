using System.Security.Claims;
using AI.Forged.TourOps.Application.Interfaces;

namespace AI.Forged.TourOps.Api.Auth;

public sealed class HttpContextRequestActorContext(IHttpContextAccessor httpContextAccessor) : IRequestActorContext
{
    public string? GetUserIdOrNull() =>
        httpContextAccessor.HttpContext?.User.FindFirstValue("sub")
        ?? httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? GetDisplayNameOrNull() =>
        httpContextAccessor.HttpContext?.User.FindFirstValue("name")
        ?? httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);

    public string? GetEmailOrNull() =>
        httpContextAccessor.HttpContext?.User.FindFirstValue("email")
        ?? httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);

    public bool IsPlatformAdmin() =>
        httpContextAccessor.HttpContext?.User.IsInRole("admin") == true;
}
