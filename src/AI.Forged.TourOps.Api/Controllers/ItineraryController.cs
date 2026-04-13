using AI.Forged.TourOps.Api.Models;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AI.Forged.TourOps.Api.Controllers;

[ApiController]
[Route("api/itineraries")]
public class ItineraryController(IItineraryService itineraryService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ItineraryResponse>> CreateItinerary([FromBody] CreateItineraryRequest request, CancellationToken cancellationToken)
    {
        var itinerary = await itineraryService.CreateItineraryAsync(new Itinerary
        {
            StartDate = request.StartDate,
            Duration = request.Duration
        },
        request.Items.Select(x => new ItineraryItem
        {
            DayNumber = x.DayNumber,
            ProductId = x.ProductId,
            Quantity = x.Quantity,
            Notes = x.Notes
        }), cancellationToken);

        return CreatedAtAction(nameof(GetItinerary), new { itineraryId = itinerary.Id }, itinerary.ToResponse());
    }

    [HttpGet("{itineraryId:guid}")]
    public async Task<ActionResult<ItineraryResponse>> GetItinerary(Guid itineraryId, CancellationToken cancellationToken)
    {
        var itinerary = await itineraryService.GetItineraryAsync(itineraryId, cancellationToken);
        return itinerary is null ? NotFound() : Ok(itinerary.ToResponse());
    }
}
