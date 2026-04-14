using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Email;
using AI.Forged.TourOps.Application.Interfaces.Operations;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Services.Email;

public class EmailService(
    IEmailRepository emailRepository,
    IBookingRepository bookingRepository,
    IBookingItemRepository bookingItemRepository,
    ICurrentUserContext currentUserContext,
    IEmailProviderService emailProviderService,
    IEmailAnalysisService emailAnalysisService,
    IHumanApprovalService humanApprovalService) : IEmailService
{
    public async Task<EmailThread> CreateThreadAsync(Guid bookingId, Guid? bookingItemId, string subject, string supplierEmail, string? externalThreadId, CancellationToken cancellationToken = default)
    {
        await ValidateContextAsync(bookingId, bookingItemId, cancellationToken);

        var thread = new EmailThread
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            BookingItemId = bookingItemId,
            Subject = NormalizeRequired(subject, "Email thread subject is required.", 512),
            SupplierEmail = NormalizeRequired(supplierEmail, "Supplier email is required.", 256),
            ExternalThreadId = NormalizeOptional(externalThreadId, 256),
            CreatedAt = DateTime.UtcNow,
            LastMessageAt = null
        };

        await emailRepository.AddThreadAsync(thread, cancellationToken);
        return await emailRepository.GetThreadByIdAsync(thread.Id, cancellationToken)
            ?? throw new InvalidOperationException("Email thread not found after creation.");
    }

    public async Task<EmailMessage> AddMessageAsync(Guid emailThreadId, EmailDirection direction, string subject, string bodyText, string? bodyHtml, string sender, string recipients, DateTime sentAt, CancellationToken cancellationToken = default)
    {
        var thread = await emailRepository.GetThreadByIdAsync(emailThreadId, cancellationToken)
            ?? throw new InvalidOperationException("Email thread not found.");

        var message = new EmailMessage
        {
            Id = Guid.NewGuid(),
            EmailThreadId = emailThreadId,
            Direction = direction,
            Subject = NormalizeRequired(subject, "Email subject is required.", 512),
            BodyText = NormalizeRequired(bodyText, "Email body text is required.", 16000),
            BodyHtml = NormalizeOptional(bodyHtml, 32000),
            Sender = NormalizeRequired(sender, "Sender is required.", 256),
            Recipients = NormalizeRequired(recipients, "Recipients are required.", 2000),
            SentAt = sentAt.ToUniversalTime(),
            RequiresHumanReview = direction == EmailDirection.Inbound,
            CreatedAt = DateTime.UtcNow
        };

        await emailRepository.AddMessageAsync(message, cancellationToken);
        thread.LastMessageAt = message.SentAt;
        await emailRepository.UpdateThreadAsync(thread, cancellationToken);

        if (direction == EmailDirection.Inbound)
        {
            await emailAnalysisService.AnalyzeThreadAsync(thread.Id, cancellationToken);
            await emailAnalysisService.SuggestTasksFromEmailAsync(thread.Id, cancellationToken);
        }

        return await emailRepository.GetMessageByIdAsync(message.Id, cancellationToken)
            ?? throw new InvalidOperationException("Email message not found after creation.");
    }

    public async Task<EmailDraft> CreateDraftAsync(Guid? bookingId, Guid? bookingItemId, Guid? emailThreadId, string subject, string body, EmailDraftGeneratedBy generatedBy, string? llmProvider, string? llmModel, string? auditMetadataJson, CancellationToken cancellationToken = default)
    {
        if (!bookingId.HasValue && !bookingItemId.HasValue && !emailThreadId.HasValue)
        {
            throw new InvalidOperationException("Drafts must be linked to a booking, booking item, or email thread.");
        }

        var now = DateTime.UtcNow;
        var draft = new EmailDraft
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            BookingItemId = bookingItemId,
            EmailThreadId = emailThreadId,
            Subject = NormalizeRequired(subject, "Draft subject is required.", 512),
            Body = NormalizeRequired(body, "Draft body is required.", 16000),
            Status = EmailDraftStatus.Draft,
            GeneratedBy = generatedBy,
            LlmProvider = NormalizeOptional(llmProvider, 128),
            LlmModel = NormalizeOptional(llmModel, 128),
            AuditMetadataJson = NormalizeOptional(auditMetadataJson, 8000),
            CreatedAt = now,
            UpdatedAt = now
        };

        await emailRepository.AddDraftAsync(draft, cancellationToken);

        if (generatedBy == EmailDraftGeneratedBy.AI)
        {
            await humanApprovalService.CreateApprovalRequestAsync(
                "ApproveAiDraft",
                nameof(EmailDraft),
                draft.Id,
                draft.AuditMetadataJson,
                cancellationToken);
        }

        return await emailRepository.GetDraftByIdAsync(draft.Id, cancellationToken)
            ?? throw new InvalidOperationException("Email draft not found after creation.");
    }

    public async Task<EmailDraft> UpdateDraftAsync(Guid draftId, string subject, string body, CancellationToken cancellationToken = default)
    {
        var draft = await emailRepository.GetDraftByIdAsync(draftId, cancellationToken)
            ?? throw new InvalidOperationException("Email draft not found.");

        if (draft.Status == EmailDraftStatus.Sent)
        {
            throw new InvalidOperationException("Sent drafts cannot be edited.");
        }

        draft.Subject = NormalizeRequired(subject, "Draft subject is required.", 512);
        draft.Body = NormalizeRequired(body, "Draft body is required.", 16000);
        draft.UpdatedAt = DateTime.UtcNow;
        await emailRepository.UpdateDraftAsync(draft, cancellationToken);
        return await emailRepository.GetDraftByIdAsync(draftId, cancellationToken)
            ?? throw new InvalidOperationException("Email draft not found after update.");
    }

    public async Task<EmailDraft> ApproveDraftAsync(Guid draftId, CancellationToken cancellationToken = default)
    {
        var draft = await emailRepository.GetDraftByIdAsync(draftId, cancellationToken)
            ?? throw new InvalidOperationException("Email draft not found.");

        if (draft.Status == EmailDraftStatus.Sent)
        {
            throw new InvalidOperationException("Sent drafts cannot be approved again.");
        }

        draft.Status = EmailDraftStatus.Approved;
        draft.ApprovedByUserId = currentUserContext.GetRequiredUserId();
        draft.ApprovedAt = DateTime.UtcNow;
        draft.UpdatedAt = DateTime.UtcNow;
        await emailRepository.UpdateDraftAsync(draft, cancellationToken);

        var approvalRequest = await humanApprovalService.CreateApprovalRequestAsync(
            "ApproveDraftForSending",
            nameof(EmailDraft),
            draft.Id,
            draft.AuditMetadataJson,
            cancellationToken);
        await humanApprovalService.ApproveActionAsync(approvalRequest.Id, "Draft approved for manual send workflow.", cancellationToken);

        return await emailRepository.GetDraftByIdAsync(draftId, cancellationToken)
            ?? throw new InvalidOperationException("Email draft not found after approval.");
    }

    public async Task<EmailDraft> SendDraftAsync(Guid draftId, CancellationToken cancellationToken = default)
    {
        var draft = await emailRepository.GetDraftByIdAsync(draftId, cancellationToken)
            ?? throw new InvalidOperationException("Email draft not found.");

        if (draft.Status != EmailDraftStatus.Approved)
        {
            throw new InvalidOperationException("Drafts must be approved by a human before sending.");
        }

        await emailProviderService.SendDraftAsync(draft, cancellationToken);
        draft.Status = EmailDraftStatus.Sent;
        draft.SentAt = DateTime.UtcNow;
        draft.UpdatedAt = DateTime.UtcNow;
        await emailRepository.UpdateDraftAsync(draft, cancellationToken);

        return await emailRepository.GetDraftByIdAsync(draftId, cancellationToken)
            ?? throw new InvalidOperationException("Email draft not found after send.");
    }

    public Task<EmailThread?> GetThreadAsync(Guid threadId, CancellationToken cancellationToken = default) =>
        emailRepository.GetThreadByIdAsync(threadId, cancellationToken);

    public Task<IReadOnlyList<EmailThread>> GetThreadsAsync(Guid? bookingId = null, CancellationToken cancellationToken = default) =>
        emailRepository.GetThreadsAsync(bookingId, cancellationToken);

    public Task<IReadOnlyList<EmailThread>> GetThreadsByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default) =>
        emailRepository.GetThreadsByBookingAsync(bookingId, cancellationToken);

    private async Task ValidateContextAsync(Guid bookingId, Guid? bookingItemId, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId, cancellationToken);
        if (booking is null)
        {
            throw new InvalidOperationException("Booking not found.");
        }

        if (bookingItemId.HasValue)
        {
            var item = await bookingItemRepository.GetByIdAsync(bookingItemId.Value, cancellationToken);
            if (item is null || item.BookingId != bookingId)
            {
                throw new InvalidOperationException("Booking item does not belong to the selected booking.");
            }
        }
    }

    private static string NormalizeRequired(string? value, string message, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException(message);
        }

        if (normalized.Length > maxLength)
        {
            throw new InvalidOperationException(message.Replace("is required", $"cannot exceed {maxLength} characters"));
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (normalized is { Length: > 0 } && normalized.Length > maxLength)
        {
            throw new InvalidOperationException($"Value cannot exceed {maxLength} characters.");
        }

        return normalized;
    }
}
