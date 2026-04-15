using AI.Forged.TourOps.Application.Models.Platform;

namespace AI.Forged.TourOps.Application.Interfaces.Platform;

public interface ISignupOnboardingService
{
    Task<SignupBootstrapModel> GetBootstrapAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SignupPlanModel>> GetPlansAsync(CancellationToken cancellationToken = default);
    Task<SignupSessionEnvelopeModel> StartSessionAsync(CancellationToken cancellationToken = default);
    Task<SignupSessionEnvelopeModel> GetSessionAsync(Guid sessionId, string accessToken, CancellationToken cancellationToken = default);
    Task<SignupSessionEnvelopeModel> SaveEmailAsync(Guid sessionId, string accessToken, SignupEmailUpdateModel model, CancellationToken cancellationToken = default);
    Task<SignupSessionEnvelopeModel> ResendVerificationAsync(Guid sessionId, string accessToken, CancellationToken cancellationToken = default);
    Task<SignupSessionEnvelopeModel> VerifyEmailAsync(Guid sessionId, SignupVerifyEmailModel model, CancellationToken cancellationToken = default);
    Task<SignupSessionEnvelopeModel> SaveOrganizationAsync(Guid sessionId, string accessToken, SignupOrganizationModel model, CancellationToken cancellationToken = default);
    Task<SignupSessionEnvelopeModel> SelectPlanAsync(Guid sessionId, string accessToken, SignupPlanSelectionModel model, CancellationToken cancellationToken = default);
    Task<SignupSessionEnvelopeModel> AcceptTermsAsync(Guid sessionId, string accessToken, SignupTermsAcceptanceModel model, CancellationToken cancellationToken = default);
    Task<SignupSessionEnvelopeModel> CreateBillingIntentAsync(Guid sessionId, string accessToken, CancellationToken cancellationToken = default);
    Task<SignupSessionEnvelopeModel> ConfirmTestPaymentAsync(Guid sessionId, string accessToken, CancellationToken cancellationToken = default);
    Task<SignupSessionEnvelopeModel> SaveAdminAsync(Guid sessionId, string accessToken, SignupAdminSetupModel model, CancellationToken cancellationToken = default);
    Task<SignupSessionEnvelopeModel> ProvisionAsync(Guid sessionId, string accessToken, CancellationToken cancellationToken = default);
    Task CancelAsync(Guid sessionId, string accessToken, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SignupSessionSummaryModel>> GetAdminSessionsAsync(int take, CancellationToken cancellationToken = default);
    Task<SignupSessionEnvelopeModel> RetryProvisioningAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<SignupSessionEnvelopeModel> ConfirmBillingForAdminAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
