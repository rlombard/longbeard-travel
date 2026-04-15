using System.Text.Json;
using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Application.Models.Platform;

namespace AI.Forged.TourOps.Infrastructure.Platform;

public sealed class ManualSignupBillingGateway(ISignupSettingsAccessor signupSettingsAccessor) : ISignupBillingGateway
{
    public Task<SignupBillingGatewayResultModel> CreateIntentAsync(SignupBillingGatewayRequestModel model, CancellationToken cancellationToken = default) =>
        Task.FromResult(new SignupBillingGatewayResultModel
        {
            Status = signupSettingsAccessor.AllowTestPaymentConfirmation
                ? Domain.Enums.SignupBillingStatus.Pending
                : Domain.Enums.SignupBillingStatus.RequiresManualReview,
            ProviderName = "Manual",
            ExternalReference = $"manual-{model.SessionId:N}",
            MetadataJson = JsonSerializer.Serialize(new
            {
                model.PlanCode,
                model.Amount,
                model.Currency,
                supportEmail = signupSettingsAccessor.BillingSupportEmail
            })
        });

    public Task<SignupBillingGatewayResultModel> ConfirmTestPaymentAsync(SignupBillingGatewayRequestModel model, CancellationToken cancellationToken = default)
    {
        if (!signupSettingsAccessor.AllowTestPaymentConfirmation)
        {
            throw new InvalidOperationException("Test payment confirmation is disabled.");
        }

        return Task.FromResult(new SignupBillingGatewayResultModel
        {
            Status = Domain.Enums.SignupBillingStatus.Confirmed,
            ProviderName = "Manual",
            ExternalReference = $"manual-paid-{model.SessionId:N}",
            MetadataJson = JsonSerializer.Serialize(new
            {
                model.PlanCode,
                model.Amount,
                model.Currency,
                confirmedVia = "test"
            })
        });
    }
}
