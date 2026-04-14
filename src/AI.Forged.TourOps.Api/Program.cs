using System.IdentityModel.Tokens.Jwt;
using AI.Forged.TourOps.Api.Auth;
using AI.Forged.TourOps.Api.Middleware;
using AI.Forged.TourOps.Application;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Infrastructure;
using AI.Forged.TourOps.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var keycloakSection = builder.Configuration.GetSection("Keycloak");
var keycloakAuthority = keycloakSection["Authority"];
var keycloakPublicAuthority = keycloakSection["PublicAuthority"] ?? keycloakAuthority;
var keycloakAudience = keycloakSection["Audience"];

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, HttpContextCurrentUserContext>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakAuthority;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = !string.IsNullOrWhiteSpace(keycloakAuthority),
            ValidateAudience = !string.IsNullOrWhiteSpace(keycloakAudience),
            ValidAudience = keycloakAudience,
            ValidIssuers = [keycloakAuthority, keycloakPublicAuthority],
            NameClaimType = "sub"
        };

        if (!string.IsNullOrWhiteSpace(keycloakAudience))
        {
            options.TokenValidationParameters.AudienceValidator = (audiences, securityToken, _) =>
            {
                if (audiences.Contains(keycloakAudience, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }

                var azp = securityToken switch
                {
                    JsonWebToken jsonWebToken => jsonWebToken.Claims.FirstOrDefault(x => x.Type == "azp")?.Value,
                    JwtSecurityToken jwtSecurityToken => jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == "azp")?.Value,
                    _ => null
                };

                return string.Equals(azp, keycloakAudience, StringComparison.OrdinalIgnoreCase);
            };
        }
    });
builder.Services.AddAuthorization();

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

app.UseAuthentication();
app.UseAuthorization();

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
