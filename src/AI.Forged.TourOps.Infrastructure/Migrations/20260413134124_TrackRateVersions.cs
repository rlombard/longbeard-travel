using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.Forged.TourOps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TrackRateVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Rates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PreviousRateId",
                table: "Rates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SupersededAt",
                table: "Rates",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Rates");

            migrationBuilder.DropColumn(
                name: "PreviousRateId",
                table: "Rates");

            migrationBuilder.DropColumn(
                name: "SupersededAt",
                table: "Rates");
        }
    }
}
