using AI.Forged.TourOps.Application.Interfaces.Email;
using AI.Forged.TourOps.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.Forged.TourOps.Infrastructure.Email;

public sealed class EmailIntegrationSyncWorker(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<EmailIntegrationSettings> settings,
    ILogger<EmailIntegrationSyncWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!settings.Value.Enabled)
        {
            logger.LogInformation("Email integration sync worker disabled.");
            return;
        }

        var interval = TimeSpan.FromSeconds(settings.Value.SyncWorkerIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IEmailIntegrationService>();
                var processed = await service.RunDueSyncAsync(stoppingToken);

                if (processed > 0)
                {
                    logger.LogInformation("Email integration sync worker processed {Count} connection(s).", processed);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Email integration sync worker cycle failed.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
