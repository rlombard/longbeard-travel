using System.Security.Claims;
using AI.Forged.TourOps.Application.Interfaces;

namespace AI.Forged.TourOps.Api.Auth;

public class HttpContextCurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    public string GetRequiredUserId()
    {
        var user = httpContextAccessor.HttpContext?.User;
        var userId = user?.FindFirstValue("sub") ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("Authenticated user id is missing.");
        }

        return userId;
    }
}
