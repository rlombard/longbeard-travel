namespace AI.Forged.TourOps.Domain.Entities;

public class EmailProviderMessageLink
{
    public Guid Id { get; set; }
    public Guid EmailProviderConnectionId { get; set; }
    public string ProviderMessageId { get; set; } = string.Empty;
    public string? ProviderThreadId { get; set; }
    public Guid EmailThreadId { get; set; }
    public Guid EmailMessageId { get; set; }
    public string? FolderName { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public EmailProviderConnection EmailProviderConnection { get; set; } = null!;
    public EmailMessage EmailMessage { get; set; } = null!;
    public EmailThread EmailThread { get; set; } = null!;
}
