using System.IdentityModel.Tokens.Jwt;
using AI.Forged.TourOps.Api.Auth;
using AI.Forged.TourOps.Api.Middleware;
using AI.Forged.TourOps.Application;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Models.AdminUsers;
using AI.Forged.TourOps.Infrastructure;
using AI.Forged.TourOps.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
var keycloakSection = builder.Configuration.GetSection("Keycloak");
var keycloakAuthority = keycloakSection["Authority"];
var keycloakPublicAuthority = keycloakSection["PublicAuthority"] ?? keycloakAuthority;
var keycloakInternalBaseUrl = keycloakSection["BaseUrl"] ?? TrimRealmUrl(keycloakAuthority);
var keycloakPublicBaseUrl = keycloakSection["PublicBaseUrl"] ?? TrimRealmUrl(keycloakPublicAuthority) ?? keycloakInternalBaseUrl;
var keycloakManagementRealm = keycloakSection["ManagementRealm"] ?? "tourops";
var keycloakManagementClientId = keycloakSection["ManagementClientId"] ?? keycloakSection["Audience"] ?? "frontend";
var keycloakTenantClientId = keycloakSection["TenantClientId"] ?? keycloakManagementClientId;
var keycloakTenantRealmPrefix = keycloakSection["TenantRealmPrefix"] ?? "tenant-";
var keycloakSecurityKeys = new KeycloakMultiRealmSecurityKeyProvider(
    keycloakInternalBaseUrl ?? "http://localhost:8080",
    keycloakPublicBaseUrl ?? "http://localhost:8080",
    keycloakManagementRealm,
    keycloakTenantRealmPrefix);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("public-signup", context =>
    {
        var key = context.Request.Headers["X-Forwarded-For"].ToString();
        if (string.IsNullOrWhiteSpace(key))
        {
            key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        return RateLimitPartition.GetFixedWindowLimiter(
            key,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });
});
builder.Services.AddScoped<ICurrentUserContext, HttpContextCurrentUserContext>();
builder.Services.AddScoped<IRequestActorContext, HttpContextRequestActorContext>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = "sub",
            IssuerValidator = (issuer, _, _) => keycloakSecurityKeys.ValidateIssuer(issuer),
            IssuerSigningKeyResolver = (token, _, _, _) => keycloakSecurityKeys.ResolveSigningKeys(token)
        };

        options.TokenValidationParameters.AudienceValidator = (audiences, securityToken, _) =>
        {
            if (audiences.Contains(keycloakManagementClientId, StringComparer.OrdinalIgnoreCase)
                || audiences.Contains(keycloakTenantClientId, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }

            var azp = securityToken switch
            {
                JsonWebToken jsonWebToken => jsonWebToken.Claims.FirstOrDefault(x => x.Type == "azp")?.Value,
                JwtSecurityToken jwtSecurityToken => jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == "azp")?.Value,
                _ => null
            };

            return string.Equals(azp, keycloakManagementClientId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(azp, keycloakTenantClientId, StringComparison.OrdinalIgnoreCase);
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity is ClaimsIdentity identity)
                {
                    KeycloakRoleClaimEnricher.Enrich(identity);
                }

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
});

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
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseRateLimiter();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

await ApplyMigrationsWithRetryAsync(app);
await EnsurePlatformSeedDataAsync(app);
await SeedDemoCatalogIfEnabledAsync(app);

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

static async Task SeedDemoCatalogIfEnabledAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var deployment = app.Configuration.GetSection("Deployment");
    var tenancy = app.Configuration.GetSection("Tenancy");
    var deploymentMode = deployment.GetValue("Mode", AI.Forged.TourOps.Domain.Enums.DeploymentMode.Standalone);
    var tenantExecutionContextAccessor = scope.ServiceProvider.GetRequiredService<AI.Forged.TourOps.Application.Interfaces.Platform.ITenantExecutionContextAccessor>();
    var tenantPlatformRepository = scope.ServiceProvider.GetRequiredService<AI.Forged.TourOps.Application.Interfaces.Platform.ITenantPlatformRepository>();
    var seeder = scope.ServiceProvider.GetRequiredService<DemoCatalogSeeder>();

    if (deploymentMode == AI.Forged.TourOps.Domain.Enums.DeploymentMode.Standalone)
    {
        if (!app.Configuration.GetValue<bool>("demo"))
        {
            return;
        }

        var standaloneTenant = await tenantPlatformRepository.GetStandaloneTenantAsync(CancellationToken.None);
        if (standaloneTenant is null)
        {
            return;
        }

        using var tenantScope = tenantExecutionContextAccessor.BeginTenantScope(standaloneTenant.Id);
        await seeder.SeedAsync();
        return;
    }

    if (!tenancy.GetValue("SeedDemoCatalogInSaas", false))
    {
        return;
    }

    var demoTenantId = tenancy.GetValue("DemoTenantId", Guid.Parse("22222222-2222-2222-2222-222222222222"));
    using var demoTenantScope = tenantExecutionContextAccessor.BeginTenantScope(demoTenantId);
    await seeder.SeedAsync();
}

static async Task EnsurePlatformSeedDataAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var repository = scope.ServiceProvider.GetRequiredService<AI.Forged.TourOps.Application.Interfaces.Platform.ITenantPlatformRepository>();
    var keycloakProvisioningService = scope.ServiceProvider.GetRequiredService<AI.Forged.TourOps.Application.Interfaces.Platform.IKeycloakProvisioningService>();
    var keycloakRealmAdminRepository = scope.ServiceProvider.GetRequiredService<AI.Forged.TourOps.Application.Interfaces.IKeycloakRealmAdminRepository>();
    var deployment = app.Configuration.GetSection("Deployment");
    var tenancy = app.Configuration.GetSection("Tenancy");
    var keycloak = app.Configuration.GetSection("Keycloak");
    var deploymentMode = deployment.GetValue("Mode", AI.Forged.TourOps.Domain.Enums.DeploymentMode.Standalone);
    var standaloneTenantId = tenancy.GetValue("StandaloneTenantId", Guid.Parse("11111111-1111-1111-1111-111111111111"));
    var seedDemoTenantInSaas = tenancy.GetValue("SeedDemoTenantInSaas", false);
    var demoTenantId = tenancy.GetValue("DemoTenantId", Guid.Parse("22222222-2222-2222-2222-222222222222"));
    var demoTenantSlug = tenancy.GetValue("DemoTenantSlug", "demo-agency")!;
    var demoTenantName = tenancy.GetValue("DemoTenantName", "Demo Agency")!;

    await repository.EnsureSeedDataAsync(
        deploymentMode,
        standaloneTenantId,
        tenancy.GetValue("StandaloneTenantSlug", "standalone")!,
        tenancy.GetValue("StandaloneTenantName", "Standalone Tenant")!,
        seedDemoTenantInSaas,
        demoTenantId,
        demoTenantSlug,
        demoTenantName,
        CancellationToken.None);

    if (deploymentMode == AI.Forged.TourOps.Domain.Enums.DeploymentMode.Standalone)
    {
        var standaloneIdentity = await keycloakProvisioningService.EnsureTenantIdentityAsync(standaloneTenantId, CancellationToken.None);
        if (standaloneIdentity.ProvisioningStatus == AI.Forged.TourOps.Domain.Enums.IdentityProvisioningStatus.Ready
            && !string.IsNullOrWhiteSpace(standaloneIdentity.RealmName))
        {
            var standaloneAdminUserId = await EnsureSeedRealmUserAsync(
                keycloakRealmAdminRepository,
                standaloneIdentity.RealmName,
                "admin",
                "admin",
                "Standalone",
                "Admin",
                "admin@tourops.local",
                ["tenant-admin"],
                CancellationToken.None);
            var standaloneOperatorUserId = await EnsureSeedRealmUserAsync(
                keycloakRealmAdminRepository,
                standaloneIdentity.RealmName,
                "user",
                "user",
                "Standalone",
                "User",
                "user@tourops.local",
                ["operator"],
                CancellationToken.None);

            await repository.UpsertMembershipAsync(new AI.Forged.TourOps.Domain.Entities.TenantUserMembership
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                TenantId = standaloneTenantId,
                UserId = standaloneAdminUserId,
                Email = "admin@tourops.local",
                DisplayName = "Standalone Admin",
                Role = AI.Forged.TourOps.Domain.Enums.TenantUserRole.TenantAdmin,
                Status = AI.Forged.TourOps.Domain.Enums.TenantUserStatus.Active,
                InvitedAt = DateTime.UtcNow,
                JoinedAt = DateTime.UtcNow
            }, CancellationToken.None);
            await repository.UpsertMembershipAsync(new AI.Forged.TourOps.Domain.Entities.TenantUserMembership
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                TenantId = standaloneTenantId,
                UserId = standaloneOperatorUserId,
                Email = "user@tourops.local",
                DisplayName = "Standalone User",
                Role = AI.Forged.TourOps.Domain.Enums.TenantUserRole.Operator,
                Status = AI.Forged.TourOps.Domain.Enums.TenantUserStatus.Active,
                InvitedAt = DateTime.UtcNow,
                JoinedAt = DateTime.UtcNow
            }, CancellationToken.None);
        }

        return;
    }

    if (deploymentMode != AI.Forged.TourOps.Domain.Enums.DeploymentMode.SaaS || !seedDemoTenantInSaas)
    {
        return;
    }

    var managementRealm = keycloak.GetValue("ManagementRealm", "tourops")!;
    await EnsureSeedRealmUserAsync(
        keycloakRealmAdminRepository,
        managementRealm,
        "platform-admin",
        "platform-admin",
        "Platform",
        "Admin",
        "platform.admin@tourops.local",
        ["admin"],
        CancellationToken.None);
    await EnsureSeedRealmUserAsync(
        keycloakRealmAdminRepository,
        managementRealm,
        "platform-user",
        "platform-user",
        "Platform",
        "User",
        "platform.user@tourops.local",
        ["operator"],
        CancellationToken.None);

    var identity = await keycloakProvisioningService.EnsureTenantIdentityAsync(demoTenantId, CancellationToken.None);
    if (identity.ProvisioningStatus != AI.Forged.TourOps.Domain.Enums.IdentityProvisioningStatus.Ready
        || string.IsNullOrWhiteSpace(identity.RealmName))
    {
        app.Logger.LogWarning("Demo SaaS tenant realm seed skipped. Tenant realm not ready for tenant {TenantId}.", demoTenantId);
        return;
    }

    var demoAdminId = await EnsureSeedRealmUserAsync(
        keycloakRealmAdminRepository,
        identity.RealmName,
        "demo-admin",
        "demo-admin",
        "Demo",
        "Admin",
        $"admin@{demoTenantSlug}.local",
        ["tenant-admin"],
        CancellationToken.None);
    var demoUserId = await EnsureSeedRealmUserAsync(
        keycloakRealmAdminRepository,
        identity.RealmName,
        "demo-user",
        "demo-user",
        "Demo",
        "User",
        $"agent@{demoTenantSlug}.local",
        ["operator"],
        CancellationToken.None);

    await repository.UpsertMembershipAsync(new AI.Forged.TourOps.Domain.Entities.TenantUserMembership
    {
        Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
        TenantId = demoTenantId,
        UserId = demoAdminId,
        Email = $"admin@{demoTenantSlug}.local",
        DisplayName = "Demo Admin",
        Role = AI.Forged.TourOps.Domain.Enums.TenantUserRole.TenantAdmin,
        Status = AI.Forged.TourOps.Domain.Enums.TenantUserStatus.Active,
        InvitedAt = DateTime.UtcNow,
        JoinedAt = DateTime.UtcNow
    }, CancellationToken.None);

    await repository.UpsertMembershipAsync(new AI.Forged.TourOps.Domain.Entities.TenantUserMembership
    {
        Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
        TenantId = demoTenantId,
        UserId = demoUserId,
        Email = $"agent@{demoTenantSlug}.local",
        DisplayName = "Demo User",
        Role = AI.Forged.TourOps.Domain.Enums.TenantUserRole.Operator,
        Status = AI.Forged.TourOps.Domain.Enums.TenantUserStatus.Active,
        InvitedAt = DateTime.UtcNow,
        JoinedAt = DateTime.UtcNow
    }, CancellationToken.None);
}

