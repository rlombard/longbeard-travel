using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Operations;
using AI.Forged.TourOps.Application.Models.Operations;
using AI.Forged.TourOps.Domain.Enums;
using OperationalTaskStatus = AI.Forged.TourOps.Domain.Enums.TaskStatus;

namespace AI.Forged.TourOps.Application.Services.Operations;

public class OperationalDecisionSupportService(IBookingRepository bookingRepository, ITaskRepository taskRepository, IEmailRepository emailRepository) : IOperationalDecisionSupportService
{
    public async Task<BookingStateAnalysis> AnalyzeBookingStateAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found.");
        var tasks = await taskRepository.GetByBookingAsync(bookingId, cancellationToken);

        var risks = new List<string>();
        if (booking.Items.Any(x => x.Status == BookingItemStatus.Pending)) risks.Add("Supplier items still pending request or confirmation.");
        if (tasks.Any(x => x.Status == OperationalTaskStatus.Blocked)) risks.Add("There are blocked operational tasks requiring human intervention.");

        return new BookingStateAnalysis
        {
            Summary = $"Booking {booking.Id} has {booking.Items.Count} supplier items and {tasks.Count} linked operational tasks.",
            Risks = risks.ToArray(),
            RecommendedActions = risks.Count == 0 ? ["Continue monitoring the booking."] : ["Review blocked/pending items before the next supplier timezone window closes."],
            Confidence = risks.Count == 0 ? 0.7m : 0.86m,
            RequiresHumanReview = risks.Count > 0
        };
    }

    public async Task<BookingStateAnalysis> AnalyzeCommunicationStateAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var threads = await emailRepository.GetThreadsByBookingAsync(bookingId, cancellationToken);
        var latestInbound = threads.SelectMany(x => x.Messages).Where(x => x.Direction == EmailDirection.Inbound).OrderByDescending(x => x.SentAt).FirstOrDefault();

        return new BookingStateAnalysis
        {
            Summary = latestInbound is null
                ? "No supplier email traffic has been recorded for this booking yet."
                : $"Latest supplier communication: {latestInbound.Subject}",
            Risks = latestInbound?.RequiresHumanReview == true ? ["Latest inbound supplier email still requires human review."] : [],
            RecommendedActions = latestInbound?.RequiresHumanReview == true ? ["Review the latest supplier email and decide the next action."] : ["No communication escalation is currently indicated."],
            Confidence = latestInbound is null ? 0.55m : 0.8m,
            RequiresHumanReview = latestInbound?.RequiresHumanReview == true
        };
    }

    public async Task<IReadOnlyList<string>> RecommendNextActionsAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var bookingAnalysis = await AnalyzeBookingStateAsync(bookingId, cancellationToken);
        var communicationAnalysis = await AnalyzeCommunicationStateAsync(bookingId, cancellationToken);

        return bookingAnalysis.RecommendedActions
            .Concat(communicationAnalysis.RecommendedActions)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
