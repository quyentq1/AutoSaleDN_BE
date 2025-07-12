using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSaleDN.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStoreListingCarSales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarInventories_CarColors_ColorId",
                table: "CarInventories");

            migrationBuilder.DropForeignKey(
                name: "FK_CarInventories_CarModels_ModelId",
                table: "CarInventories");

            migrationBuilder.DropForeignKey(
                name: "FK_CarSales_Bookings_BookingId",
                table: "CarSales");

            migrationBuilder.DropIndex(
                name: "IX_CarInventories_ColorId",
                table: "CarInventories");

            migrationBuilder.DropIndex(
                name: "IX_CarInventories_ModelId",
                table: "CarInventories");

            migrationBuilder.DropColumn(
                name: "ColorId",
                table: "CarInventories");

            migrationBuilder.DropColumn(
                name: "ImportPrice",
                table: "CarInventories");

            migrationBuilder.DropColumn(
                name: "ModelId",
                table: "CarInventories");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "StoreListings",
                newName: "InitialQuantity");

            migrationBuilder.RenameColumn(
                name: "QuantitySold",
                table: "CarInventories",
                newName: "TransactionType");

            migrationBuilder.RenameColumn(
                name: "QuantityImported",
                table: "CarInventories",
                newName: "StoreListingId");

            migrationBuilder.RenameColumn(
                name: "QuantityAvailable",
                table: "CarInventories",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "ImportDate",
                table: "CarInventories",
                newName: "TransactionDate");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "StoreListings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AvailableQuantity",
                table: "StoreListings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageCost",
                table: "StoreListings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentQuantity",
                table: "StoreListings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "LastPurchasePrice",
                table: "StoreListings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSoldDate",
                table: "StoreListings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StoreListingId",
                table: "CarSales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CarInventories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "CarInventories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReferenceId",
                table: "CarInventories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "CarInventories",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarSales_StoreListingId",
                table: "CarSales",
                column: "StoreListingId");

            migrationBuilder.CreateIndex(
                name: "IX_CarInventories_StoreListingId_TransactionDate",
                table: "CarInventories",
                columns: new[] { "StoreListingId", "TransactionDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_CarInventories_StoreListings_StoreListingId",
                table: "CarInventories",
                column: "StoreListingId",
                principalTable: "StoreListings",
                principalColumn: "StoreListingId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CarSales_Bookings_BookingId",
                table: "CarSales",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "BookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_CarSales_StoreListings_StoreListingId",
                table: "CarSales",
                column: "StoreListingId",
                principalTable: "StoreListings",
                principalColumn: "StoreListingId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarInventories_StoreListings_StoreListingId",
                table: "CarInventories");

            migrationBuilder.DropForeignKey(
                name: "FK_CarSales_Bookings_BookingId",
                table: "CarSales");

            migrationBuilder.DropForeignKey(
                name: "FK_CarSales_StoreListings_StoreListingId",
                table: "CarSales");

            migrationBuilder.DropIndex(
                name: "IX_CarSales_StoreListingId",
                table: "CarSales");

            migrationBuilder.DropIndex(
                name: "IX_CarInventories_StoreListingId_TransactionDate",
                table: "CarInventories");

            migrationBuilder.DropColumn(
                name: "AvailableQuantity",
                table: "StoreListings");

            migrationBuilder.DropColumn(
                name: "AverageCost",
                table: "StoreListings");

            migrationBuilder.DropColumn(
                name: "CurrentQuantity",
                table: "StoreListings");

            migrationBuilder.DropColumn(
                name: "LastPurchasePrice",
                table: "StoreListings");

            migrationBuilder.DropColumn(
                name: "LastSoldDate",
                table: "StoreListings");

            migrationBuilder.DropColumn(
                name: "StoreListingId",
                table: "CarSales");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CarInventories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CarInventories");

            migrationBuilder.DropColumn(
                name: "ReferenceId",
                table: "CarInventories");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "CarInventories");

            migrationBuilder.RenameColumn(
                name: "InitialQuantity",
                table: "StoreListings",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "TransactionType",
                table: "CarInventories",
                newName: "QuantitySold");

            migrationBuilder.RenameColumn(
                name: "TransactionDate",
                table: "CarInventories",
                newName: "ImportDate");

            migrationBuilder.RenameColumn(
                name: "StoreListingId",
                table: "CarInventories",
                newName: "QuantityImported");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "CarInventories",
                newName: "QuantityAvailable");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "StoreListings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "ColorId",
                table: "CarInventories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ImportPrice",
                table: "CarInventories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ModelId",
                table: "CarInventories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CarInventories_ColorId",
                table: "CarInventories",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_CarInventories_ModelId",
                table: "CarInventories",
                column: "ModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_CarInventories_CarColors_ColorId",
                table: "CarInventories",
                column: "ColorId",
                principalTable: "CarColors",
                principalColumn: "ColorId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CarInventories_CarModels_ModelId",
                table: "CarInventories",
                column: "ModelId",
                principalTable: "CarModels",
                principalColumn: "ModelId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CarSales_Bookings_BookingId",
                table: "CarSales",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
