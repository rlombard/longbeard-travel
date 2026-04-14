using AI.Forged.TourOps.Application.Models.Ai;

namespace AI.Forged.TourOps.Application.Interfaces.Ai;

public interface IAiForgedService
{
    Task<AiForgedProcessingResult> ProcessEmailPdfAsync(AiForgedDocument file, CancellationToken cancellationToken = default);
    Task<AiForgedProcessingResult> ProcessBookingContextAsync(AiForgedDocument file, CancellationToken cancellationToken = default);
    Task<AiForgedProcessingResult> ExtractTasksAsync(AiForgedDocument file, CancellationToken cancellationToken = default);
}
