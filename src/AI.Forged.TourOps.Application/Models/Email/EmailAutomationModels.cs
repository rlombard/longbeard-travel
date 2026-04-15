namespace AI.Forged.TourOps.Application.Models.Email;

public sealed class EmailThreadAutomationResultModel
{
    public Guid EmailThreadId { get; init; }
    public bool AnalysisUpdated { get; init; }
    public int TaskSuggestionsCreated { get; init; }
}

public sealed class EmailAutomationRunResultModel
{
    public int ThreadsScanned { get; init; }
    public int ThreadsProcessed { get; init; }
    public int ThreadsFailed { get; init; }
    public int TaskSuggestionsCreated { get; init; }
}
