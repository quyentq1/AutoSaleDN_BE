using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSaleDN.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDecimalPrecision : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CarListings - Price
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "CarListings",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            // CarPricingDetails - RegistrationFee
            migrationBuilder.AlterColumn<decimal>(
                name: "RegistrationFee",
                table: "CarPricingDetails",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            // CarPricingDetails - TaxRate (giữ nguyên hoặc tăng nhẹ)
            migrationBuilder.AlterColumn<decimal>(
                name: "TaxRate",
                table: "CarPricingDetails",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,4)");

            // Reports - AverageListingPrice
            migrationBuilder.AlterColumn<decimal>(
                name: "AverageListingPrice",
                table: "Reports",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            // Reports - TotalListingValue
            migrationBuilder.AlterColumn<decimal>(
                name: "TotalListingValue",
                table: "Reports",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            // Reports - TotalBookingValue
            migrationBuilder.AlterColumn<decimal>(
                name: "TotalBookingValue",
                table: "Reports",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            // Reports - TotalRevenue
            migrationBuilder.AlterColumn<decimal>(
                name: "TotalRevenue",
                table: "Reports",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            // Payments - Amount
            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            // Bookings - TotalPrice
            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "Bookings",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            // Bookings - PaidPrice
            migrationBuilder.AlterColumn<decimal>(
                name: "PaidPrice",
                table: "Bookings",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            // CarSales - FinalPrice
            migrationBuilder.AlterColumn<decimal>(
                name: "FinalPrice",
                table: "CarSales",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);
        }
    }
}
