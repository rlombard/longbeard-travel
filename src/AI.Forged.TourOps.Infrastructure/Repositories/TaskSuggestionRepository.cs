using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;
using AI.Forged.TourOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.Forged.TourOps.Infrastructure.Repositories;

public class TaskSuggestionRepository(AppDbContext dbContext) : ITaskSuggestionRepository
{
    public async Task AddRangeAsync(IEnumerable<OperationalTaskSuggestion> suggestions, CancellationToken cancellationToken = default)
    {
        dbContext.TaskSuggestions.AddRange(suggestions);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OperationalTaskSuggestion>> GetByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default) =>
        await dbContext.TaskSuggestions
            .Include(x => x.BookingItem)
                .ThenInclude(x => x!.Product)
            .Include(x => x.BookingItem)
                .ThenInclude(x => x!.Supplier)
            .Include(x => x.AcceptedTask)
            .AsNoTracking()
            .Where(x => x.BookingId == bookingId)
            .OrderBy(x => x.State == TaskSuggestionState.PendingReview ? 0 : 1)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<OperationalTaskSuggestion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.TaskSuggestions
            .Include(x => x.BookingItem)
                .ThenInclude(x => x!.Product)
            .Include(x => x.BookingItem)
                .ThenInclude(x => x!.Supplier)
            .Include(x => x.AcceptedTask)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task DeletePendingByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var pendingSuggestions = await dbContext.TaskSuggestions
            .Where(x => x.BookingId == bookingId && x.State == TaskSuggestionState.PendingReview)
            .ToListAsync(cancellationToken);

        dbContext.TaskSuggestions.RemoveRange(pendingSuggestions);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(OperationalTaskSuggestion suggestion, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.TaskSuggestions.FirstOrDefaultAsync(x => x.Id == suggestion.Id, cancellationToken)
            ?? throw new InvalidOperationException("Suggested task not found.");

        existing.Title = suggestion.Title;
        existing.Description = suggestion.Description;
        existing.SuggestedStatus = suggestion.SuggestedStatus;
        existing.SuggestedDueDate = suggestion.SuggestedDueDate;
        existing.Reason = suggestion.Reason;
        existing.Confidence = suggestion.Confidence;
        existing.RequiresHumanReview = suggestion.RequiresHumanReview;
        existing.State = suggestion.State;
        existing.Source = suggestion.Source;
        existing.LlmProvider = suggestion.LlmProvider;
        existing.LlmModel = suggestion.LlmModel;
        existing.AuditMetadataJson = suggestion.AuditMetadataJson;
        existing.AcceptedTaskId = suggestion.AcceptedTaskId;
        existing.ReviewedByUserId = suggestion.ReviewedByUserId;
        existing.ReviewedAt = suggestion.ReviewedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
