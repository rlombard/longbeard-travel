using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.Forged.TourOps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenantPlatformFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailProviderConnections_OwnerUserId_IsDefaultConnection",
                table: "EmailProviderConnections");

            migrationBuilder.DropIndex(
                name: "IX_EmailProviderConnections_OwnerUserId_MailboxAddress",
                table: "EmailProviderConnections");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:billing_mode", "standalone,trial,invoice,external_subscription")
                .Annotation("Npgsql:Enum:booking_item_status", "pending,requested,confirmed,cancelled")
                .Annotation("Npgsql:Enum:booking_status", "draft,confirmed,cancelled")
                .Annotation("Npgsql:Enum:customer_budget_band", "unknown,economy,standard,premium,luxury")
                .Annotation("Npgsql:Enum:customer_verification_status", "not_started,pending,verified,rejected,expired")
                .Annotation("Npgsql:Enum:deployment_mode", "standalone,saa_s")
                .Annotation("Npgsql:Enum:email_classification_type", "confirmation_received,partial_confirmation,needs_more_information,pricing_changed,availability_issue,no_action_needed,human_decision_required,unclear")
                .Annotation("Npgsql:Enum:email_direction", "inbound,outbound")
                .Annotation("Npgsql:Enum:email_draft_generated_by", "human,ai")
                .Annotation("Npgsql:Enum:email_draft_status", "draft,approved,sent,rejected")
                .Annotation("Npgsql:Enum:email_integration_auth_method", "o_auth2,api_key,password")
                .Annotation("Npgsql:Enum:email_integration_provider_type", "microsoft365,gmail,mailcow,send_grid,smtp_direct,generic_imap_smtp")
                .Annotation("Npgsql:Enum:email_integration_status", "draft,pending_authorization,active,needs_reconnect,error,disabled,revoked")
                .Annotation("Npgsql:Enum:human_approval_status", "pending,approved,rejected")
                .Annotation("Npgsql:Enum:identity_isolation_mode", "shared_realm,realm_per_tenant")
                .Annotation("Npgsql:Enum:identity_provisioning_status", "pending,ready,failed")
                .Annotation("Npgsql:Enum:invoice_status", "draft,received,matched,unmatched,pending_review,approved,rejected,unpaid,partially_paid,paid,overdue,rebate_pending,rebate_applied,cancelled")
                .Annotation("Npgsql:Enum:itinerary_draft_status", "draft,approved,rejected")
                .Annotation("Npgsql:Enum:license_status", "trial,active,suspended,expired,cancelled")
                .Annotation("Npgsql:Enum:monetization_transaction_status", "pending,posted,failed,voided")
                .Annotation("Npgsql:Enum:monetization_transaction_type", "usage_charge,subscription_charge,credit,adjustment")
                .Annotation("Npgsql:Enum:onboarding_status", "not_started,in_progress,completed,blocked")
                .Annotation("Npgsql:Enum:preferred_contact_method", "email,phone,whats_app,any")
                .Annotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .Annotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .Annotation("Npgsql:Enum:quote_status", "draft,generated")
                .Annotation("Npgsql:Enum:task_status", "to_do,waiting,follow_up,blocked,done")
                .Annotation("Npgsql:Enum:task_suggestion_state", "pending_review,accepted,rejected")
                .Annotation("Npgsql:Enum:tenant_status", "provisioning,active,suspended,disabled")
                .Annotation("Npgsql:Enum:tenant_user_role", "platform_admin,tenant_admin,operator")
                .Annotation("Npgsql:Enum:tenant_user_status", "invited,active,disabled")
                .Annotation("Npgsql:Enum:travel_pace", "unknown,relaxed,balanced,fast")
                .Annotation("Npgsql:Enum:travel_value_leaning", "unknown,value,balanced,luxury")
                .OldAnnotation("Npgsql:Enum:booking_item_status", "pending,requested,confirmed,cancelled")
                .OldAnnotation("Npgsql:Enum:booking_status", "draft,confirmed,cancelled")
                .OldAnnotation("Npgsql:Enum:customer_budget_band", "unknown,economy,standard,premium,luxury")
                .OldAnnotation("Npgsql:Enum:customer_verification_status", "not_started,pending,verified,rejected,expired")
                .OldAnnotation("Npgsql:Enum:email_classification_type", "confirmation_received,partial_confirmation,needs_more_information,pricing_changed,availability_issue,no_action_needed,human_decision_required,unclear")
                .OldAnnotation("Npgsql:Enum:email_direction", "inbound,outbound")
                .OldAnnotation("Npgsql:Enum:email_draft_generated_by", "human,ai")
                .OldAnnotation("Npgsql:Enum:email_draft_status", "draft,approved,sent,rejected")
                .OldAnnotation("Npgsql:Enum:email_integration_auth_method", "o_auth2,api_key,password")
                .OldAnnotation("Npgsql:Enum:email_integration_provider_type", "microsoft365,gmail,mailcow,send_grid,smtp_direct,generic_imap_smtp")
                .OldAnnotation("Npgsql:Enum:email_integration_status", "draft,pending_authorization,active,needs_reconnect,error,disabled,revoked")
                .OldAnnotation("Npgsql:Enum:human_approval_status", "pending,approved,rejected")
                .OldAnnotation("Npgsql:Enum:invoice_status", "draft,received,matched,unmatched,pending_review,approved,rejected,unpaid,partially_paid,paid,overdue,rebate_pending,rebate_applied,cancelled")
                .OldAnnotation("Npgsql:Enum:itinerary_draft_status", "draft,approved,rejected")
                .OldAnnotation("Npgsql:Enum:preferred_contact_method", "email,phone,whats_app,any")
                .OldAnnotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .OldAnnotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .OldAnnotation("Npgsql:Enum:quote_status", "draft,generated")
                .OldAnnotation("Npgsql:Enum:task_status", "to_do,waiting,follow_up,blocked,done")
                .OldAnnotation("Npgsql:Enum:task_suggestion_state", "pending_review,accepted,rejected")
                .OldAnnotation("Npgsql:Enum:travel_pace", "unknown,relaxed,balanced,fast")
                .OldAnnotation("Npgsql:Enum:travel_value_leaning", "unknown,value,balanced,luxury");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "TaskSuggestions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Tasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Suppliers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Rates",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Quotes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "QuoteLineItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ProductValidityPeriods",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ProductRooms",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ProductRateTypes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ProductRateBases",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ProductMealBases",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ProductExtras",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ProductContacts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "PaymentRecords",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "LlmAuditLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ItineraryItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ItineraryDrafts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ItineraryDraftItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Itineraries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Invoices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "InvoiceLineItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "InvoiceAttachments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "HumanApprovalRequests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "EmailThreads",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "EmailProviderMessageLinks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "EmailProviderConnections",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "EmailMessages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "EmailDrafts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Customers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "CustomerPreferenceProfiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "CustomerKycProfiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "CustomerAuditLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "BookingTravellers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Bookings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "BookingItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "LicensePlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsStandalonePlan = table.Column<bool>(type: "boolean", nullable: false),
                    MaxUsers = table.Column<int>(type: "integer", nullable: false),
                    MaxIntegrations = table.Column<int>(type: "integer", nullable: false),
                    MaxEmailAccounts = table.Column<int>(type: "integer", nullable: false),
                    MaxMonthlyAiJobs = table.Column<int>(type: "integer", nullable: false),
                    MaxMonthlyEmailSends = table.Column<int>(type: "integer", nullable: false),
                    MaxMonthlySyncOperations = table.Column<int>(type: "integer", nullable: false),
                    MaxStorageMb = table.Column<int>(type: "integer", nullable: false),
                    IncludedFeaturesJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicensePlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LegalName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    BillingEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DefaultCurrency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    TimeZone = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsStandaloneTenant = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScopeType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Action = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Result = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ActorUserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ActorDisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TargetEntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TargetEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    MetadataJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditEvents_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TenantConfigEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfigDomain = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ConfigKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    JsonValue = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: false),
                    IsEncrypted = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantConfigEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantConfigEntries_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantIdentityMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsolationMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProvisioningStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RealmName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ClientId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    IssuerUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MetadataJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantIdentityMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantIdentityMappings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantLicenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    LicensePlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BillingMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TrialEndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SuspendedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MaxUsersOverride = table.Column<int>(type: "integer", nullable: true),
                    MaxIntegrationsOverride = table.Column<int>(type: "integer", nullable: true),
                    MaxEmailAccountsOverride = table.Column<int>(type: "integer", nullable: true),
                    MaxMonthlyAiJobsOverride = table.Column<int>(type: "integer", nullable: true),
                    MaxMonthlyEmailSendsOverride = table.Column<int>(type: "integer", nullable: true),
                    MaxMonthlySyncOperationsOverride = table.Column<int>(type: "integer", nullable: true),
                    MaxStorageMbOverride = table.Column<int>(type: "integer", nullable: true),
                    FeatureOverridesJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    BillingCustomerReference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SubscriptionReference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantLicenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantLicenses_LicensePlans_LicensePlanId",
                        column: x => x.LicensePlanId,
                        principalTable: "LicensePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantLicenses_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantOnboardingStates",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CurrentStep = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CompletedStepsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    OrganizationProfileJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    AdminBootstrapJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    EmailSetupJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    BillingSetupJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantOnboardingStates", x => x.TenantId);
                    table.ForeignKey(
                        name: "FK_TenantOnboardingStates_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantUserMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    InvitedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantUserMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantUserMemberships_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsageRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MetricKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsBillable = table.Column<bool>(type: "boolean", nullable: false),
                    Source = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReferenceEntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReferenceEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    MetadataJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsageRecords_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonetizationTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsageRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransactionType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExternalReference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    MetadataJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonetizationTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonetizationTransactions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MonetizationTransactions_UsageRecords_UsageRecordId",
                        column: x => x.UsageRecordId,
                        principalTable: "UsageRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskSuggestions_TenantId",
                table: "TaskSuggestions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TenantId",
                table: "Tasks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_TenantId",
                table: "Suppliers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Rates_TenantId",
                table: "Rates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_TenantId",
                table: "Quotes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteLineItems_TenantId",
                table: "QuoteLineItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductValidityPeriods_TenantId",
                table: "ProductValidityPeriods",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TenantId",
                table: "Products",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRooms_TenantId",
                table: "ProductRooms",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRateTypes_TenantId",
                table: "ProductRateTypes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRateBases_TenantId",
                table: "ProductRateBases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMealBases_TenantId",
                table: "ProductMealBases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductExtras_TenantId",
                table: "ProductExtras",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductContacts_TenantId",
                table: "ProductContacts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecords_TenantId",
                table: "PaymentRecords",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LlmAuditLogs_TenantId",
                table: "LlmAuditLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ItineraryItems_TenantId",
                table: "ItineraryItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ItineraryDrafts_TenantId",
                table: "ItineraryDrafts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ItineraryDraftItems_TenantId",
                table: "ItineraryDraftItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Itineraries_TenantId",
                table: "Itineraries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_TenantId",
                table: "Invoices",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLineItems_TenantId",
                table: "InvoiceLineItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceAttachments_TenantId",
                table: "InvoiceAttachments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_HumanApprovalRequests_TenantId",
                table: "HumanApprovalRequests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailThreads_TenantId",
                table: "EmailThreads",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailProviderMessageLinks_TenantId",
                table: "EmailProviderMessageLinks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailProviderConnections_TenantId",
                table: "EmailProviderConnections",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailProviderConnections_TenantId_OwnerUserId_IsDefaultConn~",
                table: "EmailProviderConnections",
                columns: new[] { "TenantId", "OwnerUserId", "IsDefaultConnection" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailProviderConnections_TenantId_OwnerUserId_MailboxAddress",
                table: "EmailProviderConnections",
                columns: new[] { "TenantId", "OwnerUserId", "MailboxAddress" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_TenantId",
                table: "EmailMessages",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailDrafts_TenantId",
                table: "EmailDrafts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TenantId",
                table: "Customers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPreferenceProfiles_TenantId",
                table: "CustomerPreferenceProfiles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerKycProfiles_TenantId",
                table: "CustomerKycProfiles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAuditLogs_TenantId",
                table: "CustomerAuditLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingTravellers_TenantId",
                table: "BookingTravellers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TenantId",
                table: "Bookings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingItems_TenantId",
                table: "BookingItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_Action",
                table: "AuditEvents",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_TenantId_CreatedAt",
                table: "AuditEvents",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LicensePlans_Code",
                table: "LicensePlans",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonetizationTransactions_CreatedAt",
                table: "MonetizationTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MonetizationTransactions_TenantId",
                table: "MonetizationTransactions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MonetizationTransactions_UsageRecordId",
                table: "MonetizationTransactions",
                column: "UsageRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantConfigEntries_TenantId_ConfigDomain_ConfigKey",
                table: "TenantConfigEntries",
                columns: new[] { "TenantId", "ConfigDomain", "ConfigKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantIdentityMappings_TenantId_RealmName",
                table: "TenantIdentityMappings",
                columns: new[] { "TenantId", "RealmName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantLicenses_LicensePlanId",
                table: "TenantLicenses",
                column: "LicensePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantLicenses_TenantId",
                table: "TenantLicenses",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_IsStandaloneTenant",
                table: "Tenants",
                column: "IsStandaloneTenant");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Slug",
                table: "Tenants",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantUserMemberships_TenantId_UserId",
                table: "TenantUserMemberships",
                columns: new[] { "TenantId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantUserMemberships_UserId_Status",
                table: "TenantUserMemberships",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_UsageRecords_TenantId_MetricKey_OccurredAt",
                table: "UsageRecords",
                columns: new[] { "TenantId", "MetricKey", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditEvents");

            migrationBuilder.DropTable(
                name: "MonetizationTransactions");

            migrationBuilder.DropTable(
                name: "TenantConfigEntries");

            migrationBuilder.DropTable(
                name: "TenantIdentityMappings");

            migrationBuilder.DropTable(
                name: "TenantLicenses");

            migrationBuilder.DropTable(
                name: "TenantOnboardingStates");

            migrationBuilder.DropTable(
                name: "TenantUserMemberships");

            migrationBuilder.DropTable(
                name: "UsageRecords");

            migrationBuilder.DropTable(
                name: "LicensePlans");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_TaskSuggestions_TenantId",
                table: "TaskSuggestions");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_TenantId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_TenantId",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Rates_TenantId",
                table: "Rates");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_TenantId",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_QuoteLineItems_TenantId",
                table: "QuoteLineItems");

            migrationBuilder.DropIndex(
                name: "IX_ProductValidityPeriods_TenantId",
                table: "ProductValidityPeriods");

            migrationBuilder.DropIndex(
                name: "IX_Products_TenantId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductRooms_TenantId",
                table: "ProductRooms");

            migrationBuilder.DropIndex(
                name: "IX_ProductRateTypes_TenantId",
                table: "ProductRateTypes");

            migrationBuilder.DropIndex(
                name: "IX_ProductRateBases_TenantId",
                table: "ProductRateBases");

            migrationBuilder.DropIndex(
                name: "IX_ProductMealBases_TenantId",
                table: "ProductMealBases");

            migrationBuilder.DropIndex(
                name: "IX_ProductExtras_TenantId",
                table: "ProductExtras");

            migrationBuilder.DropIndex(
                name: "IX_ProductContacts_TenantId",
                table: "ProductContacts");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRecords_TenantId",
                table: "PaymentRecords");

            migrationBuilder.DropIndex(
                name: "IX_LlmAuditLogs_TenantId",
                table: "LlmAuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_ItineraryItems_TenantId",
                table: "ItineraryItems");

            migrationBuilder.DropIndex(
                name: "IX_ItineraryDrafts_TenantId",
                table: "ItineraryDrafts");

            migrationBuilder.DropIndex(
                name: "IX_ItineraryDraftItems_TenantId",
                table: "ItineraryDraftItems");

            migrationBuilder.DropIndex(
                name: "IX_Itineraries_TenantId",
                table: "Itineraries");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_TenantId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceLineItems_TenantId",
                table: "InvoiceLineItems");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceAttachments_TenantId",
                table: "InvoiceAttachments");

            migrationBuilder.DropIndex(
                name: "IX_HumanApprovalRequests_TenantId",
                table: "HumanApprovalRequests");

            migrationBuilder.DropIndex(
                name: "IX_EmailThreads_TenantId",
                table: "EmailThreads");

            migrationBuilder.DropIndex(
                name: "IX_EmailProviderMessageLinks_TenantId",
                table: "EmailProviderMessageLinks");

            migrationBuilder.DropIndex(
                name: "IX_EmailProviderConnections_TenantId",
                table: "EmailProviderConnections");

            migrationBuilder.DropIndex(
                name: "IX_EmailProviderConnections_TenantId_OwnerUserId_IsDefaultConn~",
                table: "EmailProviderConnections");

            migrationBuilder.DropIndex(
                name: "IX_EmailProviderConnections_TenantId_OwnerUserId_MailboxAddress",
                table: "EmailProviderConnections");

            migrationBuilder.DropIndex(
                name: "IX_EmailMessages_TenantId",
                table: "EmailMessages");

            migrationBuilder.DropIndex(
                name: "IX_EmailDrafts_TenantId",
                table: "EmailDrafts");

            migrationBuilder.DropIndex(
                name: "IX_Customers_TenantId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_CustomerPreferenceProfiles_TenantId",
                table: "CustomerPreferenceProfiles");

            migrationBuilder.DropIndex(
                name: "IX_CustomerKycProfiles_TenantId",
                table: "CustomerKycProfiles");

            migrationBuilder.DropIndex(
                name: "IX_CustomerAuditLogs_TenantId",
                table: "CustomerAuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_BookingTravellers_TenantId",
                table: "BookingTravellers");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TenantId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_BookingItems_TenantId",
                table: "BookingItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "TaskSuggestions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Rates");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "QuoteLineItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductValidityPeriods");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductRooms");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductRateTypes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductRateBases");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductMealBases");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductExtras");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductContacts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PaymentRecords");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "LlmAuditLogs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ItineraryItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ItineraryDrafts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ItineraryDraftItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Itineraries");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "InvoiceLineItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "InvoiceAttachments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "HumanApprovalRequests");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EmailThreads");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EmailProviderMessageLinks");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EmailProviderConnections");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EmailMessages");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EmailDrafts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CustomerPreferenceProfiles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CustomerKycProfiles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CustomerAuditLogs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "BookingTravellers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "BookingItems");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:booking_item_status", "pending,requested,confirmed,cancelled")
                .Annotation("Npgsql:Enum:booking_status", "draft,confirmed,cancelled")
                .Annotation("Npgsql:Enum:customer_budget_band", "unknown,economy,standard,premium,luxury")
                .Annotation("Npgsql:Enum:customer_verification_status", "not_started,pending,verified,rejected,expired")
                .Annotation("Npgsql:Enum:email_classification_type", "confirmation_received,partial_confirmation,needs_more_information,pricing_changed,availability_issue,no_action_needed,human_decision_required,unclear")
                .Annotation("Npgsql:Enum:email_direction", "inbound,outbound")
                .Annotation("Npgsql:Enum:email_draft_generated_by", "human,ai")
                .Annotation("Npgsql:Enum:email_draft_status", "draft,approved,sent,rejected")
                .Annotation("Npgsql:Enum:email_integration_auth_method", "o_auth2,api_key,password")
                .Annotation("Npgsql:Enum:email_integration_provider_type", "microsoft365,gmail,mailcow,send_grid,smtp_direct,generic_imap_smtp")
                .Annotation("Npgsql:Enum:email_integration_status", "draft,pending_authorization,active,needs_reconnect,error,disabled,revoked")
                .Annotation("Npgsql:Enum:human_approval_status", "pending,approved,rejected")
                .Annotation("Npgsql:Enum:invoice_status", "draft,received,matched,unmatched,pending_review,approved,rejected,unpaid,partially_paid,paid,overdue,rebate_pending,rebate_applied,cancelled")
                .Annotation("Npgsql:Enum:itinerary_draft_status", "draft,approved,rejected")
                .Annotation("Npgsql:Enum:preferred_contact_method", "email,phone,whats_app,any")
                .Annotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .Annotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .Annotation("Npgsql:Enum:quote_status", "draft,generated")
                .Annotation("Npgsql:Enum:task_status", "to_do,waiting,follow_up,blocked,done")
                .Annotation("Npgsql:Enum:task_suggestion_state", "pending_review,accepted,rejected")
                .Annotation("Npgsql:Enum:travel_pace", "unknown,relaxed,balanced,fast")
                .Annotation("Npgsql:Enum:travel_value_leaning", "unknown,value,balanced,luxury")
                .OldAnnotation("Npgsql:Enum:billing_mode", "standalone,trial,invoice,external_subscription")
                .OldAnnotation("Npgsql:Enum:booking_item_status", "pending,requested,confirmed,cancelled")
                .OldAnnotation("Npgsql:Enum:booking_status", "draft,confirmed,cancelled")
                .OldAnnotation("Npgsql:Enum:customer_budget_band", "unknown,economy,standard,premium,luxury")
                .OldAnnotation("Npgsql:Enum:customer_verification_status", "not_started,pending,verified,rejected,expired")
                .OldAnnotation("Npgsql:Enum:deployment_mode", "standalone,saa_s")
                .OldAnnotation("Npgsql:Enum:email_classification_type", "confirmation_received,partial_confirmation,needs_more_information,pricing_changed,availability_issue,no_action_needed,human_decision_required,unclear")
                .OldAnnotation("Npgsql:Enum:email_direction", "inbound,outbound")
                .OldAnnotation("Npgsql:Enum:email_draft_generated_by", "human,ai")
                .OldAnnotation("Npgsql:Enum:email_draft_status", "draft,approved,sent,rejected")
                .OldAnnotation("Npgsql:Enum:email_integration_auth_method", "o_auth2,api_key,password")
                .OldAnnotation("Npgsql:Enum:email_integration_provider_type", "microsoft365,gmail,mailcow,send_grid,smtp_direct,generic_imap_smtp")
                .OldAnnotation("Npgsql:Enum:email_integration_status", "draft,pending_authorization,active,needs_reconnect,error,disabled,revoked")
                .OldAnnotation("Npgsql:Enum:human_approval_status", "pending,approved,rejected")
                .OldAnnotation("Npgsql:Enum:identity_isolation_mode", "shared_realm,realm_per_tenant")
                .OldAnnotation("Npgsql:Enum:identity_provisioning_status", "pending,ready,failed")
                .OldAnnotation("Npgsql:Enum:invoice_status", "draft,received,matched,unmatched,pending_review,approved,rejected,unpaid,partially_paid,paid,overdue,rebate_pending,rebate_applied,cancelled")
                .OldAnnotation("Npgsql:Enum:itinerary_draft_status", "draft,approved,rejected")
                .OldAnnotation("Npgsql:Enum:license_status", "trial,active,suspended,expired,cancelled")
                .OldAnnotation("Npgsql:Enum:monetization_transaction_status", "pending,posted,failed,voided")
                .OldAnnotation("Npgsql:Enum:monetization_transaction_type", "usage_charge,subscription_charge,credit,adjustment")
                .OldAnnotation("Npgsql:Enum:onboarding_status", "not_started,in_progress,completed,blocked")
                .OldAnnotation("Npgsql:Enum:preferred_contact_method", "email,phone,whats_app,any")
                .OldAnnotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .OldAnnotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .OldAnnotation("Npgsql:Enum:quote_status", "draft,generated")
                .OldAnnotation("Npgsql:Enum:task_status", "to_do,waiting,follow_up,blocked,done")
                .OldAnnotation("Npgsql:Enum:task_suggestion_state", "pending_review,accepted,rejected")
                .OldAnnotation("Npgsql:Enum:tenant_status", "provisioning,active,suspended,disabled")
                .OldAnnotation("Npgsql:Enum:tenant_user_role", "platform_admin,tenant_admin,operator")
                .OldAnnotation("Npgsql:Enum:tenant_user_status", "invited,active,disabled")
                .OldAnnotation("Npgsql:Enum:travel_pace", "unknown,relaxed,balanced,fast")
                .OldAnnotation("Npgsql:Enum:travel_value_leaning", "unknown,value,balanced,luxury");

            migrationBuilder.CreateIndex(
                name: "IX_EmailProviderConnections_OwnerUserId_IsDefaultConnection",
                table: "EmailProviderConnections",
                columns: new[] { "OwnerUserId", "IsDefaultConnection" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailProviderConnections_OwnerUserId_MailboxAddress",
                table: "EmailProviderConnections",
                columns: new[] { "OwnerUserId", "MailboxAddress" });
        }
    }
}