static async Task<string> EnsureSeedRealmUserAsync(
    AI.Forged.TourOps.Application.Interfaces.IKeycloakRealmAdminRepository keycloakRealmAdminRepository,
    string realmName,
    string username,
    string password,
    string firstName,
    string lastName,
    string email,
    IReadOnlyList<string> realmRoles,
    CancellationToken cancellationToken)
{
    var existing = await keycloakRealmAdminRepository.FindUserByUsernameAsync(realmName, username, cancellationToken);
    if (existing is null)
    {
        return await keycloakRealmAdminRepository.CreateUserAsync(
            realmName,
            new KeycloakAdminCreateUserInput
            {
                Username = username,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Enabled = true,
                EmailVerified = true,
                TemporaryPassword = password,
                RequirePasswordChange = false
            },
            realmRoles,
            cancellationToken);
    }

    await keycloakRealmAdminRepository.UpdateUserAsync(
        realmName,
        existing.Id,
        new KeycloakAdminUpdateUserInput
        {
            Username = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Enabled = true,
            EmailVerified = true,
            RequiredActions = []
        },
        cancellationToken);
    await keycloakRealmAdminRepository.SetPasswordAsync(realmName, existing.Id, password, false, cancellationToken);
    await keycloakRealmAdminRepository.ReplaceRealmRolesAsync(realmName, existing.Id, realmRoles, cancellationToken);
    return existing.Id;
}

static string? TrimRealmUrl(string? authority)
{
    if (string.IsNullOrWhiteSpace(authority))
    {
        return null;
    }

    var marker = "/realms/";
    var index = authority.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
    return index >= 0 ? authority[..index] : authority;
}
