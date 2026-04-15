namespace AI.Forged.TourOps.Domain.Enums;

public enum EmailIntegrationStatus
{
    Draft = 1,
    PendingAuthorization = 2,
    Active = 3,
    NeedsReconnect = 4,
    Error = 5,
    Disabled = 6,
    Revoked = 7
}
