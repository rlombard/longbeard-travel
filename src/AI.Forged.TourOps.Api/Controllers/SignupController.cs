using AI.Forged.TourOps.Api.Models;
using AI.Forged.TourOps.Application.Interfaces.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AI.Forged.TourOps.Api.Controllers;

[ApiController]
[AllowAnonymous]
[EnableRateLimiting("public-signup")]
[Route("api/signup")]
public sealed class SignupController(ISignupOnboardingService signupOnboardingService) : ControllerBase
{
    [HttpGet("bootstrap")]
    public async Task<ActionResult<SignupBootstrapResponse>> GetBootstrap(CancellationToken cancellationToken)
    {
        var result = await signupOnboardingService.GetBootstrapAsync(cancellationToken);
        return Ok(result.ToResponse());
    }

    [HttpGet("plans")]
    public async Task<ActionResult<IReadOnlyList<SignupPlanResponse>>> GetPlans(CancellationToken cancellationToken)
    {
        var plans = await signupOnboardingService.GetPlansAsync(cancellationToken);
        return Ok(plans.Select(x => x.ToResponse()).ToList());
    }

    [HttpPost("sessions")]
    public async Task<ActionResult<SignupSessionEnvelopeResponse>> StartSession(CancellationToken cancellationToken)
    {
        var session = await signupOnboardingService.StartSessionAsync(cancellationToken);
        return Ok(session.ToResponse());
    }

    [HttpGet("sessions/{sessionId:guid}")]
    public async Task<ActionResult<SignupSessionEnvelopeResponse>> GetSession(Guid sessionId, [FromHeader(Name = "X-Signup-Access-Token")] string accessToken, CancellationToken cancellationToken)
    {
        var session = await signupOnboardingService.GetSessionAsync(sessionId, accessToken, cancellationToken);
        return Ok(session.ToResponse());
    }

    [HttpPut("sessions/{sessionId:guid}/email")]
    public async Task<ActionResult<SignupSessionEnvelopeResponse>> SaveEmail(Guid sessionId, [FromHeader(Name = "X-Signup-Access-Token")] string accessToken, [FromBody] SignupEmailUpdateRequest request, CancellationToken cancellationToken)
    {
        var session = await signupOnboardingService.SaveEmailAsync(sessionId, accessToken, request.ToModel(), cancellationToken);
        return Ok(session.ToResponse());
    }

    [HttpPost("sessions/{sessionId:guid}/email/resend")]
    public async Task<ActionResult<SignupSessionEnvelopeResponse>> ResendVerification(Guid sessionId, [FromHeader(Name = "X-Signup-Access-Token")] string accessToken, CancellationToken cancellationToken)
    {
        var session = await signupOnboardingService.ResendVerificationAsync(sessionId, accessToken, cancellationToken);
        return Ok(session.ToResponse());
    }

    [HttpPost("sessions/{sessionId:guid}/email/verify")]
    public async Task<ActionResult<SignupSessionEnvelopeResponse>> VerifyEmail(Guid sessionId, [FromBody] SignupVerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var session = await signupOnboardingService.VerifyEmailAsync(sessionId, request.ToModel(), cancellationToken);
        return Ok(session.ToResponse());
    }

    [HttpPut("sessions/{sessionId:guid}/organization")]
    public async Task<ActionResult<SignupSessionEnvelopeResponse>> SaveOrganization(Guid sessionId, [FromHeader(Name = "X-Signup-Access-Token")] string accessToken, [FromBody] SignupOrganizationRequest request, CancellationToken cancellationToken)
    {
        var session = await signupOnboardingService.SaveOrganizationAsync(sessionId, accessToken, request.ToModel(), cancellationToken);
        return Ok(session.ToResponse());
    }

    [HttpPut("sessions/{sessionId:guid}/plan")]
    public async Task<ActionResult<SignupSessionEnvelopeResponse>> SelectPlan(Guid sessionId, [FromHeader(Name = "X-Signup-Access-Token")] string accessToken, [FromBody] SignupPlanSelectionRequest request, CancellationToken cancellationToken)
    {
        var session = await signupOnboardingService.SelectPlanAsync(sessionId, accessToken, request.ToModel(), cancellationToken);
        return Ok(session.ToResponse());
    }

    [HttpPut("sessions/{sessionId:guid}/terms")]
    public async Task<ActionResult<SignupSessionEnvelopeResponse>> AcceptTerms(Guid sessionId, [FromHeader(Name = "X-Signup-Access-Token")] string accessToken, [FromBody] SignupTermsAcceptanceRequest request, CancellationToken cancellationToken)
    {
        var session = await signupOnboardingService.AcceptTermsAsync(sessionId, accessToken, request.ToModel(), cancellationToken);
        return Ok(session.ToResponse());
    }

    [HttpPost("sessions/{sessionId:guid}/billing-intent")]
    public async Task<ActionResult<SignupSessionEnvelopeResponse>> CreateBillingIntent(Guid sessionId, [FromHeader(Name = "X-Signup-Access-Token")] string accessToken, CancellationToken cancellationToken)
    {
        var session = await signupOnboardingService.CreateBillingIntentAsync(sessionId, accessToken, cancellationToken);
        return Ok(session.ToResponse());
    }

    [HttpPost("sessions/{sessionId:guid}/billing-intent/test-confirm")]
    public async Task<ActionResult<SignupSessionEnvelopeResponse>> ConfirmTestPayment(Guid sessionId, [FromHeader(Name = "X-Signup-Access-Token")] string accessToken, CancellationToken cancellationToken)
    {
        var session = await signupOnboardingService.ConfirmTestPaymentAsync(sessionId, accessToken, cancellationToken);
        return Ok(session.ToResponse());
    }

    [HttpPut("sessions/{sessionId:guid}/admin")]
    public async Task<ActionResult<SignupSessionEnvelopeResponse>> SaveAdmin(Guid sessionId, [FromHeader(Name = "X-Signup-Access-Token")] string accessToken, [FromBody] SignupAdminSetupRequest request, CancellationToken cancellationToken)
    {
        var session = await signupOnboardingService.SaveAdminAsync(sessionId, accessToken, request.ToModel(), cancellationToken);
        return Ok(session.ToResponse());
    }

    [HttpPost("sessions/{sessionId:guid}/provision")]
    public async Task<ActionResult<SignupSessionEnvelopeResponse>> Provision(Guid sessionId, [FromHeader(Name = "X-Signup-Access-Token")] string accessToken, CancellationToken cancellationToken)
    {
        var session = await signupOnboardingService.ProvisionAsync(sessionId, accessToken, cancellationToken);
        return Ok(session.ToResponse());
    }

    [HttpPost("sessions/{sessionId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid sessionId, [FromHeader(Name = "X-Signup-Access-Token")] string accessToken, CancellationToken cancellationToken)
    {
        await signupOnboardingService.CancelAsync(sessionId, accessToken, cancellationToken);
        return NoContent();
    }
}
