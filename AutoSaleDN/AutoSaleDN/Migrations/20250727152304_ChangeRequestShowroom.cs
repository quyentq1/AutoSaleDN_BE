using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSaleDN.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRequestShowroom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StoreListings_ListingId",
                table: "StoreListings");

            migrationBuilder.RenameColumn(
                name: "RentSell",
                table: "CarListings",
                newName: "Status");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "StoreListings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "StoreListings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoreListings_ListingId",
                table: "StoreListings",
                column: "ListingId",
                unique: true,
                filter: "[IsCurrent] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StoreListings_ListingId",
                table: "StoreListings");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "StoreListings");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "CarListings",
                newName: "RentSell");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "StoreListings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_StoreListings_ListingId",
                table: "StoreListings",
                column: "ListingId");
        }
    }
}
