using AI.Forged.TourOps.Api.Models;
using AI.Forged.TourOps.Application.Interfaces.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AI.Forged.TourOps.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class BookingTaskSuggestionController(IBookingTaskSuggestionService service) : ControllerBase
{
    [HttpGet("bookings/{bookingId:guid}/task-suggestions")]
    public async Task<ActionResult<IReadOnlyList<TaskSuggestionResponse>>> GetSuggestedTasks(Guid bookingId, CancellationToken cancellationToken)
    {
        var suggestions = await service.GetSuggestedTasksAsync(bookingId, cancellationToken);
        return Ok(suggestions.Select(x => x.ToResponse()).ToList());
    }

    [HttpPost("bookings/{bookingId:guid}/task-suggestions/generate")]
    public async Task<ActionResult<IReadOnlyList<TaskSuggestionResponse>>> GenerateSuggestedTasks(Guid bookingId, CancellationToken cancellationToken)
    {
        var suggestions = await service.GenerateSuggestedTasksAsync(bookingId, cancellationToken);
        return Ok(suggestions.Select(x => x.ToResponse()).ToList());
    }

    [HttpPost("bookings/{bookingId:guid}/task-suggestions/regenerate")]
    public async Task<ActionResult<IReadOnlyList<TaskSuggestionResponse>>> RegenerateSuggestedTasks(Guid bookingId, CancellationToken cancellationToken)
    {
        var suggestions = await service.RegenerateSuggestedTasksAsync(bookingId, cancellationToken);
        return Ok(suggestions.Select(x => x.ToResponse()).ToList());
    }

    [HttpPost("task-suggestions/{id:guid}/accept")]
    public async Task<ActionResult<TaskResponse>> AcceptSuggestedTask(Guid id, [FromBody] AcceptTaskSuggestionRequest request, CancellationToken cancellationToken)
    {
        var task = await service.AcceptSuggestedTaskAsync(id, request.AssignedToUserId, cancellationToken);
        return Ok(task.ToResponse());
    }

    [HttpPost("task-suggestions/{id:guid}/reject")]
    public async Task<ActionResult<TaskSuggestionResponse>> RejectSuggestedTask(Guid id, CancellationToken cancellationToken)
    {
        var suggestion = await service.RejectSuggestedTaskAsync(id, cancellationToken);
        return Ok(suggestion.ToResponse());
    }
}
