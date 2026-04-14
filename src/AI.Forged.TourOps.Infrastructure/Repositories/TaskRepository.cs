using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using OperationalTaskStatus = AI.Forged.TourOps.Domain.Enums.TaskStatus;

namespace AI.Forged.TourOps.Infrastructure.Repositories;

public class TaskRepository(AppDbContext dbContext) : ITaskRepository
{
    public async Task<OperationalTask> AddAsync(OperationalTask task, CancellationToken cancellationToken = default)
    {
        dbContext.Set<OperationalTask>().Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<IReadOnlyList<OperationalTask>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await BuildTaskQuery()
            .OrderBy(x => x.Status == OperationalTaskStatus.Done)
            .ThenBy(x => x.DueDate == null)
            .ThenBy(x => x.DueDate)
            .ThenByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

    public async Task<OperationalTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await BuildTaskQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<OperationalTask>> GetByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default) =>
        await BuildTaskQuery()
            .Where(x => x.BookingId == bookingId || (x.BookingItem != null && x.BookingItem.BookingId == bookingId))
            .OrderBy(x => x.Status == OperationalTaskStatus.Done)
            .ThenBy(x => x.DueDate == null)
            .ThenBy(x => x.DueDate)
            .ThenByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<OperationalTask>> GetByAssignedUserAsync(string userId, CancellationToken cancellationToken = default) =>
        await BuildTaskQuery()
            .Where(x => x.AssignedToUserId == userId)
            .OrderBy(x => x.Status == OperationalTaskStatus.Done)
            .ThenBy(x => x.DueDate == null)
            .ThenBy(x => x.DueDate)
            .ThenByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

    public async Task UpdateAsync(OperationalTask task, CancellationToken cancellationToken = default)
    {
        var existingTask = await dbContext.Set<OperationalTask>().FirstOrDefaultAsync(x => x.Id == task.Id, cancellationToken)
            ?? throw new InvalidOperationException("Task not found.");

        existingTask.Title = task.Title;
        existingTask.Description = task.Description;
        existingTask.Status = task.Status;
        existingTask.AssignedToUserId = task.AssignedToUserId;
        existingTask.CreatedByUserId = task.CreatedByUserId;
        existingTask.DueDate = task.DueDate;
        existingTask.UpdatedAt = task.UpdatedAt;
        existingTask.BookingId = task.BookingId;
        existingTask.BookingItemId = task.BookingItemId;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await dbContext.Set<OperationalTask>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Task not found.");

        dbContext.Remove(task);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<OperationalTask> BuildTaskQuery() =>
        dbContext.Set<OperationalTask>()
            .Include(x => x.Booking)
            .Include(x => x.BookingItem)
                .ThenInclude(x => x!.Booking)
            .Include(x => x.BookingItem)
                .ThenInclude(x => x!.Product)
            .Include(x => x.BookingItem)
                .ThenInclude(x => x!.Supplier)
            .AsNoTracking();
}
