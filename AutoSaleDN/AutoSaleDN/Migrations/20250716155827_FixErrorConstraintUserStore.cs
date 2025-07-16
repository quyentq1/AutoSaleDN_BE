using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSaleDN.Migrations
{
    /// <inheritdoc />
    public partial class FixErrorConstraintUserStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoreLocations_Users_UserId",
                table: "StoreLocations");

            migrationBuilder.DropIndex(
                name: "IX_StoreLocations_UserId",
                table: "StoreLocations");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "StoreLocations");

            migrationBuilder.AddColumn<int>(
                name: "StoreLocationId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_StoreLocationId",
                table: "Users",
                column: "StoreLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_StoreLocations_StoreLocationId",
                table: "Users",
                column: "StoreLocationId",
                principalTable: "StoreLocations",
                principalColumn: "StoreLocationId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_StoreLocations_StoreLocationId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_StoreLocationId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StoreLocationId",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "StoreLocations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StoreLocations_UserId",
                table: "StoreLocations",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StoreLocations_Users_UserId",
                table: "StoreLocations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
