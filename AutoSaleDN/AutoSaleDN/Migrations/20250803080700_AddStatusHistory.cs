using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSaleDN.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SaleStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SaleId = table.Column<int>(type: "int", nullable: false),
                    SaleStatusId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleStatusHistory_CarSales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "CarSales",
                        principalColumn: "SaleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaleStatusHistory_SaleStatus_SaleStatusId",
                        column: x => x.SaleStatusId,
                        principalTable: "SaleStatus",
                        principalColumn: "SaleStatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaleStatusHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaleStatusHistory_SaleId",
                table: "SaleStatusHistory",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleStatusHistory_SaleStatusId",
                table: "SaleStatusHistory",
                column: "SaleStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleStatusHistory_UserId",
                table: "SaleStatusHistory",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SaleStatusHistory");
        }
    }
}
