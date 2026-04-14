using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Ai;
using AI.Forged.TourOps.Application.Interfaces.Email;
using AI.Forged.TourOps.Application.Interfaces.Llm;
using AI.Forged.TourOps.Infrastructure.AiForged;
using AI.Forged.TourOps.Infrastructure.Configuration;
using AI.Forged.TourOps.Infrastructure.Data;
using AI.Forged.TourOps.Infrastructure.Email;
using AI.Forged.TourOps.Infrastructure.Llm;
using AI.Forged.TourOps.Infrastructure.Llm.Providers;
using AI.Forged.TourOps.Infrastructure.Pdf;
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

        services.Configure<AiForgedSettings>(configuration.GetSection("AiForgedSettings"));
        services.Configure<LlmSettings>(configuration.GetSection("LlmSettings"));
        services.Configure<OpenAiSettings>(configuration.GetSection("OpenAiSettings"));
        services.Configure<AzureOpenAiSettings>(configuration.GetSection("AzureOpenAiSettings"));
        services.Configure<AnthropicSettings>(configuration.GetSection("AnthropicSettings"));
        services.Configure<EmailWorkflowSettings>(configuration.GetSection("EmailWorkflow"));

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddHttpClient<OpenAiLlmProviderService>();
        services.AddHttpClient<AzureOpenAiLlmProviderService>();
        services.AddHttpClient<AnthropicLlmProviderService>();

        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IRateRepository, RateRepository>();
        services.AddScoped<IItineraryRepository, ItineraryRepository>();
        services.AddScoped<IQuoteRepository, QuoteRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IBookingItemRepository, BookingItemRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ITaskSuggestionRepository, TaskSuggestionRepository>();
        services.AddScoped<IEmailRepository, EmailRepository>();
        services.AddScoped<ILlmAuditLogRepository, LlmAuditLogRepository>();
        services.AddScoped<IHumanApprovalRepository, HumanApprovalRepository>();
        services.AddScoped<IPdfService, QuestPdfService>();
        services.AddScoped<IAiForgedService, AiForgedStubService>();
        services.AddScoped<ILlmProviderService, DeterministicLlmProviderService>();
        services.AddScoped<ILlmProviderService, OpenAiLlmProviderService>();
        services.AddScoped<ILlmProviderService, AzureOpenAiLlmProviderService>();
        services.AddScoped<ILlmProviderService, AnthropicLlmProviderService>();
        services.AddScoped<ILlmProviderResolver, LlmProviderResolver>();
        services.AddScoped<IGenericLlmService, GenericLlmService>();
        services.AddScoped<IEmailProviderService, LogOnlyEmailProviderService>();

        return services;
    }
}
