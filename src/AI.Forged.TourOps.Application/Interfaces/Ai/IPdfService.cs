using AI.Forged.TourOps.Application.Models.Ai;
using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces.Ai;

public interface IPdfService
{
    Task<AiForgedDocument> GenerateEmailThreadPdfAsync(EmailThread thread, CancellationToken cancellationToken = default);
    Task<AiForgedDocument> GenerateBookingContextPdfAsync(Booking booking, IReadOnlyList<OperationalTask> existingTasks, CancellationToken cancellationToken = default);
}
