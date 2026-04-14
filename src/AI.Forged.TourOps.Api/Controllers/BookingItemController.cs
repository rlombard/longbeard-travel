using AI.Forged.TourOps.Api.Models;
using AI.Forged.TourOps.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AI.Forged.TourOps.Api.Controllers;

[ApiController]
[Route("api/booking-items")]
public class BookingItemController(IBookingItemService bookingItemService) : ControllerBase
{
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<BookingItemResponse>> UpdateBookingItemStatus(Guid id, [FromBody] UpdateBookingItemStatusRequest request, CancellationToken cancellationToken)
    {
        var bookingItem = await bookingItemService.UpdateBookingItemStatusAsync(id, request.Status, cancellationToken);
        return Ok(bookingItem.ToResponse());
    }

    [HttpPatch("{id:guid}/note")]
    public async Task<ActionResult<BookingItemResponse>> UpdateBookingItemNote(Guid id, [FromBody] UpdateBookingItemNoteRequest request, CancellationToken cancellationToken)
    {
        var bookingItem = await bookingItemService.AddNoteAsync(id, request.Note, cancellationToken);
        return Ok(bookingItem.ToResponse());
    }
}
