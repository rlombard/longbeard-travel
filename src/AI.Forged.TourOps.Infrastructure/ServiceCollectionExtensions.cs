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
using Microsoft.Extensions.Options;

namespace AI.Forged.TourOps.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        services.Configure<AiForgedSettings>(configuration.GetSection("AiForgedSettings"));
        services.Configure<LlmSettings>(configuration.GetSection("LlmSettings"));
        services.Configure<KeycloakAdminSettings>(configuration.GetSection("KeycloakAdmin"));
        services.Configure<OpenAiSettings>(configuration.GetSection("OpenAiSettings"));
        services.Configure<AzureOpenAiSettings>(configuration.GetSection("AzureOpenAiSettings"));
        services.Configure<AnthropicSettings>(configuration.GetSection("AnthropicSettings"));
        services.Configure<GrokSettings>(configuration.GetSection("GrokSettings"));
        services.Configure<EmailWorkflowSettings>(configuration.GetSection("EmailWorkflow"));
        services.AddOptions<EmailIntegrationSettings>()
            .Bind(configuration.GetSection("EmailIntegration"))
            .ValidateDataAnnotations()
            .Validate(
                settings => !settings.Enabled || IsValidBase64Key(settings.EncryptionKey),
                "EmailIntegration:EncryptionKey must be a valid base64-encoded 32-byte key when email integrations are enabled.")
            .ValidateOnStart();
        services.AddOptions<MicrosoftEmailProviderSettings>()
            .Bind(configuration.GetSection("EmailProviders:Microsoft365"))
            .Validate(
                settings => !settings.Enabled || (!string.IsNullOrWhiteSpace(settings.ClientId) && !string.IsNullOrWhiteSpace(settings.ClientSecret) && !string.IsNullOrWhiteSpace(settings.RedirectUri)),
                "EmailProviders:Microsoft365 requires ClientId, ClientSecret, and RedirectUri when enabled.")
            .ValidateOnStart();
        services.AddOptions<GoogleEmailProviderSettings>()
            .Bind(configuration.GetSection("EmailProviders:Gmail"))
            .Validate(
                settings => !settings.Enabled || (!string.IsNullOrWhiteSpace(settings.ClientId) && !string.IsNullOrWhiteSpace(settings.ClientSecret) && !string.IsNullOrWhiteSpace(settings.RedirectUri)),
                "EmailProviders:Gmail requires ClientId, ClientSecret, and RedirectUri when enabled.")
            .ValidateOnStart();
        services.AddOptions<SendGridEmailProviderSettings>()
            .Bind(configuration.GetSection("EmailProviders:SendGrid"))
            .ValidateOnStart();

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<DemoCatalogSeeder>();
        services.AddHttpClient<IKeycloakAdminRepository, KeycloakAdminRepository>();
        services.AddHttpClient<OpenAiLlmProviderService>();
        services.AddHttpClient<AzureOpenAiLlmProviderService>();
        services.AddHttpClient<AnthropicLlmProviderService>();
        services.AddHttpClient<GrokLlmProviderService>();
        services.AddHttpClient<MicrosoftGraphEmailIntegrationProvider>();
        services.AddHttpClient<GoogleGmailEmailIntegrationProvider>();
        services.AddHttpClient<SendGridEmailIntegrationProvider>();

        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IRateRepository, RateRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IItineraryRepository, ItineraryRepository>();
        services.AddScoped<IQuoteRepository, QuoteRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IBookingItemRepository, BookingItemRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ITaskSuggestionRepository, TaskSuggestionRepository>();
        services.AddScoped<IEmailRepository, EmailRepository>();
        services.AddScoped<IEmailIntegrationRepository, EmailIntegrationRepository>();
        services.AddScoped<ILlmAuditLogRepository, LlmAuditLogRepository>();
        services.AddScoped<IHumanApprovalRepository, HumanApprovalRepository>();
        services.AddScoped<IPdfService, QuestPdfService>();
        services.AddScoped<IAiForgedService, AiForgedStubService>();
        services.AddScoped<ILlmProviderService, DeterministicLlmProviderService>();
        services.AddScoped<ILlmProviderService, OpenAiLlmProviderService>();
        services.AddScoped<ILlmProviderService, AzureOpenAiLlmProviderService>();
        services.AddScoped<ILlmProviderService, AnthropicLlmProviderService>();
        services.AddScoped<ILlmProviderService, GrokLlmProviderService>();
        services.AddScoped<ILlmProviderResolver, LlmProviderResolver>();
        services.AddScoped<IGenericLlmService, GenericLlmService>();
        services.AddScoped<IEmailConnectionSecretProtector, AesEmailConnectionSecretProtector>();
        services.AddScoped<IEmailIntegrationProvider, MicrosoftGraphEmailIntegrationProvider>();
        services.AddScoped<IEmailIntegrationProvider, GoogleGmailEmailIntegrationProvider>();
        services.AddScoped<IEmailIntegrationProvider, MailcowEmailIntegrationProvider>();
        services.AddScoped<IEmailIntegrationProvider, GenericImapSmtpEmailIntegrationProvider>();
        services.AddScoped<IEmailIntegrationProvider, SmtpDirectEmailIntegrationProvider>();
        services.AddScoped<IEmailIntegrationProvider, SendGridEmailIntegrationProvider>();
        services.AddScoped<IEmailIntegrationProviderResolver, EmailIntegrationProviderResolver>();
        services.AddScoped<IEmailProviderService, ConnectedEmailProviderService>();
        services.AddHostedService<EmailIntegrationSyncWorker>();

        return services;
    }

    private static bool IsValidBase64Key(string value)
    {
        try
        {
            return Convert.FromBase64String(value).Length == 32;
        }
        catch
        {
            return false;
        }
    }
}
