using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.Forged.TourOps.Infrastructure.Repositories;

public class EmailRepository(AppDbContext dbContext) : IEmailRepository
{
    public async Task<EmailThread> AddThreadAsync(EmailThread thread, CancellationToken cancellationToken = default)
    {
        dbContext.EmailThreads.Add(thread);
        await dbContext.SaveChangesAsync(cancellationToken);
        return thread;
    }

    public async Task<EmailMessage> AddMessageAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        dbContext.EmailMessages.Add(message);
        await dbContext.SaveChangesAsync(cancellationToken);
        return message;
    }

    public async Task<EmailDraft> AddDraftAsync(EmailDraft draft, CancellationToken cancellationToken = default)
    {
        dbContext.EmailDrafts.Add(draft);
        await dbContext.SaveChangesAsync(cancellationToken);
        return draft;
    }

    public async Task<EmailThread?> GetThreadByIdAsync(Guid threadId, CancellationToken cancellationToken = default) =>
        await BuildThreadQuery().FirstOrDefaultAsync(x => x.Id == threadId, cancellationToken);

    public async Task<EmailThread?> GetThreadByExternalThreadIdAsync(string externalThreadId, CancellationToken cancellationToken = default) =>
        await BuildThreadQuery().FirstOrDefaultAsync(x => x.ExternalThreadId == externalThreadId, cancellationToken);

    public async Task<IReadOnlyList<EmailThread>> GetThreadsAsync(Guid? bookingId = null, CancellationToken cancellationToken = default)
    {
        var query = BuildThreadQuery();

        if (bookingId.HasValue)
        {
            query = query.Where(x => x.BookingId == bookingId || (x.BookingItem != null && x.BookingItem.BookingId == bookingId));
        }

        return await query
            .OrderByDescending(x => x.LastMessageAt ?? x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmailThread>> GetThreadsByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default) =>
        await BuildThreadQuery()
            .Where(x => x.BookingId == bookingId || (x.BookingItem != null && x.BookingItem.BookingId == bookingId))
            .OrderByDescending(x => x.LastMessageAt ?? x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<EmailMessage?> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken = default) =>
        await dbContext.EmailMessages
            .Include(x => x.Thread)
            .FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);

    public async Task<EmailDraft?> GetDraftByIdAsync(Guid draftId, CancellationToken cancellationToken = default) =>
        await dbContext.EmailDrafts
            .Include(x => x.EmailThread)
            .FirstOrDefaultAsync(x => x.Id == draftId, cancellationToken);

    public async Task UpdateThreadAsync(EmailThread thread, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.EmailThreads.FirstOrDefaultAsync(x => x.Id == thread.Id, cancellationToken)
            ?? throw new InvalidOperationException("Email thread not found.");

        existing.Subject = thread.Subject;
        existing.SupplierEmail = thread.SupplierEmail;
        existing.LastMessageAt = thread.LastMessageAt;
        existing.ExternalThreadId = thread.ExternalThreadId;
        existing.BookingId = thread.BookingId;
        existing.BookingItemId = thread.BookingItemId;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateMessageAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.EmailMessages.FirstOrDefaultAsync(x => x.Id == message.Id, cancellationToken)
            ?? throw new InvalidOperationException("Email message not found.");

        existing.Subject = message.Subject;
        existing.BodyText = message.BodyText;
        existing.BodyHtml = message.BodyHtml;
        existing.Sender = message.Sender;
        existing.Recipients = message.Recipients;
        existing.SentAt = message.SentAt;
        existing.RequiresHumanReview = message.RequiresHumanReview;
        existing.AiSummary = message.AiSummary;
        existing.AiClassification = message.AiClassification;
        existing.AiConfidence = message.AiConfidence;
        existing.AiExtractedSignalsJson = message.AiExtractedSignalsJson;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateDraftAsync(EmailDraft draft, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.EmailDrafts.FirstOrDefaultAsync(x => x.Id == draft.Id, cancellationToken)
            ?? throw new InvalidOperationException("Email draft not found.");

        existing.Subject = draft.Subject;
        existing.Body = draft.Body;
        existing.Status = draft.Status;
        existing.GeneratedBy = draft.GeneratedBy;
        existing.ApprovedByUserId = draft.ApprovedByUserId;
        existing.ApprovedAt = draft.ApprovedAt;
        existing.SentAt = draft.SentAt;
        existing.LlmProvider = draft.LlmProvider;
        existing.LlmModel = draft.LlmModel;
        existing.AuditMetadataJson = draft.AuditMetadataJson;
        existing.BookingId = draft.BookingId;
        existing.BookingItemId = draft.BookingItemId;
        existing.EmailThreadId = draft.EmailThreadId;
        existing.UpdatedAt = draft.UpdatedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<EmailThread> BuildThreadQuery() =>
        dbContext.EmailThreads
            .Include(x => x.BookingItem)
                .ThenInclude(x => x!.Product)
            .Include(x => x.BookingItem)
                .ThenInclude(x => x!.Supplier)
            .Include(x => x.Messages.OrderByDescending(m => m.SentAt))
            .Include(x => x.Drafts.OrderByDescending(d => d.UpdatedAt))
            .AsNoTracking();
}
