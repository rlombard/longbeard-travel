namespace AI.Forged.TourOps.Domain.Enums;

public enum SignupSessionStatus
{
    Draft = 0,
    EmailPending = 1,
    EmailVerified = 2,
    PlanSelected = 3,
    PaymentPending = 4,
    PaymentConfirmed = 5,
    TenantProvisioning = 6,
    IdentityProvisioning = 7,
    AdminBootstrap = 8,
    ConfigSeeded = 9,
    Active = 10,
    Failed = 11,
    Cancelled = 12,
    Expired = 13
}
