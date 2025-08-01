USE [master]
GO
/****** Object:  Database [AutoSaleDN]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE DATABASE [AutoSaleDN]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'AutoSaleDN', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.QUYENTQ\MSSQL\DATA\AutoSaleDN.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'AutoSaleDN_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.QUYENTQ\MSSQL\DATA\AutoSaleDN_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
ALTER DATABASE [AutoSaleDN] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [AutoSaleDN].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [AutoSaleDN] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [AutoSaleDN] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [AutoSaleDN] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [AutoSaleDN] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [AutoSaleDN] SET ARITHABORT OFF 
GO
ALTER DATABASE [AutoSaleDN] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [AutoSaleDN] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [AutoSaleDN] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [AutoSaleDN] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [AutoSaleDN] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [AutoSaleDN] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [AutoSaleDN] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [AutoSaleDN] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [AutoSaleDN] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [AutoSaleDN] SET  ENABLE_BROKER 
GO
ALTER DATABASE [AutoSaleDN] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [AutoSaleDN] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [AutoSaleDN] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [AutoSaleDN] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [AutoSaleDN] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [AutoSaleDN] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [AutoSaleDN] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [AutoSaleDN] SET RECOVERY FULL 
GO
ALTER DATABASE [AutoSaleDN] SET  MULTI_USER 
GO
ALTER DATABASE [AutoSaleDN] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [AutoSaleDN] SET DB_CHAINING OFF 
GO
ALTER DATABASE [AutoSaleDN] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [AutoSaleDN] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [AutoSaleDN] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [AutoSaleDN] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'AutoSaleDN', N'ON'
GO
ALTER DATABASE [AutoSaleDN] SET QUERY_STORE = OFF
GO
USE [AutoSaleDN]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BlogCategories]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BlogCategories](
	[CategoryId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_BlogCategories] PRIMARY KEY CLUSTERED 
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BlogPosts]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BlogPosts](
	[PostId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[CategoryId] [int] NOT NULL,
	[Title] [nvarchar](255) NOT NULL,
	[Slug] [nvarchar](255) NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[PublishedDate] [datetime2](7) NULL,
	[IsPublished] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_BlogPosts] PRIMARY KEY CLUSTERED 
(
	[PostId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BlogPostTags]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BlogPostTags](
	[PostId] [int] NOT NULL,
	[TagId] [int] NOT NULL,
 CONSTRAINT [PK_BlogPostTags] PRIMARY KEY CLUSTERED 
(
	[PostId] ASC,
	[TagId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BlogTags]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BlogTags](
	[TagId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_BlogTags] PRIMARY KEY CLUSTERED 
(
	[TagId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CarColors]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CarColors](
	[ColorId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Status] [bit] NOT NULL,
 CONSTRAINT [PK_CarColors] PRIMARY KEY CLUSTERED 
(
	[ColorId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CarFeatures]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CarFeatures](
	[FeatureId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Status] [bit] NOT NULL,
 CONSTRAINT [PK_CarFeatures] PRIMARY KEY CLUSTERED 
(
	[FeatureId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CarImages]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CarImages](
	[ImageId] [int] IDENTITY(1,1) NOT NULL,
	[ListingId] [int] NOT NULL,
	[Url] [nvarchar](max) NULL,
	[Filename] [nvarchar](max) NULL,
 CONSTRAINT [PK_CarImages] PRIMARY KEY CLUSTERED 
(
	[ImageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CarInventories]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CarInventories](
	[InventoryId] [int] IDENTITY(1,1) NOT NULL,
	[StoreListingId] [int] NOT NULL,
	[TransactionType] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[UnitPrice] [decimal](18, 2) NULL,
	[ReferenceId] [nvarchar](max) NOT NULL,
	[Notes] [nvarchar](max) NOT NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[TransactionDate] [datetime2](7) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_CarInventories] PRIMARY KEY CLUSTERED 
(
	[InventoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CarListingFeatures]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CarListingFeatures](
	[ListingId] [int] NOT NULL,
	[FeatureId] [int] NOT NULL,
 CONSTRAINT [PK_CarListingFeatures] PRIMARY KEY CLUSTERED 
(
	[ListingId] ASC,
	[FeatureId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CarListings]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CarListings](
	[ListingId] [int] IDENTITY(1,1) NOT NULL,
	[ModelId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[Year] [int] NULL,
	[Mileage] [int] NULL,
	[Price] [decimal](18, 2) NULL,
	[Condition] [nvarchar](max) NULL,
	[DatePosted] [datetime2](7) NOT NULL,
	[DateUpdated] [datetime2](7) NOT NULL,
	[Certified] [bit] NOT NULL,
	[Vin] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[RentSell] [nvarchar](max) NULL,
 CONSTRAINT [PK_CarListings] PRIMARY KEY CLUSTERED 
(
	[ListingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CarManufacturers]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CarManufacturers](
	[ManufacturerId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_CarManufacturers] PRIMARY KEY CLUSTERED 
(
	[ManufacturerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CarModels]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CarModels](
	[ModelId] [int] IDENTITY(1,1) NOT NULL,
	[ManufacturerId] [int] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Status] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_CarModels] PRIMARY KEY CLUSTERED 
(
	[ModelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CarPricingDetails]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CarPricingDetails](
	[PricingDetailId] [int] IDENTITY(1,1) NOT NULL,
	[ListingId] [int] NOT NULL,
	[TaxRate] [decimal](18, 2) NOT NULL,
	[RegistrationFee] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_CarPricingDetails] PRIMARY KEY CLUSTERED 
(
	[PricingDetailId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CarSales]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CarSales](
	[SaleId] [int] IDENTITY(1,1) NOT NULL,
	[StoreListingId] [int] NOT NULL,
	[CustomerId] [int] NOT NULL,
	[SaleStatusId] [int] NOT NULL,
	[FinalPrice] [decimal](18, 2) NOT NULL,
	[SaleDate] [datetime2](7) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[OrderNumber] [nvarchar](450) NULL,
	[DepositAmount] [decimal](18, 2) NULL,
	[RemainingBalance] [decimal](18, 2) NULL,
	[DeliveryOption] [nvarchar](max) NULL,
	[ShippingAddressId] [int] NULL,
	[PickupStoreLocationId] [int] NULL,
	[ExpectedDeliveryDate] [datetime2](7) NULL,
	[ActualDeliveryDate] [datetime2](7) NULL,
	[DepositPaymentId] [int] NULL,
	[FullPaymentId] [int] NULL,
	[OrderType] [nvarchar](max) NULL,
	[Notes] [nvarchar](max) NULL,
	[CarListingListingId] [int] NULL,
 CONSTRAINT [PK_CarSales] PRIMARY KEY CLUSTERED 
(
	[SaleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CarServiceHistories]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CarServiceHistories](
	[HistoryId] [int] IDENTITY(1,1) NOT NULL,
	[ListingId] [int] NOT NULL,
	[RecentServicing] [bit] NOT NULL,
	[NoAccidentHistory] [bit] NOT NULL,
	[Modifications] [bit] NOT NULL,
 CONSTRAINT [PK_CarServiceHistories] PRIMARY KEY CLUSTERED 
(
	[HistoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CarSpecifications]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CarSpecifications](
	[SpecificationId] [int] IDENTITY(1,1) NOT NULL,
	[ListingId] [int] NOT NULL,
	[Engine] [nvarchar](max) NULL,
	[Transmission] [nvarchar](max) NULL,
	[FuelType] [nvarchar](max) NULL,
	[SeatingCapacity] [int] NULL,
	[InteriorColor] [nvarchar](max) NULL,
	[ExteriorColor] [nvarchar](max) NULL,
	[CarType] [nvarchar](max) NULL,
 CONSTRAINT [PK_CarSpecifications] PRIMARY KEY CLUSTERED 
(
	[SpecificationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CarVideos]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CarVideos](
	[VideoId] [int] IDENTITY(1,1) NOT NULL,
	[ListingId] [int] NOT NULL,
	[Url] [nvarchar](max) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_CarVideos] PRIMARY KEY CLUSTERED 
(
	[VideoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DeliveryAddresses]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DeliveryAddresses](
	[AddressId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[Address] [nvarchar](max) NOT NULL,
	[Note] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[RecipientName] [nvarchar](max) NULL,
	[RecipientPhone] [nvarchar](max) NULL,
	[IsDefault] [bit] NOT NULL,
	[AddressType] [nvarchar](max) NULL,
 CONSTRAINT [PK_DeliveryAddresses] PRIMARY KEY CLUSTERED 
(
	[AddressId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Payments]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Payments](
	[PaymentId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[TransactionId] [nvarchar](max) NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[PaymentMethod] [nvarchar](max) NULL,
	[PaymentStatus] [nvarchar](max) NULL,
	[DateOfPayment] [datetime2](7) NOT NULL,
	[ListingId] [int] NOT NULL,
	[AdditionalDetails] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
	[PaymentForSaleId] [int] NULL,
	[PaymentPurpose] [nvarchar](max) NULL,
 CONSTRAINT [PK_Payments] PRIMARY KEY CLUSTERED 
(
	[PaymentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PaymentTransactions]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentTransactions](
	[TransactionLogId] [int] IDENTITY(1,1) NOT NULL,
	[PaymentId] [int] NOT NULL,
	[TransactionDate] [datetime2](7) NOT NULL,
	[GatewayResponseCode] [nvarchar](max) NULL,
	[GatewayResponseMessage] [nvarchar](max) NULL,
	[TransactionStatus] [nvarchar](max) NULL,
	[AdditionalDetails] [nvarchar](max) NULL,
 CONSTRAINT [PK_PaymentTransactions] PRIMARY KEY CLUSTERED 
(
	[TransactionLogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Promotions]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Promotions](
	[PromotionId] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[DiscountAmount] [decimal](18, 2) NOT NULL,
	[StartDate] [datetime2](7) NOT NULL,
	[EndDate] [datetime2](7) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
 CONSTRAINT [PK_Promotions] PRIMARY KEY CLUSTERED 
(
	[PromotionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Reports]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Reports](
	[ReportId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[ReportType] [nvarchar](max) NOT NULL,
	[StartDate] [datetime2](7) NOT NULL,
	[EndDate] [datetime2](7) NOT NULL,
	[TotalListings] [int] NULL,
	[ActiveListings] [int] NULL,
	[SoldListings] [int] NULL,
	[RentedListings] [int] NULL,
	[AverageListingPrice] [decimal](18, 2) NULL,
	[TotalListingValue] [decimal](18, 2) NULL,
	[TotalBookings] [int] NULL,
	[PendingBookings] [int] NULL,
	[ConfirmedBookings] [int] NULL,
	[CanceledBookings] [int] NULL,
	[CompletedBookings] [int] NULL,
	[TotalBookingValue] [decimal](18, 2) NULL,
	[TotalPayments] [int] NULL,
	[SuccessfulPayments] [int] NULL,
	[FailedPayments] [int] NULL,
	[PendingPayments] [int] NULL,
	[RefundedPayments] [int] NULL,
	[TotalRevenue] [decimal](18, 2) NULL,
	[TotalReviews] [int] NULL,
	[AverageRating] [decimal](3, 2) NULL,
	[FiveStarReviews] [int] NULL,
	[FourStarReviews] [int] NULL,
	[ThreeStarReviews] [int] NULL,
	[TwoStarReviews] [int] NULL,
	[OneStarReviews] [int] NULL,
	[GeneratedAt] [datetime2](7) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Reports] PRIMARY KEY CLUSTERED 
(
	[ReportId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Reviews]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Reviews](
	[ReviewId] [int] IDENTITY(1,1) NOT NULL,
	[ListingId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[Rating] [int] NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[Reply] [nvarchar](max) NULL,
	[UpdatedAt] [datetime2](7) NULL,
 CONSTRAINT [PK_Reviews] PRIMARY KEY CLUSTERED 
(
	[ReviewId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SaleStatus]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SaleStatus](
	[SaleStatusId] [int] IDENTITY(1,1) NOT NULL,
	[StatusName] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_SaleStatus] PRIMARY KEY CLUSTERED 
(
	[SaleStatusId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StoreListings]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StoreListings](
	[StoreListingId] [int] IDENTITY(1,1) NOT NULL,
	[StoreLocationId] [int] NOT NULL,
	[ListingId] [int] NOT NULL,
	[InitialQuantity] [int] NOT NULL,
	[CurrentQuantity] [int] NOT NULL,
	[AvailableQuantity] [int] NOT NULL,
	[Status] [nvarchar](max) NOT NULL,
	[AddedDate] [datetime2](7) NOT NULL,
	[LastSoldDate] [datetime2](7) NULL,
	[RemovedDate] [datetime2](7) NULL,
	[ReasonForRemoval] [nvarchar](max) NULL,
	[LastStatusChangeDate] [datetime2](7) NULL,
	[LastPurchasePrice] [decimal](18, 2) NULL,
	[AverageCost] [decimal](18, 2) NULL,
 CONSTRAINT [PK_StoreListings] PRIMARY KEY CLUSTERED 
(
	[StoreListingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StoreLocations]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StoreLocations](
	[StoreLocationId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Address] [nvarchar](max) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
 CONSTRAINT [PK_StoreLocations] PRIMARY KEY CLUSTERED 
(
	[StoreLocationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 27/7/2025 10:02:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Password] [nvarchar](255) NOT NULL,
	[Email] [nvarchar](100) NOT NULL,
	[FullName] [nvarchar](100) NOT NULL,
	[Mobile] [nvarchar](15) NULL,
	[Role] [nvarchar](20) NOT NULL,
	[Province] [nvarchar](100) NOT NULL,
	[Status] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
	[StoreLocationId] [int] NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_BlogCategories_Name]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_BlogCategories_Name] ON [dbo].[BlogCategories]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_BlogPosts_CategoryId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_BlogPosts_CategoryId] ON [dbo].[BlogPosts]
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_BlogPosts_Slug]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_BlogPosts_Slug] ON [dbo].[BlogPosts]
(
	[Slug] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_BlogPosts_UserId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_BlogPosts_UserId] ON [dbo].[BlogPosts]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_BlogPostTags_TagId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_BlogPostTags_TagId] ON [dbo].[BlogPostTags]
(
	[TagId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_BlogTags_Name]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_BlogTags_Name] ON [dbo].[BlogTags]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_CarFeatures_Name]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_CarFeatures_Name] ON [dbo].[CarFeatures]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CarImages_ListingId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_CarImages_ListingId] ON [dbo].[CarImages]
(
	[ListingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CarInventories_StoreListingId_TransactionDate]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_CarInventories_StoreListingId_TransactionDate] ON [dbo].[CarInventories]
(
	[StoreListingId] ASC,
	[TransactionDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CarListingFeatures_FeatureId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_CarListingFeatures_FeatureId] ON [dbo].[CarListingFeatures]
(
	[FeatureId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CarListings_ModelId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_CarListings_ModelId] ON [dbo].[CarListings]
(
	[ModelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CarListings_UserId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_CarListings_UserId] ON [dbo].[CarListings]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_CarManufacturers_Name]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_CarManufacturers_Name] ON [dbo].[CarManufacturers]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CarModels_ManufacturerId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_CarModels_ManufacturerId] ON [dbo].[CarModels]
(
	[ManufacturerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CarPricingDetails_ListingId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_CarPricingDetails_ListingId] ON [dbo].[CarPricingDetails]
(
	[ListingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CarSales_CarListingListingId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_CarSales_CarListingListingId] ON [dbo].[CarSales]
(
	[CarListingListingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CarSales_CustomerId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_CarSales_CustomerId] ON [dbo].[CarSales]
(
	[CustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CarSales_DepositPaymentId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_CarSales_DepositPaymentId] ON [dbo].[CarSales]
(
	[DepositPaymentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CarSales_FullPaymentId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_CarSales_FullPaymentId] ON [dbo].[CarSales]
(
	[FullPaymentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_CarSales_OrderNumber]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_CarSales_OrderNumber] ON [dbo].[CarSales]
(
	[OrderNumber] ASC
)
WHERE ([OrderNumber] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CarSales_PickupStoreLocationId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_CarSales_PickupStoreLocationId] ON [dbo].[CarSales]
(
	[PickupStoreLocationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CarSales_SaleStatusId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_CarSales_SaleStatusId] ON [dbo].[CarSales]
(
	[SaleStatusId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CarSales_ShippingAddressId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_CarSales_ShippingAddressId] ON [dbo].[CarSales]
(
	[ShippingAddressId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CarSales_StoreListingId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_CarSales_StoreListingId] ON [dbo].[CarSales]
(
	[StoreListingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CarServiceHistories_ListingId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_CarServiceHistories_ListingId] ON [dbo].[CarServiceHistories]
(
	[ListingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CarSpecifications_ListingId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_CarSpecifications_ListingId] ON [dbo].[CarSpecifications]
(
	[ListingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CarVideos_ListingId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_CarVideos_ListingId] ON [dbo].[CarVideos]
(
	[ListingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DeliveryAddresses_UserId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_DeliveryAddresses_UserId] ON [dbo].[DeliveryAddresses]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Payments_ListingId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_Payments_ListingId] ON [dbo].[Payments]
(
	[ListingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Payments_PaymentForSaleId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_Payments_PaymentForSaleId] ON [dbo].[Payments]
(
	[PaymentForSaleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Payments_UserId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_Payments_UserId] ON [dbo].[Payments]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PaymentTransactions_PaymentId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_PaymentTransactions_PaymentId] ON [dbo].[PaymentTransactions]
(
	[PaymentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Reports_UserId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_Reports_UserId] ON [dbo].[Reports]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Reviews_ListingId_UserId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Reviews_ListingId_UserId] ON [dbo].[Reviews]
(
	[ListingId] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Reviews_UserId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_Reviews_UserId] ON [dbo].[Reviews]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_SaleStatus_StatusName]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_SaleStatus_StatusName] ON [dbo].[SaleStatus]
(
	[StatusName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_StoreListings_ListingId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_StoreListings_ListingId] ON [dbo].[StoreListings]
(
	[ListingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_StoreListings_StoreLocationId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_StoreListings_StoreLocationId] ON [dbo].[StoreListings]
(
	[StoreLocationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Users_Email]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users]
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Users_Name]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Name] ON [dbo].[Users]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Users_StoreLocationId]    Script Date: 27/7/2025 10:02:05 PM ******/
CREATE NONCLUSTERED INDEX [IX_Users_StoreLocationId] ON [dbo].[Users]
(
	[StoreLocationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[DeliveryAddresses] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDefault]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (CONVERT([bit],(0))) FOR [Status]
GO
ALTER TABLE [dbo].[BlogPosts]  WITH CHECK ADD  CONSTRAINT [FK_BlogPosts_BlogCategories_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[BlogCategories] ([CategoryId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[BlogPosts] CHECK CONSTRAINT [FK_BlogPosts_BlogCategories_CategoryId]
GO
ALTER TABLE [dbo].[BlogPosts]  WITH CHECK ADD  CONSTRAINT [FK_BlogPosts_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[BlogPosts] CHECK CONSTRAINT [FK_BlogPosts_Users_UserId]
GO
ALTER TABLE [dbo].[BlogPostTags]  WITH CHECK ADD  CONSTRAINT [FK_BlogPostTags_BlogPosts_PostId] FOREIGN KEY([PostId])
REFERENCES [dbo].[BlogPosts] ([PostId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[BlogPostTags] CHECK CONSTRAINT [FK_BlogPostTags_BlogPosts_PostId]
GO
ALTER TABLE [dbo].[BlogPostTags]  WITH CHECK ADD  CONSTRAINT [FK_BlogPostTags_BlogTags_TagId] FOREIGN KEY([TagId])
REFERENCES [dbo].[BlogTags] ([TagId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[BlogPostTags] CHECK CONSTRAINT [FK_BlogPostTags_BlogTags_TagId]
GO
ALTER TABLE [dbo].[CarImages]  WITH CHECK ADD  CONSTRAINT [FK_CarImages_CarListings_ListingId] FOREIGN KEY([ListingId])
REFERENCES [dbo].[CarListings] ([ListingId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CarImages] CHECK CONSTRAINT [FK_CarImages_CarListings_ListingId]
GO
ALTER TABLE [dbo].[CarInventories]  WITH CHECK ADD  CONSTRAINT [FK_CarInventories_StoreListings_StoreListingId] FOREIGN KEY([StoreListingId])
REFERENCES [dbo].[StoreListings] ([StoreListingId])
GO
ALTER TABLE [dbo].[CarInventories] CHECK CONSTRAINT [FK_CarInventories_StoreListings_StoreListingId]
GO
ALTER TABLE [dbo].[CarListingFeatures]  WITH CHECK ADD  CONSTRAINT [FK_CarListingFeatures_CarFeatures_FeatureId] FOREIGN KEY([FeatureId])
REFERENCES [dbo].[CarFeatures] ([FeatureId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CarListingFeatures] CHECK CONSTRAINT [FK_CarListingFeatures_CarFeatures_FeatureId]
GO
ALTER TABLE [dbo].[CarListingFeatures]  WITH CHECK ADD  CONSTRAINT [FK_CarListingFeatures_CarListings_ListingId] FOREIGN KEY([ListingId])
REFERENCES [dbo].[CarListings] ([ListingId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CarListingFeatures] CHECK CONSTRAINT [FK_CarListingFeatures_CarListings_ListingId]
GO
ALTER TABLE [dbo].[CarListings]  WITH CHECK ADD  CONSTRAINT [FK_CarListings_CarModels_ModelId] FOREIGN KEY([ModelId])
REFERENCES [dbo].[CarModels] ([ModelId])
GO
ALTER TABLE [dbo].[CarListings] CHECK CONSTRAINT [FK_CarListings_CarModels_ModelId]
GO
ALTER TABLE [dbo].[CarListings]  WITH CHECK ADD  CONSTRAINT [FK_CarListings_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CarListings] CHECK CONSTRAINT [FK_CarListings_Users_UserId]
GO
ALTER TABLE [dbo].[CarModels]  WITH CHECK ADD  CONSTRAINT [FK_CarModels_CarManufacturers_ManufacturerId] FOREIGN KEY([ManufacturerId])
REFERENCES [dbo].[CarManufacturers] ([ManufacturerId])
GO
ALTER TABLE [dbo].[CarModels] CHECK CONSTRAINT [FK_CarModels_CarManufacturers_ManufacturerId]
GO
ALTER TABLE [dbo].[CarPricingDetails]  WITH CHECK ADD  CONSTRAINT [FK_CarPricingDetails_CarListings_ListingId] FOREIGN KEY([ListingId])
REFERENCES [dbo].[CarListings] ([ListingId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CarPricingDetails] CHECK CONSTRAINT [FK_CarPricingDetails_CarListings_ListingId]
GO
ALTER TABLE [dbo].[CarSales]  WITH CHECK ADD  CONSTRAINT [FK_CarSales_CarListings_CarListingListingId] FOREIGN KEY([CarListingListingId])
REFERENCES [dbo].[CarListings] ([ListingId])
GO
ALTER TABLE [dbo].[CarSales] CHECK CONSTRAINT [FK_CarSales_CarListings_CarListingListingId]
GO
ALTER TABLE [dbo].[CarSales]  WITH CHECK ADD  CONSTRAINT [FK_CarSales_DeliveryAddresses_ShippingAddressId] FOREIGN KEY([ShippingAddressId])
REFERENCES [dbo].[DeliveryAddresses] ([AddressId])
GO
ALTER TABLE [dbo].[CarSales] CHECK CONSTRAINT [FK_CarSales_DeliveryAddresses_ShippingAddressId]
GO
ALTER TABLE [dbo].[CarSales]  WITH CHECK ADD  CONSTRAINT [FK_CarSales_Payments_DepositPaymentId] FOREIGN KEY([DepositPaymentId])
REFERENCES [dbo].[Payments] ([PaymentId])
GO
ALTER TABLE [dbo].[CarSales] CHECK CONSTRAINT [FK_CarSales_Payments_DepositPaymentId]
GO
ALTER TABLE [dbo].[CarSales]  WITH CHECK ADD  CONSTRAINT [FK_CarSales_Payments_FullPaymentId] FOREIGN KEY([FullPaymentId])
REFERENCES [dbo].[Payments] ([PaymentId])
GO
ALTER TABLE [dbo].[CarSales] CHECK CONSTRAINT [FK_CarSales_Payments_FullPaymentId]
GO
ALTER TABLE [dbo].[CarSales]  WITH CHECK ADD  CONSTRAINT [FK_CarSales_SaleStatus_SaleStatusId] FOREIGN KEY([SaleStatusId])
REFERENCES [dbo].[SaleStatus] ([SaleStatusId])
GO
ALTER TABLE [dbo].[CarSales] CHECK CONSTRAINT [FK_CarSales_SaleStatus_SaleStatusId]
GO
ALTER TABLE [dbo].[CarSales]  WITH CHECK ADD  CONSTRAINT [FK_CarSales_StoreListings_StoreListingId] FOREIGN KEY([StoreListingId])
REFERENCES [dbo].[StoreListings] ([StoreListingId])
GO
ALTER TABLE [dbo].[CarSales] CHECK CONSTRAINT [FK_CarSales_StoreListings_StoreListingId]
GO
ALTER TABLE [dbo].[CarSales]  WITH CHECK ADD  CONSTRAINT [FK_CarSales_StoreLocations_PickupStoreLocationId] FOREIGN KEY([PickupStoreLocationId])
REFERENCES [dbo].[StoreLocations] ([StoreLocationId])
GO
ALTER TABLE [dbo].[CarSales] CHECK CONSTRAINT [FK_CarSales_StoreLocations_PickupStoreLocationId]
GO
ALTER TABLE [dbo].[CarSales]  WITH CHECK ADD  CONSTRAINT [FK_CarSales_Users_CustomerId] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[CarSales] CHECK CONSTRAINT [FK_CarSales_Users_CustomerId]
GO
ALTER TABLE [dbo].[CarServiceHistories]  WITH CHECK ADD  CONSTRAINT [FK_CarServiceHistories_CarListings_ListingId] FOREIGN KEY([ListingId])
REFERENCES [dbo].[CarListings] ([ListingId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CarServiceHistories] CHECK CONSTRAINT [FK_CarServiceHistories_CarListings_ListingId]
GO
ALTER TABLE [dbo].[CarSpecifications]  WITH CHECK ADD  CONSTRAINT [FK_CarSpecifications_CarListings_ListingId] FOREIGN KEY([ListingId])
REFERENCES [dbo].[CarListings] ([ListingId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CarSpecifications] CHECK CONSTRAINT [FK_CarSpecifications_CarListings_ListingId]
GO
ALTER TABLE [dbo].[CarVideos]  WITH CHECK ADD  CONSTRAINT [FK_CarVideos_CarListings_ListingId] FOREIGN KEY([ListingId])
REFERENCES [dbo].[CarListings] ([ListingId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CarVideos] CHECK CONSTRAINT [FK_CarVideos_CarListings_ListingId]
GO
ALTER TABLE [dbo].[DeliveryAddresses]  WITH CHECK ADD  CONSTRAINT [FK_DeliveryAddresses_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[DeliveryAddresses] CHECK CONSTRAINT [FK_DeliveryAddresses_Users_UserId]
GO
ALTER TABLE [dbo].[Payments]  WITH CHECK ADD  CONSTRAINT [FK_Payments_CarListings_ListingId] FOREIGN KEY([ListingId])
REFERENCES [dbo].[CarListings] ([ListingId])
GO
ALTER TABLE [dbo].[Payments] CHECK CONSTRAINT [FK_Payments_CarListings_ListingId]
GO
ALTER TABLE [dbo].[Payments]  WITH CHECK ADD  CONSTRAINT [FK_Payments_CarSales_PaymentForSaleId] FOREIGN KEY([PaymentForSaleId])
REFERENCES [dbo].[CarSales] ([SaleId])
GO
ALTER TABLE [dbo].[Payments] CHECK CONSTRAINT [FK_Payments_CarSales_PaymentForSaleId]
GO
ALTER TABLE [dbo].[Payments]  WITH CHECK ADD  CONSTRAINT [FK_Payments_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[Payments] CHECK CONSTRAINT [FK_Payments_Users_UserId]
GO
ALTER TABLE [dbo].[PaymentTransactions]  WITH CHECK ADD  CONSTRAINT [FK_PaymentTransactions_Payments_PaymentId] FOREIGN KEY([PaymentId])
REFERENCES [dbo].[Payments] ([PaymentId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PaymentTransactions] CHECK CONSTRAINT [FK_PaymentTransactions_Payments_PaymentId]
GO
ALTER TABLE [dbo].[Reports]  WITH CHECK ADD  CONSTRAINT [FK_Reports_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Reports] CHECK CONSTRAINT [FK_Reports_Users_UserId]
GO
ALTER TABLE [dbo].[Reviews]  WITH CHECK ADD  CONSTRAINT [FK_Reviews_CarListings_ListingId] FOREIGN KEY([ListingId])
REFERENCES [dbo].[CarListings] ([ListingId])
GO
ALTER TABLE [dbo].[Reviews] CHECK CONSTRAINT [FK_Reviews_CarListings_ListingId]
GO
ALTER TABLE [dbo].[Reviews]  WITH CHECK ADD  CONSTRAINT [FK_Reviews_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[Reviews] CHECK CONSTRAINT [FK_Reviews_Users_UserId]
GO
ALTER TABLE [dbo].[StoreListings]  WITH CHECK ADD  CONSTRAINT [FK_StoreListings_CarListings_ListingId] FOREIGN KEY([ListingId])
REFERENCES [dbo].[CarListings] ([ListingId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StoreListings] CHECK CONSTRAINT [FK_StoreListings_CarListings_ListingId]
GO
ALTER TABLE [dbo].[StoreListings]  WITH CHECK ADD  CONSTRAINT [FK_StoreListings_StoreLocations_StoreLocationId] FOREIGN KEY([StoreLocationId])
REFERENCES [dbo].[StoreLocations] ([StoreLocationId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StoreListings] CHECK CONSTRAINT [FK_StoreListings_StoreLocations_StoreLocationId]
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_StoreLocations_StoreLocationId] FOREIGN KEY([StoreLocationId])
REFERENCES [dbo].[StoreLocations] ([StoreLocationId])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [FK_Users_StoreLocations_StoreLocationId]
GO
USE [master]
GO
ALTER DATABASE [AutoSaleDN] SET  READ_WRITE 
GO
