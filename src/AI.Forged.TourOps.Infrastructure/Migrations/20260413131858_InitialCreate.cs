using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.Forged.TourOps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:pricing_model", "per_person,per_group,per_unit")
                .Annotation("Npgsql:Enum:product_type", "tour,hotel,transport")
                .Annotation("Npgsql:Enum:quote_status", "draft,generated");

            migrationBuilder.CreateTable(
                name: "Itineraries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Duration = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Itineraries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItineraryId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Margin = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quotes_Itineraries_ItineraryId",
                        column: x => x.ItineraryId,
                        principalTable: "Itineraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
                    PropertyRoomMetadata = table.Column<string>(type: "jsonb", nullable: false),
                    ContractValidityPeriod = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Commission = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PhysicalStreetAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PhysicalSuburb = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PhysicalTownOrCity = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PhysicalStateOrProvince = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PhysicalCountry = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PhysicalPostCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    MailingStreetAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    MailingSuburb = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    MailingTownOrCity = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    MailingStateOrProvince = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    MailingCountry = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    MailingPostCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CheckInTime = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CheckOutTime = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    BlockOutDates = table.Column<string>(type: "text", nullable: true),
                    TourismLevyAmount = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TourismLevyCurrency = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    TourismLevyUnit = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TourismLevyAgeApplicability = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TourismLevyEffectiveDates = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TourismLevyConditions = table.Column<string>(type: "text", nullable: true),
                    TourismLevyRawText = table.Column<string>(type: "text", nullable: true),
                    TourismLevyIncluded = table.Column<bool>(type: "boolean", nullable: false),
                    RoomPolicies = table.Column<string>(type: "text", nullable: true),
                    RatePolicies = table.Column<string>(type: "text", nullable: true),
                    ChildPolicies = table.Column<string>(type: "text", nullable: true),
                    CancellationPolicies = table.Column<string>(type: "text", nullable: true),
                    Inclusions = table.Column<string>(type: "text", nullable: true),
                    Exclusions = table.Column<string>(type: "text", nullable: true),
                    Specials = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItineraryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItineraryId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayNumber = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItineraryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItineraryItems_Itineraries_ItineraryId",
                        column: x => x.ItineraryId,
                        principalTable: "Itineraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItineraryItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ContactName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContactPhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductContacts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductExtras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ChargeUnit = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Charge = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductExtras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductExtras_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductRooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MinimumOccupancy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MaximumOccupancy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AdditionalNotes = table.Column<string>(type: "text", nullable: false),
                    RateConditions = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductRooms_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuoteLineItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    BaseCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AdjustedCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MarkupPercentage = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuoteLineItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuoteLineItems_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductRoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    SeasonStart = table.Column<DateOnly>(type: "date", nullable: false),
                    SeasonEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    PricingModel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BaseCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    MinPax = table.Column<int>(type: "integer", nullable: true),
                    MaxPax = table.Column<int>(type: "integer", nullable: true),
                    ChildDiscount = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: true),
                    SingleSupplement = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    ValidityPeriod = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ValidityPeriodDescription = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RateVariation = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RateTypeName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RateBasis = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OccupancyType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    MealBasis = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    MinimumStay = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rates_ProductRooms_ProductRoomId",
                        column: x => x.ProductRoomId,
                        principalTable: "ProductRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Rates_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItineraryItems_ItineraryId",
                table: "ItineraryItems",
                column: "ItineraryId");

            migrationBuilder.CreateIndex(
                name: "IX_ItineraryItems_ProductId",
                table: "ItineraryItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductContacts_ProductId",
                table: "ProductContacts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductExtras_ProductId",
                table: "ProductExtras",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRooms_ProductId",
                table: "ProductRooms",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SupplierId",
                table: "Products",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteLineItems_ProductId",
                table: "QuoteLineItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteLineItems_QuoteId",
                table: "QuoteLineItems",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_ItineraryId",
                table: "Quotes",
                column: "ItineraryId");

            migrationBuilder.CreateIndex(
                name: "IX_Rates_ProductId",
                table: "Rates",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Rates_ProductRoomId",
                table: "Rates",
                column: "ProductRoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItineraryItems");

            migrationBuilder.DropTable(
                name: "ProductContacts");

            migrationBuilder.DropTable(
                name: "ProductExtras");

            migrationBuilder.DropTable(
                name: "QuoteLineItems");

            migrationBuilder.DropTable(
                name: "Rates");

            migrationBuilder.DropTable(
                name: "Quotes");

            migrationBuilder.DropTable(
                name: "ProductRooms");

            migrationBuilder.DropTable(
                name: "Itineraries");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Suppliers");
        }
    }
}
