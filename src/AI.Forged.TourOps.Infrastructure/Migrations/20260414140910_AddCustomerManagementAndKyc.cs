using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.Forged.TourOps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerManagementAndKyc : Migration
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

            migrationBuilder.AddColumn<Guid>(
                name: "LeadCustomerId",
                table: "Quotes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LeadCustomerId",
                table: "Itineraries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LeadCustomerId",
                table: "Bookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LastName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Nationality = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CountryOfResidence = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    PreferredContactMethod = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookingTravellers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationshipToLeadCustomer = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingTravellers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingTravellers_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingTravellers_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ChangedByUserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ChangedFieldsJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerAuditLogs_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerKycProfiles",
                columns: table => new
                {
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    PassportNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DocumentReference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PassportExpiry = table.Column<DateOnly>(type: "date", nullable: true),
                    IssuingCountry = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VisaNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    EmergencyContactName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EmergencyContactRelationship = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VerificationStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VerificationNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ProfileDataConsentGranted = table.Column<bool>(type: "boolean", nullable: false),
                    KycDataConsentGranted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerKycProfiles", x => x.CustomerId);
                    table.ForeignKey(
                        name: "FK_CustomerKycProfiles_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerPreferenceProfiles",
                columns: table => new
                {
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    BudgetBand = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AccommodationPreference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RoomPreference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DietaryRequirementsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ActivityPreferencesJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    AccessibilityRequirementsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    PaceOfTravel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ValueLeaning = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TransportPreferencesJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SpecialOccasionsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DislikedExperiencesJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    PreferredDestinationsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    AvoidedDestinationsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    OperatorNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerPreferenceProfiles", x => x.CustomerId);
                    table.ForeignKey(
                        name: "FK_CustomerPreferenceProfiles_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_LeadCustomerId",
                table: "Quotes",
                column: "LeadCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Itineraries_LeadCustomerId",
                table: "Itineraries",
                column: "LeadCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_LeadCustomerId",
                table: "Bookings",
                column: "LeadCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingTravellers_BookingId_CustomerId",
                table: "BookingTravellers",
                columns: new[] { "BookingId", "CustomerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingTravellers_CustomerId",
                table: "BookingTravellers",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAuditLogs_CreatedAt",
                table: "CustomerAuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAuditLogs_CustomerId",
                table: "CustomerAuditLogs",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CountryOfResidence",
                table: "Customers",
                column: "CountryOfResidence");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_LastName_FirstName",
                table: "Customers",
                columns: new[] { "LastName", "FirstName" });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Phone",
                table: "Customers",
                column: "Phone");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Customers_LeadCustomerId",
                table: "Bookings",
                column: "LeadCustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Itineraries_Customers_LeadCustomerId",
                table: "Itineraries",
                column: "LeadCustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_Customers_LeadCustomerId",
                table: "Quotes",
                column: "LeadCustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Customers_LeadCustomerId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Itineraries_Customers_LeadCustomerId",
                table: "Itineraries");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_Customers_LeadCustomerId",
                table: "Quotes");

            migrationBuilder.DropTable(
                name: "BookingTravellers");

            migrationBuilder.DropTable(
                name: "CustomerAuditLogs");

            migrationBuilder.DropTable(
                name: "CustomerKycProfiles");

            migrationBuilder.DropTable(
                name: "CustomerPreferenceProfiles");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_LeadCustomerId",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_Itineraries_LeadCustomerId",
                table: "Itineraries");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_LeadCustomerId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "LeadCustomerId",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "LeadCustomerId",
                table: "Itineraries");

            migrationBuilder.DropColumn(
                name: "LeadCustomerId",
                table: "Bookings");

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
        }
    }
}
