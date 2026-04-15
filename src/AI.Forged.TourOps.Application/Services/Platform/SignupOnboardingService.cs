using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using AI.Forged.TourOps.Application.Interfaces;
using AI.Forged.TourOps.Application.Interfaces.Platform;
using AI.Forged.TourOps.Application.Models.AdminUsers;
using AI.Forged.TourOps.Application.Models.Platform;
using AI.Forged.TourOps.Domain.Entities;
using AI.Forged.TourOps.Domain.Enums;

namespace AI.Forged.TourOps.Application.Services.Platform;

public sealed partial class SignupOnboardingService(
    ITenantPlatformRepository tenantPlatformRepository,
    ITenantExecutionContextAccessor tenantExecutionContextAccessor,
    ISignupSettingsAccessor signupSettingsAccessor,
    IPlatformEmailSender platformEmailSender,
    ISignupBillingGateway signupBillingGateway,
    IKeycloakProvisioningService keycloakProvisioningService,
    IKeycloakRealmAdminRepository keycloakRealmAdminRepository,
    IAuditService auditService) : ISignupOnboardingService
{
    public Task<SignupBootstrapModel> GetBootstrapAsync(CancellationToken cancellationToken = default)
    {
        var enabled = IsSignupEnabled();
        return Task.FromResult(new SignupBootstrapModel
        {
            Enabled = enabled,
            DisabledReason = enabled ? string.Empty : tenantExecutionContextAccessor.DeploymentMode == DeploymentMode.Standalone
                ? "Signup is disabled for standalone deployments."
                : "Signup is currently unavailable.",
            AllowTestPaymentConfirmation = signupSettingsAccessor.AllowTestPaymentConfirmation,
            SupportEmail = signupSettingsAccessor.BillingSupportEmail
        });
    }

    public async Task<IReadOnlyList<SignupPlanModel>> GetPlansAsync(CancellationToken cancellationToken = default)
    {
        EnsureSignupEnabled();
        var plans = await tenantPlatformRepository.GetPublicSignupPlansAsync(cancellationToken);
        return plans.Select(MapPlan).ToList();
    }

    public async Task<SignupSessionEnvelopeModel> StartSessionAsync(CancellationToken cancellationToken = default)
    {
        EnsureSignupEnabled();

        var now = DateTime.UtcNow;
        var accessToken = GenerateToken();
        var session = new SignupSession
        {
            Id = Guid.NewGuid(),
            AccessTokenHash = HashToken(accessToken),
            Status = SignupSessionStatus.Draft,
            CurrentStep = "welcome",
            BillingStatus = SignupBillingStatus.NotRequired,
            ExpiresAt = now.AddHours(signupSettingsAccessor.SessionExpirationHours),
            CreatedAt = now,
            UpdatedAt = now
        };

        await tenantPlatformRepository.AddSignupSessionAsync(session, cancellationToken);
        await WriteAuditAsync("SignupStarted", "Draft", null, session, cancellationToken);

        return new SignupSessionEnvelopeModel
        {
            Session = MapSession(session, null),
            AccessToken = accessToken
        };
    }

    public async Task<SignupSessionEnvelopeModel> GetSessionAsync(Guid sessionId, string accessToken, CancellationToken cancellationToken = default)
    {
        EnsureSignupEnabled();
        var session = await ValidateSessionAccessAsync(sessionId, accessToken, cancellationToken);
        return new SignupSessionEnvelopeModel
        {
            Session = MapSession(session, session.EmailVerification)
        };
    }

    public async Task<SignupSessionEnvelopeModel> SaveEmailAsync(Guid sessionId, string accessToken, SignupEmailUpdateModel model, CancellationToken cancellationToken = default)
    {
        EnsureSignupEnabled();
        var session = await ValidateSessionAccessAsync(sessionId, accessToken, cancellationToken);
        var normalizedEmail = NormalizeEmail(model.Email);

        if (await tenantPlatformRepository.IsSignupEmailAlreadyUsedAsync(normalizedEmail, cancellationToken))
        {
            throw new InvalidOperationException("Email cannot be used for signup.");
        }

        var verificationToken = GenerateToken();
        var now = DateTime.UtcNow;
        var emailChanged = !string.Equals(session.NormalizedEmail, normalizedEmail, StringComparison.OrdinalIgnoreCase);

        session.Email = model.Email.Trim();
        session.NormalizedEmail = normalizedEmail;
        session.EmailVerifiedAt = emailChanged ? null : session.EmailVerifiedAt;
        session.Status = SignupSessionStatus.EmailPending;
        session.CurrentStep = "verification";
        session.UpdatedAt = now;
        session.ExpiresAt = now.AddHours(signupSettingsAccessor.SessionExpirationHours);

        var verification = session.EmailVerification ?? new SignupEmailVerification
        {
            SignupSessionId = session.Id
        };
        verification.TokenHash = HashToken(verificationToken);
        verification.LastSentEmail = session.Email;
        verification.SendCount = emailChanged ? 1 : verification.SendCount + 1;
        verification.SentAt = now;
        verification.ExpiresAt = now.AddMinutes(signupSettingsAccessor.VerificationTokenMinutes);
        verification.VerifiedAt = null;
        verification.ConsumedAt = null;
        verification.UpdatedAt = now;

        await tenantPlatformRepository.UpsertSignupEmailVerificationAsync(verification, cancellationToken);
        session.EmailVerification = verification;
        await tenantPlatformRepository.UpdateSignupSessionAsync(session, cancellationToken);

        var verificationLink = BuildVerificationLink(session.Id, verificationToken);
        await platformEmailSender.SendSignupVerificationAsync(session.Email, session.OrganizationName ?? "TourOps signup", verificationLink, cancellationToken);
        await WriteAuditAsync("SignupVerificationSent", "Success", session.Email, session, cancellationToken);

        return new SignupSessionEnvelopeModel
        {
            Session = MapSession(session, verification),
            DebugVerificationToken = signupSettingsAccessor.ExposeDebugTokens ? verificationToken : null
        };
    }

    public async Task<SignupSessionEnvelopeModel> ResendVerificationAsync(Guid sessionId, string accessToken, CancellationToken cancellationToken = default)
    {
        EnsureSignupEnabled();
        var session = await ValidateSessionAccessAsync(sessionId, accessToken, cancellationToken);
        if (string.IsNullOrWhiteSpace(session.Email))
        {
            throw new InvalidOperationException("Email is required before verification can be resent.");
        }

        var verification = session.EmailVerification
            ?? throw new InvalidOperationException("Email verification has not been started.");
        var now = DateTime.UtcNow;
        if (verification.SentAt.AddSeconds(signupSettingsAccessor.VerificationResendSeconds) > now)
        {
            throw new InvalidOperationException("Verification email was sent recently. Wait before resending.");
        }

        var verificationToken = GenerateToken();
        verification.TokenHash = HashToken(verificationToken);
        verification.SendCount += 1;
        verification.SentAt = now;
        verification.ExpiresAt = now.AddMinutes(signupSettingsAccessor.VerificationTokenMinutes);
        verification.VerifiedAt = null;
        verification.ConsumedAt = null;
        verification.UpdatedAt = now;
        session.Status = SignupSessionStatus.EmailPending;
        session.CurrentStep = "verification";
        session.UpdatedAt = now;

        await tenantPlatformRepository.UpsertSignupEmailVerificationAsync(verification, cancellationToken);
        await tenantPlatformRepository.UpdateSignupSessionAsync(session, cancellationToken);

        await platformEmailSender.SendSignupVerificationAsync(
            session.Email,
            session.OrganizationName ?? "TourOps signup",
            BuildVerificationLink(session.Id, verificationToken),
            cancellationToken);
        await WriteAuditAsync("SignupVerificationResent", "Success", session.Email, session, cancellationToken);

        return new SignupSessionEnvelopeModel
        {
            Session = MapSession(session, verification),
            DebugVerificationToken = signupSettingsAccessor.ExposeDebugTokens ? verificationToken : null
        };
    }

    public async Task<SignupSessionEnvelopeModel> VerifyEmailAsync(Guid sessionId, SignupVerifyEmailModel model, CancellationToken cancellationToken = default)
    {
        EnsureSignupEnabled();
        var session = await tenantPlatformRepository.GetSignupSessionAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException("Signup session was not found.");
        await EnsureNotExpiredAsync(session, cancellationToken);

        var verification = session.EmailVerification
            ?? throw new InvalidOperationException("Email verification was not found.");
        if (verification.ConsumedAt.HasValue || verification.ExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Verification token is no longer valid.");
        }

        EnsureTokenMatch(verification.TokenHash, model.Token);

        var newAccessToken = GenerateToken();
        var now = DateTime.UtcNow;
        verification.VerifiedAt = now;
        verification.ConsumedAt = now;
        verification.UpdatedAt = now;
        session.AccessTokenHash = HashToken(newAccessToken);
        session.EmailVerifiedAt = now;
        session.Status = session.SelectedPlanCode is null ? SignupSessionStatus.EmailVerified : ResolvePostVerificationStatus(session);
        session.CurrentStep = ResolveNextStepAfterVerification(session);
        session.UpdatedAt = now;

        await tenantPlatformRepository.UpsertSignupEmailVerificationAsync(verification, cancellationToken);
        await tenantPlatformRepository.UpdateSignupSessionAsync(session, cancellationToken);
        await WriteAuditAsync("SignupEmailVerified", "Success", session.Email, session, cancellationToken);

        return new SignupSessionEnvelopeModel
        {
            Session = MapSession(session, verification),
            AccessToken = newAccessToken
        };
    }

    public async Task<SignupSessionEnvelopeModel> SaveOrganizationAsync(Guid sessionId, string accessToken, SignupOrganizationModel model, CancellationToken cancellationToken = default)
    {
        EnsureSignupEnabled();
        var session = await ValidateSessionAccessAsync(sessionId, accessToken, cancellationToken);
        EnsureEmailVerified(session);

        var tenantSlug = NormalizeTenantSlug(model.TenantSlug);
        if (!string.Equals(session.TenantSlug, tenantSlug, StringComparison.OrdinalIgnoreCase)
            && await tenantPlatformRepository.IsTenantSlugAlreadyUsedAsync(tenantSlug, cancellationToken))
        {
            throw new InvalidOperationException("Tenant code is already in use.");
        }

        session.OrganizationName = NormalizeRequired(model.OrganizationName, "Organization name is required.", 200);
        session.OrganizationLegalName = NormalizeOptional(model.OrganizationLegalName, 256);
        session.TenantSlug = tenantSlug;
        session.BillingEmail = NormalizeEmail(model.BillingEmail);
        session.DefaultCurrency = NormalizeRequired(model.DefaultCurrency, "Default currency is required.", 8).ToUpperInvariant();
        session.TimeZone = NormalizeRequired(model.TimeZone, "Time zone is required.", 128);
        session.OrganizationProfileJson = JsonSerializer.Serialize(model);
        session.CurrentStep = session.SelectedPlanCode is null ? "plan" : ResolveNextStepAfterOrganization(session);
        session.UpdatedAt = DateTime.UtcNow;

        await tenantPlatformRepository.UpdateSignupSessionAsync(session, cancellationToken);
        await WriteAuditAsync("SignupOrganizationSaved", "Success", session.Email, session, cancellationToken);

        return new SignupSessionEnvelopeModel
        {
            Session = MapSession(session, session.EmailVerification)
        };
    }

    public async Task<SignupSessionEnvelopeModel> SelectPlanAsync(Guid sessionId, string accessToken, SignupPlanSelectionModel model, CancellationToken cancellationToken = default)
    {
        EnsureSignupEnabled();
        var session = await ValidateSessionAccessAsync(sessionId, accessToken, cancellationToken);
        var plan = await tenantPlatformRepository.GetLicensePlanByCodeAsync(model.PlanCode.Trim(), cancellationToken)
            ?? throw new InvalidOperationException("Signup plan was not found.");
        if (!plan.IsPublicSignupEnabled || plan.IsStandalonePlan)
        {
            throw new InvalidOperationException("Plan is not available for public signup.");
        }

        session.SelectedPlanId = plan.Id;
        session.SelectedPlanCode = plan.Code;
        session.BillingStatus = plan.SignupKind switch
        {
            LicenseSignupKind.Paid => SignupBillingStatus.Pending,
            _ => SignupBillingStatus.NotRequired
        };
        session.Status = plan.SignupKind == LicenseSignupKind.Paid ? SignupSessionStatus.PaymentPending : SignupSessionStatus.PlanSelected;
        session.CurrentStep = ResolveNextStepAfterPlanSelection(session, plan);
        session.UpdatedAt = DateTime.UtcNow;

        await tenantPlatformRepository.UpdateSignupSessionAsync(session, cancellationToken);
        await WriteAuditAsync("SignupPlanSelected", plan.Code, session.Email, session, cancellationToken);

        return new SignupSessionEnvelopeModel
        {
            Session = MapSession(session, session.EmailVerification)
        };
    }

    public async Task<SignupSessionEnvelopeModel> AcceptTermsAsync(Guid sessionId, string accessToken, SignupTermsAcceptanceModel model, CancellationToken cancellationToken = default)
    {
        EnsureSignupEnabled();
        var session = await ValidateSessionAccessAsync(sessionId, accessToken, cancellationToken);
        await GetSelectedPlanAsync(session, cancellationToken);
        if (!model.Accepted)
        {
            throw new InvalidOperationException("Terms acceptance is required.");
        }

        session.TermsAccepted = true;
        session.TermsAcceptedAt = DateTime.UtcNow;
        session.CurrentStep = ResolveNextStepAfterTerms(session);
        session.UpdatedAt = DateTime.UtcNow;

        await tenantPlatformRepository.UpdateSignupSessionAsync(session, cancellationToken);
        await WriteAuditAsync("SignupTermsAccepted", "Success", session.Email, session, cancellationToken);

        return new SignupSessionEnvelopeModel
        {
            Session = MapSession(session, session.EmailVerification)
        };
    }

    public async Task<SignupSessionEnvelopeModel> CreateBillingIntentAsync(Guid sessionId, string accessToken, CancellationToken cancellationToken = default)
    {
        EnsureSignupEnabled();
        var session = await ValidateSessionAccessAsync(sessionId, accessToken, cancellationToken);
        var plan = await GetSelectedPlanAsync(session, cancellationToken);
        EnsureOrganizationReady(session);

        if (plan.SignupKind != LicenseSignupKind.Paid)
        {
            throw new InvalidOperationException("Billing is not required for this plan.");
        }

        var gatewayResult = await signupBillingGateway.CreateIntentAsync(new SignupBillingGatewayRequestModel
        {
            SessionId = session.Id,
            PlanId = plan.Id,
            PlanCode = plan.Code,
            PlanName = plan.Name,
            Amount = plan.MonthlyPrice,
            Currency = plan.Currency,
            Email = session.Email ?? string.Empty,
            OrganizationName = session.OrganizationName ?? string.Empty
        }, cancellationToken);

        var billingIntent = session.BillingIntent ?? new SignupBillingIntent
        {
            Id = Guid.NewGuid(),
            SignupSessionId = session.Id,
            LicensePlanId = plan.Id,
            CreatedAt = DateTime.UtcNow
        };

        billingIntent.LicensePlanId = plan.Id;
        billingIntent.Status = gatewayResult.Status;
        billingIntent.BillingMode = BillingMode.Invoice;
        billingIntent.Amount = plan.MonthlyPrice;
        billingIntent.Currency = plan.Currency;
        billingIntent.ProviderName = gatewayResult.ProviderName;
        billingIntent.ExternalReference = gatewayResult.ExternalReference;
        billingIntent.CheckoutUrl = gatewayResult.CheckoutUrl;
        billingIntent.MetadataJson = gatewayResult.MetadataJson;
        billingIntent.UpdatedAt = DateTime.UtcNow;
        billingIntent.ConfirmedAt = gatewayResult.Status == SignupBillingStatus.Confirmed ? DateTime.UtcNow : billingIntent.ConfirmedAt;

        session.BillingStatus = gatewayResult.Status;
        session.BillingIntentId = billingIntent.Id;
        session.Status = gatewayResult.Status == SignupBillingStatus.Confirmed ? SignupSessionStatus.PaymentConfirmed : SignupSessionStatus.PaymentPending;
        session.CurrentStep = gatewayResult.Status == SignupBillingStatus.Confirmed ? "admin" : "billing";
        session.UpdatedAt = DateTime.UtcNow;

        await tenantPlatformRepository.UpsertSignupBillingIntentAsync(billingIntent, cancellationToken);
        session.BillingIntent = billingIntent;
        await tenantPlatformRepository.UpdateSignupSessionAsync(session, cancellationToken);
        await WriteAuditAsync("SignupBillingIntentCreated", gatewayResult.Status.ToString(), session.Email, session, cancellationToken);

        return new SignupSessionEnvelopeModel
        {
            Session = MapSession(session, session.EmailVerification)
        };
    }

    public async Task<SignupSessionEnvelopeModel> ConfirmTestPaymentAsync(Guid sessionId, string accessToken, CancellationToken cancellationToken = default)
    {
        EnsureSignupEnabled();
        if (!signupSettingsAccessor.AllowTestPaymentConfirmation)
        {
            throw new InvalidOperationException("Test payment confirmation is disabled.");
        }

        var session = await ValidateSessionAccessAsync(sessionId, accessToken, cancellationToken);
        var plan = await GetSelectedPlanAsync(session, cancellationToken);
        if (plan.SignupKind != LicenseSignupKind.Paid)
        {
            throw new InvalidOperationException("Selected plan does not require payment.");
        }

        var gatewayResult = await signupBillingGateway.ConfirmTestPaymentAsync(new SignupBillingGatewayRequestModel
        {
            SessionId = session.Id,
            PlanId = plan.Id,
            PlanCode = plan.Code,
            PlanName = plan.Name,
            Amount = plan.MonthlyPrice,
            Currency = plan.Currency,
            Email = session.Email ?? string.Empty,
            OrganizationName = session.OrganizationName ?? string.Empty
        }, cancellationToken);

        var billingIntent = session.BillingIntent
            ?? throw new InvalidOperationException("Billing intent was not created.");
        billingIntent.Status = gatewayResult.Status;
        billingIntent.ExternalReference = gatewayResult.ExternalReference;
        billingIntent.MetadataJson = gatewayResult.MetadataJson;
        billingIntent.UpdatedAt = DateTime.UtcNow;
        billingIntent.ConfirmedAt = gatewayResult.Status == SignupBillingStatus.Confirmed ? DateTime.UtcNow : billingIntent.ConfirmedAt;

        session.BillingStatus = gatewayResult.Status;
        session.Status = gatewayResult.Status == SignupBillingStatus.Confirmed ? SignupSessionStatus.PaymentConfirmed : SignupSessionStatus.PaymentPending;
        session.CurrentStep = gatewayResult.Status == SignupBillingStatus.Confirmed ? "admin" : "billing";
        session.UpdatedAt = DateTime.UtcNow;

        await tenantPlatformRepository.UpsertSignupBillingIntentAsync(billingIntent, cancellationToken);
        await tenantPlatformRepository.UpdateSignupSessionAsync(session, cancellationToken);
        await WriteAuditAsync("SignupBillingConfirmed", gatewayResult.Status.ToString(), session.Email, session, cancellationToken);

        return new SignupSessionEnvelopeModel
        {
            Session = MapSession(session, session.EmailVerification)
        };
    }

    public async Task<SignupSessionEnvelopeModel> SaveAdminAsync(Guid sessionId, string accessToken, SignupAdminSetupModel model, CancellationToken cancellationToken = default)
    {
        EnsureSignupEnabled();
        var session = await ValidateSessionAccessAsync(sessionId, accessToken, cancellationToken);
        EnsureEmailVerified(session);
        EnsureOrganizationReady(session);

        session.AdminEmail = NormalizeEmail(model.Email);
        session.AdminFirstName = NormalizeRequired(model.FirstName, "First name is required.", 128);
        session.AdminLastName = NormalizeRequired(model.LastName, "Last name is required.", 128);
        session.AdminUsername = NormalizeRequired(model.Username, "Username is required.", 128);
        session.AdminBootstrapJson = JsonSerializer.Serialize(model);
        session.CurrentStep = "provision";
        session.UpdatedAt = DateTime.UtcNow;

        await tenantPlatformRepository.UpdateSignupSessionAsync(session, cancellationToken);
        await WriteAuditAsync("SignupAdminPrepared", "Success", session.Email, session, cancellationToken);

        return new SignupSessionEnvelopeModel
        {
            Session = MapSession(session, session.EmailVerification)
        };
    }

    public async Task<SignupSessionEnvelopeModel> ProvisionAsync(Guid sessionId, string accessToken, CancellationToken cancellationToken = default)
    {
        EnsureSignupEnabled();
        var session = await ValidateSessionAccessAsync(sessionId, accessToken, cancellationToken);
        return await ProvisionInternalAsync(session, cancellationToken);
    }

    public async Task CancelAsync(Guid sessionId, string accessToken, CancellationToken cancellationToken = default)
    {
        EnsureSignupEnabled();
        var session = await ValidateSessionAccessAsync(sessionId, accessToken, cancellationToken);
        session.Status = SignupSessionStatus.Cancelled;
        session.CurrentStep = "cancelled";
        session.UpdatedAt = DateTime.UtcNow;
        await tenantPlatformRepository.UpdateSignupSessionAsync(session, cancellationToken);
        await WriteAuditAsync("SignupCancelled", "Cancelled", session.Email, session, cancellationToken);
    }

    public async Task<IReadOnlyList<SignupSessionSummaryModel>> GetAdminSessionsAsync(int take, CancellationToken cancellationToken = default)
    {
        var sessions = await tenantPlatformRepository.GetSignupSessionsAsync(take, cancellationToken);
        return sessions.Select(MapSummary).ToList();
    }

    public async Task<SignupSessionEnvelopeModel> RetryProvisioningAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await tenantPlatformRepository.GetSignupSessionAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException("Signup session was not found.");
        return await ProvisionInternalAsync(session, cancellationToken);
    }

    public async Task<SignupSessionEnvelopeModel> ConfirmBillingForAdminAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await tenantPlatformRepository.GetSignupSessionAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException("Signup session was not found.");
        var billingIntent = session.BillingIntent
            ?? throw new InvalidOperationException("Billing intent was not found.");

        billingIntent.Status = SignupBillingStatus.Confirmed;
        billingIntent.ConfirmedAt = DateTime.UtcNow;
        billingIntent.UpdatedAt = DateTime.UtcNow;
        session.BillingStatus = SignupBillingStatus.Confirmed;
        session.Status = SignupSessionStatus.PaymentConfirmed;
        session.CurrentStep = "admin";
        session.UpdatedAt = DateTime.UtcNow;

        await tenantPlatformRepository.UpsertSignupBillingIntentAsync(billingIntent, cancellationToken);
        await tenantPlatformRepository.UpdateSignupSessionAsync(session, cancellationToken);
        await WriteAuditAsync("SignupBillingConfirmedByAdmin", "Confirmed", session.Email, session, cancellationToken);

        return new SignupSessionEnvelopeModel
        {
            Session = MapSession(session, session.EmailVerification)
        };
    }

    private async Task<SignupSessionEnvelopeModel> ProvisionInternalAsync(SignupSession session, CancellationToken cancellationToken)
    {
        EnsureEmailVerified(session);
        EnsureOrganizationReady(session);
        EnsureAdminReady(session);
        var plan = await GetSelectedPlanAsync(session, cancellationToken);
        EnsureTermsSatisfied(session, plan);
        EnsureBillingSatisfied(session, plan);

        try
        {
            session.ProvisioningAttemptCount += 1;
            session.Status = SignupSessionStatus.TenantProvisioning;
            session.CurrentStep = "provision";
            session.LastError = null;
            session.UpdatedAt = DateTime.UtcNow;
            await tenantPlatformRepository.UpdateSignupSessionAsync(session, cancellationToken);

            var tenant = await EnsureTenantAsync(session, cancellationToken);
            session.TenantId = tenant.Id;
            await WriteAuditAsync("SignupTenantCreated", "Success", session.Email, session, cancellationToken);

            session.Status = SignupSessionStatus.IdentityProvisioning;
            session.UpdatedAt = DateTime.UtcNow;
            await tenantPlatformRepository.UpdateSignupSessionAsync(session, cancellationToken);

            var identity = await keycloakProvisioningService.EnsureTenantIdentityAsync(tenant.Id, cancellationToken);
            if (identity.ProvisioningStatus != IdentityProvisioningStatus.Ready)
            {
                throw new InvalidOperationException(identity.LastError ?? "Tenant identity provisioning failed.");
            }
            await WriteAuditAsync("SignupIdentityProvisioned", "Success", session.Email, session, cancellationToken);

            session.Status = SignupSessionStatus.AdminBootstrap;
            session.UpdatedAt = DateTime.UtcNow;
            await tenantPlatformRepository.UpdateSignupSessionAsync(session, cancellationToken);

            var bootstrapResult = await EnsureAdminBootstrapAsync(session, identity.RealmName, cancellationToken);
            await EnsureMembershipAsync(tenant.Id, session, bootstrapResult, cancellationToken);
            await WriteAuditAsync("SignupAdminBootstrapped", "Success", session.Email, session, cancellationToken);

            session.Status = SignupSessionStatus.ConfigSeeded;
            session.UpdatedAt = DateTime.UtcNow;
            await tenantPlatformRepository.UpdateSignupSessionAsync(session, cancellationToken);

            await EnsureDefaultTenantConfigAsync(tenant.Id, session, cancellationToken);
            await EnsureTenantLicenseAsync(tenant.Id, plan, session, cancellationToken);
            await EnsureTenantOnboardingCompletedAsync(tenant.Id, session, cancellationToken);
            tenant.Status = TenantStatus.Active;
            tenant.UpdatedAt = DateTime.UtcNow;
            await tenantPlatformRepository.UpdateTenantAsync(tenant, cancellationToken);

            session.Status = SignupSessionStatus.Active;
            session.CurrentStep = "completed";
            session.UpdatedAt = DateTime.UtcNow;
            session.ActivationResultJson = JsonSerializer.Serialize(new ActivationResultEnvelope
            {
                TemporaryPassword = bootstrapResult.TemporaryPassword
            });
            await tenantPlatformRepository.UpdateSignupSessionAsync(session, cancellationToken);
            await WriteAuditAsync("SignupCompleted", "Success", session.Email, session, cancellationToken);

            return new SignupSessionEnvelopeModel
            {
                Session = MapSession(session, session.EmailVerification, bootstrapResult.TemporaryPassword)
            };
        }
        catch (Exception ex)
        {
            session.Status = SignupSessionStatus.Failed;
            session.LastError = ex.Message;
            session.UpdatedAt = DateTime.UtcNow;
            await tenantPlatformRepository.UpdateSignupSessionAsync(session, cancellationToken);
            await WriteAuditAsync("SignupProvisioningFailed", "Failed", session.Email, session, cancellationToken);
            throw;
        }
    }

    private async Task<Tenant> EnsureTenantAsync(SignupSession session, CancellationToken cancellationToken)
    {
        if (session.TenantId.HasValue)
        {
            var existing = await tenantPlatformRepository.GetTenantByIdAsync(session.TenantId.Value, cancellationToken);
            if (existing is not null)
            {
                return existing;
            }
        }

        var now = DateTime.UtcNow;
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Slug = session.TenantSlug!,
            Name = session.OrganizationName!,
            LegalName = session.OrganizationLegalName,
            BillingEmail = session.BillingEmail,
            DefaultCurrency = session.DefaultCurrency!,
            TimeZone = session.TimeZone!,
            Status = TenantStatus.Provisioning,
            IsStandaloneTenant = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        await tenantPlatformRepository.AddTenantAsync(tenant, cancellationToken);
        session.TenantId = tenant.Id;
        session.UpdatedAt = DateTime.UtcNow;
        await tenantPlatformRepository.UpdateSignupSessionAsync(session, cancellationToken);
        return tenant;
    }

    private async Task EnsureTenantLicenseAsync(Guid tenantId, LicensePlan plan, SignupSession session, CancellationToken cancellationToken)
    {
        var existing = await tenantPlatformRepository.GetLicenseAsync(tenantId, cancellationToken);
        var now = DateTime.UtcNow;
        var license = existing ?? new TenantLicense
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CreatedAt = now
        };

        license.LicensePlanId = plan.Id;
        license.Status = plan.SignupKind switch
        {
            LicenseSignupKind.Free => LicenseStatus.Active,
            LicenseSignupKind.Trial => LicenseStatus.Trial,
            LicenseSignupKind.Paid => LicenseStatus.Active,
            _ => LicenseStatus.Active
        };
        license.BillingMode = plan.SignupKind switch
        {
            LicenseSignupKind.Free => BillingMode.Free,
            LicenseSignupKind.Trial => BillingMode.Trial,
            LicenseSignupKind.Paid => BillingMode.Invoice,
            _ => BillingMode.Invoice
        };
        license.StartsAt = existing?.StartsAt ?? now;
        license.TrialEndsAt = plan.SignupKind == LicenseSignupKind.Trial && plan.TrialDays > 0 ? now.AddDays(plan.TrialDays) : null;
        license.FeatureOverridesJson = "[]";
        license.BillingCustomerReference = session.NormalizedEmail;
        license.SubscriptionReference = session.BillingIntent?.ExternalReference;
        license.UpdatedAt = now;

        await tenantPlatformRepository.UpsertLicenseAsync(license, cancellationToken);

        if (plan.SignupKind == LicenseSignupKind.Paid)
        {
            var existingTransactions = await tenantPlatformRepository.GetTransactionsAsync(tenantId, cancellationToken);
            if (!existingTransactions.Any(x =>
                    x.TransactionType == MonetizationTransactionType.SubscriptionCharge
                    && string.Equals(x.ExternalReference, session.BillingIntent?.ExternalReference, StringComparison.OrdinalIgnoreCase)))
            {
                await tenantPlatformRepository.AddMonetizationTransactionAsync(new MonetizationTransaction
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    TransactionType = MonetizationTransactionType.SubscriptionCharge,
                    Status = MonetizationTransactionStatus.Posted,
                    Amount = plan.MonthlyPrice,
                    Currency = plan.Currency,
                    PeriodStart = now,
                    PeriodEnd = now.AddMonths(1),
                    ExternalReference = session.BillingIntent?.ExternalReference,
                    MetadataJson = JsonSerializer.Serialize(new { plan = plan.Code, source = "signup" }),
                    CreatedAt = now,
                    UpdatedAt = now
                }, cancellationToken);
            }
        }
    }

    private async Task EnsureTenantOnboardingCompletedAsync(Guid tenantId, SignupSession session, CancellationToken cancellationToken)
    {
        var onboarding = await tenantPlatformRepository.GetOnboardingAsync(tenantId, cancellationToken)
            ?? new TenantOnboardingState
            {
                TenantId = tenantId,
                StartedAt = DateTime.UtcNow
            };

        onboarding.Status = OnboardingStatus.Completed;
        onboarding.CurrentStep = "completed";
        onboarding.CompletedStepsJson = "[\"signup\",\"verification\",\"organization\",\"plan\",\"billing\",\"identity\",\"admin\",\"activation\"]";
        onboarding.OrganizationProfileJson = session.OrganizationProfileJson;
        onboarding.AdminBootstrapJson = session.AdminBootstrapJson;
        onboarding.BillingSetupJson = JsonSerializer.Serialize(new
        {
            session.SelectedPlanCode,
            session.BillingStatus
        });
        onboarding.CompletedAt ??= DateTime.UtcNow;
        onboarding.UpdatedAt = DateTime.UtcNow;
        onboarding.LastError = null;

        await tenantPlatformRepository.UpsertOnboardingAsync(onboarding, cancellationToken);
    }

    private async Task EnsureDefaultTenantConfigAsync(Guid tenantId, SignupSession session, CancellationToken cancellationToken)
    {
        await UpsertSystemConfigAsync(tenantId, "branding", "supportEmail", JsonSerializer.Serialize(session.BillingEmail ?? session.Email), cancellationToken);
        await UpsertSystemConfigAsync(tenantId, "email", "defaultSenderName", JsonSerializer.Serialize(session.OrganizationName ?? "Operations"), cancellationToken);
        await UpsertSystemConfigAsync(tenantId, "email", "replyToAddress", JsonSerializer.Serialize(session.BillingEmail ?? session.Email), cancellationToken);
        await UpsertSystemConfigAsync(tenantId, "email", "signatureHtml", JsonSerializer.Serialize($"<p>{session.OrganizationName}</p>"), cancellationToken);
    }

    private async Task UpsertSystemConfigAsync(Guid tenantId, string domain, string key, string jsonValue, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var existing = await tenantPlatformRepository.GetConfigEntryAsync(tenantId, domain, key, cancellationToken);
        var entry = existing ?? new TenantConfigEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CreatedAt = now
        };
        entry.ConfigDomain = domain;
        entry.ConfigKey = key;
        entry.JsonValue = jsonValue;
        entry.IsEncrypted = false;
        entry.UpdatedByUserId = "system:signup";
        entry.UpdatedAt = now;
        await tenantPlatformRepository.UpsertConfigEntryAsync(entry, cancellationToken);
    }

    private async Task<AdminBootstrapResult> EnsureAdminBootstrapAsync(SignupSession session, string realmName, CancellationToken cancellationToken)
    {
        var existing = TryReadActivationResult(session.ActivationResultJson);
        if (!string.IsNullOrWhiteSpace(existing.UserId))
        {
            return existing;
        }

        var temporaryPassword = GenerateTemporaryPassword();
        var userId = await keycloakRealmAdminRepository.CreateUserAsync(
            realmName,
            new KeycloakAdminCreateUserInput
            {
                Username = session.AdminUsername!,
                Email = session.AdminEmail!,
                FirstName = session.AdminFirstName!,
                LastName = session.AdminLastName!,
                Enabled = true,
                EmailVerified = true,
                TemporaryPassword = temporaryPassword
            },
            ["tenant-admin"],
            cancellationToken);

        var result = new AdminBootstrapResult
        {
            UserId = userId,
            TemporaryPassword = temporaryPassword
        };

        session.ActivationResultJson = JsonSerializer.Serialize(new ActivationResultEnvelope
        {
            UserId = userId,
            TemporaryPassword = temporaryPassword
        });
        session.UpdatedAt = DateTime.UtcNow;
        await tenantPlatformRepository.UpdateSignupSessionAsync(session, cancellationToken);
        return result;
    }

    private async Task EnsureMembershipAsync(Guid tenantId, SignupSession session, AdminBootstrapResult bootstrapResult, CancellationToken cancellationToken)
    {
        var membership = await tenantPlatformRepository.GetMembershipAsync(tenantId, bootstrapResult.UserId, cancellationToken)
            ?? new TenantUserMembership
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                InvitedAt = DateTime.UtcNow
            };

        membership.UserId = bootstrapResult.UserId;
        membership.Email = session.AdminEmail!;
        membership.DisplayName = $"{session.AdminFirstName} {session.AdminLastName}".Trim();
        membership.Role = TenantUserRole.TenantAdmin;
        membership.Status = TenantUserStatus.Active;
        membership.JoinedAt ??= DateTime.UtcNow;
        membership.LastSeenAt = null;
        await tenantPlatformRepository.UpsertMembershipAsync(membership, cancellationToken);
    }

    private async Task<LicensePlan> GetSelectedPlanAsync(SignupSession session, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(session.SelectedPlanCode))
        {
            throw new InvalidOperationException("Signup plan was not selected.");
        }

        return await tenantPlatformRepository.GetLicensePlanByCodeAsync(session.SelectedPlanCode, cancellationToken)
            ?? throw new InvalidOperationException("Signup plan was not found.");
    }

    private static SignupPlanModel MapPlan(LicensePlan plan) => new()
    {
        Id = plan.Id,
        Code = plan.Code,
        Name = plan.Name,
        Description = plan.Description,
        SignupKind = plan.SignupKind,
        TrialDays = plan.TrialDays,
        MonthlyPrice = plan.MonthlyPrice,
        Currency = plan.Currency,
        RequiresTermsAcceptance = plan.RequiresTermsAcceptance,
        IncludedFeatures = ParseStringList(plan.IncludedFeaturesJson),
        Limits = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["users"] = plan.MaxUsers,
            ["integrations"] = plan.MaxIntegrations,
            ["emailAccounts"] = plan.MaxEmailAccounts,
            ["monthlyAiJobs"] = plan.MaxMonthlyAiJobs,
            ["monthlyEmailSends"] = plan.MaxMonthlyEmailSends,
            ["monthlySyncOperations"] = plan.MaxMonthlySyncOperations,
            ["storageMb"] = plan.MaxStorageMb
        }
    };

    private static SignupSessionModel MapSession(SignupSession session, SignupEmailVerification? verification, string? temporaryPassword = null)
    {
        var existingActivation = TryReadActivationResult(session.ActivationResultJson);
        return new SignupSessionModel
        {
            Id = session.Id,
            Status = session.Status,
            CurrentStep = session.CurrentStep,
            Email = session.Email,
            EmailVerified = session.EmailVerifiedAt.HasValue,
            EmailVerifiedAt = session.EmailVerifiedAt,
            VerificationSentAt = verification?.SentAt,
            VerificationExpiresAt = verification?.ExpiresAt,
            SelectedPlanId = session.SelectedPlanId,
            SelectedPlanCode = session.SelectedPlanCode,
            BillingStatus = session.BillingStatus,
            BillingIntentId = session.BillingIntentId,
            TenantId = session.TenantId,
            TermsAccepted = session.TermsAccepted,
            OrganizationName = session.OrganizationName,
            OrganizationLegalName = session.OrganizationLegalName,
            TenantSlug = session.TenantSlug,
            BillingEmail = session.BillingEmail,
            DefaultCurrency = session.DefaultCurrency,
            TimeZone = session.TimeZone,
            AdminEmail = session.AdminEmail,
            AdminFirstName = session.AdminFirstName,
            AdminLastName = session.AdminLastName,
            AdminUsername = session.AdminUsername,
            TemporaryPassword = temporaryPassword ?? existingActivation.TemporaryPassword,
            LaunchPath = session.Status == SignupSessionStatus.Active ? "/" : null,
            LastError = session.LastError,
            ExpiresAt = session.ExpiresAt,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt
        };
    }

    private static SignupSessionSummaryModel MapSummary(SignupSession session) => new()
    {
        Id = session.Id,
        Status = session.Status,
        CurrentStep = session.CurrentStep,
        Email = session.Email,
        OrganizationName = session.OrganizationName,
        TenantSlug = session.TenantSlug,
        SelectedPlanCode = session.SelectedPlanCode,
        BillingStatus = session.BillingStatus,
        TenantId = session.TenantId,
        LastError = session.LastError,
        CreatedAt = session.CreatedAt,
        UpdatedAt = session.UpdatedAt
    };

    private async Task<SignupSession> ValidateSessionAccessAsync(Guid sessionId, string accessToken, CancellationToken cancellationToken)
    {
        var session = await tenantPlatformRepository.GetSignupSessionAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException("Signup session was not found.");
        await EnsureNotExpiredAsync(session, cancellationToken);
        EnsureTokenMatch(session.AccessTokenHash, accessToken);
        return session;
    }

    private async Task EnsureNotExpiredAsync(SignupSession session, CancellationToken cancellationToken)
    {
        if (session.ExpiresAt >= DateTime.UtcNow)
        {
            return;
        }

        session.Status = SignupSessionStatus.Expired;
        session.CurrentStep = "expired";
        session.UpdatedAt = DateTime.UtcNow;
        await tenantPlatformRepository.UpdateSignupSessionAsync(session, cancellationToken);
        throw new InvalidOperationException("Signup session has expired.");
    }

    private static void EnsureTokenMatch(string tokenHash, string token)
    {
        var candidateHash = HashToken(token);
        if (!CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(tokenHash),
                Convert.FromHexString(candidateHash)))
        {
            throw new InvalidOperationException("Signup session token is invalid.");
        }
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));

    private static string GenerateToken()
    {
        Span<byte> bytes = stackalloc byte[24];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

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

    private string BuildVerificationLink(Guid sessionId, string verificationToken)
    {
        var separator = signupSettingsAccessor.PublicSignupUrl.Contains('?', StringComparison.Ordinal) ? "&" : "?";
        return $"{signupSettingsAccessor.PublicSignupUrl}{separator}session={sessionId}&verify={Uri.EscapeDataString(verificationToken)}";
    }

    private static string NormalizeEmail(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || !normalized.Contains('@', StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Valid email is required.");
        }

        return normalized;
    }

    private static string NormalizeTenantSlug(string slug)
    {
        var normalized = slug.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || !TenantSlugRegex().IsMatch(normalized))
        {
            throw new InvalidOperationException("Tenant code must use lowercase letters, numbers, and hyphen only.");
        }

        return normalized;
    }

    private static string NormalizeRequired(string value, string error, int maxLength)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException(error);
        }

        return normalized.Length > maxLength ? normalized[..maxLength] : normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        return normalized.Length > maxLength ? normalized[..maxLength] : normalized;
    }

    private static void EnsureEmailVerified(SignupSession session)
    {
        if (!session.EmailVerifiedAt.HasValue)
        {
            throw new InvalidOperationException("Email must be verified before continuing.");
        }
    }

    private static void EnsureOrganizationReady(SignupSession session)
    {
        if (string.IsNullOrWhiteSpace(session.OrganizationName)
            || string.IsNullOrWhiteSpace(session.TenantSlug)
            || string.IsNullOrWhiteSpace(session.BillingEmail)
            || string.IsNullOrWhiteSpace(session.DefaultCurrency)
            || string.IsNullOrWhiteSpace(session.TimeZone))
        {
            throw new InvalidOperationException("Organization details are incomplete.");
        }
    }

    private static void EnsureAdminReady(SignupSession session)
    {
        if (string.IsNullOrWhiteSpace(session.AdminEmail)
            || string.IsNullOrWhiteSpace(session.AdminFirstName)
            || string.IsNullOrWhiteSpace(session.AdminLastName)
            || string.IsNullOrWhiteSpace(session.AdminUsername))
        {
            throw new InvalidOperationException("Initial admin details are incomplete.");
        }
    }

    private static void EnsureTermsSatisfied(SignupSession session, LicensePlan plan)
    {
        if (plan.RequiresTermsAcceptance && !session.TermsAccepted)
        {
            throw new InvalidOperationException("Terms must be accepted before provisioning.");
        }
    }

    private static void EnsureBillingSatisfied(SignupSession session, LicensePlan plan)
    {
        if (plan.SignupKind == LicenseSignupKind.Paid && session.BillingStatus != SignupBillingStatus.Confirmed)
        {
            throw new InvalidOperationException("Billing must be confirmed before provisioning.");
        }
    }

    private bool IsSignupEnabled() =>
        signupSettingsAccessor.IsEnabled
        && (tenantExecutionContextAccessor.DeploymentMode == DeploymentMode.SaaS || signupSettingsAccessor.AllowInStandalone);

    private void EnsureSignupEnabled()
    {
        if (!IsSignupEnabled())
        {
            throw new InvalidOperationException("Public signup is not enabled for this deployment.");
        }
    }

    private async Task WriteAuditAsync(string action, string result, string? actorEmail, SignupSession session, CancellationToken cancellationToken)
    {
        await auditService.WriteAsync(new AuditEventCreateModel
        {
            TenantId = session.TenantId,
            ScopeType = "Signup",
            Action = action,
            Result = result,
            TargetEntityType = nameof(SignupSession),
            TargetEntityId = session.Id,
            MetadataJson = JsonSerializer.Serialize(new
            {
                actorEmail,
                session.Email,
                session.SelectedPlanCode,
                session.TenantSlug,
                session.Status,
                session.CurrentStep
            })
        }, cancellationToken);
    }

    private static SignupSessionStatus ResolvePostVerificationStatus(SignupSession session) =>
        session.BillingStatus == SignupBillingStatus.Confirmed ? SignupSessionStatus.PaymentConfirmed : SignupSessionStatus.PlanSelected;

    private static string ResolveNextStepAfterVerification(SignupSession session) =>
        string.IsNullOrWhiteSpace(session.OrganizationName) ? "organization" : ResolveNextStepAfterOrganization(session);

    private static string ResolveNextStepAfterOrganization(SignupSession session)
    {
        if (string.IsNullOrWhiteSpace(session.SelectedPlanCode))
        {
            return "plan";
        }

        return session.TermsAccepted ? ResolveNextStepAfterTerms(session) : "terms";
    }

    private static string ResolveNextStepAfterPlanSelection(SignupSession session, LicensePlan plan)
    {
        if (!session.EmailVerifiedAt.HasValue)
        {
            return "verification";
        }

        if (string.IsNullOrWhiteSpace(session.OrganizationName))
        {
            return "organization";
        }

        if (plan.RequiresTermsAcceptance && !session.TermsAccepted)
        {
            return "terms";
        }

        if (plan.SignupKind == LicenseSignupKind.Paid && session.BillingStatus != SignupBillingStatus.Confirmed)
        {
            return "billing";
        }

        return "admin";
    }

    private static string ResolveNextStepAfterTerms(SignupSession session) =>
        session.BillingStatus is SignupBillingStatus.Pending or SignupBillingStatus.RequiresManualReview ? "billing" : "admin";

    private static IReadOnlyList<string> ParseStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static AdminBootstrapResult TryReadActivationResult(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new AdminBootstrapResult();
        }

        try
        {
            var payload = JsonSerializer.Deserialize<ActivationResultEnvelope>(json);
            return new AdminBootstrapResult
            {
                UserId = payload?.UserId,
                TemporaryPassword = payload?.TemporaryPassword
            };
        }
        catch
        {
            return new AdminBootstrapResult();
        }
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex TenantSlugRegex();

    private sealed class ActivationResultEnvelope
    {
        public string? UserId { get; init; }
        public string? TemporaryPassword { get; init; }
    }

    private sealed class AdminBootstrapResult
    {
        public string UserId { get; init; } = string.Empty;
        public string? TemporaryPassword { get; init; }
    }
}
