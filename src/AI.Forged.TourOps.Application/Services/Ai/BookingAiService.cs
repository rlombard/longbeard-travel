using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Ai;
using AI.Forged.TourOps.Application.Models.Operations;

namespace AI.Forged.TourOps.Application.Services.Ai;

public class BookingAiService(
    IBookingRepository bookingRepository,
    ITaskRepository taskRepository,
    IPdfService pdfService,
    IAiForgedService aiForgedService) : IBookingAiService
{
    public async Task<IReadOnlyList<SuggestedTaskCandidate>> GenerateSuggestedTasksAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found.");
        var existingTasks = await taskRepository.GetByBookingAsync(bookingId, cancellationToken);
        var pdf = await pdfService.GenerateBookingContextPdfAsync(booking, existingTasks, cancellationToken);
        var result = await aiForgedService.ProcessBookingContextAsync(pdf, cancellationToken);

        return result.Tasks
            .Select(x => new SuggestedTaskCandidate
            {
                Title = x.Title,
                Description = x.Description,
                SuggestedStatus = x.SuggestedStatus,
                SuggestedDueDate = x.SuggestedDueDate,
                BookingId = x.BookingId == Guid.Empty ? bookingId : x.BookingId,
                BookingItemId = x.BookingItemId,
                Reason = x.Reason,
                Confidence = x.Confidence,
                RequiresHumanReview = true
            })
            .ToList();
    }
}
