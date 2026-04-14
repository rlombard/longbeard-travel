using AI.Forged.TourOps.Domain.Enums;
using System.Text.Json.Serialization;

namespace AI.Forged.TourOps.Api.Models;

public sealed class CreateBookingRequest
{
    public Guid QuoteId { get; set; }
}

public sealed class UpdateBookingStatusRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BookingStatus Status { get; set; }
}

public sealed class UpdateBookingItemStatusRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BookingItemStatus Status { get; set; }
}

public sealed class UpdateBookingItemNoteRequest
{
    public string? Note { get; set; }
}

public sealed class BookingListItemResponse
{
    public Guid Id { get; set; }
    public Guid QuoteId { get; set; }
    public Guid? LeadCustomerId { get; set; }
    public string? LeadCustomerName { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }
}

public sealed class BookingResponse
{
    public Guid Id { get; set; }
    public Guid QuoteId { get; set; }
    public Guid? LeadCustomerId { get; set; }
    public string? LeadCustomerName { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<BookingItemResponse> Items { get; set; } = [];
    public List<BookingTravellerResponse> Travellers { get; set; } = [];
}

public sealed class BookingItemResponse
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BookingItemStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class BookingTravellerResponse
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? RelationshipToLeadCustomer { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
