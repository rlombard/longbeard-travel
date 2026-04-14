using AI.Forged.TourOps.Api.Models;
using AI.Forged.TourOps.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AI.Forged.TourOps.Api.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingController(IBookingService bookingService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookingListItemResponse>>> GetBookings(CancellationToken cancellationToken)
    {
        var bookings = await bookingService.GetBookingsAsync(cancellationToken);
        return Ok(bookings.Select(x => x.ToListItemResponse()).ToList());
    }

    [HttpPost("from-quote")]
    public async Task<ActionResult<BookingResponse>> CreateFromQuote([FromBody] CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var booking = await bookingService.CreateBookingFromQuoteAsync(request.QuoteId, cancellationToken);
        return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking.ToResponse());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingResponse>> GetBooking(Guid id, CancellationToken cancellationToken)
    {
        var booking = await bookingService.GetBookingAsync(id, cancellationToken);
        return booking is null ? NotFound() : Ok(booking.ToResponse());
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<BookingResponse>> UpdateBookingStatus(Guid id, [FromBody] UpdateBookingStatusRequest request, CancellationToken cancellationToken)
    {
        var booking = await bookingService.UpdateBookingStatusAsync(id, request.Status, cancellationToken);
        return Ok(booking.ToResponse());
    }
}
