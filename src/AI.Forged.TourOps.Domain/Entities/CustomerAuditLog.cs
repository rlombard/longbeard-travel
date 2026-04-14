namespace AI.Forged.TourOps.Domain.Entities;

public class CustomerAuditLog
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string ChangedByUserId { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ChangedFieldsJson { get; set; }
    public DateTime CreatedAt { get; set; }

    public Customer Customer { get; set; } = null!;
}
