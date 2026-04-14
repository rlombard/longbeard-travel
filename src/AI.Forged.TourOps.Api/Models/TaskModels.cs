using AI.Forged.TourOps.Domain.Enums;
using System.Text.Json.Serialization;
using OperationalTaskStatus = AI.Forged.TourOps.Domain.Enums.TaskStatus;

namespace AI.Forged.TourOps.Api.Models;

public sealed class TaskRequest
{
    public Guid? BookingId { get; set; }
    public Guid? BookingItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public string AssignedToUserId { get; set; } = string.Empty;
}

public sealed class UpdateTaskStatusRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OperationalTaskStatus Status { get; set; }
}

public sealed class UpdateTaskDetailsRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
}

public sealed class AssignTaskRequest
{
    public string UserId { get; set; } = string.Empty;
}

public sealed class TaskResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OperationalTaskStatus Status { get; set; }
    public string AssignedToUserId { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public Guid? BookingId { get; set; }
    public Guid? BookingItemId { get; set; }
    public Guid? RelatedBookingId { get; set; }
    public string? ProductName { get; set; }
    public string? SupplierName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
