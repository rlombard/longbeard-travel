using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Ai;
using AI.Forged.TourOps.Application.Interfaces.Email;
using AI.Forged.TourOps.Application.Models.Email;

namespace AI.Forged.TourOps.Application.Services.Email;

public sealed class EmailAutomationService(
    IEmailRepository emailRepository,
    IEmailAiService emailAiService) : IEmailAutomationService
{
    public async Task<EmailAutomationRunResultModel> ProcessPendingThreadsAsync(int take, CancellationToken cancellationToken = default)
    {
        var threads = await emailRepository.GetThreadsPendingAutomationAsync(take, cancellationToken);
        var processed = 0;
        var failed = 0;
        var taskSuggestionsCreated = 0;

        foreach (var thread in threads)
        {
            try
            {
                var result = await emailAiService.ProcessThreadAutomationAsync(thread.Id, cancellationToken);
                processed++;
                taskSuggestionsCreated += result.TaskSuggestionsCreated;
            }
            catch
            {
                failed++;
            }
        }

        return new EmailAutomationRunResultModel
        {
            ThreadsScanned = threads.Count,
            ThreadsProcessed = processed,
            ThreadsFailed = failed,
            TaskSuggestionsCreated = taskSuggestionsCreated
        };
    }
}
