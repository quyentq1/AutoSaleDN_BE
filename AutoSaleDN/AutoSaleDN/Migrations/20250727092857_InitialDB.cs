using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSaleDN.Migrations
{
    /// <inheritdoc />
    public partial class InitialDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlogCategories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogCategories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "BlogTags",
                columns: table => new
                {
                    TagId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogTags", x => x.TagId);
                });

            migrationBuilder.CreateTable(
                name: "CarColors",
                columns: table => new
                {
                    ColorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarColors", x => x.ColorId);
                });

            migrationBuilder.CreateTable(
                name: "CarFeatures",
                columns: table => new
                {
                    FeatureId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarFeatures", x => x.FeatureId);
                });

            migrationBuilder.CreateTable(
                name: "CarManufacturers",
                columns: table => new
                {
                    ManufacturerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarManufacturers", x => x.ManufacturerId);
                });

            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    PromotionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.PromotionId);
                });

            migrationBuilder.CreateTable(
                name: "SaleStatus",
                columns: table => new
                {
                    SaleStatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleStatus", x => x.SaleStatusId);
                });

            migrationBuilder.CreateTable(
                name: "StoreLocations",
                columns: table => new
                {
                    StoreLocationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreLocations", x => x.StoreLocationId);
                });

            migrationBuilder.CreateTable(
                name: "CarModels",
                columns: table => new
                {
                    ModelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManufacturerId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarModels", x => x.ModelId);
                    table.ForeignKey(
                        name: "FK_CarModels_CarManufacturers_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "CarManufacturers",
                        principalColumn: "ManufacturerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StoreLocationId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_StoreLocations_StoreLocationId",
                        column: x => x.StoreLocationId,
                        principalTable: "StoreLocations",
                        principalColumn: "StoreLocationId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BlogPosts",
                columns: table => new
                {
                    PostId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPosts", x => x.PostId);
                    table.ForeignKey(
                        name: "FK_BlogPosts_BlogCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "BlogCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlogPosts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarListings",
                columns: table => new
                {
                    ListingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModelId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: true),
                    Mileage = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Condition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatePosted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Certified = table.Column<bool>(type: "bit", nullable: false),
                    Vin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RentSell = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarListings", x => x.ListingId);
                    table.ForeignKey(
                        name: "FK_CarListings_CarModels_ModelId",
                        column: x => x.ModelId,
                        principalTable: "CarModels",
                        principalColumn: "ModelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarListings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryAddresses",
                columns: table => new
                {
                    AddressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecipientName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecipientPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AddressType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryAddresses", x => x.AddressId);
                    table.ForeignKey(
                        name: "FK_DeliveryAddresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ReportType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalListings = table.Column<int>(type: "int", nullable: true),
                    ActiveListings = table.Column<int>(type: "int", nullable: true),
                    SoldListings = table.Column<int>(type: "int", nullable: true),
                    RentedListings = table.Column<int>(type: "int", nullable: true),
                    AverageListingPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalListingValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalBookings = table.Column<int>(type: "int", nullable: true),
                    PendingBookings = table.Column<int>(type: "int", nullable: true),
                    ConfirmedBookings = table.Column<int>(type: "int", nullable: true),
                    CanceledBookings = table.Column<int>(type: "int", nullable: true),
                    CompletedBookings = table.Column<int>(type: "int", nullable: true),
                    TotalBookingValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalPayments = table.Column<int>(type: "int", nullable: true),
                    SuccessfulPayments = table.Column<int>(type: "int", nullable: true),
                    FailedPayments = table.Column<int>(type: "int", nullable: true),
                    PendingPayments = table.Column<int>(type: "int", nullable: true),
                    RefundedPayments = table.Column<int>(type: "int", nullable: true),
                    TotalRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalReviews = table.Column<int>(type: "int", nullable: true),
                    AverageRating = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    FiveStarReviews = table.Column<int>(type: "int", nullable: true),
                    FourStarReviews = table.Column<int>(type: "int", nullable: true),
                    ThreeStarReviews = table.Column<int>(type: "int", nullable: true),
                    TwoStarReviews = table.Column<int>(type: "int", nullable: true),
                    OneStarReviews = table.Column<int>(type: "int", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_Reports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlogPostTags",
                columns: table => new
                {
                    PostId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPostTags", x => new { x.PostId, x.TagId });
                    table.ForeignKey(
                        name: "FK_BlogPostTags_BlogPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "BlogPosts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlogPostTags_BlogTags_TagId",
                        column: x => x.TagId,
                        principalTable: "BlogTags",
                        principalColumn: "TagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarImages",
                columns: table => new
                {
                    ImageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ListingId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Filename = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarImages", x => x.ImageId);
                    table.ForeignKey(
                        name: "FK_CarImages_CarListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "CarListings",
                        principalColumn: "ListingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarListingFeatures",
                columns: table => new
                {
                    ListingId = table.Column<int>(type: "int", nullable: false),
                    FeatureId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarListingFeatures", x => new { x.ListingId, x.FeatureId });
                    table.ForeignKey(
                        name: "FK_CarListingFeatures_CarFeatures_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "CarFeatures",
                        principalColumn: "FeatureId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarListingFeatures_CarListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "CarListings",
                        principalColumn: "ListingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarPricingDetails",
                columns: table => new
                {
                    PricingDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ListingId = table.Column<int>(type: "int", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RegistrationFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarPricingDetails", x => x.PricingDetailId);
                    table.ForeignKey(
                        name: "FK_CarPricingDetails_CarListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "CarListings",
                        principalColumn: "ListingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarServiceHistories",
                columns: table => new
                {
                    HistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ListingId = table.Column<int>(type: "int", nullable: false),
                    RecentServicing = table.Column<bool>(type: "bit", nullable: false),
                    NoAccidentHistory = table.Column<bool>(type: "bit", nullable: false),
                    Modifications = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarServiceHistories", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_CarServiceHistories_CarListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "CarListings",
                        principalColumn: "ListingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarSpecifications",
                columns: table => new
                {
                    SpecificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ListingId = table.Column<int>(type: "int", nullable: false),
                    Engine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Transmission = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FuelType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeatingCapacity = table.Column<int>(type: "int", nullable: true),
                    InteriorColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExteriorColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CarType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarSpecifications", x => x.SpecificationId);
                    table.ForeignKey(
                        name: "FK_CarSpecifications_CarListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "CarListings",
                        principalColumn: "ListingId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    ReviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ListingId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reply = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.ReviewId);
                    table.ForeignKey(
                        name: "FK_Reviews_CarListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "CarListings",
                        principalColumn: "ListingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StoreListings",
                columns: table => new
                {
                    StoreListingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreLocationId = table.Column<int>(type: "int", nullable: false),
                    ListingId = table.Column<int>(type: "int", nullable: false),
                    InitialQuantity = table.Column<int>(type: "int", nullable: false),
                    CurrentQuantity = table.Column<int>(type: "int", nullable: false),
                    AvailableQuantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSoldDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RemovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReasonForRemoval = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastStatusChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastPurchasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AverageCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreListings", x => x.StoreListingId);
                    table.ForeignKey(
                        name: "FK_StoreListings_CarListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "CarListings",
                        principalColumn: "ListingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StoreListings_StoreLocations_StoreLocationId",
                        column: x => x.StoreLocationId,
                        principalTable: "StoreLocations",
                        principalColumn: "StoreLocationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarInventories",
                columns: table => new
                {
                    InventoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreListingId = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReferenceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarInventories", x => x.InventoryId);
                    table.ForeignKey(
                        name: "FK_CarInventories_StoreListings_StoreListingId",
                        column: x => x.StoreListingId,
                        principalTable: "StoreListings",
                        principalColumn: "StoreListingId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarSales",
                columns: table => new
                {
                    SaleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreListingId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    SaleStatusId = table.Column<int>(type: "int", nullable: false),
                    FinalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SaleDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrderNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DepositAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RemainingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeliveryOption = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShippingAddressId = table.Column<int>(type: "int", nullable: true),
                    PickupStoreLocationId = table.Column<int>(type: "int", nullable: true),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DepositPaymentId = table.Column<int>(type: "int", nullable: true),
                    FullPaymentId = table.Column<int>(type: "int", nullable: true),
                    OrderType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CarListingListingId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarSales", x => x.SaleId);
                    table.ForeignKey(
                        name: "FK_CarSales_CarListings_CarListingListingId",
                        column: x => x.CarListingListingId,
                        principalTable: "CarListings",
                        principalColumn: "ListingId");
                    table.ForeignKey(
                        name: "FK_CarSales_DeliveryAddresses_ShippingAddressId",
                        column: x => x.ShippingAddressId,
                        principalTable: "DeliveryAddresses",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarSales_SaleStatus_SaleStatusId",
                        column: x => x.SaleStatusId,
                        principalTable: "SaleStatus",
                        principalColumn: "SaleStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarSales_StoreListings_StoreListingId",
                        column: x => x.StoreListingId,
                        principalTable: "StoreListings",
                        principalColumn: "StoreListingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarSales_StoreLocations_PickupStoreLocationId",
                        column: x => x.PickupStoreLocationId,
                        principalTable: "StoreLocations",
                        principalColumn: "StoreLocationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarSales_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfPayment = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ListingId = table.Column<int>(type: "int", nullable: false),
                    AdditionalDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentForSaleId = table.Column<int>(type: "int", nullable: true),
                    PaymentPurpose = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_CarListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "CarListings",
                        principalColumn: "ListingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_CarSales_PaymentForSaleId",
                        column: x => x.PaymentForSaleId,
                        principalTable: "CarSales",
                        principalColumn: "SaleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    TransactionLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GatewayResponseCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GatewayResponseMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalDetails = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.TransactionLogId);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogCategories_Name",
                table: "BlogCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_CategoryId",
                table: "BlogPosts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_Slug",
                table: "BlogPosts",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_UserId",
                table: "BlogPosts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPostTags_TagId",
                table: "BlogPostTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogTags_Name",
                table: "BlogTags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarFeatures_Name",
                table: "CarFeatures",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarImages_ListingId",
                table: "CarImages",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_CarInventories_StoreListingId_TransactionDate",
                table: "CarInventories",
                columns: new[] { "StoreListingId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CarListingFeatures_FeatureId",
                table: "CarListingFeatures",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_CarListings_ModelId",
                table: "CarListings",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_CarListings_UserId",
                table: "CarListings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CarManufacturers_Name",
                table: "CarManufacturers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarModels_ManufacturerId",
                table: "CarModels",
                column: "ManufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_CarPricingDetails_ListingId",
                table: "CarPricingDetails",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_CarSales_CarListingListingId",
                table: "CarSales",
                column: "CarListingListingId");

            migrationBuilder.CreateIndex(
                name: "IX_CarSales_CustomerId",
                table: "CarSales",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CarSales_DepositPaymentId",
                table: "CarSales",
                column: "DepositPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_CarSales_FullPaymentId",
                table: "CarSales",
                column: "FullPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_CarSales_OrderNumber",
                table: "CarSales",
                column: "OrderNumber",
                unique: true,
                filter: "[OrderNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CarSales_PickupStoreLocationId",
                table: "CarSales",
                column: "PickupStoreLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CarSales_SaleStatusId",
                table: "CarSales",
                column: "SaleStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_CarSales_ShippingAddressId",
                table: "CarSales",
                column: "ShippingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_CarSales_StoreListingId",
                table: "CarSales",
                column: "StoreListingId");

            migrationBuilder.CreateIndex(
                name: "IX_CarServiceHistories_ListingId",
                table: "CarServiceHistories",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_CarSpecifications_ListingId",
                table: "CarSpecifications",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_CarVideos_ListingId",
                table: "CarVideos",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryAddresses_UserId",
                table: "DeliveryAddresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ListingId",
                table: "Payments",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentForSaleId",
                table: "Payments",
                column: "PaymentForSaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_PaymentId",
                table: "PaymentTransactions",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_UserId",
                table: "Reports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ListingId_UserId",
                table: "Reviews",
                columns: new[] { "ListingId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleStatus_StatusName",
                table: "SaleStatus",
                column: "StatusName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoreListings_ListingId",
                table: "StoreListings",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreListings_StoreLocationId",
                table: "StoreListings",
                column: "StoreLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Name",
                table: "Users",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_StoreLocationId",
                table: "Users",
                column: "StoreLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_CarSales_Payments_DepositPaymentId",
                table: "CarSales",
                column: "DepositPaymentId",
                principalTable: "Payments",
                principalColumn: "PaymentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CarSales_Payments_FullPaymentId",
                table: "CarSales",
                column: "FullPaymentId",
                principalTable: "Payments",
                principalColumn: "PaymentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarListings_Users_UserId",
                table: "CarListings");

            migrationBuilder.DropForeignKey(
                name: "FK_CarSales_Users_CustomerId",
                table: "CarSales");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryAddresses_Users_UserId",
                table: "DeliveryAddresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_UserId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_CarSales_CarListings_CarListingListingId",
                table: "CarSales");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_CarListings_ListingId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_StoreListings_CarListings_ListingId",
                table: "StoreListings");

            migrationBuilder.DropForeignKey(
                name: "FK_CarSales_StoreListings_StoreListingId",
                table: "CarSales");

            migrationBuilder.DropForeignKey(
                name: "FK_CarSales_DeliveryAddresses_ShippingAddressId",
                table: "CarSales");

            migrationBuilder.DropForeignKey(
                name: "FK_CarSales_Payments_DepositPaymentId",
                table: "CarSales");

            migrationBuilder.DropForeignKey(
                name: "FK_CarSales_Payments_FullPaymentId",
                table: "CarSales");

            migrationBuilder.DropTable(
                name: "BlogPostTags");

            migrationBuilder.DropTable(
                name: "CarColors");

            migrationBuilder.DropTable(
                name: "CarImages");

            migrationBuilder.DropTable(
                name: "CarInventories");

            migrationBuilder.DropTable(
                name: "CarListingFeatures");

            migrationBuilder.DropTable(
                name: "CarPricingDetails");

            migrationBuilder.DropTable(
                name: "CarServiceHistories");

            migrationBuilder.DropTable(
                name: "CarSpecifications");

            migrationBuilder.DropTable(
                name: "CarVideos");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "Promotions");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "BlogPosts");

            migrationBuilder.DropTable(
                name: "BlogTags");

            migrationBuilder.DropTable(
                name: "CarFeatures");

            migrationBuilder.DropTable(
                name: "BlogCategories");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "CarListings");

            migrationBuilder.DropTable(
                name: "CarModels");

            migrationBuilder.DropTable(
                name: "CarManufacturers");

            migrationBuilder.DropTable(
                name: "StoreListings");

            migrationBuilder.DropTable(
                name: "DeliveryAddresses");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "CarSales");

            migrationBuilder.DropTable(
                name: "SaleStatus");

            migrationBuilder.DropTable(
                name: "StoreLocations");
        }
    }
}
