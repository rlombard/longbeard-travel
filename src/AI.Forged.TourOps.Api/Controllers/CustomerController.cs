using AI.Forged.TourOps.Api.Models;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Models.Customers;
using Microsoft.AspNetCore.Mvc;

namespace AI.Forged.TourOps.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomerController(ICustomerService customerService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> CreateCustomer([FromBody] CustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await customerService.CreateCustomerAsync(request.ToCreateModel(), cancellationToken);
        return CreatedAtAction(nameof(GetCustomer), new { customerId = customer.Id }, customer.ToResponse());
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerListItemResponse>>> SearchCustomers(
        [FromQuery] string? searchTerm,
        [FromQuery] string? countryOfResidence,
        [FromQuery] string? nationality,
        CancellationToken cancellationToken)
    {
        var customers = await customerService.SearchCustomersAsync(new CustomerSearchQueryModel
        {
            SearchTerm = searchTerm,
            CountryOfResidence = countryOfResidence,
            Nationality = nationality
        }, cancellationToken);

        return Ok(customers.Select(x => x.ToResponse()).ToList());
    }

    [HttpGet("{customerId:guid}")]
    public async Task<ActionResult<CustomerResponse>> GetCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await customerService.GetCustomerAsync(customerId, cancellationToken);
        return customer is null ? NotFound() : Ok(customer.ToResponse());
    }

    [HttpPut("{customerId:guid}")]
    public async Task<ActionResult<CustomerResponse>> UpdateCustomer(Guid customerId, [FromBody] CustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await customerService.UpdateCustomerAsync(customerId, request.ToUpdateModel(), cancellationToken);
        return Ok(customer.ToResponse());
    }

    [HttpPut("{customerId:guid}/kyc")]
    public async Task<ActionResult<CustomerResponse>> UpdateKyc(Guid customerId, [FromBody] CustomerKycRequest request, CancellationToken cancellationToken)
    {
        var customer = await customerService.UpdateKycAsync(customerId, request.ToModel(), cancellationToken);
        return Ok(customer.ToResponse());
    }

    [HttpPut("{customerId:guid}/preferences")]
    public async Task<ActionResult<CustomerResponse>> UpdatePreferences(Guid customerId, [FromBody] CustomerPreferenceRequest request, CancellationToken cancellationToken)
    {
        var customer = await customerService.UpdatePreferencesAsync(customerId, request.ToModel(), cancellationToken);
        return Ok(customer.ToResponse());
    }

    [HttpPost("{customerId:guid}/quotes/{quoteId:guid}")]
    public async Task<ActionResult<CustomerLinkResponse>> AttachToQuote(Guid customerId, Guid quoteId, CancellationToken cancellationToken)
    {
        var result = await customerService.AttachCustomerToQuoteAsync(customerId, quoteId, cancellationToken);
        return Ok(result.ToResponse());
    }

    [HttpPost("{customerId:guid}/itineraries/{itineraryId:guid}")]
    public async Task<ActionResult<CustomerLinkResponse>> AttachToItinerary(Guid customerId, Guid itineraryId, CancellationToken cancellationToken)
    {
        var result = await customerService.AttachCustomerToItineraryAsync(customerId, itineraryId, cancellationToken);
        return Ok(result.ToResponse());
    }

    [HttpPost("{customerId:guid}/bookings/{bookingId:guid}")]
    public async Task<ActionResult<CustomerLinkResponse>> AttachToBooking(Guid customerId, Guid bookingId, CancellationToken cancellationToken)
    {
        var result = await customerService.AttachCustomerToBookingAsync(customerId, bookingId, cancellationToken);
        return Ok(result.ToResponse());
    }

    [HttpPut("{customerId:guid}/bookings/{bookingId:guid}/traveller")]
    public async Task<ActionResult<CustomerLinkResponse>> UpsertBookingTraveller(Guid customerId, Guid bookingId, [FromBody] BookingTravellerRequest request, CancellationToken cancellationToken)
    {
        var result = await customerService.AddTravellerToBookingAsync(customerId, bookingId, request.ToModel(), cancellationToken);
        return Ok(result.ToResponse());
    }

    [HttpDelete("{customerId:guid}/bookings/{bookingId:guid}/traveller")]
    public async Task<IActionResult> RemoveBookingTraveller(Guid customerId, Guid bookingId, CancellationToken cancellationToken)
    {
        await customerService.RemoveTravellerFromBookingAsync(customerId, bookingId, cancellationToken);
        return NoContent();
    }
}
