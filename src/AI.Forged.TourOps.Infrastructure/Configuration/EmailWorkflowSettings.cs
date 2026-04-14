namespace AI.Forged.TourOps.Infrastructure.Configuration;

public sealed class EmailWorkflowSettings
{
    public string Provider { get; set; } = "LogOnly";
    public bool LogSentEmails { get; set; } = true;
    public string DefaultFromAddress { get; set; } = "ops@tourops.local";
}
