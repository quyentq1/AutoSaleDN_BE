using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSaleDN.Migrations
{
    /// <inheritdoc />
    public partial class AddCarVideoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CarVideos",
                columns: table => new
                {
                    VideoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ListingId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarVideos", x => x.VideoId);
                    table.ForeignKey(
                        name: "FK_CarVideos_CarListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "CarListings",
                        principalColumn: "ListingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarVideos_ListingId",
                table: "CarVideos",
                column: "ListingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarVideos");
        }
    }
}
