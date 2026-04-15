using AI.Forged.TourOps.Application.Models.Email;

namespace AI.Forged.TourOps.Application.Interfaces.Email;

public interface IEmailAutomationService
{
    Task<EmailAutomationRunResultModel> ProcessPendingThreadsAsync(int take, CancellationToken cancellationToken = default);
}
