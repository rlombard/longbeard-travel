using AI.Forged.TourOps.Application.Models.Platform;

namespace AI.Forged.TourOps.Application.Interfaces.Platform;

public interface ILicensePolicyService
{
    Task<FeatureAccessResultModel> EvaluateAsync(string featureKey, CancellationToken cancellationToken = default);
    Task AssertAllowedAsync(string featureKey, CancellationToken cancellationToken = default);
    Task<TenantLicenseModel?> GetCurrentLicenseAsync(CancellationToken cancellationToken = default);
}
