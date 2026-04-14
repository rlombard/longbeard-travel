using AI.Forged.TourOps.Application.Interfaces.Email;
using AI.Forged.TourOps.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AI.Forged.TourOps.Infrastructure.Email;

public class LogOnlyEmailProviderService(ILogger<LogOnlyEmailProviderService> logger) : IEmailProviderService
{
    public Task<string> SendDraftAsync(EmailDraft draft, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Simulated sending email draft {DraftId} with subject '{Subject}'.", draft.Id, draft.Subject);
        return Task.FromResult($"logonly:{draft.Id}");
    }
}
