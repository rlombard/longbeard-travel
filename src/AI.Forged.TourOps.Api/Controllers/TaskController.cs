using AI.Forged.TourOps.Api.Models;
using AI.Forged.TourOps.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AI.Forged.TourOps.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public class TaskController(ITaskService taskService, ICurrentUserContext currentUserContext) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TaskResponse>> CreateTask([FromBody] TaskRequest request, CancellationToken cancellationToken)
    {
        var task = await taskService.CreateTaskAsync(
            request.BookingId,
            request.BookingItemId,
            request.Title,
            request.Description,
            request.DueDate,
            request.AssignedToUserId,
            cancellationToken);

        return CreatedAtAction(nameof(GetTasks), new { bookingId = task.BookingId ?? task.BookingItem?.BookingId }, task.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetTasks([FromQuery] Guid? bookingId, CancellationToken cancellationToken)
    {
        var tasks = bookingId.HasValue
            ? await taskService.GetTasksByBookingAsync(bookingId.Value, cancellationToken)
            : await taskService.GetTasksAsync(cancellationToken);

        return Ok(tasks.Select(x => x.ToResponse()).ToList());
    }

    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetMyTasks(CancellationToken cancellationToken)
    {
        var tasks = await taskService.GetTasksForUserAsync(currentUserContext.GetRequiredUserId(), cancellationToken);
        return Ok(tasks.Select(x => x.ToResponse()).ToList());
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<TaskResponse>> UpdateTaskStatus(Guid id, [FromBody] UpdateTaskStatusRequest request, CancellationToken cancellationToken)
    {
        var task = await taskService.UpdateTaskStatusAsync(id, request.Status, cancellationToken);
        return Ok(task.ToResponse());
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<TaskResponse>> UpdateTask(Guid id, [FromBody] UpdateTaskDetailsRequest request, CancellationToken cancellationToken)
    {
        var task = await taskService.UpdateTaskDetailsAsync(id, request.Title, request.Description, request.DueDate, cancellationToken);
        return Ok(task.ToResponse());
    }

    [HttpPatch("{id:guid}/assign")]
    public async Task<ActionResult<TaskResponse>> AssignTask(Guid id, [FromBody] AssignTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await taskService.AssignTaskAsync(id, request.UserId, cancellationToken);
        return Ok(task.ToResponse());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTask(Guid id, CancellationToken cancellationToken)
    {
        await taskService.DeleteTaskAsync(id, cancellationToken);
        return NoContent();
    }
}
