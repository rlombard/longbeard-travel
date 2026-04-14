using System.Text;
using AI.Forged.TourOps.Application.Interfaces.Ai;
using AI.Forged.TourOps.Application.Models.Ai;
using AI.Forged.TourOps.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AI.Forged.TourOps.Infrastructure.Pdf;

public class QuestPdfService : IPdfService
{
    static QuestPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<AiForgedDocument> GenerateEmailThreadPdfAsync(EmailThread thread, CancellationToken cancellationToken = default)
    {
        var lines = new List<string>
        {
            "AI Forged Tour Ops - Email Thread",
            $"ThreadId: {thread.Id}",
            $"BookingId: {thread.BookingId}",
            $"BookingItemId: {thread.BookingItemId}",
            $"Subject: {thread.Subject}",
            $"SupplierEmail: {thread.SupplierEmail}",
            string.Empty,
            "Conversation"
        };

        foreach (var message in thread.Messages.OrderBy(x => x.SentAt))
        {
            lines.Add($"[{message.Direction}] {message.SentAt:u} | {message.Subject}");
            lines.Add($"From: {message.Sender}");
            lines.Add($"To: {message.Recipients}");
            lines.Add(message.BodyText);
            lines.Add(string.Empty);
        }

        var text = string.Join("\n", lines);
        var metadata = new Dictionary<string, string>
        {
            ["threadId"] = thread.Id.ToString(),
            ["bookingId"] = (thread.BookingId ?? thread.BookingItem?.BookingId ?? Guid.Empty).ToString(),
            ["bookingItemId"] = thread.BookingItemId?.ToString() ?? string.Empty,
            ["supplierName"] = thread.BookingItem?.Supplier?.Name ?? thread.SupplierEmail,
            ["latestSubject"] = thread.Messages.OrderByDescending(x => x.SentAt).Select(x => x.Subject).FirstOrDefault() ?? thread.Subject,
            ["referenceDate"] = DateTime.UtcNow.ToString("O")
        };

        return Task.FromResult(new AiForgedDocument
        {
            FileName = $"email-thread-{thread.Id}.pdf",
            ContentType = "application/pdf",
            Content = RenderPdf("Email Thread", lines),
            TextContent = text,
            Metadata = metadata
        });
    }

    public Task<AiForgedDocument> GenerateBookingContextPdfAsync(Booking booking, IReadOnlyList<OperationalTask> existingTasks, CancellationToken cancellationToken = default)
    {
        var lines = new List<string>
        {
            "AI Forged Tour Ops - Booking Context",
            $"BookingId: {booking.Id}",
            $"QuoteId: {booking.QuoteId}",
            $"Status: {booking.Status}",
            $"CreatedAt: {booking.CreatedAt:u}",
            string.Empty,
            "Booking Items"
        };

        foreach (var item in booking.Items)
        {
            lines.Add($"ITEM|{item.Id}|{item.Product?.Name ?? item.ProductId.ToString()}|{item.Supplier?.Name ?? item.SupplierId.ToString()}|{item.Status}|{item.Notes ?? string.Empty}");
        }

        lines.Add(string.Empty);
        lines.Add("Existing Tasks");

        foreach (var task in existingTasks.OrderBy(x => x.CreatedAt))
        {
            lines.Add($"TASK|{task.Title}|{task.Status}|{task.BookingItemId}|{task.DueDate:O}|{task.Description ?? string.Empty}");
        }

        var text = string.Join("\n", lines);
        return Task.FromResult(new AiForgedDocument
        {
            FileName = $"booking-context-{booking.Id}.pdf",
            ContentType = "application/pdf",
            Content = RenderPdf("Booking Context", lines),
            TextContent = text,
            Metadata = new Dictionary<string, string>
            {
                ["bookingId"] = booking.Id.ToString(),
                ["referenceDate"] = DateTime.UtcNow.ToString("O")
            }
        });
    }

    private static byte[] RenderPdf(string title, IReadOnlyList<string> lines)
    {
        using var stream = new MemoryStream();

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(32);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Column(column =>
                {
                    column.Spacing(6);
                    column.Item().Text(title).FontSize(18).Bold();
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            column.Item().Text(" ");
                        }
                        else
                        {
                            column.Item().Text(line);
                        }
                    }
                });
            });
        }).GeneratePdf(stream);

        return stream.ToArray();
    }
}
