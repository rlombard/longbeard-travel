using System.Globalization;
using System.Text.RegularExpressions;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Models;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Services;

public class IngestionService(
    ISupplierRepository supplierRepository,
    ISupplierService supplierService,
    IProductService productService,
    IRateService rateService) : IIngestionService
{
    private static readonly Regex PropertyRoomMetaRegex = new(
        "propertyName:'(?<property>(?:\\.|[^'])*)',roomNames:'(?<roomNames>(?:\\.|[^'])*)',rateTypes:'(?<rateTypes>(?:\\.|[^'])*)',rateBases:'(?<rateBases>(?:\\.|[^'])*)',mealBases:'(?<mealBases>(?:\\.|[^'])*)',validityPeriods:'(?<validityPeriods>(?:\\.|[^'])*)'",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public async Task<Rate> ProcessRatePayloadAsync(IngestionRatePayload payload, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(payload.Currency))
        {
            throw new InvalidOperationException("Currency is required.");
        }

        var rate = new Rate
        {
            ProductId = payload.ProductId,
            SeasonStart = payload.SeasonStart,
            SeasonEnd = payload.SeasonEnd,
            PricingModel = payload.PricingModel,
            BaseCost = payload.BaseCost,
            Currency = payload.Currency,
            MinPax = payload.MinPax,
            MaxPax = payload.MaxPax,
            ChildDiscount = payload.ChildDiscount,
            SingleSupplement = payload.SingleSupplement,
            Capacity = payload.Capacity
        };

        return await rateService.CreateRateAsync(rate, cancellationToken);
    }

    public async Task<IngestionPropertyBundleResult> ProcessPropertyBundleAsync(IngestionPropertyBundlePayload payload, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(payload.Owner))
        {
            throw new ArgumentException("Owner is required.");
        }

        if (payload.PropertyDetails.Count == 0)
        {
            throw new ArgumentException("At least one property detail entry is required.");
        }

        var supplier = await GetOrCreateSupplierAsync(payload.Owner, cancellationToken);
        var roomMetaByProperty = ParsePropertyRoomMetadata(payload.PropertyRoomMeta);
        var roomPricingByProperty = payload.RoomPricingDetails
            .GroupBy(x => NormalizeKey(x.PropertyName), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.Last(), StringComparer.OrdinalIgnoreCase);
        var contentByProperty = payload.PropertyContentDetails
            .GroupBy(x => NormalizeKey(x.PropertyName), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.Last(), StringComparer.OrdinalIgnoreCase);

        var result = new IngestionPropertyBundleResult
        {
            SupplierId = supplier.Id,
            SupplierName = supplier.Name
        };

        foreach (var propertyDetail in payload.PropertyDetails)
        {
            var propertyKey = NormalizeKey(propertyDetail.Name);
            if (string.IsNullOrWhiteSpace(propertyKey))
            {
                throw new ArgumentException("Property name is required.");
            }

            roomPricingByProperty.TryGetValue(propertyKey, out var roomPricing);
            contentByProperty.TryGetValue(propertyKey, out var propertyContent);
            roomMetaByProperty.TryGetValue(propertyKey, out var propertyMetaSegments);
            propertyMetaSegments ??= [];

            var productRooms = BuildProductRooms(roomPricing, propertyMetaSegments);
            var product = new Product
            {
                SupplierId = supplier.Id,
                Name = propertyDetail.Name.Trim(),
                Type = ProductType.Hotel,
                ContractValidityPeriod = NullIfWhitespace(propertyDetail.ValidityPeriod),
                Commission = NullIfWhitespace(propertyDetail.Commission),
                PhysicalStreetAddress = NullIfWhitespace(propertyDetail.PhysicalAddress.StreetAddress),
                PhysicalSuburb = NullIfWhitespace(propertyDetail.PhysicalAddress.Suburb),
                PhysicalTownOrCity = NullIfWhitespace(propertyDetail.PhysicalAddress.TownOrCity),
                PhysicalStateOrProvince = NullIfWhitespace(propertyDetail.PhysicalAddress.StateOrProvince),
                PhysicalCountry = NullIfWhitespace(propertyDetail.PhysicalAddress.Country),
                PhysicalPostCode = NullIfWhitespace(propertyDetail.PhysicalAddress.PostCode),
                MailingStreetAddress = NullIfWhitespace(propertyDetail.MailingAddress.StreetAddress),
                MailingSuburb = NullIfWhitespace(propertyDetail.MailingAddress.Suburb),
                MailingTownOrCity = NullIfWhitespace(propertyDetail.MailingAddress.TownOrCity),
                MailingStateOrProvince = NullIfWhitespace(propertyDetail.MailingAddress.StateOrProvince),
                MailingCountry = NullIfWhitespace(propertyDetail.MailingAddress.Country),
                MailingPostCode = NullIfWhitespace(propertyDetail.MailingAddress.PostCode),
                CheckInTime = NullIfWhitespace(propertyDetail.CheckInTime),
                CheckOutTime = NullIfWhitespace(propertyDetail.CheckOutTime),
                BlockOutDates = NullIfWhitespace(propertyDetail.BlockOutDates),
                TourismLevyAmount = NullIfWhitespace(propertyDetail.TourismLevy.Amount),
                TourismLevyCurrency = NullIfWhitespace(propertyDetail.TourismLevy.Currency),
                TourismLevyUnit = NullIfWhitespace(propertyDetail.TourismLevy.Unit),
                TourismLevyAgeApplicability = NullIfWhitespace(propertyDetail.TourismLevy.AgeApplicability),
                TourismLevyEffectiveDates = NullIfWhitespace(propertyDetail.TourismLevy.EffectiveDates),
                TourismLevyConditions = NullIfWhitespace(propertyDetail.TourismLevy.Conditions),
                TourismLevyRawText = NullIfWhitespace(propertyDetail.TourismLevy.RawText),
                TourismLevyIncluded = propertyDetail.TourismLevyIncluded,
                RoomPolicies = NullIfWhitespace(propertyContent?.Policies.RoomPolicies),
                RatePolicies = NullIfWhitespace(propertyContent?.Policies.RatePolicies),
                ChildPolicies = NullIfWhitespace(propertyContent?.Policies.ChildPolicies),
                CancellationPolicies = NullIfWhitespace(propertyContent?.Policies.CancellationPolicies),
                Inclusions = NullIfWhitespace(propertyContent?.Policies.Inclusions),
                Exclusions = NullIfWhitespace(propertyContent?.Policies.Exclusions),
                Specials = NullIfWhitespace(propertyContent?.Specials),
                Contacts = propertyDetail.Contacts.Select(x => new ProductContact
                {
                    ContactType = x.ContactType,
                    ContactName = x.ContactName,
                    ContactEmail = x.ContactEmail,
                    ContactPhoneNumber = x.ContactPhoneNumber
                }).ToList(),
                Extras = propertyContent?.Extras.Select(x => new ProductExtra
                {
                    Description = x.Description,
                    ChargeUnit = x.ChargeUnit,
                    Charge = x.Charge
                }).ToList() ?? [],
                Rooms = productRooms,
                RateTypes = BuildRateTypes(roomPricing, propertyMetaSegments),
                RateBases = BuildRateBases(roomPricing, propertyMetaSegments),
                MealBases = BuildMealBases(roomPricing, propertyMetaSegments),
                ValidityPeriods = BuildValidityPeriods(propertyDetail.ValidityPeriod, roomPricing, propertyMetaSegments)
            };

            var createdProduct = await productService.CreateProductAsync(product, cancellationToken);
            var createdRoomsByName = createdProduct.Rooms.ToDictionary(x => NormalizeKey(x.Name), x => x, StringComparer.OrdinalIgnoreCase);
            var createdRates = 0;

            if (roomPricing is not null)
            {
                foreach (var room in roomPricing.Rooms)
                {
                    if (!createdRoomsByName.TryGetValue(NormalizeKey(room.Name), out var createdRoom))
                    {
                        continue;
                    }

                    foreach (var availability in room.RateAvailability)
                    {
                        var seasonRange = ParseDateRange(availability.ValidityPeriod, propertyDetail.ValidityPeriod);

                        foreach (var roomRate in availability.Rates)
                        {
                            var pricingModel = DeterminePricingModel(roomRate.RateBasis, roomRate.OccupancyType, roomRate.RateTypeName);
                            var maxPax = ParseUpperBound(room.MaximumOccupancy);

                            await rateService.CreateRateAsync(new Rate
                            {
                                ProductId = createdProduct.Id,
                                ProductRoomId = createdRoom.Id,
                                SeasonStart = seasonRange.start,
                                SeasonEnd = seasonRange.end,
                                PricingModel = pricingModel,
                                BaseCost = roomRate.RateValue,
                                Currency = NormalizeCurrency(propertyDetail.Currency),
                                MinPax = ParseLowerBound(room.MinimumOccupancy),
                                MaxPax = maxPax,
                                Capacity = pricingModel == PricingModel.PerUnit ? maxPax : null,
                                ValidityPeriod = NullIfWhitespace(availability.ValidityPeriod),
                                ValidityPeriodDescription = NullIfWhitespace(availability.ValidityPeriodDescription),
                                RateVariation = NullIfWhitespace(roomRate.RateVariation),
                                RateTypeName = NullIfWhitespace(roomRate.RateTypeName),
                                RateBasis = NullIfWhitespace(roomRate.RateBasis),
                                OccupancyType = NullIfWhitespace(roomRate.OccupancyType),
                                MealBasis = NullIfWhitespace(roomRate.MealBasis),
                                MinimumStay = NullIfWhitespace(roomRate.MinimumStay)
                            }, cancellationToken);

                            createdRates++;
                        }
                    }
                }
            }

            result.Products.Add(new IngestionProductResult
            {
                ProductId = createdProduct.Id,
                ProductName = createdProduct.Name,
                RoomCount = createdProduct.Rooms.Count,
                RateCount = createdRates,
                RateTypeCount = createdProduct.RateTypes.Count,
                RateBasisCount = createdProduct.RateBases.Count,
                MealBasisCount = createdProduct.MealBases.Count,
                ValidityPeriodCount = createdProduct.ValidityPeriods.Count
            });
        }

        result.ProductCount = result.Products.Count;
        result.RoomCount = result.Products.Sum(x => x.RoomCount);
        result.RateCount = result.Products.Sum(x => x.RateCount);
        return result;
    }

    private async Task<Supplier> GetOrCreateSupplierAsync(string owner, CancellationToken cancellationToken)
    {
        var normalizedOwner = owner.Trim();
        var existingSupplier = await supplierRepository.GetByNameAsync(normalizedOwner, cancellationToken);
        if (existingSupplier is not null)
        {
            return existingSupplier;
        }

        return await supplierService.CreateSupplierAsync(new Supplier
        {
            Name = normalizedOwner
        }, cancellationToken);
    }

    private static List<ProductRoom> BuildProductRooms(IngestionRoomPricingPayload? roomPricing, IReadOnlyList<PropertyRoomMetaSegment> propertyMetaSegments)
    {
        var rooms = new List<ProductRoom>();
        var roomNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (roomPricing is not null)
        {
            foreach (var room in roomPricing.Rooms)
            {
                var normalizedRoomName = NormalizeKey(room.Name);
                if (string.IsNullOrWhiteSpace(normalizedRoomName) || !roomNames.Add(normalizedRoomName))
                {
                    continue;
                }

                rooms.Add(new ProductRoom
                {
                    Name = room.Name,
                    MinimumOccupancy = NullIfWhitespace(room.MinimumOccupancy),
                    MaximumOccupancy = NullIfWhitespace(room.MaximumOccupancy),
                    AdditionalNotes = NullIfWhitespace(room.AdditionalNotes),
                    RateConditions = NullIfWhitespace(room.RateConditions)
                });
            }
        }

        foreach (var roomName in propertyMetaSegments.SelectMany(x => x.RoomNames))
        {
            var normalizedRoomName = NormalizeKey(roomName);
            if (string.IsNullOrWhiteSpace(normalizedRoomName) || !roomNames.Add(normalizedRoomName))
            {
                continue;
            }

            rooms.Add(new ProductRoom
            {
                Name = roomName
            });
        }

        return rooms;
    }

    private static List<ProductRateType> BuildRateTypes(IngestionRoomPricingPayload? roomPricing, IReadOnlyList<PropertyRoomMetaSegment> propertyMetaSegments) =>
        CollectDistinctValues(
            roomPricing?.Rooms.SelectMany(x => x.RateAvailability).SelectMany(x => x.Rates).Select(x => x.RateTypeName) ?? [],
            propertyMetaSegments.SelectMany(x => x.RateTypes),
            value => new ProductRateType { Name = value });

    private static List<ProductRateBasis> BuildRateBases(IngestionRoomPricingPayload? roomPricing, IReadOnlyList<PropertyRoomMetaSegment> propertyMetaSegments) =>
        CollectDistinctValues(
            roomPricing?.Rooms.SelectMany(x => x.RateAvailability).SelectMany(x => x.Rates).Select(x => x.RateBasis) ?? [],
            propertyMetaSegments.SelectMany(x => x.RateBases),
            value => new ProductRateBasis { Name = value });

    private static List<ProductMealBasis> BuildMealBases(IngestionRoomPricingPayload? roomPricing, IReadOnlyList<PropertyRoomMetaSegment> propertyMetaSegments) =>
        CollectDistinctValues(
            roomPricing?.Rooms.SelectMany(x => x.RateAvailability).SelectMany(x => x.Rates).Select(x => x.MealBasis) ?? [],
            propertyMetaSegments.SelectMany(x => x.MealBases),
            value => new ProductMealBasis { Name = value });

    private static List<ProductValidityPeriod> BuildValidityPeriods(string? contractValidityPeriod, IngestionRoomPricingPayload? roomPricing, IReadOnlyList<PropertyRoomMetaSegment> propertyMetaSegments) =>
        CollectDistinctValues(
            roomPricing?.Rooms.SelectMany(x => x.RateAvailability).Select(x => x.ValidityPeriod) ?? [],
            propertyMetaSegments.SelectMany(x => x.ValidityPeriods).Append(contractValidityPeriod ?? string.Empty),
            value => new ProductValidityPeriod { Value = value });

    private static List<TValueEntity> CollectDistinctValues<TValueEntity>(
        IEnumerable<string> first,
        IEnumerable<string> second,
        Func<string, TValueEntity> factory)
    {
        return first
            .Concat(second)
            .Select(NullIfWhitespace)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(value => factory(value!))
            .ToList();
    }

    private static Dictionary<string, List<PropertyRoomMetaSegment>> ParsePropertyRoomMetadata(IEnumerable<string> propertyRoomMeta)
    {
        var metadataByProperty = new Dictionary<string, List<PropertyRoomMetaSegment>>(StringComparer.OrdinalIgnoreCase);

        foreach (var value in propertyRoomMeta)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            foreach (Match match in PropertyRoomMetaRegex.Matches(value))
            {
                var propertyName = NormalizeKey(Unescape(match.Groups["property"].Value));
                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    continue;
                }

                if (!metadataByProperty.TryGetValue(propertyName, out var entries))
                {
                    entries = [];
                    metadataByProperty[propertyName] = entries;
                }

                entries.Add(new PropertyRoomMetaSegment(
                    Unescape(match.Groups["property"].Value),
                    SplitEscapedValues(match.Groups["roomNames"].Value),
                    SplitEscapedValues(match.Groups["rateTypes"].Value),
                    SplitEscapedValues(match.Groups["rateBases"].Value),
                    SplitEscapedValues(match.Groups["mealBases"].Value),
                    SplitEscapedValues(match.Groups["validityPeriods"].Value)));
            }
        }

        return metadataByProperty;
    }

    private static List<string> SplitEscapedValues(string value)
    {
        return Regex.Split(value, @"(?<!\\),")
            .Select(Unescape)
            .Select(NullIfWhitespace)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .ToList();
    }

    private static (DateOnly start, DateOnly end) ParseDateRange(string? primaryValue, string? fallbackValue)
    {
        if (TryParseDateRange(primaryValue, out var dateRange))
        {
            return dateRange;
        }

        if (TryParseDateRange(fallbackValue, out dateRange))
        {
            return dateRange;
        }

        throw new InvalidOperationException($"Unable to parse validity period '{primaryValue ?? fallbackValue}'.");
    }

    private static bool TryParseDateRange(string? value, out (DateOnly start, DateOnly end) dateRange)
    {
        dateRange = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var matches = Regex.Matches(value, @"\b\d{2}/\d{2}/\d{4}\b");
        if (matches.Count < 2)
        {
            return false;
        }

        if (!DateOnly.TryParseExact(matches[0].Value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start))
        {
            return false;
        }

        if (!DateOnly.TryParseExact(matches[1].Value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var end))
        {
            return false;
        }

        dateRange = (start, end);
        return true;
    }

    private static PricingModel DeterminePricingModel(string? rateBasis, string? occupancyType, string? rateTypeName)
    {
        var combined = string.Join(' ', new[] { rateBasis, occupancyType, rateTypeName }
            .Where(x => !string.IsNullOrWhiteSpace(x)))
            .ToLowerInvariant();

        if (combined.Contains("person") || combined.Contains("per person") || combined.Contains("pp"))
        {
            return PricingModel.PerPerson;
        }

        if (combined.Contains("group") || combined.Contains("per group"))
        {
            return PricingModel.PerGroup;
        }

        return PricingModel.PerUnit;
    }

    private static int? ParseLowerBound(string? value)
    {
        var numbers = ParseNumbers(value);
        return numbers.Count == 0 ? null : numbers.Min();
    }

    private static int? ParseUpperBound(string? value)
    {
        var numbers = ParseNumbers(value);
        return numbers.Count == 0 ? null : numbers.Max();
    }

    private static List<int> ParseNumbers(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : Regex.Matches(value, @"\d+")
                .Select(x => int.Parse(x.Value, CultureInfo.InvariantCulture))
                .ToList();

    private static string NormalizeKey(string? value) => value?.Trim() ?? string.Empty;

    private static string NormalizeCurrency(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("Currency is required.");
        }

        return value.Trim().ToUpperInvariant();
    }

    private static string? NullIfWhitespace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Unescape(string value) => Regex.Replace(value, @"\\(.)", "$1");

    private sealed record PropertyRoomMetaSegment(
        string PropertyName,
        List<string> RoomNames,
        List<string> RateTypes,
        List<string> RateBases,
        List<string> MealBases,
        List<string> ValidityPeriods);
}
