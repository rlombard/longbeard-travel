using AI.Forged.TourOps.Api.Models;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Ai;
using Microsoft.AspNetCore.Mvc;

namespace AI.Forged.TourOps.Api.Controllers;

[ApiController]
[Route("api/itineraries")]
public class ItineraryController(IItineraryService itineraryService, IItineraryAiService itineraryAiService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ItineraryResponse>> CreateItinerary([FromBody] CreateItineraryRequest request, CancellationToken cancellationToken)
    {
        var itinerary = await itineraryService.CreateItineraryAsync(request.ToModel(), cancellationToken);

        return CreatedAtAction(nameof(GetItinerary), new { itineraryId = itinerary.Id }, itinerary.ToResponse());
    }

    [HttpGet("{itineraryId:guid}")]
    public async Task<ActionResult<ItineraryResponse>> GetItinerary(Guid itineraryId, CancellationToken cancellationToken)
    {
        var itinerary = await itineraryService.GetItineraryAsync(itineraryId, cancellationToken);
        return itinerary is null ? NotFound() : Ok(itinerary.ToResponse());
    }

    [HttpPost("ai/product-assist")]
    public async Task<ActionResult<ProductAssistResponse>> ProductAssist([FromBody] ProductAssistRequest request, CancellationToken cancellationToken)
    {
        var result = await itineraryAiService.GetProductAssistanceAsync(request.ToModel(), cancellationToken);
        return Ok(result.ToResponse());
    }

    [HttpPost("ai/draft")]
    public async Task<ActionResult<ItineraryDraftResponse>> GenerateDraft([FromBody] GenerateItineraryDraftRequestDto request, CancellationToken cancellationToken)
    {
        var draft = await itineraryAiService.GenerateDraftAsync(request.ToModel(), cancellationToken);
        return Ok(draft.ToResponse());
    }

    [HttpPost("ai/drafts/{draftId:guid}/approve")]
    public async Task<ActionResult<ItineraryDraftApprovalResponse>> ApproveDraft(Guid draftId, [FromBody] ApproveItineraryDraftRequestDto request, CancellationToken cancellationToken)
    {
        var result = await itineraryAiService.ApproveDraftAsync(draftId, request.ToModel(), cancellationToken);
        return Ok(result.ToResponse());
    }
}
