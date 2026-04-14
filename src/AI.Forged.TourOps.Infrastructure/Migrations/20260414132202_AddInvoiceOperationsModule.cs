using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.Forged.TourOps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceOperationsModule : Migration
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
                .Annotation("Npgsql:Enum:invoice_status", "draft,received,matched,unmatched,pending_review,approved,rejected,unpaid,partially_paid,paid,overdue,rebate_pending,rebate_applied,cancelled")
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
                .OldAnnotation("Npgsql:Enum:itinerary_draft_status", "draft,approved,rejected")
                .OldAnnotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .OldAnnotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .OldAnnotation("Npgsql:Enum:quote_status", "draft,generated")
                .OldAnnotation("Npgsql:Enum:task_status", "to_do,waiting,follow_up,blocked,done")
                .OldAnnotation("Npgsql:Enum:task_suggestion_state", "pending_review,accepted,rejected");

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceSystem = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExternalSourceReference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    InvoiceNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplierName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    BookingItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuoteId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmailThreadId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewTaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    SubtotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RebateAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    RebateAppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RawExtractionPayloadJson = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: true),
                    NormalizedSnapshotJson = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: true),
                    ExtractionConfidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    ExtractionIssuesJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    UnresolvedFieldsJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    RequiresHumanReview = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_BookingItems_BookingItemId",
                        column: x => x.BookingItemId,
                        principalTable: "BookingItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Invoices_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Invoices_EmailThreads_EmailThreadId",
                        column: x => x.EmailThreadId,
                        principalTable: "EmailThreads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Invoices_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Invoices_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Invoices_Tasks_ReviewTaskId",
                        column: x => x.ReviewTaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalFileReference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SourceUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MetadataJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceAttachments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLineItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalLineReference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BookingItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ServiceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceLineItems_BookingItems_BookingItemId",
                        column: x => x.BookingItemId,
                        principalTable: "BookingItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InvoiceLineItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalPaymentReference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RecordedByUserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MetadataJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentRecords_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceAttachments_InvoiceId",
                table: "InvoiceAttachments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLineItems_BookingItemId",
                table: "InvoiceLineItems",
                column: "BookingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLineItems_InvoiceId",
                table: "InvoiceLineItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BookingId",
                table: "Invoices",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BookingItemId",
                table: "Invoices",
                column: "BookingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_DueDate",
                table: "Invoices",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_EmailThreadId",
                table: "Invoices",
                column: "EmailThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_QuoteId",
                table: "Invoices",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ReviewTaskId",
                table: "Invoices",
                column: "ReviewTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SourceSystem_ExternalSourceReference",
                table: "Invoices",
                columns: new[] { "SourceSystem", "ExternalSourceReference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Status",
                table: "Invoices",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SupplierId",
                table: "Invoices",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecords_InvoiceId",
                table: "PaymentRecords",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecords_PaidAt",
                table: "PaymentRecords",
                column: "PaidAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceAttachments");

            migrationBuilder.DropTable(
                name: "InvoiceLineItems");

            migrationBuilder.DropTable(
                name: "PaymentRecords");

            migrationBuilder.DropTable(
                name: "Invoices");

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
                .OldAnnotation("Npgsql:Enum:invoice_status", "draft,received,matched,unmatched,pending_review,approved,rejected,unpaid,partially_paid,paid,overdue,rebate_pending,rebate_applied,cancelled")
                .OldAnnotation("Npgsql:Enum:itinerary_draft_status", "draft,approved,rejected")
                .OldAnnotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .OldAnnotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .OldAnnotation("Npgsql:Enum:quote_status", "draft,generated")
                .OldAnnotation("Npgsql:Enum:task_status", "to_do,waiting,follow_up,blocked,done")
                .OldAnnotation("Npgsql:Enum:task_suggestion_state", "pending_review,accepted,rejected");
        }
    }
}
