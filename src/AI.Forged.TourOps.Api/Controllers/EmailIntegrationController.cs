using AI.Forged.TourOps.Api.Models;
using AI.Forged.TourOps.Application.Interfaces.Email;
using AI.Forged.TourOps.Application.Models.EmailIntegrations;
using AI.Forged.TourOps.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AI.Forged.TourOps.Api.Controllers;

[ApiController]
[Route("api/email-integrations")]
public sealed class EmailIntegrationController(IEmailIntegrationService emailIntegrationService) : ControllerBase
{
    [Authorize]
    [HttpGet("connections")]
    public async Task<ActionResult<IReadOnlyList<EmailProviderConnectionListItemResponse>>> GetConnections(CancellationToken cancellationToken)
    {
        var connections = await emailIntegrationService.GetConnectionsAsync(cancellationToken);
        return Ok(connections.Select(x => x.ToResponse()).ToList());
    }

    [Authorize]
    [HttpGet("connections/{connectionId:guid}")]
    public async Task<ActionResult<EmailProviderConnectionResponse>> GetConnection(Guid connectionId, CancellationToken cancellationToken)
    {
        var connection = await emailIntegrationService.GetConnectionAsync(connectionId, cancellationToken);
        return connection is null ? NotFound() : Ok(connection.ToResponse());
    }

    [Authorize]
    [HttpPost("connections")]
    public async Task<ActionResult<EmailProviderConnectionResponse>> CreateConnection([FromBody] CreateEmailProviderConnectionRequest request, CancellationToken cancellationToken)
    {
        var connection = await emailIntegrationService.CreateConnectionAsync(request.ToModel(), cancellationToken);
        return CreatedAtAction(nameof(GetConnection), new { connectionId = connection.Id }, connection.ToResponse());
    }

    [Authorize]
    [HttpPost("oauth/start")]
    public async Task<ActionResult<EmailOAuthStartResponse>> StartOAuth([FromBody] StartEmailProviderOAuthRequest request, CancellationToken cancellationToken)
    {
        var result = await emailIntegrationService.StartOAuthAsync(request.ToModel(), cancellationToken);
        return Ok(result.ToResponse());
    }

    [AllowAnonymous]
    [HttpGet("oauth/callback/{providerType}")]
    public async Task<IActionResult> CompleteOAuth(
        string providerType,
        [FromQuery] string state,
        [FromQuery] string? code,
        [FromQuery] string? error,
        [FromQuery] string? error_description,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<EmailIntegrationProviderType>(providerType, true, out var parsedProviderType))
        {
            return BadRequest(new { error = "Unknown email integration provider type." });
        }

        var result = await emailIntegrationService.CompleteOAuthAsync(new CompleteEmailOAuthCallbackModel
        {
            ProviderType = parsedProviderType,
            State = state,
            Code = code ?? string.Empty,
            Error = error,
            ErrorDescription = error_description
        }, cancellationToken);

        if (!string.IsNullOrWhiteSpace(result.RedirectUrl))
        {
            var separator = result.RedirectUrl.Contains('?', StringComparison.Ordinal) ? "&" : "?";
            return Redirect($"{result.RedirectUrl}{separator}status=connected&connectionId={result.ConnectionId}");
        }

        return Ok(result.ToResponse());
    }

    [Authorize]
    [HttpPost("connections/{connectionId:guid}/test")]
    public async Task<ActionResult<EmailConnectionTestResponse>> TestConnection(Guid connectionId, CancellationToken cancellationToken)
    {
        var result = await emailIntegrationService.TestConnectionAsync(connectionId, cancellationToken);
        return Ok(result.ToResponse());
    }

    [Authorize]
    [HttpPost("connections/{connectionId:guid}/sync")]
    public async Task<ActionResult<EmailSyncResponse>> SyncConnection(Guid connectionId, CancellationToken cancellationToken)
    {
        var result = await emailIntegrationService.SyncConnectionAsync(connectionId, cancellationToken);
        return Ok(result.ToResponse());
    }

    [Authorize]
    [HttpPost("connections/{connectionId:guid}/send")]
    public async Task<ActionResult<EmailSendResponse>> SendMessage(Guid connectionId, [FromBody] SendConnectedEmailMessageRequest request, CancellationToken cancellationToken)
    {
        var result = await emailIntegrationService.SendMessageAsync(connectionId, request.ToModel(), cancellationToken);
        return Ok(result.ToResponse());
    }

    [Authorize]
    [HttpDelete("connections/{connectionId:guid}")]
    public async Task<ActionResult<DisconnectEmailConnectionResponse>> Disconnect(Guid connectionId, CancellationToken cancellationToken)
    {
        var result = await emailIntegrationService.DisconnectAsync(connectionId, cancellationToken);
        return Ok(result.ToResponse());
    }

    [AllowAnonymous]
    [HttpGet("webhooks/microsoft-graph")]
    public async Task<IActionResult> ValidateMicrosoftGraphWebhook([FromQuery] string? validationToken, CancellationToken cancellationToken)
    {
        var result = await emailIntegrationService.HandleMicrosoftGraphWebhookAsync(validationToken, "{}", cancellationToken);
        return string.IsNullOrWhiteSpace(result.ValidationResponse) ? Ok(result.ToResponse()) : Content(result.ValidationResponse, "text/plain");
    }

    [AllowAnonymous]
    [HttpPost("webhooks/microsoft-graph")]
    public async Task<ActionResult<EmailWebhookResponse>> HandleMicrosoftGraphWebhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var result = await emailIntegrationService.HandleMicrosoftGraphWebhookAsync(null, payload, cancellationToken);
        return Ok(result.ToResponse());
    }
}
