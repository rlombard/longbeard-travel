using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.Forged.TourOps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSaasSignupOnboarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:billing_mode", "standalone,trial,invoice,external_subscription,free")
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
                .Annotation("Npgsql:Enum:license_signup_kind", "hidden,free,trial,paid")
                .Annotation("Npgsql:Enum:license_status", "trial,active,suspended,expired,cancelled")
                .Annotation("Npgsql:Enum:monetization_transaction_status", "pending,posted,failed,voided")
                .Annotation("Npgsql:Enum:monetization_transaction_type", "usage_charge,subscription_charge,credit,adjustment")
                .Annotation("Npgsql:Enum:onboarding_status", "not_started,in_progress,completed,blocked")
                .Annotation("Npgsql:Enum:preferred_contact_method", "email,phone,whats_app,any")
                .Annotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .Annotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .Annotation("Npgsql:Enum:quote_status", "draft,generated")
                .Annotation("Npgsql:Enum:signup_billing_status", "not_required,pending,confirmed,failed,cancelled,requires_manual_review")
                .Annotation("Npgsql:Enum:signup_session_status", "draft,email_pending,email_verified,plan_selected,payment_pending,payment_confirmed,tenant_provisioning,identity_provisioning,admin_bootstrap,config_seeded,active,failed,cancelled,expired")
                .Annotation("Npgsql:Enum:task_status", "to_do,waiting,follow_up,blocked,done")
                .Annotation("Npgsql:Enum:task_suggestion_state", "pending_review,accepted,rejected")
                .Annotation("Npgsql:Enum:tenant_status", "provisioning,active,suspended,disabled")
                .Annotation("Npgsql:Enum:tenant_user_role", "platform_admin,tenant_admin,operator")
                .Annotation("Npgsql:Enum:tenant_user_status", "invited,active,disabled")
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

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "LicensePlans",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublicSignupEnabled",
                table: "LicensePlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyPrice",
                table: "LicensePlans",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresTermsAcceptance",
                table: "LicensePlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SignupKind",
                table: "LicensePlans",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SignupSortOrder",
                table: "LicensePlans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TrialDays",
                table: "LicensePlans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SignupSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessTokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CurrentStep = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailVerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SelectedPlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    SelectedPlanCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BillingStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BillingIntentId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    TermsAccepted = table.Column<bool>(type: "boolean", nullable: false),
                    TermsAcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OrganizationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OrganizationLegalName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TenantSlug = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BillingEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DefaultCurrency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    TimeZone = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OrganizationProfileJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    AdminEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AdminFirstName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AdminLastName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AdminUsername = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AdminBootstrapJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    ActivationResultJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ProvisioningAttemptCount = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignupSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignupSessions_LicensePlans_SelectedPlanId",
                        column: x => x.SelectedPlanId,
                        principalTable: "LicensePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SignupSessions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SignupBillingIntents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SignupSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    LicensePlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BillingMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExternalReference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CheckoutUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MetadataJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignupBillingIntents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignupBillingIntents_LicensePlans_LicensePlanId",
                        column: x => x.LicensePlanId,
                        principalTable: "LicensePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SignupBillingIntents_SignupSessions_SignupSessionId",
                        column: x => x.SignupSessionId,
                        principalTable: "SignupSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SignupEmailVerifications",
                columns: table => new
                {
                    SignupSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastSentEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SendCount = table.Column<int>(type: "integer", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConsumedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAttemptIpAddress = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignupEmailVerifications", x => x.SignupSessionId);
                    table.ForeignKey(
                        name: "FK_SignupEmailVerifications_SignupSessions_SignupSessionId",
                        column: x => x.SignupSessionId,
                        principalTable: "SignupSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LicensePlans_IsPublicSignupEnabled_SignupSortOrder",
                table: "LicensePlans",
                columns: new[] { "IsPublicSignupEnabled", "SignupSortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SignupBillingIntents_LicensePlanId",
                table: "SignupBillingIntents",
                column: "LicensePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SignupBillingIntents_SignupSessionId",
                table: "SignupBillingIntents",
                column: "SignupSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SignupBillingIntents_Status",
                table: "SignupBillingIntents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SignupEmailVerifications_ExpiresAt",
                table: "SignupEmailVerifications",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_SignupSessions_ExpiresAt",
                table: "SignupSessions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_SignupSessions_NormalizedEmail",
                table: "SignupSessions",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_SignupSessions_SelectedPlanId",
                table: "SignupSessions",
                column: "SelectedPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SignupSessions_Status",
                table: "SignupSessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SignupSessions_TenantId",
                table: "SignupSessions",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SignupBillingIntents");

            migrationBuilder.DropTable(
                name: "SignupEmailVerifications");

            migrationBuilder.DropTable(
                name: "SignupSessions");

            migrationBuilder.DropIndex(
                name: "IX_LicensePlans_IsPublicSignupEnabled_SignupSortOrder",
                table: "LicensePlans");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "LicensePlans");

            migrationBuilder.DropColumn(
                name: "IsPublicSignupEnabled",
                table: "LicensePlans");

            migrationBuilder.DropColumn(
                name: "MonthlyPrice",
                table: "LicensePlans");

            migrationBuilder.DropColumn(
                name: "RequiresTermsAcceptance",
                table: "LicensePlans");

            migrationBuilder.DropColumn(
                name: "SignupKind",
                table: "LicensePlans");

            migrationBuilder.DropColumn(
                name: "SignupSortOrder",
                table: "LicensePlans");

            migrationBuilder.DropColumn(
                name: "TrialDays",
                table: "LicensePlans");

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
                .OldAnnotation("Npgsql:Enum:billing_mode", "standalone,trial,invoice,external_subscription,free")
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
                .OldAnnotation("Npgsql:Enum:license_signup_kind", "hidden,free,trial,paid")
                .OldAnnotation("Npgsql:Enum:license_status", "trial,active,suspended,expired,cancelled")
                .OldAnnotation("Npgsql:Enum:monetization_transaction_status", "pending,posted,failed,voided")
                .OldAnnotation("Npgsql:Enum:monetization_transaction_type", "usage_charge,subscription_charge,credit,adjustment")
                .OldAnnotation("Npgsql:Enum:onboarding_status", "not_started,in_progress,completed,blocked")
                .OldAnnotation("Npgsql:Enum:preferred_contact_method", "email,phone,whats_app,any")
                .OldAnnotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .OldAnnotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .OldAnnotation("Npgsql:Enum:quote_status", "draft,generated")
                .OldAnnotation("Npgsql:Enum:signup_billing_status", "not_required,pending,confirmed,failed,cancelled,requires_manual_review")
                .OldAnnotation("Npgsql:Enum:signup_session_status", "draft,email_pending,email_verified,plan_selected,payment_pending,payment_confirmed,tenant_provisioning,identity_provisioning,admin_bootstrap,config_seeded,active,failed,cancelled,expired")
                .OldAnnotation("Npgsql:Enum:task_status", "to_do,waiting,follow_up,blocked,done")
                .OldAnnotation("Npgsql:Enum:task_suggestion_state", "pending_review,accepted,rejected")
                .OldAnnotation("Npgsql:Enum:tenant_status", "provisioning,active,suspended,disabled")
                .OldAnnotation("Npgsql:Enum:tenant_user_role", "platform_admin,tenant_admin,operator")
                .OldAnnotation("Npgsql:Enum:tenant_user_status", "invited,active,disabled")
                .OldAnnotation("Npgsql:Enum:travel_pace", "unknown,relaxed,balanced,fast")
                .OldAnnotation("Npgsql:Enum:travel_value_leaning", "unknown,value,balanced,luxury");
        }
    }
}
