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

        public DbSet<CarVideo> CarVideos { get; set; }

        public DbSet<SaleStatusHistory> SaleStatusHistory { get; set; }

        public DbSet<StoreListing> StoreListings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
            modelBuilder.Entity<CarListing>().Property(c => c.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<CarPricingDetail>().Property(c => c.TaxRate).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<CarPricingDetail>().Property(c => c.RegistrationFee).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Payment>().Property(p => p.Amount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<CarSale>().Property(c => c.FinalPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Report>().Property(r => r.AverageListingPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Report>().Property(r => r.TotalListingValue).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Report>().Property(r => r.TotalRevenue).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Report>().Property(r => r.AverageRating).HasColumnType("decimal(3,2)");

            // --- Cấu hình cho CarSale ---
            modelBuilder.Entity<CarSale>(entity =>
            {
                entity.HasIndex(cs => cs.OrderNumber).IsUnique();
                entity.Property(cs => cs.DepositAmount).HasColumnType("decimal(18,2)");
                entity.Property(cs => cs.RemainingBalance).HasColumnType("decimal(18,2)");

                entity.HasOne(cs => cs.ShippingAddress)
                      .WithMany()
                      .HasForeignKey(cs => cs.ShippingAddressId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cs => cs.PickupStoreLocation) // Giữ lại cấu hình này nếu bạn có thuộc tính điều hướng trong CarSale.cs
                      .WithMany()
                      .HasForeignKey(cs => cs.PickupStoreLocationId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cs => cs.DepositPayment)
                      .WithMany()
                      .HasForeignKey(cs => cs.DepositPaymentId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cs => cs.FullPayment)
                      .WithMany()
                      .HasForeignKey(cs => cs.FullPaymentId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);

                // CarSale liên kết với StoreListing
                entity.HasOne(s => s.StoreListing)
                      .WithMany(sl => sl.CarSales)
                      .HasForeignKey(s => s.StoreListingId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Customer)
                      .WithMany(u => u.CarSales)
                      .HasForeignKey(s => s.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.SaleStatus)
                      .WithMany(s => s.CarSales)
                      .HasForeignKey(s => s.SaleStatusId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Cấu hình cho Payment ---
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasOne(p => p.PaymentForSale)
                      .WithMany()
                      .HasForeignKey(p => p.PaymentForSaleId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.User)
                      .WithMany(u => u.Payments)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Listing)
                      .WithMany(l => l.Payments)
                      .HasForeignKey(p => p.ListingId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Cấu hình cho DeliveryAddress ---
            modelBuilder.Entity<DeliveryAddress>(entity =>
            {
                entity.Property(da => da.IsDefault).HasDefaultValue(false);
                entity.Property(da => da.Address).IsRequired();

                entity.HasOne(da => da.User)
                      .WithMany(u => u.DeliveryAddresses)
                      .HasForeignKey(da => da.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Cấu hình cho User ---
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Status).HasDefaultValue(false);
                entity.HasOne(u => u.StoreLocation)
                    .WithMany(sl => sl.Users)
                    .HasForeignKey(u => u.StoreLocationId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // --- Cấu hình CarListing và CarModel ---
            modelBuilder.Entity<CarListing>()
                .HasOne(cl => cl.Model)
                .WithMany(cm => cm.CarListings)
                .HasForeignKey(cl => cl.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Cấu hình CarManufacturer và CarModel ---
            modelBuilder.Entity<CarManufacturer>()
                .HasMany(m => m.CarModels)
                .WithOne(cm => cm.CarManufacturer)
                .HasForeignKey(cm => cm.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Cấu hình StoreListing (quan trọng) ---
            modelBuilder.Entity<StoreListing>(entity =>
            {
                entity.HasKey(sl => sl.StoreListingId);

                // Mối quan hệ một-nhiều từ StoreLocation đến StoreListing
                entity.HasOne(sl => sl.StoreLocation)
                      .WithMany(s => s.StoreListings)
                      .HasForeignKey(sl => sl.StoreLocationId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Mối quan hệ một-nhiều từ CarListing đến StoreListing
                entity.HasOne(sl => sl.CarListing)
                      .WithMany(cl => cl.StoreListings)
                      .HasForeignKey(sl => sl.ListingId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Ràng buộc duy nhất: Chỉ một StoreListing có IsCurrent = true cho mỗi ListingId
                // Điều này đảm bảo mỗi CarListing chỉ có thể ở 1 StoreLocation "hiện tại"
                entity.HasIndex(sl => sl.ListingId)
                      .HasFilter("[IsCurrent] = 1") // Chỉ áp dụng cho các bản ghi IsCurrent = true
                      .IsUnique();

                // Các cấu hình khác cho StoreListing (nếu có)
                entity.Property(sl => sl.InitialQuantity).IsRequired();
                entity.Property(sl => sl.CurrentQuantity).IsRequired();
                entity.Property(sl => sl.AvailableQuantity).IsRequired();
                entity.Property(sl => sl.Status).IsRequired().HasMaxLength(50);
                entity.Property(sl => sl.AddedDate).IsRequired();
                entity.Property(sl => sl.IsCurrent).IsRequired().HasDefaultValue(true);
            });

            // --- Cấu hình CarInventory ---
            modelBuilder.Entity<CarInventory>(entity =>
            {
                // CarInventory liên kết với StoreListing
                entity.HasOne(ci => ci.StoreListing)
                      .WithMany(sl => sl.Inventories) // Giả sử StoreListing có ICollection<CarInventory> Inventories
                      .HasForeignKey(ci => ci.StoreListingId)
                      .OnDelete(DeleteBehavior.Restrict); // Giữ hành vi xóa theo ý bạn

                entity.HasIndex(ci => new { ci.StoreListingId, ci.TransactionDate });
            });


            // --- Cấu hình CarVideo ---
            modelBuilder.Entity<CarVideo>(entity =>
            {
                entity.HasKey(e => e.VideoId);
                entity.Property(e => e.Url).IsRequired();
                entity.HasOne(e => e.CarListing)
                        .WithMany(cl => cl.CarVideos)
                        .HasForeignKey(e => e.ListingId)
                        .OnDelete(DeleteBehavior.Cascade);
            });

            // --- Cấu hình CarImage ---
            modelBuilder.Entity<CarImage>()
                .HasOne(ci => ci.CarListing)
                .WithMany(cl => cl.CarImages)
                .HasForeignKey(ci => ci.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Cấu hình Review ---
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
        }
    }
}