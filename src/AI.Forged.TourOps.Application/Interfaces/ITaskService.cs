using AI.Forged.TourOps.Domain.Entities;
using OperationalTaskStatus = AI.Forged.TourOps.Domain.Enums.TaskStatus;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface ITaskService
{
    Task<OperationalTask> CreateTaskAsync(
        Guid? bookingId,
        Guid? bookingItemId,
        string title,
        string? description,
        DateTime? dueDate,
        string assignedToUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OperationalTask>> GetTasksAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OperationalTask>> GetTasksByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OperationalTask>> GetTasksForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<OperationalTask> UpdateTaskStatusAsync(Guid id, OperationalTaskStatus status, CancellationToken cancellationToken = default);
    Task<OperationalTask> UpdateTaskDetailsAsync(Guid id, string title, string? description, DateTime? dueDate, CancellationToken cancellationToken = default);
    Task<OperationalTask> AssignTaskAsync(Guid id, string userId, CancellationToken cancellationToken = default);
    Task DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default);
}
