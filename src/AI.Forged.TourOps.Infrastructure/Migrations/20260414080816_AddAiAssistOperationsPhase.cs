using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.Forged.TourOps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiAssistOperationsPhase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:booking_item_status", "pending,requested,confirmed,cancelled")
                .Annotation("Npgsql:Enum:booking_status", "draft,confirmed,cancelled")
                .Annotation("Npgsql:Enum:email_classification_type", "confirmation_received,partial_confirmation,needs_more_information,pricing_changed,availability_issue,no_action_needed,human_decision_required,unclear")
                .Annotation("Npgsql:Enum:email_direction", "inbound,outbound")
                .Annotation("Npgsql:Enum:email_draft_generated_by", "human,ai")
                .Annotation("Npgsql:Enum:email_draft_status", "draft,approved,sent,rejected")
                .Annotation("Npgsql:Enum:human_approval_status", "pending,approved,rejected")
                .Annotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .Annotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .Annotation("Npgsql:Enum:quote_status", "draft,generated")
                .Annotation("Npgsql:Enum:task_status", "to_do,waiting,follow_up,blocked,done")
                .Annotation("Npgsql:Enum:task_suggestion_state", "pending_review,accepted,rejected")
                .OldAnnotation("Npgsql:Enum:booking_item_status", "pending,requested,confirmed,cancelled")
                .OldAnnotation("Npgsql:Enum:booking_status", "draft,confirmed,cancelled")
                .OldAnnotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .OldAnnotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .OldAnnotation("Npgsql:Enum:quote_status", "draft,generated")
                .OldAnnotation("Npgsql:Enum:task_status", "to_do,waiting,follow_up,blocked,done");

            migrationBuilder.CreateTable(
                name: "EmailThreads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    BookingItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExternalThreadId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Subject = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    SupplierEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailThreads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailThreads_BookingItems_BookingItemId",
                        column: x => x.BookingItemId,
                        principalTable: "BookingItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailThreads_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HumanApprovalRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ReviewedByUserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PayloadJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    DecisionNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HumanApprovalRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LlmAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Operation = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Provider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Model = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PromptSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ResponseSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    StructuredResultJson = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: true),
                    MetadataJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LlmAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskSuggestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    SuggestedStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SuggestedDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Confidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    RequiresHumanReview = table.Column<bool>(type: "boolean", nullable: false),
                    State = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Source = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LlmProvider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LlmModel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AuditMetadataJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    AcceptedTaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedByUserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskSuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskSuggestions_BookingItems_BookingItemId",
                        column: x => x.BookingItemId,
                        principalTable: "BookingItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskSuggestions_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskSuggestions_Tasks_AcceptedTaskId",
                        column: x => x.AcceptedTaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EmailDrafts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    BookingItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmailThreadId = table.Column<Guid>(type: "uuid", nullable: true),
                    Subject = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Body = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    GeneratedBy = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LlmProvider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LlmModel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AuditMetadataJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailDrafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailDrafts_BookingItems_BookingItemId",
                        column: x => x.BookingItemId,
                        principalTable: "BookingItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailDrafts_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailDrafts_EmailThreads_EmailThreadId",
                        column: x => x.EmailThreadId,
                        principalTable: "EmailThreads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EmailMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailThreadId = table.Column<Guid>(type: "uuid", nullable: false),
                    Direction = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Subject = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    BodyText = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: false),
                    BodyHtml = table.Column<string>(type: "character varying(32000)", maxLength: 32000, nullable: true),
                    Sender = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Recipients = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequiresHumanReview = table.Column<bool>(type: "boolean", nullable: false),
                    AiSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    AiClassification = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    AiConfidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: true),
                    AiExtractedSignalsJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailMessages_EmailThreads_EmailThreadId",
                        column: x => x.EmailThreadId,
                        principalTable: "EmailThreads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailDrafts_BookingId",
                table: "EmailDrafts",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailDrafts_BookingItemId",
                table: "EmailDrafts",
                column: "BookingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailDrafts_EmailThreadId",
                table: "EmailDrafts",
                column: "EmailThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailDrafts_Status",
                table: "EmailDrafts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_EmailThreadId",
                table: "EmailMessages",
                column: "EmailThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_SentAt",
                table: "EmailMessages",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmailThreads_BookingId",
                table: "EmailThreads",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailThreads_BookingItemId",
                table: "EmailThreads",
                column: "BookingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailThreads_SupplierEmail",
                table: "EmailThreads",
                column: "SupplierEmail");

            migrationBuilder.CreateIndex(
                name: "IX_HumanApprovalRequests_EntityType_EntityId",
                table: "HumanApprovalRequests",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_HumanApprovalRequests_Status",
                table: "HumanApprovalRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LlmAuditLogs_Category",
                table: "LlmAuditLogs",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_LlmAuditLogs_CreatedAt",
                table: "LlmAuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LlmAuditLogs_Provider",
                table: "LlmAuditLogs",
                column: "Provider");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSuggestions_AcceptedTaskId",
                table: "TaskSuggestions",
                column: "AcceptedTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSuggestions_BookingId",
                table: "TaskSuggestions",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSuggestions_BookingItemId",
                table: "TaskSuggestions",
                column: "BookingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSuggestions_State",
                table: "TaskSuggestions",
                column: "State");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailDrafts");

            migrationBuilder.DropTable(
                name: "EmailMessages");

            migrationBuilder.DropTable(
                name: "HumanApprovalRequests");

            migrationBuilder.DropTable(
                name: "LlmAuditLogs");

            migrationBuilder.DropTable(
                name: "TaskSuggestions");

            migrationBuilder.DropTable(
                name: "EmailThreads");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:booking_item_status", "pending,requested,confirmed,cancelled")
                .Annotation("Npgsql:Enum:booking_status", "draft,confirmed,cancelled")
                .Annotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .Annotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .Annotation("Npgsql:Enum:quote_status", "draft,generated")
                .Annotation("Npgsql:Enum:task_status", "to_do,waiting,follow_up,blocked,done")
                .OldAnnotation("Npgsql:Enum:booking_item_status", "pending,requested,confirmed,cancelled")
                .OldAnnotation("Npgsql:Enum:booking_status", "draft,confirmed,cancelled")
                .OldAnnotation("Npgsql:Enum:email_classification_type", "confirmation_received,partial_confirmation,needs_more_information,pricing_changed,availability_issue,no_action_needed,human_decision_required,unclear")
                .OldAnnotation("Npgsql:Enum:email_direction", "inbound,outbound")
                .OldAnnotation("Npgsql:Enum:email_draft_generated_by", "human,ai")
                .OldAnnotation("Npgsql:Enum:email_draft_status", "draft,approved,sent,rejected")
                .OldAnnotation("Npgsql:Enum:human_approval_status", "pending,approved,rejected")
                .OldAnnotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .OldAnnotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .OldAnnotation("Npgsql:Enum:quote_status", "draft,generated")
                .OldAnnotation("Npgsql:Enum:task_status", "to_do,waiting,follow_up,blocked,done")
                .OldAnnotation("Npgsql:Enum:task_suggestion_state", "pending_review,accepted,rejected");
        }
    }
}
