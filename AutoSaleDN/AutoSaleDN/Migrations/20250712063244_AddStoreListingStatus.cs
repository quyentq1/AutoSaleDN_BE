using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSaleDN.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreListingStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ListingStatus",
                table: "CarListings");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStatusChangeDate",
                table: "StoreListings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonForRemoval",
                table: "StoreListings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "StoreListings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastStatusChangeDate",
                table: "StoreListings");

            migrationBuilder.DropColumn(
                name: "ReasonForRemoval",
                table: "StoreListings");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "StoreListings");

            migrationBuilder.AddColumn<string>(
                name: "ListingStatus",
                table: "CarListings",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
