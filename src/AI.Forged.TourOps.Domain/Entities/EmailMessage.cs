using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Domain.Entities;

public class EmailMessage
{
    public Guid Id { get; set; }
    public Guid EmailThreadId { get; set; }
    public EmailDirection Direction { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyText { get; set; } = string.Empty;
    public string? BodyHtml { get; set; }
    public string Sender { get; set; } = string.Empty;
    public string Recipients { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool RequiresHumanReview { get; set; }
    public string? AiSummary { get; set; }
    public EmailClassificationType? AiClassification { get; set; }
    public decimal? AiConfidence { get; set; }
    public string? AiExtractedSignalsJson { get; set; }
    public DateTime CreatedAt { get; set; }

    public EmailThread Thread { get; set; } = null!;
}
