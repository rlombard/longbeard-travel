using System.Globalization;
using AI.Forged.TourOps.Application.Interfaces.Ai;
using AI.Forged.TourOps.Application.Models.Ai;
using AI.Forged.TourOps.Application.Models.Operations;
using AI.Forged.TourOps.Domain.Enums;
using AI.Forged.TourOps.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using OperationalTaskStatus = AI.Forged.TourOps.Domain.Enums.TaskStatus;

namespace AI.Forged.TourOps.Infrastructure.AiForged;

public class AiForgedStubService(IOptions<AiForgedSettings> settings) : IAiForgedService
{
    private const string ProviderName = "AiForgedStub";
    private readonly AiForgedSettings settings = settings.Value;

    public Task<AiForgedProcessingResult> ProcessEmailPdfAsync(AiForgedDocument file, CancellationToken cancellationToken = default)
    {
        var text = file.TextContent;
        var bookingId = ParseGuid(file.Metadata, "bookingId");
        var bookingItemId = ParseGuid(file.Metadata, "bookingItemId");
        var supplierName = file.Metadata.TryGetValue("supplierName", out var rawSupplier) ? rawSupplier : "Supplier";
        var referenceDate = ParseDateTime(file.Metadata, "referenceDate") ?? DateTime.UtcNow;

        var classification = EmailClassificationType.NoActionNeeded;
        var summary = "No urgent supplier-side action was detected in the email thread.";
        var reason = "The thread does not currently show a clear blocker, missing detail request, or confirmation change.";
        var confidence = 0.62m;
        var requiresHumanReview = false;
        var missingInformationItems = new List<string>();
        var recommendedActions = new List<string>();
        var tasks = new List<SuggestedTaskCandidate>();
        AiForgedSuggestedReply? reply = null;

        var normalized = text.ToLowerInvariant();

        if (normalized.Contains("passport") || normalized.Contains("dietary") || normalized.Contains("guest details") || normalized.Contains("please confirm") || normalized.Contains("need"))
        {
            classification = EmailClassificationType.NeedsMoreInformation;
            summary = "Supplier is asking for missing guest information before final confirmation can proceed.";
            reason = "The thread contains a direct request for missing operational details, which risks another timezone delay cycle if not answered promptly.";
            confidence = 0.91m;
            requiresHumanReview = true;
            if (normalized.Contains("passport")) missingInformationItems.Add("Passport names/details");
            if (normalized.Contains("dietary")) missingInformationItems.Add("Dietary requirements");
            if (missingInformationItems.Count == 0) missingInformationItems.Add("Requested guest details");
            recommendedActions.Add("Review the request and provide the missing guest information.");
            tasks.Add(new SuggestedTaskCandidate
            {
                Title = "Provide supplier with missing booking details",
                Description = $"Send the missing guest information requested by {supplierName}: {string.Join(", ", missingInformationItems)}.",
                SuggestedStatus = OperationalTaskStatus.ToDo,
                SuggestedDueDate = referenceDate.AddHours(4),
                BookingId = bookingId,
                BookingItemId = bookingItemId,
                Reason = reason,
                Confidence = confidence,
                RequiresHumanReview = true
            });
            reply = new AiForgedSuggestedReply
            {
                Subject = BuildReplySubject(file.Metadata, "Booking Request"),
                Body = $"Hello {supplierName},\n\nThank you for your message. We are reviewing the requested guest details and will send the missing information shortly.\n\nKind regards,\nTour Ops"
            };
        }
        else if (normalized.Contains("confirmed") || normalized.Contains("we can confirm"))
        {
            classification = EmailClassificationType.ConfirmationReceived;
            summary = "Supplier appears to have confirmed the requested service.";
            reason = "Confirmation language was detected in the conversation and should be reviewed by an operator before any onward commitment.";
            confidence = 0.87m;
            recommendedActions.Add("Review the confirmation and decide whether any additional follow-up is still required.");
            reply = new AiForgedSuggestedReply
            {
                Subject = BuildReplySubject(file.Metadata, "Booking Request"),
                Body = $"Hello {supplierName},\n\nThank you for the confirmation. We are reviewing the details internally and will revert with any follow-up if needed.\n\nKind regards,\nTour Ops"
            };
        }
        else if (normalized.Contains("price") || normalized.Contains("rate change") || normalized.Contains("surcharge"))
        {
            classification = EmailClassificationType.PricingChanged;
            summary = "Supplier has referenced a pricing change that needs operator review.";
            reason = "Pricing or surcharge language was detected and cannot be accepted automatically.";
            confidence = 0.93m;
            requiresHumanReview = true;
            recommendedActions.Add("Review the pricing change before responding or updating the booking.");
            tasks.Add(new SuggestedTaskCandidate
            {
                Title = "Review supplier pricing change",
                Description = "A supplier pricing adjustment was detected in the thread and needs human approval before any response is sent.",
                SuggestedStatus = OperationalTaskStatus.Blocked,
                SuggestedDueDate = referenceDate,
                BookingId = bookingId,
                BookingItemId = bookingItemId,
                Reason = reason,
                Confidence = confidence,
                RequiresHumanReview = true
            });
            reply = new AiForgedSuggestedReply
            {
                Subject = BuildReplySubject(file.Metadata, "Booking Request"),
                Body = $"Hello {supplierName},\n\nThank you for the update. We need to review the pricing change internally before we confirm the next step. We will come back to you as soon as possible.\n\nKind regards,\nTour Ops"
            };
        }
        else if (normalized.Contains("pickup time") || normalized.Contains("changed to") || normalized.Contains("instead") || normalized.Contains("unavailable") || normalized.Contains("not available"))
        {
            classification = normalized.Contains("unavailable") || normalized.Contains("not available")
                ? EmailClassificationType.AvailabilityIssue
                : EmailClassificationType.HumanDecisionRequired;
            summary = classification == EmailClassificationType.AvailabilityIssue
                ? "Supplier has indicated an availability issue that needs operator review."
                : "Supplier has changed an operational detail that should be reviewed before confirming back.";
            reason = "The thread contains a change or limitation that impacts execution and requires human judgment.";
            confidence = 0.86m;
            requiresHumanReview = true;
            recommendedActions.Add("Review the changed operational detail before responding.");
            tasks.Add(new SuggestedTaskCandidate
            {
                Title = "Review supplier change before responding",
                Description = summary,
                SuggestedStatus = OperationalTaskStatus.Blocked,
                SuggestedDueDate = referenceDate,
                BookingId = bookingId,
                BookingItemId = bookingItemId,
                Reason = reason,
                Confidence = confidence,
                RequiresHumanReview = true
            });
            reply = new AiForgedSuggestedReply
            {
                Subject = BuildReplySubject(file.Metadata, "Booking Request"),
                Body = $"Hello {supplierName},\n\nThank you for the update. We are reviewing the changed operational detail internally and will revert shortly.\n\nKind regards,\nTour Ops"
            };
        }

        return Task.FromResult(new AiForgedProcessingResult
        {
            Tasks = tasks,
            EmailClassification = classification,
            Summary = summary,
            Reason = reason,
            Confidence = confidence,
            RequiresHumanReview = requiresHumanReview,
            MissingInformationItems = missingInformationItems,
            RecommendedActions = recommendedActions,
            SuggestedReply = reply,
            Provider = ProviderName,
            ProcessingMode = $"PdfIngestion:{settings.BaseUrl}"
        });
    }

