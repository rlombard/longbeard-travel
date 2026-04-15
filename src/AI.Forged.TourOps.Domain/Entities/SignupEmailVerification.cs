namespace AI.Forged.TourOps.Domain.Entities;

public class SignupEmailVerification
{
    public Guid SignupSessionId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public string? LastSentEmail { get; set; }
    public int SendCount { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public string? LastAttemptIpAddress { get; set; }
    public DateTime UpdatedAt { get; set; }

    public SignupSession SignupSession { get; set; } = null!;
}
