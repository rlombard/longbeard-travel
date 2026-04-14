using System.Text;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Operations;
using AI.Forged.TourOps.Application.Models.Email;

namespace AI.Forged.TourOps.Application.Services.Operations;

public class CommunicationContextService(IBookingRepository bookingRepository, IEmailRepository emailRepository) : ICommunicationContextService
{
    public async Task<EmailThreadContext> BuildBookingCommunicationContextAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found.");
        var threads = await emailRepository.GetThreadsByBookingAsync(bookingId, cancellationToken);

        return new EmailThreadContext
        {
            BookingId = booking.Id,
            BookingSummary = $"Booking {booking.Id} is {booking.Status} with {booking.Items.Count} supplier booking items.",
            SupplierEmail = threads.FirstOrDefault()?.SupplierEmail ?? string.Empty,
            Subject = threads.FirstOrDefault()?.Subject ?? string.Empty,
            MessageTimeline = threads
                .SelectMany(x => x.Messages)
                .OrderByDescending(x => x.SentAt)
                .Take(8)
                .Select(x => $"[{x.Direction}] {x.Subject}: {x.BodyText[..Math.Min(x.BodyText.Length, 180)]}")
                .ToList()
        };
    }

    public async Task<string> BuildSupplierConversationSummaryAsync(Guid emailThreadId, CancellationToken cancellationToken = default)
    {
        var thread = await emailRepository.GetThreadByIdAsync(emailThreadId, cancellationToken)
            ?? throw new InvalidOperationException("Email thread not found.");

        var builder = new StringBuilder();
        builder.AppendLine($"Thread subject: {thread.Subject}");
        builder.AppendLine($"Supplier email: {thread.SupplierEmail}");

        foreach (var message in thread.Messages.OrderBy(x => x.SentAt).TakeLast(10))
        {
            builder.AppendLine($"- {message.Direction} {message.SentAt:u}: {message.Subject}");
            builder.AppendLine(message.AiSummary ?? message.BodyText[..Math.Min(message.BodyText.Length, 220)]);
        }

        return builder.ToString().Trim();
    }
}
