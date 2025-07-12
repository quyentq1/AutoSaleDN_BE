using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSaleDN.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using AutoSaleDN.DTO;
using System.Reflection;

namespace AutoSaleDN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AutoSaleDbContext _context;
        public AdminController(AutoSaleDbContext context)
        {
            _context = context;
        }
        [HttpGet("customers")]
        public async Task<ActionResult<IEnumerable<object>>> GetCustomers()
        {
            var customers = await _context.Users
                .Where(u => u.Role == "Customer")
                .Select(u => new
                {
                    u.UserId,
                    u.Name,
                    u.Email,
                    u.FullName,
                    u.Mobile,
                    u.Role,
                    u.CreatedAt,
                    u.UpdatedAt
                }).ToListAsync();
            return Ok(customers);
        }
        [HttpGet("customers/{id}")]
        public async Task<ActionResult<object>> GetCustomer(int id)
        {
            var user = await _context.Users
                .Where(u => u.UserId == id && u.Role == "Customer")
                .Select(u => new
                {
                    u.UserId,
                    u.Name,
                    u.Email,
                    u.FullName,
                    u.Mobile,
                    u.Role,
                    u.Province,
                    u.CreatedAt,
                    u.UpdatedAt
                }).FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            // Get sales transaction history for this customer (as buyer)
            var transactions = await (
                from sale in _context.CarSales
                join storelisting in _context.StoreListings on sale.StoreListingId equals storelisting.StoreListingId
                join listing in _context.CarListings on storelisting.ListingId equals listing.ListingId
                join model in _context.CarModels on listing.ModelId equals model.ModelId
                join manu in _context.CarManufacturers on model.ManufacturerId equals manu.ManufacturerId
                join status in _context.SaleStatus on sale.SaleStatusId equals status.SaleStatusId
                where listing.UserId == id // The customer is the buyer (listing.UserId) - adjust if your logic is different!
                select new
                {
                    sale.SaleId,
                    sale.SaleDate,
                    sale.FinalPrice,
                    SaleStatus = status.StatusName,
                    Car = new
                    {
                        listing.ListingId,
                        Manufacturer = manu.Name,
                        Model = model.Name,
                        listing.Year,
                        listing.Mileage,
                        listing.Price,
                        listing.Condition,
                        listing.RentSell
                    },
                    sale.CreatedAt,
                    sale.UpdatedAt
                }
            ).OrderByDescending(s => s.SaleDate)
             .ToListAsync();

            // Optionally, include bookings, payments, ... as needed.

            return Ok(new
            {
                user,
                salesHistory = transactions
            });
        }

        [HttpPost("customers")]
        public async Task<ActionResult> CreateCustomer([FromBody] User model)
        {
            if (await _context.Users.AnyAsync(x => x.Email == model.Email || x.Name == model.Name))
                return BadRequest("Email or Username already exists.");

            var customer = new User
            {
                Name = model.Name,
                Email = model.Email,
                FullName = model.FullName,
                Mobile = model.Mobile,
                Role = "Customer",
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(customer);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Customer created successfully" });
        }

        [HttpPut("customers/{id}")]
        public async Task<ActionResult> UpdateCustomer(int id, [FromBody] User model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Customer");
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Mobile = model.Mobile;
            user.UpdatedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(model.Password))
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Customer updated successfully" });
        }

        [HttpDelete("customers/{id}")]
        public async Task<ActionResult> DeleteCustomer(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Customer");
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Customer deleted successfully" });
        }

        [HttpGet("cars")]
        public async Task<IActionResult> GetCars()
        {
            var cars = await (
                from listing in _context.CarListings
                join model in _context.CarModels on listing.ModelId equals model.ModelId
                join manu in _context.CarManufacturers on model.ManufacturerId equals manu.ManufacturerId
                from spec in _context.CarSpecifications.Where(x => x.ListingId == listing.ListingId).DefaultIfEmpty()
                let available_units = _context.CarListings.Count(x => x.ModelId == listing.ModelId)
                select new
                {
                    listing.ListingId,
                    listing.ModelId,
                    Model = model.Name,
                    Manufacturer = manu.Name,
                    Color = spec.ExteriorColor ?? "Unknown",
                    listing.UserId,
                    listing.Year,
                    listing.Mileage,
                    listing.Price,
                    listing.Condition,
                    listing.RentSell,
                    listing.Vin,
                    Transmission = spec.Transmission ?? "Automatic",
                    SeatingCapacity = spec.SeatingCapacity ?? 5,
                    Certified = listing.Certified,
                    Images = _context.CarImages.Where(img => img.ListingId == listing.ListingId).Select(img => img.Url).ToList(),
                    Available_Units = available_units
                }
            ).ToListAsync();

            return Ok(cars);
        }

        [HttpGet("cars/{id}")]
        public async Task<ActionResult<object>> GetCar(int id)
        {
            var car = await _context.CarListings
                .Where(c => c.ListingId == id)
                .Select(c => new
                {
                    c.ListingId,
                    c.ModelId,
                    c.UserId,
                    c.Year,
                    c.Mileage,
                    c.Price,
                    c.Condition,
                    c.RentSell,
                    c.DatePosted,
                    c.DateUpdated
                }).FirstOrDefaultAsync();

            if (car == null)
                return NotFound();
            return Ok(car);
        }

        [HttpPost("cars/add")]
        public async Task<IActionResult> AddNewCar([FromBody] AddCarDto dto)
        {
            // 1. Tạo CarListing
            var car = new CarListing
            {
                ModelId = dto.ModelId,
                UserId = dto.UserId,
                Year = dto.Year,
                Mileage = dto.Mileage,
                Price = dto.Price,
                Condition = dto.Condition,
                RentSell = dto.RentSell,
                Description = dto.Description,
                Certified = dto.Certified,
                Vin = dto.Vin,
                DatePosted = DateTime.Now,
                DateUpdated = DateTime.Now,
            };
            _context.CarListings.Add(car);
            await _context.SaveChangesAsync();

            // 2. CarSpecification
            var spec = new CarSpecification
            {
                ListingId = car.ListingId,
                ExteriorColor = dto.Color,
                InteriorColor = dto.InteriorColor,
                Transmission = dto.Transmission,
                Engine = dto.Engine,
                FuelType = dto.FuelType,
                CarType = dto.CarType,
                SeatingCapacity = dto.SeatingCapacity
            };
            _context.CarSpecifications.Add(spec);

            // 3. CarPricingDetail
            var pricing = new CarPricingDetail
            {
                ListingId = car.ListingId,
                RegistrationFee = dto.RegistrationFee,
                TaxRate = dto.TaxRate
            };
            _context.CarPricingDetails.Add(pricing);


            // 5. CarImages
            foreach (var url in dto.ImageUrls)
            {
                _context.CarImages.Add(new CarImage
                {
                    ListingId = car.ListingId,
                    Url = url
                });
            }

            // 6. CarListingFeature
            foreach (var fid in dto.FeatureIds)
            {
                _context.CarListingFeatures.Add(new CarListingFeature
                {
                    ListingId = car.ListingId,
                    FeatureId = fid
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Car added successfully" });
        }

        [HttpPut("cars/{id}")]
        public async Task<ActionResult> UpdateCar(int id, [FromBody] CarListing model)
        {
            var car = await _context.CarListings.FirstOrDefaultAsync(c => c.ListingId == id);
            if (car == null) return NotFound();

            car.ModelId = model.ModelId;
            car.UserId = model.UserId;
            car.Year = model.Year;
            car.Mileage = model.Mileage;
            car.Price = model.Price;
            car.Condition = model.Condition;
            car.RentSell = model.RentSell;
            car.DateUpdated = DateTime.UtcNow;
            car.Certified = model.Certified;
            car.Vin = model.Vin;
            car.Description = model.Description;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Car updated successfully" });
        }

        [HttpDelete("cars/{id}")]
        public async Task<ActionResult> DeleteCar(int id)
        {
            var car = await _context.CarListings.FirstOrDefaultAsync(c => c.ListingId == id);
            if (car == null) return NotFound();

            _context.CarListings.Remove(car);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Car deleted successfully" });
        }

        [HttpGet("cars/add-form-data")]
        public async Task<IActionResult> GetAddCarFormData()
        {
            var models = await _context.CarModels.Include(x => x.Manufacturer).ToListAsync();
            var colors = await _context.CarColors.ToListAsync();
            var features = await _context.CarFeatures.ToListAsync();
            return Ok(new
            {
                models = models.Select(m => new
                {
                    m.ModelId,
                    m.Name,
                    ManufacturerName = m.Manufacturer.Name
                }),
                colors,
                features
            });
        }



        public class AddCarDto
        {
            public int ModelId { get; set; }
            public int UserId { get; set; }
            public int Year { get; set; }
            public int Mileage { get; set; }
            public decimal Price { get; set; }
            public string Location { get; set; }
            public string Condition { get; set; }
            public string RentSell { get; set; }
            public string Description { get; set; }
            public bool Certified { get; set; }
            public string Vin { get; set; }
            public string Color { get; set; }
            public string InteriorColor { get; set; }
            public string Transmission { get; set; }
            public string Engine { get; set; }
            public string FuelType { get; set; }
            public string CarType { get; set; }
            public int SeatingCapacity { get; set; }
            public decimal RegistrationFee { get; set; }
            public decimal TaxRate { get; set; }
            public int ColorId { get; set; }
            public int QuantityImported { get; set; }
            public DateTime ImportDate { get; set; }
            public decimal ImportPrice { get; set; }
            public string Notes { get; set; }
            public List<string> ImageUrls { get; set; }
            public List<int> FeatureIds { get; set; }
        }

        // 1. Quản lý nhân viên (Seller)
        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees()
        {
            var employees = await _context.Users
                .Where(u => u.Role == "Seller")
                .Select(u => new
                {
                    u.UserId,
                    u.Name,
                    u.Email,
                    u.FullName,
                    u.Mobile,
                    u.Role,
                    u.CreatedAt,
                    u.UpdatedAt
                }).ToListAsync();
            return Ok(employees);
        }

        [HttpGet("employees/{id}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            var employee = await _context.Users
                .Where(u => u.UserId == id && u.Role == "Seller")
                .Select(u => new
                {
                    u.UserId,
                    u.Name,
                    u.Email,
                    u.FullName,
                    u.Mobile,
                    u.Role,
                    u.CreatedAt,
                    u.UpdatedAt
                }).FirstOrDefaultAsync();
            if (employee == null) return NotFound();
            return Ok(employee);
        }

        [HttpPost("employees")]
        public async Task<IActionResult> CreateEmployee([FromBody] User model)
        {
            if (await _context.Users.AnyAsync(x => x.Email == model.Email || x.Name == model.Name))
                return BadRequest("Email or Username already exists.");

            var employee = new User
            {
                Name = model.Name,
                Email = model.Email,
                FullName = model.FullName,
                Mobile = model.Mobile,
                Role = "Seller",
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(employee);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Employee created successfully" });
        }

        [HttpPut("employees/{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] User model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Seller");
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Mobile = model.Mobile;
            user.UpdatedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(model.Password))
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Employee updated successfully" });
        }

        [HttpDelete("employees/{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Seller");
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Employee deleted successfully" });
        }

        // 2. Quản lý địa điểm cửa hàng
        [HttpGet("locations")]
        public async Task<IActionResult> GetStoreLocations()
        {
            var locations = await _context.StoreLocations.ToListAsync();
            return Ok(locations);
        }

        [HttpPost("locations")]
        public async Task<IActionResult> AddStoreLocation([FromBody] StoreLocation model)
        {
            _context.StoreLocations.Add(model);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Store location added successfully" });
        }

        [HttpPut("locations/{id}")]
        public async Task<IActionResult> UpdateStoreLocation(int id, [FromBody] StoreLocation model)
        {
            var location = await _context.StoreLocations.FindAsync(id);
            if (location == null) return NotFound();

            location.Name = model.Name;
            location.Address = model.Address;
            location.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Store location updated successfully" });
        }

        // 3. Quản lý khuyến mãi
        [HttpGet("promotions")]
        public async Task<IActionResult> GetPromotions()
        {
            var promotions = await _context.Promotions.ToListAsync();
            return Ok(promotions);
        }

        [HttpPost("promotions")]
        public async Task<IActionResult> AddPromotion([FromBody] Promotion model)
        {
            model.CreatedAt = DateTime.UtcNow;
            _context.Promotions.Add(model);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Promotion added successfully" });
        }

        [HttpPut("promotions/{id}")]
        public async Task<IActionResult> UpdatePromotion(int id, [FromBody] Promotion model)
        {
            var promo = await _context.Promotions.FindAsync(id);
            if (promo == null) return NotFound();

            promo.Title = model.Title;
            promo.Description = model.Description;
            promo.DiscountAmount = model.DiscountAmount;
            promo.StartDate = model.StartDate;
            promo.EndDate = model.EndDate;
            promo.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Promotion updated successfully" });
        }

        [HttpDelete("promotions/{id}")]
        public async Task<IActionResult> DeletePromotion(int id)
        {
            var promo = await _context.Promotions.FindAsync(id);
            if (promo == null) return NotFound();

            _context.Promotions.Remove(promo);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Promotion deleted successfully" });
        }

        // 4. Quản lý blog (BlogPost)
        [HttpGet("blog-posts")]
        public async Task<IActionResult> GetBlogPosts()
        {
            var posts = await _context.BlogPosts
                .Include(p => p.Category)
                .Select(p => new
                {
                    p.PostId,
                    p.Title,
                    p.Slug,
                    p.Content,
                    p.IsPublished,
                    p.PublishedDate,
                    p.CreatedAt,
                    p.UpdatedAt,
                    Category = new { p.CategoryId, p.Category.Name },
                    p.UserId
                }).ToListAsync();
            return Ok(posts);
        }

        [HttpPost("blog-posts")]
        public async Task<IActionResult> AddBlogPost([FromBody] BlogPost model)
        {
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;
            _context.BlogPosts.Add(model);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Blog post added successfully" });
        }

        [HttpPut("blog-posts/{id}")]
        public async Task<IActionResult> UpdateBlogPost(int id, [FromBody] BlogPost model)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return NotFound();

            post.Title = model.Title;
            post.Slug = model.Slug;
            post.Content = model.Content;
            post.CategoryId = model.CategoryId;
            post.IsPublished = model.IsPublished;
            post.PublishedDate = model.PublishedDate;
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Blog post updated successfully" });
        }

        [HttpDelete("blog-posts/{id}")]
        public async Task<IActionResult> DeleteBlogPost(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return NotFound();

            _context.BlogPosts.Remove(post);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Blog post deleted successfully" });
        }

        // 5. Báo cáo doanh thu theo ngày/tháng/năm
        [HttpGet("reports/revenue/daily")]
        public async Task<IActionResult> GetDailyRevenueReport(DateTime? date = null)
        {
            var targetDate = date?.Date ?? DateTime.UtcNow.Date;
            var sales = await _context.CarSales
                .Where(s => s.SaleDate.HasValue && s.SaleDate.Value.Date == targetDate)
                .SumAsync(s => (decimal?)s.FinalPrice) ?? 0;
            return Ok(new { date = targetDate, totalRevenue = sales });
        }

        [HttpGet("reports/revenue/monthly")]
        public async Task<IActionResult> GetMonthlyRevenueReport(int? year = null, int? month = null)
        {
            var y = year ?? DateTime.UtcNow.Year;
            var m = month ?? DateTime.UtcNow.Month;
            var sales = await _context.CarSales
                .Where(s => s.SaleDate.HasValue && s.SaleDate.Value.Year == y && s.SaleDate.Value.Month == m)
                .SumAsync(s => (decimal?)s.FinalPrice) ?? 0;
            return Ok(new { year = y, month = m, totalRevenue = sales });
        }

        [HttpGet("reports/revenue/yearly")]
        public async Task<IActionResult> GetYearlyRevenueReport(int? year = null)
        {
            var y = year ?? DateTime.UtcNow.Year;
            var sales = await _context.CarSales
                .Where(s => s.SaleDate.HasValue && s.SaleDate.Value.Year == y)
                .SumAsync(s => (decimal?)s.FinalPrice) ?? 0;
            return Ok(new { year = y, totalRevenue = sales });
        }


        [HttpGet("reports/top-selling-cars")]
        public async Task<ActionResult<IEnumerable<TopSellingCarDto>>> GetTopSellingCars()
        {
            try
            {
                // Lấy danh sách các xe đã bán
                var carData = await _context.CarListings
                    .Include(cl => cl.Model)
                    .ThenInclude(m => m.Manufacturer)
                    .Include(cl => cl.CarImages)
                    .Include(cl => cl.Reviews)
                    .Include(cl => cl.CarSales)
                    .Select(cl => new
                    {
                        cl.ModelId,
                        ModelName = cl.Model.Name,
                        ManufacturerName = cl.Model.Manufacturer.Name,
                        Image = cl.CarImages.FirstOrDefault(),
                        Reviews = cl.Reviews,
                        Sales = cl.CarSales
                    })
                    .ToListAsync();

                // Tính toán và group data
                var topCars = carData
                    .GroupBy(cl => new
                    {
                        cl.ModelId,
                        cl.ModelName,
                        cl.ManufacturerName
                    })
                    .Select(g => new TopSellingCarDto
                    {
                        ModelId = g.Key.ModelId,
                        ModelName = g.Key.ModelName,
                        ManufacturerName = g.Key.ManufacturerName,
                        ImageUrl = g.FirstOrDefault()?.Image?.Url,
                        TotalSold = g.Count(),
                        Revenue = g.Sum(cl => cl.Sales.Sum(cs => cs.FinalPrice)),
                        AverageRating = g.SelectMany(cl => cl.Reviews).Any()
                    ? (int)g.SelectMany(cl => cl.Reviews).Average(r => r.Rating)
                    : 0,
                        TotalReviews = g.SelectMany(cl => cl.Reviews).Count()
                    })
                    .OrderByDescending(c => c.Revenue)
                    .Take(10);

                return Ok(topCars);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("reports/cars-in-showroom")]
        public async Task<ActionResult<ShowroomInventoryDto>> GetCarsInShowroom()
        {
            try
            {
                var inventory = new ShowroomInventoryDto();

                var allShowrooms = await _context.StoreLocations.ToListAsync();

                // Lấy tất cả store listings với thông tin liên quan
                var storeListings = await _context.StoreListings
                    .Include(sl => sl.StoreLocation)
                    .Include(sl => sl.CarListing)
                        .ThenInclude(cl => cl.Model)
                            .ThenInclude(m => m.Manufacturer)
                    .Where(sl => sl.Status == "IN_STOCK" && sl.RemovedDate == null)
                    .Select(sl => new
                    {
                        StoreListing = sl,
                        CurrentQuantity = sl.CurrentQuantity,
                        AvailableQuantity = sl.AvailableQuantity,
                        CarListing = sl.CarListing,
                        Manufacturer = sl.CarListing.Model.Manufacturer
                    })
                    .ToListAsync();

                foreach (var showroom in allShowrooms)
                {
                    var listings = storeListings
                        .Where(sl => sl.StoreListing.StoreLocationId == showroom.StoreLocationId)
                        .ToList();

                    int totalCars = listings.Sum(sl => sl.CurrentQuantity);
                    int availableCars = listings.Sum(sl => sl.AvailableQuantity);

                    var brands = listings
                        .GroupBy(sl => sl.Manufacturer.Name)
                        .Select(b => new CarBrandStatsDto
                        {
                            BrandName = b.Key,
                            TotalCars = b.Sum(sl => sl.CurrentQuantity),
                            AvailableCars = b.Sum(sl => sl.AvailableQuantity),
                            AverageCost = b.Average(sl => sl.StoreListing.AverageCost ?? 0),
                            LastPurchasePrice = b.Max(sl => sl.StoreListing.LastPurchasePrice ?? 0)
                        })
                        .ToList();

                    var models = listings
                        .Select(sl => new CarModelStatsDto
                        {
                            ModelName = sl.CarListing.Model.Name,
                            ManufacturerName = sl.Manufacturer.Name,
                            CurrentQuantity = sl.CurrentQuantity,
                            AvailableQuantity = sl.AvailableQuantity,
                            AverageCost = sl.StoreListing.AverageCost ?? 0,
                            LastPurchasePrice = sl.StoreListing.LastPurchasePrice ?? 0,
                            LastImportDate = sl.StoreListing.Inventories
                                .Where(i => i.TransactionType == 1) // Nhập hàng
                                .OrderByDescending(i => i.TransactionDate)
                                .Select(i => i.TransactionDate)
                                .FirstOrDefault()
                        })
                        .GroupBy(m => new { m.ModelName, m.ManufacturerName })
                        .Select(g => new CarModelStatsDto
                        {
                            ModelName = g.Key.ModelName,
                            ManufacturerName = g.Key.ManufacturerName,
                            CurrentQuantity = g.Sum(m => m.CurrentQuantity),
                            AvailableQuantity = g.Sum(m => m.AvailableQuantity),
                            AverageCost = g.Average(m => m.AverageCost),
                            LastPurchasePrice = g.Max(m => m.LastPurchasePrice),
                            LastImportDate = g.Max(m => m.LastImportDate)
                        })
                        .ToList();

                    inventory.Showrooms[showroom.Name] = new ShowroomDetailsDto
                    {
                        TotalCars = totalCars,
                        AvailableCars = availableCars,
                        Brands = brands,
                        Models = models
                    };
                }

                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    } 

}