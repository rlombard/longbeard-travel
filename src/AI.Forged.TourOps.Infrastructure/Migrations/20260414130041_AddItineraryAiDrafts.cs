using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.Forged.TourOps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddItineraryAiDrafts : Migration
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
                .Annotation("Npgsql:Enum:itinerary_draft_status", "draft,approved,rejected")
                .Annotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .Annotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .Annotation("Npgsql:Enum:quote_status", "draft,generated")
                .Annotation("Npgsql:Enum:task_status", "to_do,waiting,follow_up,blocked,done")
                .Annotation("Npgsql:Enum:task_suggestion_state", "pending_review,accepted,rejected")
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

            migrationBuilder.CreateTable(
                name: "ItineraryDrafts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ProposedStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Duration = table.Column<int>(type: "integer", nullable: false),
                    InputJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    CustomerBrief = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    AssumptionsJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    CaveatsJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    DataGapsJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    LlmProvider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LlmModel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AuditMetadataJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PersistedItineraryId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedByUserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItineraryDrafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItineraryDrafts_Itineraries_PersistedItineraryId",
                        column: x => x.PersistedItineraryId,
                        principalTable: "Itineraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ItineraryDraftItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItineraryDraftId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayNumber = table.Column<int>(type: "integer", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SupplierName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Confidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsUnresolved = table.Column<bool>(type: "boolean", nullable: false),
                    WarningFlagsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    MissingDataJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItineraryDraftItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItineraryDraftItems_ItineraryDrafts_ItineraryDraftId",
                        column: x => x.ItineraryDraftId,
                        principalTable: "ItineraryDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItineraryDraftItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItineraryDraftItems_ItineraryDraftId_DayNumber_Sequence",
                table: "ItineraryDraftItems",
                columns: new[] { "ItineraryDraftId", "DayNumber", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_ItineraryDraftItems_ProductId",
                table: "ItineraryDraftItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ItineraryDrafts_PersistedItineraryId",
                table: "ItineraryDrafts",
                column: "PersistedItineraryId");

            migrationBuilder.CreateIndex(
                name: "IX_ItineraryDrafts_RequestedByUserId",
                table: "ItineraryDrafts",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ItineraryDrafts_Status",
                table: "ItineraryDrafts",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItineraryDraftItems");

            migrationBuilder.DropTable(
                name: "ItineraryDrafts");

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
                .OldAnnotation("Npgsql:Enum:email_classification_type", "confirmation_received,partial_confirmation,needs_more_information,pricing_changed,availability_issue,no_action_needed,human_decision_required,unclear")
                .OldAnnotation("Npgsql:Enum:email_direction", "inbound,outbound")
                .OldAnnotation("Npgsql:Enum:email_draft_generated_by", "human,ai")
                .OldAnnotation("Npgsql:Enum:email_draft_status", "draft,approved,sent,rejected")
                .OldAnnotation("Npgsql:Enum:human_approval_status", "pending,approved,rejected")
                .OldAnnotation("Npgsql:Enum:itinerary_draft_status", "draft,approved,rejected")
                .OldAnnotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .OldAnnotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .OldAnnotation("Npgsql:Enum:quote_status", "draft,generated")
                .OldAnnotation("Npgsql:Enum:task_status", "to_do,waiting,follow_up,blocked,done")
                .OldAnnotation("Npgsql:Enum:task_suggestion_state", "pending_review,accepted,rejected");
        }
    }
}
