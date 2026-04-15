using AI.Forged.TourOps.Application.Models.Platform;

namespace AI.Forged.TourOps.Application.Interfaces.Platform;

public interface ISignupBillingGateway
{
    Task<SignupBillingGatewayResultModel> CreateIntentAsync(SignupBillingGatewayRequestModel model, CancellationToken cancellationToken = default);
    Task<SignupBillingGatewayResultModel> ConfirmTestPaymentAsync(SignupBillingGatewayRequestModel model, CancellationToken cancellationToken = default);
}
