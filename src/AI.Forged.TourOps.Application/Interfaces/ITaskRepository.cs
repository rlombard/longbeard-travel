using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface ITaskRepository
{
    Task<OperationalTask> AddAsync(OperationalTask task, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OperationalTask>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OperationalTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OperationalTask>> GetByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OperationalTask>> GetByAssignedUserAsync(string userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(OperationalTask task, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
