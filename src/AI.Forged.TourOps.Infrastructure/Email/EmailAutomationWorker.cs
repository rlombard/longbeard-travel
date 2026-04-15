using AI.Forged.TourOps.Application.Interfaces.Email;
using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.Forged.TourOps.Infrastructure.Email;

public sealed class EmailAutomationWorker(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<EmailIntegrationSettings> settings,
    ILogger<EmailAutomationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!settings.Value.Enabled || !settings.Value.AutomationEnabled)
        {
            logger.LogInformation("Email automation worker disabled.");
            return;
        }

        var interval = TimeSpan.FromSeconds(settings.Value.AutomationWorkerIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var automationService = scope.ServiceProvider.GetRequiredService<IEmailAutomationService>();
                var tenantRepository = scope.ServiceProvider.GetRequiredService<ITenantPlatformRepository>();
                var tenantExecutionContextAccessor = scope.ServiceProvider.GetRequiredService<ITenantExecutionContextAccessor>();

                var processedThreads = 0;
                var failedThreads = 0;
                var taskSuggestions = 0;

                if (tenantExecutionContextAccessor.DeploymentMode == Domain.Enums.DeploymentMode.Standalone)
                {
                    var tenant = await tenantRepository.GetStandaloneTenantAsync(stoppingToken);
                    if (tenant is not null)
                    {
                        using var tenantScope = tenantExecutionContextAccessor.BeginTenantScope(tenant.Id);
                        var result = await automationService.ProcessPendingThreadsAsync(settings.Value.AutomationBatchSize, stoppingToken);
                        processedThreads += result.ThreadsProcessed;
                        failedThreads += result.ThreadsFailed;
                        taskSuggestions += result.TaskSuggestionsCreated;
                    }
                }
                else
                {
                    var tenants = await tenantRepository.GetTenantsAsync(stoppingToken);
                    foreach (var tenant in tenants.Where(x => !x.IsStandaloneTenant && x.Status == Domain.Enums.TenantStatus.Active))
                    {
                        using var tenantScope = tenantExecutionContextAccessor.BeginTenantScope(tenant.Id);
                        var result = await automationService.ProcessPendingThreadsAsync(settings.Value.AutomationBatchSize, stoppingToken);
                        processedThreads += result.ThreadsProcessed;
                        failedThreads += result.ThreadsFailed;
                        taskSuggestions += result.TaskSuggestionsCreated;
                    }
                }

                if (processedThreads > 0 || failedThreads > 0)
                {
                    logger.LogInformation(
                        "Email automation worker processed {Processed} thread(s), failed {Failed}, created {TaskSuggestions} task suggestion(s).",
                        processedThreads,
                        failedThreads,
                        taskSuggestions);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Email automation worker cycle failed.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
