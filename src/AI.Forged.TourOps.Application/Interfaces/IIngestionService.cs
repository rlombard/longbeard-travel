using AI.Forged.TourOps.Application.Models;
using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Interfaces;

public interface IIngestionService
{
    Task<Rate> ProcessRatePayloadAsync(IngestionRatePayload payload, CancellationToken cancellationToken = default);
    Task<IngestionPropertyBundleResult> ProcessPropertyBundleAsync(IngestionPropertyBundlePayload payload, CancellationToken cancellationToken = default);
}
