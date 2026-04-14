using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Ai;
using AI.Forged.TourOps.Application.Interfaces.Email;
using AI.Forged.TourOps.Application.Interfaces.Operations;
using AI.Forged.TourOps.Application.Interfaces.Tasks;
using AI.Forged.TourOps.Application.Services;
using AI.Forged.TourOps.Application.Services.Ai;
using AI.Forged.TourOps.Application.Services.Email;
using AI.Forged.TourOps.Application.Services.Operations;
using AI.Forged.TourOps.Application.Services.Tasks;
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
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IBookingItemService, BookingItemService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IBookingAiService, BookingAiService>();
        services.AddScoped<IBookingTaskSuggestionService, BookingTaskSuggestionService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailAiService, EmailAiService>();
        services.AddScoped<IEmailAnalysisService>(provider => provider.GetRequiredService<IEmailAiService>() as IEmailAnalysisService
            ?? throw new InvalidOperationException("Email AI service registration is invalid."));
        services.AddScoped<ICommunicationContextService, CommunicationContextService>();
        services.AddScoped<IOperationalDecisionSupportService, OperationalDecisionSupportService>();
        services.AddScoped<IHumanApprovalService, HumanApprovalService>();
        services.AddScoped<IIngestionService, IngestionService>();

        return services;
    }
}
