using System.ComponentModel.DataAnnotations;

namespace AI.Forged.TourOps.Infrastructure.Configuration;

public sealed class EmailIntegrationSettings
{
    public bool Enabled { get; set; } = true;

    [Required]
    public string EncryptionKey { get; set; } = string.Empty;

    [Range(1, 120)]
    public int SyncWorkerIntervalSeconds { get; set; } = 60;

    [Range(1, 500)]
    public int SyncBatchSize { get; set; } = 50;

    public bool AutomationEnabled { get; set; } = true;

    [Range(1, 120)]
    public int AutomationWorkerIntervalSeconds { get; set; } = 30;

    [Range(1, 200)]
    public int AutomationBatchSize { get; set; } = 25;
}
