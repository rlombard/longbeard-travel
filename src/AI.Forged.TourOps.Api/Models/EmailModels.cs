using System.Text.Json.Serialization;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Api.Models;

public sealed class CreateEmailThreadRequest
{
    public Guid? BookingItemId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string SupplierEmail { get; set; } = string.Empty;
    public string? ExternalThreadId { get; set; }
}

public sealed class AddEmailMessageRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EmailDirection Direction { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyText { get; set; } = string.Empty;
    public string? BodyHtml { get; set; }
    public string Sender { get; set; } = string.Empty;
    public string Recipients { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}

public sealed class CreateEmailDraftRequest
{
    public Guid? BookingId { get; set; }
    public Guid? BookingItemId { get; set; }
    public Guid? EmailThreadId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public sealed class UpdateEmailDraftRequest
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public sealed class EmailThreadAiAnalysisResponse
{
    public Guid EmailThreadId { get; set; }
    public string Summary { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EmailClassificationType Classification { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public bool RequiresHumanReview { get; set; }
    public string[] RecommendedActions { get; set; } = [];
    public string[] MissingInformationItems { get; set; } = [];
}

public sealed class EmailThreadResponse
{
    public Guid Id { get; set; }
    public Guid? BookingId { get; set; }
    public Guid? BookingItemId { get; set; }
    public Guid? RelatedBookingId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string SupplierEmail { get; set; } = string.Empty;
    public DateTime? LastMessageAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<EmailMessageResponse> Messages { get; set; } = [];
    public List<EmailDraftResponse> Drafts { get; set; } = [];
}

public sealed class EmailMessageResponse
{
    public Guid Id { get; set; }
    public Guid EmailThreadId { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EmailDirection Direction { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyText { get; set; } = string.Empty;
    public string? BodyHtml { get; set; }
    public string Sender { get; set; } = string.Empty;
    public string Recipients { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool RequiresHumanReview { get; set; }
    public string? AiSummary { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EmailClassificationType? AiClassification { get; set; }
    public decimal? AiConfidence { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class EmailDraftResponse
{
    public Guid Id { get; set; }
    public Guid? BookingId { get; set; }
    public Guid? BookingItemId { get; set; }
    public Guid? EmailThreadId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EmailDraftStatus Status { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EmailDraftGeneratedBy GeneratedBy { get; set; }
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public bool GeneratedByAi { get; set; }
    public string? LlmProvider { get; set; }
    public string? LlmModel { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
