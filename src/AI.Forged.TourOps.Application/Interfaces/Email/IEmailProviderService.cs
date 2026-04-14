using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces.Email;

public interface IEmailProviderService
{
    Task<string> SendDraftAsync(EmailDraft draft, CancellationToken cancellationToken = default);
}
