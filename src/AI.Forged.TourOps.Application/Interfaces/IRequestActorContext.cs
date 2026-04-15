namespace AI.Forged.TourOps.Application.Interfaces;

public interface IRequestActorContext
{
    string? GetUserIdOrNull();
    string? GetDisplayNameOrNull();
    string? GetEmailOrNull();
    bool IsPlatformAdmin();
}
