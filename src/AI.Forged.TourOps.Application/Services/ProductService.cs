using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;

namespace AI.Forged.TourOps.Application.Services;

public class ProductService(IProductRepository productRepository, ISupplierRepository supplierRepository) : IProductService
{
    public async Task<Product> CreateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        await ValidateAndNormalizeProductAsync(product, cancellationToken);
        product.Id = Guid.NewGuid();
        product.CreatedAt = DateTime.UtcNow;
        AssignChildIdentifiers(product);
        return await productRepository.AddAsync(product, cancellationToken);
    }

    public Task<Product?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default) =>
        productRepository.GetByIdAsync(productId, cancellationToken);

    public Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken cancellationToken = default) =>
        productRepository.GetAllAsync(cancellationToken);

    public async Task<Product> UpdateProductAsync(Guid productId, Product product, CancellationToken cancellationToken = default)
    {
        var existingProduct = await productRepository.GetByIdAsync(productId, cancellationToken);
        if (existingProduct is null)
        {
            throw new InvalidOperationException("Product not found.");
        }

        await ValidateAndNormalizeProductAsync(product, cancellationToken);

        existingProduct.SupplierId = product.SupplierId;
        existingProduct.Name = product.Name;
        existingProduct.Type = product.Type;
        existingProduct.ContractValidityPeriod = product.ContractValidityPeriod;
        existingProduct.Commission = product.Commission;
        existingProduct.PhysicalStreetAddress = product.PhysicalStreetAddress;
        existingProduct.PhysicalSuburb = product.PhysicalSuburb;
        existingProduct.PhysicalTownOrCity = product.PhysicalTownOrCity;
        existingProduct.PhysicalStateOrProvince = product.PhysicalStateOrProvince;
        existingProduct.PhysicalCountry = product.PhysicalCountry;
        existingProduct.PhysicalPostCode = product.PhysicalPostCode;
        existingProduct.MailingStreetAddress = product.MailingStreetAddress;
        existingProduct.MailingSuburb = product.MailingSuburb;
        existingProduct.MailingTownOrCity = product.MailingTownOrCity;
        existingProduct.MailingStateOrProvince = product.MailingStateOrProvince;
        existingProduct.MailingCountry = product.MailingCountry;
        existingProduct.MailingPostCode = product.MailingPostCode;
        existingProduct.CheckInTime = product.CheckInTime;
        existingProduct.CheckOutTime = product.CheckOutTime;
        existingProduct.BlockOutDates = product.BlockOutDates;
        existingProduct.TourismLevyAmount = product.TourismLevyAmount;
        existingProduct.TourismLevyCurrency = product.TourismLevyCurrency;
        existingProduct.TourismLevyUnit = product.TourismLevyUnit;
        existingProduct.TourismLevyAgeApplicability = product.TourismLevyAgeApplicability;
        existingProduct.TourismLevyEffectiveDates = product.TourismLevyEffectiveDates;
        existingProduct.TourismLevyConditions = product.TourismLevyConditions;
        existingProduct.TourismLevyRawText = product.TourismLevyRawText;
        existingProduct.TourismLevyIncluded = product.TourismLevyIncluded;
        existingProduct.RoomPolicies = product.RoomPolicies;
        existingProduct.RatePolicies = product.RatePolicies;
        existingProduct.ChildPolicies = product.ChildPolicies;
        existingProduct.CancellationPolicies = product.CancellationPolicies;
        existingProduct.Inclusions = product.Inclusions;
        existingProduct.Exclusions = product.Exclusions;
        existingProduct.Specials = product.Specials;
        existingProduct.Contacts = product.Contacts;
        existingProduct.Extras = product.Extras;
        existingProduct.Rooms = product.Rooms;
        existingProduct.RateTypes = product.RateTypes;
        existingProduct.RateBases = product.RateBases;
        existingProduct.MealBases = product.MealBases;
        existingProduct.ValidityPeriods = product.ValidityPeriods;

        AssignChildIdentifiers(existingProduct);

        return await productRepository.UpdateAsync(existingProduct, cancellationToken);
    }

    private async Task ValidateAndNormalizeProductAsync(Product product, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            throw new ArgumentException("Product name is required.");
        }

        var supplier = await supplierRepository.GetByIdAsync(product.SupplierId, cancellationToken);
        if (supplier is null)
        {
            throw new InvalidOperationException("Supplier not found.");
        }

        product.Name = product.Name.Trim();
        product.ContractValidityPeriod = NormalizeNullable(product.ContractValidityPeriod);
        product.Commission = NormalizeNullable(product.Commission);
        product.PhysicalStreetAddress = NormalizeNullable(product.PhysicalStreetAddress);
        product.PhysicalSuburb = NormalizeNullable(product.PhysicalSuburb);
        product.PhysicalTownOrCity = NormalizeNullable(product.PhysicalTownOrCity);
        product.PhysicalStateOrProvince = NormalizeNullable(product.PhysicalStateOrProvince);
        product.PhysicalCountry = NormalizeNullable(product.PhysicalCountry);
        product.PhysicalPostCode = NormalizeNullable(product.PhysicalPostCode);
        product.MailingStreetAddress = NormalizeNullable(product.MailingStreetAddress);
        product.MailingSuburb = NormalizeNullable(product.MailingSuburb);
        product.MailingTownOrCity = NormalizeNullable(product.MailingTownOrCity);
        product.MailingStateOrProvince = NormalizeNullable(product.MailingStateOrProvince);
        product.MailingCountry = NormalizeNullable(product.MailingCountry);
        product.MailingPostCode = NormalizeNullable(product.MailingPostCode);
        product.CheckInTime = NormalizeNullable(product.CheckInTime);
        product.CheckOutTime = NormalizeNullable(product.CheckOutTime);
        product.BlockOutDates = NormalizeNullable(product.BlockOutDates);
        product.TourismLevyAmount = NormalizeNullable(product.TourismLevyAmount);
        product.TourismLevyCurrency = NormalizeNullable(product.TourismLevyCurrency);
        product.TourismLevyUnit = NormalizeNullable(product.TourismLevyUnit);
        product.TourismLevyAgeApplicability = NormalizeNullable(product.TourismLevyAgeApplicability);
        product.TourismLevyEffectiveDates = NormalizeNullable(product.TourismLevyEffectiveDates);
        product.TourismLevyConditions = NormalizeNullable(product.TourismLevyConditions);
        product.TourismLevyRawText = NormalizeNullable(product.TourismLevyRawText);
        product.RoomPolicies = NormalizeNullable(product.RoomPolicies);
        product.RatePolicies = NormalizeNullable(product.RatePolicies);
        product.ChildPolicies = NormalizeNullable(product.ChildPolicies);
        product.CancellationPolicies = NormalizeNullable(product.CancellationPolicies);
        product.Inclusions = NormalizeNullable(product.Inclusions);
        product.Exclusions = NormalizeNullable(product.Exclusions);
        product.Specials = NormalizeNullable(product.Specials);

        ValidateContacts(product.Contacts);
        ValidateExtras(product.Extras);
        ValidateRooms(product.Rooms);
        DeduplicateLookupValues(product);
        ValidateLookupValues(product.RateTypes.Select(x => x.Name), "rate type");
        ValidateLookupValues(product.RateBases.Select(x => x.Name), "rate basis");
        ValidateLookupValues(product.MealBases.Select(x => x.Name), "meal basis");
        ValidateLookupValues(product.ValidityPeriods.Select(x => x.Value), "validity period");
    }

    private static void AssignChildIdentifiers(Product product)
    {
        foreach (var contact in product.Contacts)
        {
            if (contact.Id == Guid.Empty)
            {
                contact.Id = Guid.NewGuid();
            }

            contact.ProductId = product.Id;
            contact.ContactType = contact.ContactType.Trim();
            contact.ContactName = contact.ContactName.Trim();
            contact.ContactEmail = contact.ContactEmail.Trim();
            contact.ContactPhoneNumber = contact.ContactPhoneNumber.Trim();
        }

        foreach (var extra in product.Extras)
        {
            if (extra.Id == Guid.Empty)
            {
                extra.Id = Guid.NewGuid();
            }

            extra.ProductId = product.Id;
            extra.Description = extra.Description.Trim();
            extra.ChargeUnit = extra.ChargeUnit.Trim();
            extra.Charge = extra.Charge.Trim();
        }

        foreach (var room in product.Rooms)
        {
            if (room.Id == Guid.Empty)
            {
                room.Id = Guid.NewGuid();
            }

            room.ProductId = product.Id;
            room.Name = room.Name.Trim();
            room.MinimumOccupancy = NormalizeNullable(room.MinimumOccupancy);
            room.MaximumOccupancy = NormalizeNullable(room.MaximumOccupancy);
            room.AdditionalNotes = NormalizeNullable(room.AdditionalNotes);
            room.RateConditions = NormalizeNullable(room.RateConditions);
        }

        foreach (var rateType in product.RateTypes)
        {
            if (rateType.Id == Guid.Empty)
            {
                rateType.Id = Guid.NewGuid();
            }

            rateType.ProductId = product.Id;
            rateType.Name = rateType.Name.Trim();
        }

        foreach (var rateBasis in product.RateBases)
        {
            if (rateBasis.Id == Guid.Empty)
            {
                rateBasis.Id = Guid.NewGuid();
            }

            rateBasis.ProductId = product.Id;
            rateBasis.Name = rateBasis.Name.Trim();
        }

        foreach (var mealBasis in product.MealBases)
        {
            if (mealBasis.Id == Guid.Empty)
            {
                mealBasis.Id = Guid.NewGuid();
            }

            mealBasis.ProductId = product.Id;
            mealBasis.Name = mealBasis.Name.Trim();
        }

        foreach (var validityPeriod in product.ValidityPeriods)
        {
            if (validityPeriod.Id == Guid.Empty)
            {
                validityPeriod.Id = Guid.NewGuid();
            }

            validityPeriod.ProductId = product.Id;
            validityPeriod.Value = validityPeriod.Value.Trim();
        }
    }

    private static void DeduplicateLookupValues(Product product)
    {
        product.RateTypes = DeduplicateByName(product.RateTypes, x => x.Name);
        product.RateBases = DeduplicateByName(product.RateBases, x => x.Name);
        product.MealBases = DeduplicateByName(product.MealBases, x => x.Name);
        product.ValidityPeriods = DeduplicateByName(product.ValidityPeriods, x => x.Value);
    }

    private static List<TItem> DeduplicateByName<TItem>(IEnumerable<TItem> items, Func<TItem, string> selector)
    {
        return items
            .Where(x => !string.IsNullOrWhiteSpace(selector(x)))
            .GroupBy(x => selector(x).Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToList();
    }

    private static string? NormalizeNullable(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ValidateContacts(IEnumerable<ProductContact> contacts)
    {
        foreach (var contact in contacts)
        {
            if (string.IsNullOrWhiteSpace(contact.ContactType))
            {
                throw new ArgumentException("Contact type is required.");
            }

            if (string.IsNullOrWhiteSpace(contact.ContactName))
            {
                throw new ArgumentException("Contact name is required.");
            }

            if (string.IsNullOrWhiteSpace(contact.ContactEmail))
            {
                throw new ArgumentException("Contact email is required.");
            }

            if (string.IsNullOrWhiteSpace(contact.ContactPhoneNumber))
            {
                throw new ArgumentException("Contact phone number is required.");
            }
        }
    }

    private static void ValidateExtras(IEnumerable<ProductExtra> extras)
    {
        foreach (var extra in extras)
        {
            if (string.IsNullOrWhiteSpace(extra.Description))
            {
                throw new ArgumentException("Extra description is required.");
            }

            if (string.IsNullOrWhiteSpace(extra.ChargeUnit))
            {
                throw new ArgumentException("Extra charge unit is required.");
            }

            if (string.IsNullOrWhiteSpace(extra.Charge))
            {
                throw new ArgumentException("Extra charge is required.");
            }
        }
    }

    private static void ValidateRooms(IEnumerable<ProductRoom> rooms)
    {
        foreach (var room in rooms)
        {
            if (string.IsNullOrWhiteSpace(room.Name))
            {
                throw new ArgumentException("Room name is required.");
            }
        }
    }

    private static void ValidateLookupValues(IEnumerable<string> values, string label)
    {
        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"Product {label} is required.");
            }
        }
    }
}
