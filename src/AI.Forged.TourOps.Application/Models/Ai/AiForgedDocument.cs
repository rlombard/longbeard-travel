namespace AI.Forged.TourOps.Application.Models.Ai;

public sealed class AiForgedDocument
{
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = "application/pdf";
    public byte[] Content { get; init; } = [];
    public string TextContent { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}
