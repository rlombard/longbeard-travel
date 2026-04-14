using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces.Llm;
using AI.Forged.TourOps.Application.Models.Llm;
using AI.Forged.TourOps.Domain.Enums;
using AI.Forged.TourOps.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using OperationalTaskStatus = AI.Forged.TourOps.Domain.Enums.TaskStatus;

namespace AI.Forged.TourOps.Infrastructure.Llm.Providers;

public class DeterministicLlmProviderService(IOptions<LlmSettings> settings) : ILlmProviderService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string ProviderName => "Deterministic";

    public Task<LlmProviderResponse> GenerateAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        var content = request.Operation switch
        {
            "booking-task-suggestions.generate" => BuildBookingTaskSuggestions(request),
            "email.extract-signals" or "email.classify" or "email.summarize" => BuildEmailSignalResponse(request),
            "email.draft-reply" => BuildDraftReply(request),
            "itinerary.product-assist" => BuildItineraryProductAssist(request),
            "itinerary.draft-generate" => BuildItineraryDraft(request),
            "operations.communication-summary" or "operations.booking-state" => BuildSummary(request),
            _ => JsonSerializer.Serialize(new { message = request.Prompt }, JsonOptions)
        };

        return Task.FromResult(new LlmProviderResponse
        {
            Content = content,
            Provider = ProviderName,
            Model = request.Model ?? settings.Value.DefaultModel,
            FinishReason = "stop"
        });
    }

    private static string BuildBookingTaskSuggestions(LlmRequest request)
    {
        var contextJson = request.Metadata.TryGetValue("contextJson", out var rawContext) ? rawContext : "{}";
        var context = JsonSerializer.Deserialize<BookingSuggestionContext>(contextJson, JsonOptions) ?? new BookingSuggestionContext();
        var suggestions = new List<object>();

        foreach (var item in context.Items)
        {
            if (item.Status == "Pending")
            {
                suggestions.Add(new
                {
                    title = $"Request confirmation from {item.SupplierName}",
                    description = $"Ask {item.SupplierName} to confirm {item.ProductName} and acknowledge the booking request.",
                    suggestedStatus = OperationalTaskStatus.ToDo,
                    suggestedDueDate = context.ReferenceDate.AddHours(4),
                    bookingId = context.BookingId,
                    bookingItemId = item.BookingItemId,
                    reason = "The supplier-facing booking item is still pending and should be moved into an active request loop.",
                    confidence = 0.91m,
                    requiresHumanReview = true
                });
            }

            if (item.Status == "Requested")
            {
                suggestions.Add(new
                {
                    title = $"Follow up on {item.ProductName} availability",
                    description = $"Keep the booking moving by following up with {item.SupplierName} before the next timezone delay window.",
                    suggestedStatus = OperationalTaskStatus.Waiting,
                    suggestedDueDate = context.ReferenceDate.AddHours(12),
                    bookingId = context.BookingId,
                    bookingItemId = item.BookingItemId,
                    reason = "The item has been requested but is not yet confirmed, so the operator should monitor response timing closely.",
                    confidence = 0.86m,
                    requiresHumanReview = true
                });
            }

            if (item.Status == "Confirmed" && item.Notes?.Contains("transfer", StringComparison.OrdinalIgnoreCase) == true)
            {
                suggestions.Add(new
                {
                    title = $"Reconfirm transfer timing for {item.ProductName}",
                    description = $"Double-check final pickup timing with {item.SupplierName} before arrival to avoid execution-day confusion.",
                    suggestedStatus = OperationalTaskStatus.FollowUp,
                    suggestedDueDate = context.ReferenceDate.AddDays(2),
                    bookingId = context.BookingId,
                    bookingItemId = item.BookingItemId,
                    reason = "Transfer items are sensitive to timing changes and benefit from proactive reconfirmation.",
                    confidence = 0.72m,
                    requiresHumanReview = true
                });
            }
        }

        if (context.Items.Any(x => x.Status == "Cancelled"))
        {
            suggestions.Add(new
            {
                title = "Escalate cancelled supplier item for operator review",
                description = "A supplier-side cancellation affects booking delivery and needs explicit operator intervention.",
                suggestedStatus = OperationalTaskStatus.Blocked,
                suggestedDueDate = context.ReferenceDate,
                bookingId = context.BookingId,
                bookingItemId = (Guid?)null,
                reason = "A cancelled booking item can impact the itinerary and cannot be handled autonomously.",
                confidence = 0.94m,
                requiresHumanReview = true
            });
        }

        return JsonSerializer.Serialize(suggestions, JsonOptions);
    }

    private static string BuildEmailSignalResponse(LlmRequest request)
    {
        var body = request.Metadata.TryGetValue("bodyText", out var rawBody) ? rawBody : request.Prompt;
        var normalized = body.ToLowerInvariant();

        var classification = EmailClassificationType.NoActionNeeded;
        var reason = "The message does not appear to require a commercial or operational decision.";
        var summary = body.Length > 220 ? body[..220] + "..." : body;
        var requiresHumanReview = false;
        var missingItems = new List<string>();
        var recommendedActions = new List<string>();
        var confidence = 0.62m;

        if (normalized.Contains("passport") || normalized.Contains("dietary") || normalized.Contains("please confirm") || normalized.Contains("need"))
        {
            classification = EmailClassificationType.NeedsMoreInformation;
            reason = "The supplier is asking for information that is missing from the current workflow.";
            requiresHumanReview = true;
            confidence = 0.89m;
            if (normalized.Contains("passport")) missingItems.Add("Passport names/details");
            if (normalized.Contains("dietary")) missingItems.Add("Dietary requirements");
            recommendedActions.Add("Create a task to gather and send missing guest details.");
        }
        else if (normalized.Contains("confirmed") || normalized.Contains("we can confirm"))
        {
            classification = EmailClassificationType.ConfirmationReceived;
            reason = "The supplier appears to be confirming the requested service.";
            confidence = 0.87m;
            recommendedActions.Add("Review the confirmation and update supplier booking status if appropriate.");
        }
        else if (normalized.Contains("partially confirm") || normalized.Contains("partial") || (normalized.Contains("confirm") && normalized.Contains("but")))
        {
            classification = EmailClassificationType.PartialConfirmation;
            reason = "The supplier response contains a partial confirmation or a condition that needs review.";
            requiresHumanReview = true;
            confidence = 0.78m;
            recommendedActions.Add("Review what remains outstanding before communicating back to the client.");
        }
        else if (normalized.Contains("price") || normalized.Contains("rate change") || normalized.Contains("surcharge"))
        {
            classification = EmailClassificationType.PricingChanged;
            reason = "The supplier message references a pricing change that cannot be accepted automatically.";
            requiresHumanReview = true;
            confidence = 0.92m;
            recommendedActions.Add("Create a human review task for changed pricing.");
        }
        else if (normalized.Contains("unavailable") || normalized.Contains("not available") || normalized.Contains("cannot accommodate"))
        {
            classification = EmailClassificationType.AvailabilityIssue;
            reason = "The supplier indicates an availability problem or inability to fulfil the request.";
            requiresHumanReview = true;
            confidence = 0.91m;
            recommendedActions.Add("Review alternatives before responding to the supplier or client.");
        }
        else if (normalized.Contains("pickup time") || normalized.Contains("changed to") || normalized.Contains("instead"))
        {
            classification = EmailClassificationType.HumanDecisionRequired;
            reason = "The supplier has introduced a change that should be reviewed by an operator before confirmation.";
            requiresHumanReview = true;
            confidence = 0.83m;
            recommendedActions.Add("Create a task to review the changed operational detail.");
        }
        else if (normalized.Contains("unclear") || normalized.Contains("not sure"))
        {
            classification = EmailClassificationType.Unclear;
            reason = "The message lacks enough clarity to determine the correct next action safely.";
            requiresHumanReview = true;
            confidence = 0.55m;
            recommendedActions.Add("Ask a clarifying question and review manually.");
        }

        return JsonSerializer.Serialize(new
        {
            classification,
            summary,
            reason,
            confidence,
            requiresHumanReview,
            hasConfirmation = classification == EmailClassificationType.ConfirmationReceived,
            hasPricingChange = classification == EmailClassificationType.PricingChanged,
            hasAvailabilityIssue = classification == EmailClassificationType.AvailabilityIssue,
            requestsMoreInformation = classification == EmailClassificationType.NeedsMoreInformation,
            missingInformationItems = missingItems,
            recommendedActions
        }, JsonOptions);
    }

    private static string BuildDraftReply(LlmRequest request)
    {
        var supplierName = request.Metadata.TryGetValue("supplierName", out var rawSupplierName) ? rawSupplierName : "the supplier";
        var classification = request.Metadata.TryGetValue("classification", out var rawClassification) ? rawClassification : "Unclear";
        var subject = request.Metadata.TryGetValue("subject", out var rawSubject) && !string.IsNullOrWhiteSpace(rawSubject)
            ? $"Re: {rawSubject}"
            : "Operational follow-up";

        var body = classification switch
        {
            "NeedsMoreInformation" => $"Hello {supplierName},\n\nThank you for your note. We are reviewing the requested guest details and will come back to you shortly with the missing information.\n\nKind regards,\nTour Ops",
            "ConfirmationReceived" => $"Hello {supplierName},\n\nThank you for the confirmation. We are reviewing the details internally and will confirm the next operational step shortly.\n\nKind regards,\nTour Ops",
            "PricingChanged" => $"Hello {supplierName},\n\nThank you for the update. We need to review the pricing change internally before we can confirm how to proceed. We will revert as soon as possible.\n\nKind regards,\nTour Ops",
            _ => $"Hello {supplierName},\n\nThank you for your message. We are reviewing the details and will respond shortly.\n\nKind regards,\nTour Ops"
        };

        return JsonSerializer.Serialize(new { subject, body }, JsonOptions);
    }

    private static string BuildSummary(LlmRequest request)
    {
        var prompt = request.Prompt.Length > 280 ? request.Prompt[..280] + "..." : request.Prompt;
        return JsonSerializer.Serialize(new { message = prompt }, JsonOptions);
    }

    private static string BuildItineraryProductAssist(LlmRequest request)
    {
        var contextJson = request.Metadata.TryGetValue("contextJson", out var rawContext) ? rawContext : "{}";
        var context = JsonSerializer.Deserialize<ItineraryPlanningContext>(contextJson, JsonOptions) ?? new ItineraryPlanningContext();

        var ranked = context.Candidates
            .OrderByDescending(x => x.DeterministicScore)
            .ThenBy(x => x.ProductName, StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .Select(candidate => new
            {
                productId = candidate.ProductId,
                matchScore = Math.Min(1m, candidate.DeterministicScore + 0.05m),
                reason = candidate.Reasons.FirstOrDefault()
                    ?? $"Catalog metadata suggests {candidate.ProductName} could fit the request.",
                warnings = candidate.Warnings,
                assumptionFlags = candidate.DeterministicScore < 0.55m
                    ? new[] { "Confidence is limited by available metadata." }
                    : Array.Empty<string>(),
                missingData = candidate.MissingData
            });

        return JsonSerializer.Serialize(ranked, JsonOptions);
    }

    private static string BuildItineraryDraft(LlmRequest request)
    {
        var contextJson = request.Metadata.TryGetValue("contextJson", out var rawContext) ? rawContext : "{}";
        var context = JsonSerializer.Deserialize<ItineraryPlanningContext>(contextJson, JsonOptions) ?? new ItineraryPlanningContext();

        var hotel = context.Candidates
            .Where(x => string.Equals(x.ProductType, ProductType.Hotel.ToString(), StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.DeterministicScore)
            .FirstOrDefault();
        var tour = context.Candidates
            .Where(x => string.Equals(x.ProductType, ProductType.Tour.ToString(), StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.DeterministicScore)
            .FirstOrDefault();
        var transport = context.Candidates
            .Where(x => string.Equals(x.ProductType, ProductType.Transport.ToString(), StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.DeterministicScore)
            .FirstOrDefault();

        var items = new List<object>();
        for (var day = 1; day <= Math.Max(1, context.Duration); day++)
        {
            var sequence = 1;

            if (day == 1)
            {
                items.Add(BuildDraftItem(day, sequence++, "Arrival transfer", transport));
            }

            items.Add(BuildDraftItem(day, sequence++, day == 1 ? "Accommodation check-in" : "Accommodation stay", hotel));

            if (day < Math.Max(1, context.Duration))
            {
                items.Add(BuildDraftItem(day, sequence++, $"Suggested activity for day {day}", tour));
            }

            if (day == Math.Max(1, context.Duration))
            {
                items.Add(BuildDraftItem(day, sequence, "Departure transfer", transport));
            }
        }

        var dataGaps = new List<string>();
        if (hotel is null) dataGaps.Add("Hotel product match missing.");
        if (tour is null) dataGaps.Add("Tour product match missing.");
        if (transport is null) dataGaps.Add("Transport product match missing.");

        return JsonSerializer.Serialize(new
        {
            assumptions = new[]
            {
                "Deterministic provider built a simple skeleton itinerary from the top catalog matches."
            },
            caveats = new[]
            {
                "This draft should be reviewed by an operator before it is saved."
            },
            dataGaps,
            items
        }, JsonOptions);
    }

    private static object BuildDraftItem(int day, int sequence, string title, ItineraryCandidate? candidate)
    {
        if (candidate is null)
        {
            return new
            {
                dayNumber = day,
                sequence,
                title,
                productId = (Guid?)null,
                quantity = 1,
                notes = "No catalog product was matched automatically.",
                confidence = 0.35m,
                reason = "Operator review is required because no suitable product was found in the shortlist.",
                warnings = new[] { "Product mapping unresolved." },
                missingData = new[] { "Matching product selection." }
            };
        }

        return new
        {
            dayNumber = day,
            sequence,
            title,
            productId = candidate.ProductId,
            quantity = 1,
            notes = (string?)null,
            confidence = Math.Min(1m, candidate.DeterministicScore + 0.05m),
            reason = candidate.Reasons.FirstOrDefault()
                ?? $"Catalog metadata suggests {candidate.ProductName} is a reasonable fit.",
            warnings = candidate.Warnings,
            missingData = candidate.MissingData
        };
    }

    private sealed class BookingSuggestionContext
    {
        public Guid BookingId { get; set; }
        public DateTime ReferenceDate { get; set; } = DateTime.UtcNow;
        public List<BookingSuggestionItem> Items { get; set; } = [];
    }

    private sealed class BookingSuggestionItem
    {
        public Guid BookingItemId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    private sealed class ItineraryPlanningContext
    {
        public int Duration { get; set; }
        public List<ItineraryCandidate> Candidates { get; set; } = [];
    }

    private sealed class ItineraryCandidate
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public decimal DeterministicScore { get; set; }
        public List<string> Reasons { get; set; } = [];
        public List<string> Warnings { get; set; } = [];
        public List<string> MissingData { get; set; } = [];
    }
}
