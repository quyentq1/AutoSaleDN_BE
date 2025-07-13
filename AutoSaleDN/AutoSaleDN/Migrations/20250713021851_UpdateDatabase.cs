using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSaleDN.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarSales_CarListings_ListingId",
                table: "CarSales");

            migrationBuilder.DropIndex(
                name: "IX_CarSales_ListingId",
                table: "CarSales");

            migrationBuilder.DropColumn(
                name: "ListingId",
                table: "CarSales");

            migrationBuilder.AddColumn<int>(
                name: "CarListingListingId",
                table: "CarSales",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarSales_CarListingListingId",
                table: "CarSales",
                column: "CarListingListingId");

            migrationBuilder.CreateIndex(
                name: "IX_CarSales_CustomerId",
                table: "CarSales",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_CarSales_CarListings_CarListingListingId",
                table: "CarSales",
                column: "CarListingListingId",
                principalTable: "CarListings",
                principalColumn: "ListingId");

            migrationBuilder.AddForeignKey(
                name: "FK_CarSales_Users_CustomerId",
                table: "CarSales",
                column: "CustomerId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarSales_CarListings_CarListingListingId",
                table: "CarSales");

            migrationBuilder.DropForeignKey(
                name: "FK_CarSales_Users_CustomerId",
                table: "CarSales");

            migrationBuilder.DropIndex(
                name: "IX_CarSales_CarListingListingId",
                table: "CarSales");

            migrationBuilder.DropIndex(
                name: "IX_CarSales_CustomerId",
                table: "CarSales");

            migrationBuilder.DropColumn(
                name: "CarListingListingId",
                table: "CarSales");

            migrationBuilder.AddColumn<int>(
                name: "ListingId",
                table: "CarSales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CarSales_ListingId",
                table: "CarSales",
                column: "ListingId");

            migrationBuilder.AddForeignKey(
                name: "FK_CarSales_CarListings_ListingId",
                table: "CarSales",
                column: "ListingId",
                principalTable: "CarListings",
                principalColumn: "ListingId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
