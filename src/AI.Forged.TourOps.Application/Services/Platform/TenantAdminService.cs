using System.Security.Cryptography;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Email;
using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Application.Models.AdminUsers;
using AI.Forged.TourOps.Application.Models.Platform;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Services.Platform;

public sealed class TenantAdminService(
    IRequestActorContext requestActorContext,
    ITenantExecutionContextAccessor tenantExecutionContextAccessor,
    ITenantPlatformRepository tenantPlatformRepository,
    ITenantConfigurationService tenantConfigurationService,
    IEmailIntegrationService emailIntegrationService,
    IKeycloakRealmAdminRepository keycloakRealmAdminRepository) : ITenantAdminService
{
    public async Task<TenantAdminWorkspaceModel> GetWorkspaceAsync(CancellationToken cancellationToken = default)
    {
        var context = await ResolveContextAsync(cancellationToken);
        return await BuildWorkspaceAsync(context, cancellationToken);
    }

    public async Task<TenantAdminWorkspaceModel> UpdateWorkspaceProfileAsync(UpdateTenantWorkspaceProfileModel model, CancellationToken cancellationToken = default)
    {
        var context = await ResolveContextAsync(cancellationToken);
        ValidateWorkspaceProfileModel(model);

        context.Tenant.Name = model.TenantName.Trim();
        context.Tenant.LegalName = NormalizeOptional(model.LegalName);
        context.Tenant.DefaultCurrency = model.DefaultCurrency.Trim().ToUpperInvariant();
        context.Tenant.TimeZone = model.TimeZone.Trim();
        context.Tenant.BillingEmail = NormalizeOptional(model.BillingEmail);
        context.Tenant.Notes = NormalizeOptional(model.Notes);
        context.Tenant.UpdatedAt = DateTime.UtcNow;

        await tenantPlatformRepository.UpdateTenantAsync(context.Tenant, cancellationToken);
        return await BuildWorkspaceAsync(context, cancellationToken);
    }

    private async Task<TenantAdminWorkspaceModel> BuildWorkspaceAsync(TenantAdminContext context, CancellationToken cancellationToken)
    {
        var configs = await tenantConfigurationService.GetAsync(context.Tenant.Id, null, cancellationToken);
        var memberships = await tenantPlatformRepository.GetMembershipsAsync(context.Tenant.Id, cancellationToken);
        var emailConnections = await emailIntegrationService.GetConnectionsAsync(cancellationToken);

        return new TenantAdminWorkspaceModel
        {
            TenantId = context.Tenant.Id,
            TenantSlug = context.Tenant.Slug,
            TenantName = context.Tenant.Name,
            LegalName = context.Tenant.LegalName,
            DefaultCurrency = context.Tenant.DefaultCurrency,
            TimeZone = context.Tenant.TimeZone,
            BillingEmail = context.Tenant.BillingEmail,
            Notes = context.Tenant.Notes,
            RealmName = context.Identity.RealmName,
            ConfigEntries = configs.OrderBy(x => x.ConfigDomain, StringComparer.OrdinalIgnoreCase).ThenBy(x => x.ConfigKey, StringComparer.OrdinalIgnoreCase).ToList(),
            Users = memberships.OrderByDescending(x => x.Role).ThenBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase).Select(MapUser).ToList(),
            EmailConnections = emailConnections.OrderByDescending(x => x.IsDefaultConnection).ThenBy(x => x.ConnectionName, StringComparer.OrdinalIgnoreCase).ToList()
        };
    }

    public async Task<TenantConfigEntryModel> UpsertConfigAsync(UpsertTenantConfigModel model, CancellationToken cancellationToken = default)
    {
        var context = await ResolveContextAsync(cancellationToken);
        return await tenantConfigurationService.UpsertAsync(context.Tenant.Id, model, cancellationToken);
    }

    public async Task<TenantWorkspaceUserCreateResultModel> CreateUserAsync(CreateTenantWorkspaceUserModel model, CancellationToken cancellationToken = default)
    {
        var context = await ResolveContextAsync(cancellationToken);
        ValidateCreateModel(model);

        var temporaryPassword = NormalizePassword(model.TemporaryPassword) ?? GenerateTemporaryPassword();
        var userId = await keycloakRealmAdminRepository.CreateUserAsync(
            context.Identity.RealmName,
            new KeycloakAdminCreateUserInput
            {
                Username = model.Username.Trim(),
                Email = model.Email.Trim(),
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Enabled = true,
                EmailVerified = false,
                TemporaryPassword = temporaryPassword
            },
            MapRealmRoles(model.Role),
            cancellationToken);

        var membership = new TenantUserMembership
        {
            Id = Guid.NewGuid(),
            TenantId = context.Tenant.Id,
            UserId = userId,
            Email = model.Email.Trim(),
            DisplayName = $"{model.FirstName.Trim()} {model.LastName.Trim()}".Trim(),
            Role = model.Role,
            Status = TenantUserStatus.Active,
            InvitedAt = DateTime.UtcNow,
            JoinedAt = DateTime.UtcNow,
            LastSeenAt = null
        };

        await tenantPlatformRepository.UpsertMembershipAsync(membership, cancellationToken);

        return new TenantWorkspaceUserCreateResultModel
        {
            User = MapUser(membership, model.Username.Trim(), model.FirstName.Trim(), model.LastName.Trim(), true),
            TemporaryPassword = temporaryPassword
        };
    }

    public async Task<TenantWorkspaceUserModel> UpdateUserAsync(string userId, UpdateTenantWorkspaceUserModel model, CancellationToken cancellationToken = default)
    {
        var context = await ResolveContextAsync(cancellationToken);
        ValidateUserId(userId);
        ValidateUpdateModel(model);

        var membership = await tenantPlatformRepository.GetMembershipAsync(context.Tenant.Id, userId.Trim(), cancellationToken)
            ?? throw new InvalidOperationException("Tenant user membership was not found.");
        var (firstName, lastName) = SplitName(membership.DisplayName, model.FirstName, model.LastName);

        await keycloakRealmAdminRepository.UpdateUserAsync(
            context.Identity.RealmName,
            membership.UserId,
            new KeycloakAdminUpdateUserInput
            {
                Username = ResolveUsername(membership),
                Email = model.Email.Trim(),
                FirstName = firstName,
                LastName = lastName,
                Enabled = model.Enabled,
                EmailVerified = false
            },
            cancellationToken);
        await keycloakRealmAdminRepository.ReplaceRealmRolesAsync(context.Identity.RealmName, membership.UserId, MapRealmRoles(model.Role), cancellationToken);

        membership.Email = model.Email.Trim();
        membership.DisplayName = $"{firstName} {lastName}".Trim();
        membership.Role = model.Role;
        membership.Status = model.Enabled ? TenantUserStatus.Active : TenantUserStatus.Disabled;
        membership.LastSeenAt = membership.LastSeenAt;

        await tenantPlatformRepository.UpsertMembershipAsync(membership, cancellationToken);
        return MapUser(membership, ResolveUsername(membership), firstName, lastName, model.Enabled);
    }

    public async Task<TenantWorkspaceUserPasswordResetResultModel> ResetPasswordAsync(string userId, ResetTenantWorkspaceUserPasswordModel model, CancellationToken cancellationToken = default)
    {
        var context = await ResolveContextAsync(cancellationToken);
        ValidateUserId(userId);

        var membership = await tenantPlatformRepository.GetMembershipAsync(context.Tenant.Id, userId.Trim(), cancellationToken)
            ?? throw new InvalidOperationException("Tenant user membership was not found.");
        var temporaryPassword = NormalizePassword(model.TemporaryPassword) ?? GenerateTemporaryPassword();

        await keycloakRealmAdminRepository.ResetTemporaryPasswordAsync(context.Identity.RealmName, membership.UserId, temporaryPassword, cancellationToken);

        return new TenantWorkspaceUserPasswordResetResultModel
        {
            UserId = membership.UserId,
            TemporaryPassword = temporaryPassword
        };
    }

    private async Task<TenantAdminContext> ResolveContextAsync(CancellationToken cancellationToken)
    {
        var tenantId = tenantExecutionContextAccessor.CurrentTenantId
            ?? throw new InvalidOperationException("Tenant scope is required.");
        var tenant = await tenantPlatformRepository.GetTenantByIdAsync(tenantId, cancellationToken)
            ?? throw new InvalidOperationException("Tenant not found.");
        var identity = (await tenantPlatformRepository.GetIdentityMappingsAsync(tenantId, cancellationToken)).FirstOrDefault()
            ?? throw new InvalidOperationException("Tenant identity mapping was not found.");

        if (!requestActorContext.IsPlatformAdmin())
        {
            var userId = requestActorContext.GetUserIdOrNull();
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new InvalidOperationException("Authenticated tenant admin user was not resolved.");
            }

            var membership = await tenantPlatformRepository.GetMembershipAsync(tenantId, userId, cancellationToken)
                ?? throw new InvalidOperationException("Tenant membership was not found.");

            if (membership.Role != TenantUserRole.TenantAdmin || membership.Status != TenantUserStatus.Active)
            {
                throw new InvalidOperationException("Tenant admin access is required.");
            }
        }

        return new TenantAdminContext
        {
            Tenant = tenant,
            Identity = identity
        };
    }

    private static TenantWorkspaceUserModel MapUser(TenantUserMembership membership) =>
        MapUser(membership, ResolveUsername(membership), ResolveFirstName(membership.DisplayName), ResolveLastName(membership.DisplayName), membership.Status == TenantUserStatus.Active);

    private static TenantWorkspaceUserModel MapUser(TenantUserMembership membership, string username, string firstName, string lastName, bool enabled) => new()
    {
        MembershipId = membership.Id,
        UserId = membership.UserId,
        Username = username,
        Email = membership.Email,
        FirstName = firstName,
        LastName = lastName,
        DisplayName = membership.DisplayName,
        Role = membership.Role,
        Status = membership.Status,
        Enabled = enabled,
        InvitedAt = membership.InvitedAt,
        LastSeenAt = membership.LastSeenAt
    };

    private static IReadOnlyList<string> MapRealmRoles(TenantUserRole role) => role switch
    {
        TenantUserRole.TenantAdmin => ["tenant-admin"],
        _ => ["operator"]
    };

    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("User id is required.");
        }
    }

    private static void ValidateCreateModel(CreateTenantWorkspaceUserModel model)
    {
        ValidateEmail(model.Email);

        if (string.IsNullOrWhiteSpace(model.Username))
        {
            throw new InvalidOperationException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(model.FirstName))
        {
            throw new InvalidOperationException("First name is required.");
        }

        if (string.IsNullOrWhiteSpace(model.LastName))
        {
            throw new InvalidOperationException("Last name is required.");
        }
    }

    private static void ValidateUpdateModel(UpdateTenantWorkspaceUserModel model)
    {
        ValidateEmail(model.Email);
    }

    private static void ValidateWorkspaceProfileModel(UpdateTenantWorkspaceProfileModel model)
    {
        if (string.IsNullOrWhiteSpace(model.TenantName))
        {
            throw new InvalidOperationException("Tenant name is required.");
        }

        if (string.IsNullOrWhiteSpace(model.DefaultCurrency))
        {
            throw new InvalidOperationException("Default currency is required.");
        }

        if (string.IsNullOrWhiteSpace(model.TimeZone))
        {
            throw new InvalidOperationException("Time zone is required.");
        }

        if (!string.IsNullOrWhiteSpace(model.BillingEmail))
        {
            ValidateEmail(model.BillingEmail);
        }
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@', StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Valid email is required.");
        }
    }

    private static string? NormalizePassword(string? password) =>
        string.IsNullOrWhiteSpace(password) ? null : password.Trim();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string GenerateTemporaryPassword()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@$%";
        Span<byte> bytes = stackalloc byte[14];
        RandomNumberGenerator.Fill(bytes);
        var chars = new char[bytes.Length];
        for (var i = 0; i < bytes.Length; i++)
        {
            chars[i] = alphabet[bytes[i] % alphabet.Length];
        }

        return new string(chars);
    }

    private static string ResolveUsername(TenantUserMembership membership) =>
        membership.Email.Contains('@', StringComparison.Ordinal) ? membership.Email[..membership.Email.IndexOf('@')] : membership.Email;

    private static (string FirstName, string LastName) SplitName(string displayName, string? firstNameOverride, string? lastNameOverride)
    {
        var firstName = string.IsNullOrWhiteSpace(firstNameOverride) ? ResolveFirstName(displayName) : firstNameOverride.Trim();
        var lastName = string.IsNullOrWhiteSpace(lastNameOverride) ? ResolveLastName(displayName) : lastNameOverride.Trim();
        return (firstName, lastName);
    }

    private static string ResolveFirstName(string displayName)
    {
        var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length > 0 ? parts[0] : displayName;
    }

    private static string ResolveLastName(string displayName)
    {
        var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : "-";
    }

    private sealed class TenantAdminContext
    {
        public Tenant Tenant { get; init; } = null!;
        public TenantIdentityMapping Identity { get; init; } = null!;
    }
}
