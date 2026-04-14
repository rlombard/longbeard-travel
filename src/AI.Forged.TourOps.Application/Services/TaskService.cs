using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;
using OperationalTaskStatus = AI.Forged.TourOps.Domain.Enums.TaskStatus;

namespace AI.Forged.TourOps.Application.Services;

public class TaskService(
    ITaskRepository taskRepository,
    IBookingRepository bookingRepository,
    IBookingItemRepository bookingItemRepository,
    ICurrentUserContext currentUserContext) : ITaskService
{
    public async Task<OperationalTask> CreateTaskAsync(
        Guid? bookingId,
        Guid? bookingItemId,
        string title,
        string? description,
        DateTime? dueDate,
        string assignedToUserId,
        CancellationToken cancellationToken = default)
    {
        var normalizedTitle = NormalizeTitle(title);
        var normalizedDescription = NormalizeDescription(description);
        var normalizedAssignee = NormalizeUserId(assignedToUserId, "Assigned user is required.");

        await ValidateContextAsync(bookingId, bookingItemId, cancellationToken);

        var now = DateTime.UtcNow;
        var task = new OperationalTask
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            BookingItemId = bookingItemId,
            Title = normalizedTitle,
            Description = normalizedDescription,
            Status = OperationalTaskStatus.ToDo,
            AssignedToUserId = normalizedAssignee,
            CreatedByUserId = currentUserContext.GetRequiredUserId(),
            DueDate = NormalizeDueDate(dueDate),
            CreatedAt = now,
            UpdatedAt = now
        };

        await taskRepository.AddAsync(task, cancellationToken);

        return await taskRepository.GetByIdAsync(task.Id, cancellationToken)
            ?? throw new InvalidOperationException("Task not found after creation.");
    }

    public Task<IReadOnlyList<OperationalTask>> GetTasksAsync(CancellationToken cancellationToken = default) =>
        taskRepository.GetAllAsync(cancellationToken);

    public Task<IReadOnlyList<OperationalTask>> GetTasksByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default) =>
        taskRepository.GetByBookingAsync(bookingId, cancellationToken);

    public Task<IReadOnlyList<OperationalTask>> GetTasksForUserAsync(string userId, CancellationToken cancellationToken = default) =>
        taskRepository.GetByAssignedUserAsync(NormalizeUserId(userId, "User id is required."), cancellationToken);

    public async Task<OperationalTask> UpdateTaskStatusAsync(Guid id, OperationalTaskStatus status, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Task not found.");

        if (!IsValidStatusTransition(task.Status, status))
        {
            throw new InvalidOperationException($"Invalid task status transition from '{task.Status}' to '{status}'.");
        }

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;

        await taskRepository.UpdateAsync(task, cancellationToken);

        return await taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Task not found after update.");
    }

    public async Task<OperationalTask> UpdateTaskDetailsAsync(Guid id, string title, string? description, DateTime? dueDate, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Task not found.");

        task.Title = NormalizeTitle(title);
        task.Description = NormalizeDescription(description);
        task.DueDate = NormalizeDueDate(dueDate);
        task.UpdatedAt = DateTime.UtcNow;

        await taskRepository.UpdateAsync(task, cancellationToken);

        return await taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Task not found after update.");
    }

    public async Task<OperationalTask> AssignTaskAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Task not found.");

        task.AssignedToUserId = NormalizeUserId(userId, "Assigned user is required.");
        task.UpdatedAt = DateTime.UtcNow;

        await taskRepository.UpdateAsync(task, cancellationToken);

        return await taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Task not found after update.");
    }

    public async Task DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Task not found.");

        await taskRepository.DeleteAsync(task.Id, cancellationToken);
    }

    private async Task ValidateContextAsync(Guid? bookingId, Guid? bookingItemId, CancellationToken cancellationToken)
    {
        var hasBookingId = bookingId.HasValue;
        var hasBookingItemId = bookingItemId.HasValue;

        if (hasBookingId == hasBookingItemId)
        {
            throw new InvalidOperationException("Task must be linked to either a booking or a booking item.");
        }

        if (hasBookingId)
        {
            var booking = await bookingRepository.GetByIdAsync(bookingId!.Value, cancellationToken);
            if (booking is null)
            {
                throw new InvalidOperationException("Booking not found.");
            }
        }

        if (hasBookingItemId)
        {
            var bookingItem = await bookingItemRepository.GetByIdAsync(bookingItemId!.Value, cancellationToken);
            if (bookingItem is null)
            {
                throw new InvalidOperationException("Booking item not found.");
            }
        }
    }

    private static bool IsValidStatusTransition(OperationalTaskStatus currentStatus, OperationalTaskStatus nextStatus)
    {
        if (currentStatus == nextStatus)
        {
            return true;
        }

        if (nextStatus is OperationalTaskStatus.Blocked or OperationalTaskStatus.Done)
        {
            return true;
        }

        return currentStatus switch
        {
            OperationalTaskStatus.Blocked => nextStatus == OperationalTaskStatus.ToDo,
            OperationalTaskStatus.ToDo => nextStatus == OperationalTaskStatus.Waiting,
            OperationalTaskStatus.Waiting => nextStatus == OperationalTaskStatus.FollowUp,
            OperationalTaskStatus.FollowUp => nextStatus == OperationalTaskStatus.ToDo,
            _ => false
        };
    }

    private static string NormalizeTitle(string title)
    {
        var normalized = title?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("Task title is required.");
        }

        if (normalized.Length > 200)
        {
            throw new InvalidOperationException("Task title cannot exceed 200 characters.");
        }

        return normalized;
    }

    private static string? NormalizeDescription(string? description)
    {
        var normalized = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (normalized is { Length: > 4000 })
        {
            throw new InvalidOperationException("Task description cannot exceed 4000 characters.");
        }

        return normalized;
    }

    private static string NormalizeUserId(string? userId, string message)
    {
        var normalized = userId?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException(message);
        }

        if (normalized.Length > 256)
        {
            throw new InvalidOperationException("User id cannot exceed 256 characters.");
        }

        return normalized;
    }

    private static DateTime? NormalizeDueDate(DateTime? dueDate) => dueDate?.ToUniversalTime();
}
