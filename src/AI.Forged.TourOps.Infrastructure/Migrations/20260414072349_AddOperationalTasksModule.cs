using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.Forged.TourOps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalTasksModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:booking_item_status", "pending,requested,confirmed,cancelled")
                .Annotation("Npgsql:Enum:booking_status", "draft,confirmed,cancelled")
                .Annotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .Annotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .Annotation("Npgsql:Enum:quote_status", "draft,generated")
                .Annotation("Npgsql:Enum:task_status", "to_do,waiting,follow_up,blocked,done")
                .OldAnnotation("Npgsql:Enum:booking_item_status", "pending,requested,confirmed,cancelled")
                .OldAnnotation("Npgsql:Enum:booking_status", "draft,confirmed,cancelled")
                .OldAnnotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .OldAnnotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .OldAnnotation("Npgsql:Enum:quote_status", "draft,generated");

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    BookingItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AssignedToUserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_BookingItems_BookingItemId",
                        column: x => x.BookingItemId,
                        principalTable: "BookingItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tasks_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedToUserId",
                table: "Tasks",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_BookingId",
                table: "Tasks",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_BookingItemId",
                table: "Tasks",
                column: "BookingItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:booking_item_status", "pending,requested,confirmed,cancelled")
                .Annotation("Npgsql:Enum:booking_status", "draft,confirmed,cancelled")
                .Annotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .Annotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .Annotation("Npgsql:Enum:quote_status", "draft,generated")
                .OldAnnotation("Npgsql:Enum:booking_item_status", "pending,requested,confirmed,cancelled")
                .OldAnnotation("Npgsql:Enum:booking_status", "draft,confirmed,cancelled")
                .OldAnnotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .OldAnnotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .OldAnnotation("Npgsql:Enum:quote_status", "draft,generated")
                .OldAnnotation("Npgsql:Enum:task_status", "to_do,waiting,follow_up,blocked,done");
        }
    }
}
