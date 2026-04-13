using AI.Forged.TourOps.Api.Middleware;
using AI.Forged.TourOps.Application;
using AI.Forged.TourOps.Infrastructure;
using AI.Forged.TourOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

await ApplyMigrationsWithRetryAsync(app);

await app.RunAsync();

static async Task ApplyMigrationsWithRetryAsync(WebApplication app)
{
    const int maxAttempts = 10;
    var delay = TimeSpan.FromSeconds(3);

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            app.Logger.LogWarning(
                ex,
                "Database migration failed on attempt {Attempt}/{MaxAttempts}. Retrying in {DelaySeconds}s.",
                attempt,
                maxAttempts,
                delay.TotalSeconds);

            await Task.Delay(delay);
        }
    }

    using var finalScope = app.Services.CreateScope();
    var finalDbContext = finalScope.ServiceProvider.GetRequiredService<AppDbContext>();
    await finalDbContext.Database.MigrateAsync();
}
