namespace AI.Forged.TourOps.Domain.Entities;

public class InvoiceAttachment
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public string? ExternalFileReference { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public string? SourceUrl { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime CreatedAt { get; set; }

    public Invoice Invoice { get; set; } = null!;
}
