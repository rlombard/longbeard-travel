using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.Forged.TourOps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeconstructProductMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TourismLevyRawText",
                table: "Products",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TourismLevyConditions",
                table: "Products",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Specials",
                table: "Products",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RoomPolicies",
                table: "Products",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RatePolicies",
                table: "Products",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Inclusions",
                table: "Products",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Exclusions",
                table: "Products",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ChildPolicies",
                table: "Products",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CancellationPolicies",
                table: "Products",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BlockOutDates",
                table: "Products",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RateConditions",
                table: "ProductRooms",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "MinimumOccupancy",
                table: "ProductRooms",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "MaximumOccupancy",
                table: "ProductRooms",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "AdditionalNotes",
                table: "ProductRooms",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "ProductMealBases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductMealBases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductMealBases_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductRateBases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductRateBases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductRateBases_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductRateTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductRateTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductRateTypes_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductValidityPeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductValidityPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductValidityPeriods_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductMealBases_ProductId",
                table: "ProductMealBases",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRateBases_ProductId",
                table: "ProductRateBases",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRateTypes_ProductId",
                table: "ProductRateTypes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductValidityPeriods_ProductId",
                table: "ProductValidityPeriods",
                column: "ProductId");

            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pgcrypto;");

            migrationBuilder.Sql("""
                INSERT INTO "ProductRateTypes" ("Id", "ProductId", "Name")
                SELECT gen_random_uuid(), values."ProductId", values."Name"
                FROM (
                    SELECT DISTINCT source."ProductId", source."Name"
                    FROM (
                        SELECT r."ProductId", btrim(r."RateTypeName") AS "Name"
                        FROM "Rates" r
                        WHERE r."RateTypeName" IS NOT NULL AND btrim(r."RateTypeName") <> ''

                        UNION

                        SELECT p."Id", btrim(split_item."ValuePart")
                        FROM "Products" p
                        CROSS JOIN LATERAL jsonb_array_elements(COALESCE(p."PropertyRoomMetadata", '[]'::jsonb)) entry("JsonValue")
                        CROSS JOIN LATERAL regexp_split_to_table(COALESCE(entry."JsonValue" ->> 'rateTypes', ''), E'\\s*,\\s*') split_item("ValuePart")
                        WHERE btrim(split_item."ValuePart") <> ''
                    ) source
                ) values;
                """);

            migrationBuilder.Sql("""
                INSERT INTO "ProductRateBases" ("Id", "ProductId", "Name")
                SELECT gen_random_uuid(), values."ProductId", values."Name"
                FROM (
                    SELECT DISTINCT source."ProductId", source."Name"
                    FROM (
                        SELECT r."ProductId", btrim(r."RateBasis") AS "Name"
                        FROM "Rates" r
                        WHERE r."RateBasis" IS NOT NULL AND btrim(r."RateBasis") <> ''

                        UNION

                        SELECT p."Id", btrim(split_item."ValuePart")
                        FROM "Products" p
                        CROSS JOIN LATERAL jsonb_array_elements(COALESCE(p."PropertyRoomMetadata", '[]'::jsonb)) entry("JsonValue")
                        CROSS JOIN LATERAL regexp_split_to_table(COALESCE(entry."JsonValue" ->> 'rateBases', ''), E'\\s*,\\s*') split_item("ValuePart")
                        WHERE btrim(split_item."ValuePart") <> ''
                    ) source
                ) values;
                """);

            migrationBuilder.Sql("""
                INSERT INTO "ProductMealBases" ("Id", "ProductId", "Name")
                SELECT gen_random_uuid(), values."ProductId", values."Name"
                FROM (
                    SELECT DISTINCT source."ProductId", source."Name"
                    FROM (
                        SELECT r."ProductId", btrim(r."MealBasis") AS "Name"
                        FROM "Rates" r
                        WHERE r."MealBasis" IS NOT NULL AND btrim(r."MealBasis") <> ''

                        UNION

                        SELECT p."Id", btrim(split_item."ValuePart")
                        FROM "Products" p
                        CROSS JOIN LATERAL jsonb_array_elements(COALESCE(p."PropertyRoomMetadata", '[]'::jsonb)) entry("JsonValue")
                        CROSS JOIN LATERAL regexp_split_to_table(COALESCE(entry."JsonValue" ->> 'mealBases', ''), E'\\s*,\\s*') split_item("ValuePart")
                        WHERE btrim(split_item."ValuePart") <> ''
                    ) source
                ) values;
                """);

            migrationBuilder.Sql("""
                INSERT INTO "ProductValidityPeriods" ("Id", "ProductId", "Value")
                SELECT gen_random_uuid(), values."ProductId", values."Value"
                FROM (
                    SELECT DISTINCT source."ProductId", source."Value"
                    FROM (
                        SELECT p."Id" AS "ProductId", btrim(p."ContractValidityPeriod") AS "Value"
                        FROM "Products" p
                        WHERE p."ContractValidityPeriod" IS NOT NULL AND btrim(p."ContractValidityPeriod") <> ''

                        UNION

                        SELECT r."ProductId", btrim(r."ValidityPeriod")
                        FROM "Rates" r
                        WHERE r."ValidityPeriod" IS NOT NULL AND btrim(r."ValidityPeriod") <> ''

                        UNION

                        SELECT p."Id", btrim(split_item."ValuePart")
                        FROM "Products" p
                        CROSS JOIN LATERAL jsonb_array_elements(COALESCE(p."PropertyRoomMetadata", '[]'::jsonb)) entry("JsonValue")
                        CROSS JOIN LATERAL regexp_split_to_table(COALESCE(entry."JsonValue" ->> 'validityPeriods', ''), E'\\s*,\\s*') split_item("ValuePart")
                        WHERE btrim(split_item."ValuePart") <> ''
                    ) source
                ) values;
                """);

            migrationBuilder.Sql("""
                INSERT INTO "ProductRooms" ("Id", "ProductId", "Name", "MinimumOccupancy", "MaximumOccupancy", "AdditionalNotes", "RateConditions")
                SELECT gen_random_uuid(), values."ProductId", values."Name", NULL, NULL, NULL, NULL
                FROM (
                    SELECT DISTINCT p."Id" AS "ProductId", btrim(split_item."ValuePart") AS "Name"
                    FROM "Products" p
                    CROSS JOIN LATERAL jsonb_array_elements(COALESCE(p."PropertyRoomMetadata", '[]'::jsonb)) entry("JsonValue")
                    CROSS JOIN LATERAL regexp_split_to_table(COALESCE(entry."JsonValue" ->> 'roomNames', ''), E'\\s*,\\s*') split_item("ValuePart")
                    WHERE btrim(split_item."ValuePart") <> ''
                ) values
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "ProductRooms" existing
                    WHERE existing."ProductId" = values."ProductId"
                      AND lower(existing."Name") = lower(values."Name")
                );
                """);

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PropertyRoomMetadata",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductMealBases");

            migrationBuilder.DropTable(
                name: "ProductRateBases");

            migrationBuilder.DropTable(
                name: "ProductRateTypes");

            migrationBuilder.DropTable(
                name: "ProductValidityPeriods");

            migrationBuilder.AlterColumn<string>(
                name: "TourismLevyRawText",
                table: "Products",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TourismLevyConditions",
                table: "Products",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Specials",
                table: "Products",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RoomPolicies",
                table: "Products",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RatePolicies",
                table: "Products",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Inclusions",
                table: "Products",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Exclusions",
                table: "Products",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ChildPolicies",
                table: "Products",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CancellationPolicies",
                table: "Products",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BlockOutDates",
                table: "Products",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "Products",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PropertyRoomMetadata",
                table: "Products",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "RateConditions",
                table: "ProductRooms",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MinimumOccupancy",
                table: "ProductRooms",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaximumOccupancy",
                table: "ProductRooms",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AdditionalNotes",
                table: "ProductRooms",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);
        }
    }
}
