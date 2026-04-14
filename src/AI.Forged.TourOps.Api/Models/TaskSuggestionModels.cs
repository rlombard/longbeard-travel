using System.Text.Json.Serialization;
using AI.Forged.TourOps.Domain.Enums;
using OperationalTaskStatus = AI.Forged.TourOps.Domain.Enums.TaskStatus;

namespace AI.Forged.TourOps.Api.Models;

public sealed class AcceptTaskSuggestionRequest
{
    public string AssignedToUserId { get; set; } = string.Empty;
}

public sealed class TaskSuggestionResponse
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid? BookingItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OperationalTaskStatus SuggestedStatus { get; set; }
    public DateTime? SuggestedDueDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public bool RequiresHumanReview { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaskSuggestionState State { get; set; }
    public string? Source { get; set; }
    public Guid? AcceptedTaskId { get; set; }
    public string? ReviewedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ProductName { get; set; }
    public string? SupplierName { get; set; }
}
