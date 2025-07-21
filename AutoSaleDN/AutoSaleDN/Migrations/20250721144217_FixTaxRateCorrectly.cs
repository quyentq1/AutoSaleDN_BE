using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSaleDN.Migrations
{
    /// <inheritdoc />
    public partial class FixTaxRateCorrectly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
            name: "TaxRate",
            table: "CarPricingDetails",
            type: "decimal(18,2)",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(6,4)");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