    public Task<AiForgedProcessingResult> ProcessBookingContextAsync(AiForgedDocument file, CancellationToken cancellationToken = default)
    {
        var bookingId = ParseGuid(file.Metadata, "bookingId");
        var referenceDate = ParseDateTime(file.Metadata, "referenceDate") ?? DateTime.UtcNow;
        var items = new List<SuggestedTaskCandidate>();
        var text = file.TextContent;

        foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!line.StartsWith("ITEM|", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var parts = line.Split('|');
            if (parts.Length < 6)
            {
                continue;
            }

            var bookingItemId = Guid.TryParse(parts[1], out var parsedItemId) ? parsedItemId : (Guid?)null;
            var productName = parts[2];
            var supplierName = parts[3];
            var status = parts[4];
            var notes = parts[5];

            if (status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            {
                items.Add(new SuggestedTaskCandidate
                {
                    Title = $"Request confirmation from {supplierName}",
                    Description = $"Move {productName} into an active supplier request loop before the next timezone delay window closes.",
                    SuggestedStatus = OperationalTaskStatus.ToDo,
                    SuggestedDueDate = referenceDate.AddHours(4),
                    BookingId = bookingId,
                    BookingItemId = bookingItemId,
                    Reason = "The booking item is still pending and needs an active supplier request.",
                    Confidence = 0.9m,
                    RequiresHumanReview = true
                });
            }
            else if (status.Equals("Requested", StringComparison.OrdinalIgnoreCase))
            {
                items.Add(new SuggestedTaskCandidate
                {
                    Title = $"Follow up on {productName} availability",
                    Description = $"Check in with {supplierName} before another supplier response cycle is lost.",
                    SuggestedStatus = OperationalTaskStatus.Waiting,
                    SuggestedDueDate = referenceDate.AddHours(12),
                    BookingId = bookingId,
                    BookingItemId = bookingItemId,
                    Reason = "The supplier-facing item is requested but not yet confirmed.",
                    Confidence = 0.86m,
                    RequiresHumanReview = true
                });
            }
            else if (status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase) && notes.Contains("transfer", StringComparison.OrdinalIgnoreCase))
            {
                items.Add(new SuggestedTaskCandidate
                {
                    Title = $"Reconfirm transfer timing for {productName}",
                    Description = $"Double-check transfer timing with {supplierName} before arrival.",
                    SuggestedStatus = OperationalTaskStatus.FollowUp,
                    SuggestedDueDate = referenceDate.AddDays(2),
                    BookingId = bookingId,
                    BookingItemId = bookingItemId,
                    Reason = "Confirmed transfer services benefit from proactive reconfirmation.",
                    Confidence = 0.74m,
                    RequiresHumanReview = true
                });
            }
            else if (status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                items.Add(new SuggestedTaskCandidate
                {
                    Title = "Escalate cancelled supplier item",
                    Description = $"A cancelled supplier booking item for {productName} needs operator review.",
                    SuggestedStatus = OperationalTaskStatus.Blocked,
                    SuggestedDueDate = referenceDate,
                    BookingId = bookingId,
                    BookingItemId = bookingItemId,
                    Reason = "Cancelled supplier services cannot be resolved automatically.",
                    Confidence = 0.95m,
                    RequiresHumanReview = true
                });
            }
        }

        return Task.FromResult(new AiForgedProcessingResult
        {
            Tasks = items,
            EmailClassification = EmailClassificationType.NoActionNeeded,
            Summary = "Booking context processed through AI Forged booking analysis stub.",
            Reason = items.Count == 0
                ? "No obvious operational follow-up was detected in the current booking context."
                : "Booking item status and notes indicate operational follow-up opportunities.",
            Confidence = items.Count == 0 ? 0.55m : 0.84m,
            RequiresHumanReview = true,
            Provider = ProviderName,
            ProcessingMode = $"PdfIngestion:{settings.BaseUrl}"
        });
    }

    public Task<AiForgedProcessingResult> ExtractTasksAsync(AiForgedDocument file, CancellationToken cancellationToken = default) =>
        ProcessEmailPdfAsync(file, cancellationToken);

    private static Guid ParseGuid(IReadOnlyDictionary<string, string> metadata, string key) =>
        metadata.TryGetValue(key, out var raw) && Guid.TryParse(raw, out var parsed)
            ? parsed
            : Guid.Empty;

    private static DateTime? ParseDateTime(IReadOnlyDictionary<string, string> metadata, string key) =>
        metadata.TryGetValue(key, out var raw) && DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var parsed)
            ? parsed
            : null;

    private static string BuildReplySubject(IReadOnlyDictionary<string, string> metadata, string fallback)
    {
        if (metadata.TryGetValue("latestSubject", out var latestSubject) && !string.IsNullOrWhiteSpace(latestSubject))
        {
            return $"Re: {latestSubject}";
        }

        return $"Re: {fallback}";
    }
}
