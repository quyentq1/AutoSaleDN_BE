using Microsoft.EntityFrameworkCore;

namespace AutoSaleDN.Models
{
    public class AutoSaleDbContext : DbContext
    {
        public AutoSaleDbContext(DbContextOptions<AutoSaleDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<CarManufacturer> CarManufacturers { get; set; }
        public DbSet<CarModel> CarModels { get; set; }
        public DbSet<CarListing> CarListings { get; set; }
        public DbSet<CarSpecification> CarSpecifications { get; set; }
        public DbSet<CarFeature> CarFeatures { get; set; }
        public DbSet<CarListingFeature> CarListingFeatures { get; set; }
        public DbSet<CarServiceHistory> CarServiceHistories { get; set; }
        public DbSet<CarPricingDetail> CarPricingDetails { get; set; }
        public DbSet<CarImage> CarImages { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<BlogCategory> BlogCategories { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BlogTag> BlogTags { get; set; }
        public DbSet<BlogPostTag> BlogPostTags { get; set; }
        public DbSet<SaleStatus> SaleStatus { get; set; }
        public DbSet<CarSale> CarSales { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        public DbSet<CarInventory> CarInventories { get; set; }
        public DbSet<CarColor> CarColors { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<StoreLocation> StoreLocations { get; set; }
        public DbSet<DeliveryAddress> DeliveryAddresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Unique constraints
            modelBuilder.Entity<User>().HasIndex(u => u.Name).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<CarManufacturer>().HasIndex(m => m.Name).IsUnique();
            modelBuilder.Entity<CarFeature>().HasIndex(f => f.Name).IsUnique();
            modelBuilder.Entity<BlogCategory>().HasIndex(c => c.Name).IsUnique();
            modelBuilder.Entity<BlogTag>().HasIndex(t => t.Name).IsUnique();
            modelBuilder.Entity<BlogPost>().HasIndex(p => p.Slug).IsUnique();
            modelBuilder.Entity<SaleStatus>().HasIndex(s => s.StatusName).IsUnique();

            // Composite keys
            modelBuilder.Entity<CarListingFeature>().HasKey(clf => new { clf.ListingId, clf.FeatureId });
            modelBuilder.Entity<BlogPostTag>().HasKey(bpt => new { bpt.PostId, bpt.TagId });
            modelBuilder.Entity<Review>().HasIndex(r => new { r.ListingId, r.UserId }).IsUnique();

            // Decimal precision
            modelBuilder.Entity<CarListing>().Property(c => c.Price).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<CarPricingDetail>().Property(c => c.TaxRate).HasColumnType("decimal(5,4)");
            modelBuilder.Entity<CarPricingDetail>().Property(c => c.RegistrationFee).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Booking>().Property(b => b.TotalPrice).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Booking>().Property(b => b.PaidPrice).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Payment>().Property(p => p.Amount).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<CarSale>().Property(c => c.FinalPrice).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Report>().Property(r => r.AverageListingPrice).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Report>().Property(r => r.TotalListingValue).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Report>().Property(r => r.TotalBookingValue).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Report>().Property(r => r.TotalRevenue).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Report>().Property(r => r.AverageRating).HasColumnType("decimal(3,2)");

            // Restrict Delete (prevent cascade cycles)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Listing)
                .WithMany(l => l.Bookings)
                .HasForeignKey(b => b.ListingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Listing)
                .WithMany(l => l.Payments)
                .HasForeignKey(p => p.ListingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CarSale>()
                .HasOne(cs => cs.Listing)
                .WithMany(cl => cl.CarSales)
                .HasForeignKey(cs => cs.ListingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CarSale>()
                .HasOne(cs => cs.Booking)
                .WithMany(b => b.CarSales)
                .HasForeignKey(cs => cs.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CarSale>()
                .HasOne(cs => cs.SaleStatus)
                .WithMany(ss => ss.CarSales)
                .HasForeignKey(cs => cs.SaleStatusId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Listing)
                .WithMany(l => l.Reviews)
                .HasForeignKey(r => r.ListingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StoreLocation>()
                .HasOne(s => s.User)
                .WithOne()
                .HasForeignKey<StoreLocation>(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
