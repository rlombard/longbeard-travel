namespace AI.Forged.TourOps.Api.Models;

public sealed class CreateItineraryRequest
{
    public DateOnly StartDate { get; set; }
    public int Duration { get; set; }
    public List<CreateItineraryItemRequest> Items { get; set; } = [];
}

public sealed class CreateItineraryItemRequest
{
    public int DayNumber { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}

public sealed class ItineraryResponse
{
    public Guid Id { get; set; }
    public Guid? LeadCustomerId { get; set; }
    public string? LeadCustomerName { get; set; }
    public DateOnly StartDate { get; set; }
    public int Duration { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ItineraryItemResponse> Items { get; set; } = [];
}

public sealed class ItineraryItemResponse
{
    public Guid Id { get; set; }
    public int DayNumber { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}
