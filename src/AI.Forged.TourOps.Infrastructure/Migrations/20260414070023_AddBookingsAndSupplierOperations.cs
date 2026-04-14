using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.Forged.TourOps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingsAndSupplierOperations : Migration
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
                .OldAnnotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .OldAnnotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .OldAnnotation("Npgsql:Enum:quote_status", "draft,generated");

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookingItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingItems_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingItems_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingItems_BookingId",
                table: "BookingItems",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingItems_ProductId",
                table: "BookingItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingItems_SupplierId",
                table: "BookingItems",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_QuoteId",
                table: "Bookings",
                column: "QuoteId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingItems");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .Annotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .Annotation("Npgsql:Enum:quote_status", "draft,generated")
                .OldAnnotation("Npgsql:Enum:booking_item_status", "pending,requested,confirmed,cancelled")
                .OldAnnotation("Npgsql:Enum:booking_status", "draft,confirmed,cancelled")
                .OldAnnotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .OldAnnotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .OldAnnotation("Npgsql:Enum:quote_status", "draft,generated");
        }
    }
}
