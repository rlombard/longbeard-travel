using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Infrastructure.Data;
using AI.Forged.TourOps.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AI.Forged.TourOps.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IRateRepository, RateRepository>();
        services.AddScoped<IItineraryRepository, ItineraryRepository>();
        services.AddScoped<IQuoteRepository, QuoteRepository>();

        return services;
    }
}
