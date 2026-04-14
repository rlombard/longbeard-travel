using AI.Forged.TourOps.Application.Models.Itineraries;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.Forged.TourOps.Infrastructure.Repositories;

public class ProductRepository(AppDbContext dbContext) : IProductRepository
{
    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await IncludeDetails(dbContext.Products)
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await IncludeDetails(dbContext.Products)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var productIds = ids.Distinct().ToArray();
        if (productIds.Length == 0)
        {
            return [];
        }

        return await IncludePlanningDetails(dbContext.Products)
            .AsNoTracking()
            .Where(x => productIds.Contains(x.Id))
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> SearchForItineraryPlanningAsync(ProductCatalogFilter filter, CancellationToken cancellationToken = default)
    {
        var query = IncludePlanningDetails(dbContext.Products).AsNoTracking();

        if (filter.ProductTypes.Count > 0)
        {
            query = query.Where(x => filter.ProductTypes.Contains(x.Type));
        }

        foreach (var term in filter.LocationTerms.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var pattern = $"%{term}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.Name, pattern) ||
                (x.PhysicalTownOrCity != null && EF.Functions.ILike(x.PhysicalTownOrCity, pattern)) ||
                (x.PhysicalStateOrProvince != null && EF.Functions.ILike(x.PhysicalStateOrProvince, pattern)) ||
                (x.PhysicalCountry != null && EF.Functions.ILike(x.PhysicalCountry, pattern)) ||
                (x.PhysicalSuburb != null && EF.Functions.ILike(x.PhysicalSuburb, pattern)) ||
                EF.Functions.ILike(x.Supplier.Name, pattern));
        }

        var products = await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var searchTerms = filter.SearchTerms
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (searchTerms.Count > 0)
        {
            var matchedProducts = products
                .Where(product => MatchesPlanningSearch(product, searchTerms))
                .ToList();

            if (matchedProducts.Count > 0)
            {
                products = matchedProducts;
            }
        }

        return products
            .Take(filter.MaxResults <= 0 ? 40 : filter.MaxResults)
            .ToList();
    }

    public async Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        var existingProduct = await IncludeDetails(dbContext.Products)
            .FirstOrDefaultAsync(x => x.Id == product.Id, cancellationToken)
            ?? throw new InvalidOperationException("Product not found.");

        dbContext.Entry(existingProduct).CurrentValues.SetValues(product);
        SyncChildren(existingProduct.Contacts, product.Contacts, dbContext.ProductContacts);
        SyncChildren(existingProduct.Extras, product.Extras, dbContext.ProductExtras);
        SyncChildren(existingProduct.Rooms, product.Rooms, dbContext.ProductRooms);
        SyncChildren(existingProduct.RateTypes, product.RateTypes, dbContext.ProductRateTypes);
        SyncChildren(existingProduct.RateBases, product.RateBases, dbContext.ProductRateBases);
        SyncChildren(existingProduct.MealBases, product.MealBases, dbContext.ProductMealBases);
        SyncChildren(existingProduct.ValidityPeriods, product.ValidityPeriods, dbContext.ProductValidityPeriods);

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(product.Id, cancellationToken)
            ?? throw new InvalidOperationException("Product not found after update.");
    }

    private static IQueryable<Product> IncludeDetails(IQueryable<Product> query) =>
        query
            .Include(x => x.Contacts)
            .Include(x => x.Extras)
            .Include(x => x.Rooms)
            .Include(x => x.RateTypes)
            .Include(x => x.RateBases)
            .Include(x => x.MealBases)
            .Include(x => x.ValidityPeriods)
            .AsSplitQuery();

    private static IQueryable<Product> IncludePlanningDetails(IQueryable<Product> query) =>
        IncludeDetails(query)
            .Include(x => x.Supplier)
            .Include(x => x.Rates);

    private static bool MatchesPlanningSearch(Product product, IReadOnlyList<string> searchTerms)
    {
        var searchableValues = new[]
        {
            product.Name,
            product.Inclusions,
            product.Exclusions,
            product.Specials,
            product.Supplier.Name,
            string.Join(' ', product.Extras.Select(x => x.Description)),
            string.Join(' ', product.Rooms.Select(x => x.Name)),
            string.Join(' ', product.RateTypes.Select(x => x.Name)),
            string.Join(' ', product.RateBases.Select(x => x.Name)),
            string.Join(' ', product.MealBases.Select(x => x.Name)),
            string.Join(' ', product.ValidityPeriods.Select(x => x.Value))
        };

        var haystack = string.Join(' ', searchableValues.Where(x => !string.IsNullOrWhiteSpace(x)));
        return searchTerms.Any(term => haystack.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static void SyncChildren<TEntity>(
        ICollection<TEntity> existingItems,
        ICollection<TEntity> incomingItems,
        DbSet<TEntity> dbSet)
        where TEntity : class
    {
        foreach (var existingItem in existingItems.ToList())
        {
            if (!incomingItems.Any(x => dbSet.Entry(x).Property("Id").CurrentValue?.Equals(dbSet.Entry(existingItem).Property("Id").CurrentValue) == true))
            {
                dbSet.Remove(existingItem);
            }
        }

        foreach (var incomingItem in incomingItems)
        {
            var incomingId = dbSet.Entry(incomingItem).Property("Id").CurrentValue;
            var existingItem = existingItems.FirstOrDefault(x => dbSet.Entry(x).Property("Id").CurrentValue?.Equals(incomingId) == true);
            if (existingItem is null)
            {
                existingItems.Add(incomingItem);
                continue;
            }

            dbSet.Entry(existingItem).CurrentValues.SetValues(incomingItem);
        }
    }
}
