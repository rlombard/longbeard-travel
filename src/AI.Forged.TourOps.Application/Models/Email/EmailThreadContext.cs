namespace AI.Forged.TourOps.Application.Models.Email;

public sealed class EmailThreadContext
{
    public Guid BookingId { get; init; }
    public Guid? BookingItemId { get; init; }
    public string BookingSummary { get; init; } = string.Empty;
    public string SupplierEmail { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public IReadOnlyList<string> MessageTimeline { get; init; } = [];
}
