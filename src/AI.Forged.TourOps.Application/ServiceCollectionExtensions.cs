using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AI.Forged.TourOps.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IRateService, RateService>();
        services.AddScoped<IItineraryService, ItineraryService>();
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<IQuoteService, QuoteService>();
        services.AddScoped<IIngestionService, IngestionService>();

        return services;
    }
}
