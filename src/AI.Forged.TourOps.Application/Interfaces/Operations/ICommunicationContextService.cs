using AI.Forged.TourOps.Application.Models.Email;

namespace AI.Forged.TourOps.Application.Interfaces.Operations;

public interface ICommunicationContextService
{
    Task<EmailThreadContext> BuildBookingCommunicationContextAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<string> BuildSupplierConversationSummaryAsync(Guid emailThreadId, CancellationToken cancellationToken = default);
}
