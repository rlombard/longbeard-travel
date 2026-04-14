using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Models.Itineraries;
using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Services;

public class ItineraryService(IItineraryRepository itineraryRepository) : IItineraryService
{
    public async Task<ItineraryModel> CreateItineraryAsync(CreateItineraryModel request, CancellationToken cancellationToken = default)
    {
        if (request.Duration <= 0)
        {
            throw new ArgumentException("Duration must be greater than zero.");
        }

        var itineraryItems = request.Items.ToList();
        if (itineraryItems.Count == 0)
        {
            throw new ArgumentException("At least one itinerary item is required.");
        }

        var itinerary = new Itinerary
        {
            Id = Guid.NewGuid(),
            StartDate = request.StartDate,
            Duration = request.Duration,
            CreatedAt = DateTime.UtcNow
        };

        var entities = itineraryItems.Select(item =>
        {
            if (item.DayNumber <= 0 || item.DayNumber > request.Duration)
            {
                throw new ArgumentException("Item day number must fall within the itinerary duration.");
            }

            if (item.ProductId == Guid.Empty)
            {
                throw new ArgumentException("Product id is required.");
            }

            if (item.Quantity <= 0)
            {
                throw new ArgumentException("Item quantity must be greater than zero.");
            }

            return new ItineraryItem
            {
                Id = Guid.NewGuid(),
                ItineraryId = itinerary.Id,
                DayNumber = item.DayNumber,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Notes = NormalizeOptional(item.Notes)
            };
        }).ToList();

        var created = await itineraryRepository.AddAsync(itinerary, entities, cancellationToken);
        return Map(created);
    }

    public async Task<ItineraryModel?> GetItineraryAsync(Guid itineraryId, CancellationToken cancellationToken = default)
    {
        var itinerary = await itineraryRepository.GetByIdAsync(itineraryId, cancellationToken);
        return itinerary is null ? null : Map(itinerary);
    }

    private static ItineraryModel Map(Itinerary itinerary) => new()
    {
        Id = itinerary.Id,
        LeadCustomerId = itinerary.LeadCustomerId,
        LeadCustomerName = itinerary.LeadCustomer is null ? null : $"{itinerary.LeadCustomer.FirstName} {itinerary.LeadCustomer.LastName}".Trim(),
        StartDate = itinerary.StartDate,
        Duration = itinerary.Duration,
        CreatedAt = itinerary.CreatedAt,
        Items = itinerary.Items
            .OrderBy(x => x.DayNumber)
            .ThenBy(x => x.Id)
            .Select(item => new ItineraryItemModel
            {
                Id = item.Id,
                DayNumber = item.DayNumber,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Notes = item.Notes
            })
            .ToList()
    };

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
