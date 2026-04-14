using AI.Forged.TourOps.Api.Models;
using AI.Forged.TourOps.Application.Interfaces.Email;
using AI.Forged.TourOps.Application.Interfaces.Tasks;
using AI.Forged.TourOps.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AI.Forged.TourOps.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class EmailController(IEmailService emailService, IEmailAnalysisService emailAnalysisService, IBookingTaskSuggestionService bookingTaskSuggestionService) : ControllerBase
{
    [HttpGet("email-threads")]
    public async Task<ActionResult<IReadOnlyList<EmailThreadResponse>>> GetAllThreads([FromQuery] Guid? bookingId, CancellationToken cancellationToken)
    {
        var threads = await emailService.GetThreadsAsync(bookingId, cancellationToken);
        return Ok(threads.Select(x => x.ToResponse()).ToList());
    }

    [HttpGet("bookings/{bookingId:guid}/email-threads")]
    public async Task<ActionResult<IReadOnlyList<EmailThreadResponse>>> GetThreads(Guid bookingId, CancellationToken cancellationToken)
    {
        var threads = await emailService.GetThreadsByBookingAsync(bookingId, cancellationToken);
        return Ok(threads.Select(x => x.ToResponse()).ToList());
    }

    [HttpPost("bookings/{bookingId:guid}/email-threads")]
    public async Task<ActionResult<EmailThreadResponse>> CreateThread(Guid bookingId, [FromBody] CreateEmailThreadRequest request, CancellationToken cancellationToken)
    {
        var thread = await emailService.CreateThreadAsync(bookingId, request.BookingItemId, request.Subject, request.SupplierEmail, request.ExternalThreadId, cancellationToken);
        return Ok(thread.ToResponse());
    }

    [HttpGet("email-threads/{threadId:guid}")]
    public async Task<ActionResult<EmailThreadResponse>> GetThread(Guid threadId, CancellationToken cancellationToken)
    {
        var thread = await emailService.GetThreadAsync(threadId, cancellationToken);
        return thread is null ? NotFound() : Ok(thread.ToResponse());
    }

    [HttpPost("email-threads/{threadId:guid}/messages")]
    public async Task<ActionResult<EmailMessageResponse>> AddMessage(Guid threadId, [FromBody] AddEmailMessageRequest request, CancellationToken cancellationToken)
    {
        var message = await emailService.AddMessageAsync(threadId, request.Direction, request.Subject, request.BodyText, request.BodyHtml, request.Sender, request.Recipients, request.SentAt, cancellationToken);
        return Ok(message.ToResponse());
    }

    [HttpPost("email-threads/{threadId:guid}/analyze")]
    public async Task<ActionResult<EmailThreadAiAnalysisResponse>> AnalyzeThread(Guid threadId, CancellationToken cancellationToken)
    {
        var analysis = await emailAnalysisService.AnalyzeThreadAsync(threadId, cancellationToken);
        return Ok(new EmailThreadAiAnalysisResponse
        {
            EmailThreadId = threadId,
            Summary = analysis.Summary,
            Classification = analysis.Classification,
            Reason = analysis.Reason,
            Confidence = analysis.Confidence,
            RequiresHumanReview = analysis.RequiresHumanReview,
            RecommendedActions = analysis.RecommendedActions,
            MissingInformationItems = analysis.MissingInformationItems
        });
    }

    [HttpPost("email-threads/{threadId:guid}/generate-tasks")]
    public async Task<ActionResult<IReadOnlyList<TaskSuggestionResponse>>> GenerateTasks(Guid threadId, CancellationToken cancellationToken)
    {
        var generated = await emailAnalysisService.SuggestTasksFromEmailAsync(threadId, cancellationToken);
        var thread = await emailService.GetThreadAsync(threadId, cancellationToken)
            ?? throw new InvalidOperationException("Email thread not found after task generation.");
        var bookingId = thread.BookingId ?? thread.BookingItem?.BookingId
            ?? throw new InvalidOperationException("Email thread is not linked to a booking.");
        var suggestions = await bookingTaskSuggestionService.GetSuggestedTasksAsync(bookingId, cancellationToken);
        return Ok(suggestions.Where(x => x.Source == "AiForgedEmail" && x.State == AI.Forged.TourOps.Domain.Enums.TaskSuggestionState.PendingReview).Select(x => x.ToResponse()).ToList());
    }

    [HttpPost("email-threads/{threadId:guid}/suggest-reply")]
    [HttpPost("email-threads/{threadId:guid}/draft-reply")]
    public async Task<ActionResult<EmailDraftResponse>> DraftReply(Guid threadId, CancellationToken cancellationToken)
    {
        var draft = await emailAnalysisService.SuggestReplyAsync(threadId, cancellationToken);
        return Ok(draft.ToResponse());
    }

    [HttpPost("email-drafts")]
    public async Task<ActionResult<EmailDraftResponse>> CreateDraft([FromBody] CreateEmailDraftRequest request, CancellationToken cancellationToken)
    {
        var draft = await emailService.CreateDraftAsync(request.BookingId, request.BookingItemId, request.EmailThreadId, request.Subject, request.Body, EmailDraftGeneratedBy.Human, null, null, null, cancellationToken);
        return Ok(draft.ToResponse());
    }

    [HttpPatch("email-drafts/{id:guid}")]
    public async Task<ActionResult<EmailDraftResponse>> UpdateDraft(Guid id, [FromBody] UpdateEmailDraftRequest request, CancellationToken cancellationToken)
    {
        var draft = await emailService.UpdateDraftAsync(id, request.Subject, request.Body, cancellationToken);
        return Ok(draft.ToResponse());
    }

    [HttpPost("email-drafts/{id:guid}/approve")]
    public async Task<ActionResult<EmailDraftResponse>> ApproveDraft(Guid id, CancellationToken cancellationToken)
    {
        var draft = await emailService.ApproveDraftAsync(id, cancellationToken);
        return Ok(draft.ToResponse());
    }

    [HttpPost("email-drafts/{id:guid}/send")]
    public async Task<ActionResult<EmailDraftResponse>> SendDraft(Guid id, CancellationToken cancellationToken)
    {
        var draft = await emailService.SendDraftAsync(id, cancellationToken);
        return Ok(draft.ToResponse());
    }
}
