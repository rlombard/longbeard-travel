namespace AI.Forged.TourOps.Domain.Entities;

public class LlmAuditLog
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? PromptSummary { get; set; }
    public string? ResponseSummary { get; set; }
    public string? StructuredResultJson { get; set; }
    public string? MetadataJson { get; set; }
    public bool Success { get; set; }
    public DateTime CreatedAt { get; set; }
}
