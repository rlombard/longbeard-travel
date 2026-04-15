using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.Forged.TourOps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailProviderIntegrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                .OldAnnotation("Npgsql:Enum:booking_item_status", "pending,requested,confirmed,cancelled")
                .OldAnnotation("Npgsql:Enum:booking_status", "draft,confirmed,cancelled")
                .OldAnnotation("Npgsql:Enum:customer_budget_band", "unknown,economy,standard,premium,luxury")
                .OldAnnotation("Npgsql:Enum:customer_verification_status", "not_started,pending,verified,rejected,expired")
                .OldAnnotation("Npgsql:Enum:email_classification_type", "confirmation_received,partial_confirmation,needs_more_information,pricing_changed,availability_issue,no_action_needed,human_decision_required,unclear")
                .OldAnnotation("Npgsql:Enum:email_direction", "inbound,outbound")
                .OldAnnotation("Npgsql:Enum:email_draft_generated_by", "human,ai")
                .OldAnnotation("Npgsql:Enum:email_draft_status", "draft,approved,sent,rejected")
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

            migrationBuilder.CreateTable(
                name: "EmailProviderConnections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ConnectionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProviderType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AuthMethod = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MailboxAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ExternalAccountId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AllowSend = table.Column<bool>(type: "boolean", nullable: false),
                    AllowSync = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefaultConnection = table.Column<bool>(type: "boolean", nullable: false),
                    ConnectionSettingsJson = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: true),
                    EncryptedCredentialsJson = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: true),
                    AccessTokenExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OAuthState = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    OAuthStateExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OAuthReturnUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SyncCursorJson = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: true),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextSyncAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSuccessfulSendAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastTestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    WebhookSubscriptionId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    WebhookSubscriptionExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailProviderConnections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailProviderMessageLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailProviderConnectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderMessageId = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ProviderThreadId = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    EmailThreadId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    FolderName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailProviderMessageLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailProviderMessageLinks_EmailMessages_EmailMessageId",
                        column: x => x.EmailMessageId,
                        principalTable: "EmailMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailProviderMessageLinks_EmailProviderConnections_EmailPro~",
                        column: x => x.EmailProviderConnectionId,
                        principalTable: "EmailProviderConnections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailProviderMessageLinks_EmailThreads_EmailThreadId",
                        column: x => x.EmailThreadId,
                        principalTable: "EmailThreads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailProviderConnections_OAuthState",
                table: "EmailProviderConnections",
                column: "OAuthState");

            migrationBuilder.CreateIndex(
                name: "IX_EmailProviderConnections_OwnerUserId",
                table: "EmailProviderConnections",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailProviderConnections_OwnerUserId_IsDefaultConnection",
                table: "EmailProviderConnections",
                columns: new[] { "OwnerUserId", "IsDefaultConnection" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailProviderConnections_OwnerUserId_MailboxAddress",
                table: "EmailProviderConnections",
                columns: new[] { "OwnerUserId", "MailboxAddress" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailProviderConnections_Status_NextSyncAt",
                table: "EmailProviderConnections",
                columns: new[] { "Status", "NextSyncAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailProviderMessageLinks_EmailMessageId",
                table: "EmailProviderMessageLinks",
                column: "EmailMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailProviderMessageLinks_EmailProviderConnectionId_Provid~1",
                table: "EmailProviderMessageLinks",
                columns: new[] { "EmailProviderConnectionId", "ProviderThreadId" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailProviderMessageLinks_EmailProviderConnectionId_Provide~",
                table: "EmailProviderMessageLinks",
                columns: new[] { "EmailProviderConnectionId", "ProviderMessageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailProviderMessageLinks_EmailThreadId",
                table: "EmailProviderMessageLinks",
                column: "EmailThreadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailProviderMessageLinks");

            migrationBuilder.DropTable(
                name: "EmailProviderConnections");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:booking_item_status", "pending,requested,confirmed,cancelled")
                .Annotation("Npgsql:Enum:booking_status", "draft,confirmed,cancelled")
                .Annotation("Npgsql:Enum:customer_budget_band", "unknown,economy,standard,premium,luxury")
                .Annotation("Npgsql:Enum:customer_verification_status", "not_started,pending,verified,rejected,expired")
                .Annotation("Npgsql:Enum:email_classification_type", "confirmation_received,partial_confirmation,needs_more_information,pricing_changed,availability_issue,no_action_needed,human_decision_required,unclear")
                .Annotation("Npgsql:Enum:email_direction", "inbound,outbound")
                .Annotation("Npgsql:Enum:email_draft_generated_by", "human,ai")
                .Annotation("Npgsql:Enum:email_draft_status", "draft,approved,sent,rejected")
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
        }
    }
}
