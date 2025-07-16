using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSaleDN.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using AutoSaleDN.DTO;
using System.Reflection;
using OfficeOpenXml;

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
                    u.UpdatedAt,
                    u.Province,
                    u.Status
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
                    u.UpdatedAt,
                    u.Status
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
                where sale.CustomerId == id
                select new
                {
                    sale.SaleId,
                    sale.SaleDate,
                    sale.FinalPrice,
                    SaleStatus = status.StatusName,
                    Car = new
                    {
                        listing.ListingId,
                        listing.ModelId,
                        Model = model.Name,
                        Manufacturer = manu.Name,
                        listing.Year,
                        listing.Mileage,
                        listing.Price,
                        listing.Condition,
                        listing.RentSell,
                        listing.Vin,
                        Transmission = listing.Specifications != null && listing.Specifications.Any()
                            ? listing.Specifications.First().Transmission
                            : "Automatic",
                        SeatingCapacity = listing.Specifications != null && listing.Specifications.Any()
                            ? listing.Specifications.First().SeatingCapacity
                            : 5,
                        Certified = listing.Certified,
                        Images = _context.CarImages
                            .Where(img => img.ListingId == listing.ListingId)
                            .Select(img => img.Url)
                            .ToList(),
                        Available_Units = _context.CarListings
                            .Count(x => x.ModelId == listing.ModelId)
                    },
                    // Thông tin người bán
                    Seller = new
                    {
                        SellerId = listing.UserId,
                        SellerName = _context.Users
                            .Where(u => u.UserId == listing.UserId)
                            .Select(u => u.FullName ?? u.Name)
                            .FirstOrDefault()
                    },
                    sale.CreatedAt,
                    sale.UpdatedAt
                }
            ).OrderByDescending(s => s.SaleDate)
             .ToListAsync();

            return Ok(new
            {
                user,
                salesHistory = transactions
            });
        }

        [HttpPost("customers")]
        public async Task<ActionResult> CreateCustomer([FromBody] CustomerDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.NewPassword) || string.IsNullOrWhiteSpace(model.FullName))
            {
                return BadRequest("Email, Full Name, and Password are required for new customer creation.");
            }

            if (await _context.Users.AnyAsync(x => x.Email == model.Email || x.Name == model.Email))
            {
                return BadRequest("Email or Username already exists.");
            }

            var customer = new User
            {
                Name = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Mobile = model.Mobile,
                Province = model.Province,
                Role = "Customer",
                Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(customer);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Customer created successfully" });
        }

        [HttpPut("customers/{id}")]
        public async Task<ActionResult> UpdateCustomer(int id, [FromBody] CustomerDto model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Customer");

            if (user == null)
            {
                return NotFound($"Customer with ID {id} not found.");
            }

            if (!string.IsNullOrEmpty(model.Email) && model.Email != user.Email)
            {
                if (await _context.Users.AnyAsync(x => x.Email == model.Email))
                {
                    return BadRequest("Email already exists for another user.");
                }
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Mobile = model.Mobile;
            user.Province = model.Province;

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            }

            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Customer updated successfully" });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("A concurrency error occurred. The customer might have been updated or deleted by another user.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("toggle-status/{id}")]
        public async Task<ActionResult> ToggleCustomerStatus(int id, [FromBody] CustomerStatusUpdateDto model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Customer");

            if (user == null)
            {
                return NotFound($"Customer with ID {id} not found.");
            }

            // Ensure the provided status value is valid (true/false)
            if (model == null || !model.Status.HasValue)
            {
                return BadRequest("New status value is required.");
            }

            user.Status = model.Status.Value;
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                string action = user.Status ? "activated" : "deactivated";
                return Ok(new { message = $"Customer account {action} successfully" });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("A concurrency error occurred. The customer might have been updated or deleted by another user.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Seller
        [HttpGet("sellers")]
        public async Task<ActionResult<IEnumerable<object>>> GetSellers()
        {
            var customers = await _context.Users
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
                    u.UpdatedAt,
                    u.Province,
                    u.Status,
                    u.StoreLocationId
                }).ToListAsync();
            return Ok(customers);
        }

        [HttpPost("sellers")]
        public async Task<ActionResult> CreateSeller([FromBody] SellerDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password) || string.IsNullOrWhiteSpace(model.FullName))
            {
                return BadRequest("Email, Full Name, and Password are required for new Seller creation.");
            }

            if (await _context.Users.AnyAsync(x => x.Email == model.Email || x.Name == model.Email))
            {
                return BadRequest("Email or Username already exists.");
            }

            var customer = new User
            {
                Name = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Mobile = model.Mobile,
                Province = model.Province,
                Role = "Seller",
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                StoreLocationId = model.storeLocationId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(customer);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Customer created successfully" });
        }

        [HttpPut("sellers/{id}")]
        public async Task<ActionResult> UpdateSellers(int id, [FromBody] SellerDto model)
        {
            var seller = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Seller");
            //var showroom = await _context.S.FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Seller");

            if (seller == null)
            {
                return NotFound($"Seller with ID {id} not found.");
            }

            if (!string.IsNullOrEmpty(model.Email) && model.Email != seller.Email)
            {
                if (await _context.Users.AnyAsync(x => x.Email == model.Email))
                {
                    return BadRequest("Email already exists for another user.");
                }
            }

            seller.FullName = model.FullName;
            seller.Email = model.Email;
            seller.Mobile = model.Mobile;
            seller.Province = model.Province;
            seller.UpdatedAt = DateTime.UtcNow;
            seller.StoreLocationId = model.storeLocationId;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Seller updated successfully" });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("A concurrency error occurred. The Seller might have been updated or deleted by another user.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("sellers/toggle-status/{id}")]
        public async Task<ActionResult> ToggleSellerStatus(int id, [FromBody] CustomerStatusUpdateDto model)
        {
            var seller = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Seller");

            if (seller == null)
            {
                return NotFound($"Customer with ID {id} not found.");
            }

            // Ensure the provided status value is valid (true/false)
            if (model == null || !model.Status.HasValue)
            {
                return BadRequest("New status value is required.");
            }

            seller.Status = model.Status.Value;
            seller.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                string action = seller.Status ? "activated" : "deactivated";
                return Ok(new { message = $"Seller account {action} successfully" });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("A concurrency error occurred. The seller might have been updated or deleted by another user.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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
                    Color = spec != null ? spec.ExteriorColor : "Unknown",
                    listing.UserId,
                    listing.Year,
                    listing.Mileage,
                    listing.Price,
                    listing.Condition,
                    listing.RentSell,
                    listing.Vin,
                    Transmission = spec != null ? spec.Transmission : "Automatic",
                    SeatingCapacity = spec != null ? spec.SeatingCapacity : 5,
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
                var topCars = await _context.CarListings
                    .Include(cl => cl.Model)
                    .ThenInclude(m => m.Manufacturer)
                    .Include(cl => cl.CarImages)
                    .GroupBy(cl => new
                    {
                        cl.ModelId,
                        ModelName = cl.Model.Name,
                        ManufacturerName = cl.Model.Manufacturer.Name
                    })
                    .Select(g => new TopSellingCarDto
                    {
                        ModelId = g.Key.ModelId,
                        ModelName = g.Key.ModelName,
                        ManufacturerName = g.Key.ManufacturerName,
                        ImageUrl = g.FirstOrDefault().CarImages.FirstOrDefault().Url,
                        TotalSold = _context.CarSales
                            .Where(cs => _context.StoreListings
                                .Where(sl => sl.ListingId == g.FirstOrDefault().ListingId)
                                .Select(sl => sl.StoreListingId)
                                .Contains(cs.StoreListingId))
                            .Count(),
                        Revenue = _context.CarSales
                            .Where(cs => _context.StoreListings
                                .Where(sl => sl.ListingId == g.FirstOrDefault().ListingId)
                                .Select(sl => sl.StoreListingId)
                                .Contains(cs.StoreListingId))
                            .Sum(cs => cs.FinalPrice),
                        AverageRating = _context.Reviews
                            .Where(r => g.Any(cl => cl.ListingId == r.ListingId))
                            .Any() ? (int)_context.Reviews
                            .Where(r => g.Any(cl => cl.ListingId == r.ListingId))
                            .Average(r => r.Rating) : 0,
                        TotalReviews = _context.Reviews
                            .Where(r => g.Any(cl => cl.ListingId == r.ListingId))
                            .Count()
                    })
                    .OrderByDescending(c => c.Revenue)
                    .Take(10)
                    .ToListAsync();

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

                inventory.Showrooms = new Dictionary<string, ShowroomDetailsDto>();

                var allShowrooms = await _context.StoreLocations.ToListAsync();

                var storeListings = await _context.StoreListings
                    .Include(sl => sl.StoreLocation)
                    .Include(sl => sl.CarListing)
                        .ThenInclude(cl => cl.Model)
                            .ThenInclude(m => m.Manufacturer)
                    .Include(sl => sl.Inventories)
                    .Where(sl => sl.Status == "IN_STOCK" && sl.RemovedDate == null)
                    .Select(sl => new
                    {
                        StoreListing = sl,
                        CurrentQuantity = sl.CurrentQuantity,
                        AvailableQuantity = sl.AvailableQuantity,
                        CarListing = sl.CarListing,
                        Manufacturer = sl.CarListing.Model.Manufacturer,
                        Model = sl.CarListing.Model,
                        Inventories = sl.Inventories
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
                            AverageCost = b.Where(sl => sl.StoreListing.AverageCost.HasValue)
                                          .Select(sl => sl.StoreListing.AverageCost.Value)
                                          .DefaultIfEmpty(0)
                                          .Average(),
                            LastPurchasePrice = b.Where(sl => sl.StoreListing.LastPurchasePrice.HasValue)
                                               .Select(sl => sl.StoreListing.LastPurchasePrice.Value)
                                               .DefaultIfEmpty(0)
                                               .Max()
                        })
                        .ToList();

                    var models = listings
                        .Select(sl => new CarModelStatsDto
                        {
                            ModelName = sl.Model.Name,
                            ManufacturerName = sl.Manufacturer.Name,
                            CurrentQuantity = sl.CurrentQuantity,
                            AvailableQuantity = sl.AvailableQuantity,
                            AverageCost = sl.StoreListing.AverageCost ?? 0,
                            LastPurchasePrice = sl.StoreListing.LastPurchasePrice ?? 0,
                            LastImportDate = sl.Inventories
                                .Where(i => i.TransactionType == 1)
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
                            AverageCost = g.Where(m => m.AverageCost > 0)
                                          .Select(m => m.AverageCost)
                                          .DefaultIfEmpty(0)
                                          .Average(),
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

        [HttpGet("showrooms")]
        public async Task<IActionResult> GetShowrooms()
        {
            try
            {
                var showrooms = await _context.StoreLocations
                    .Select(sl => new
                    {
                        Id = sl.StoreLocationId,
                        Name = sl.Name,
                        Location = sl.Address,
                        TotalCars = sl.StoreListings.Sum(sl => sl.CurrentQuantity),
                        SoldThisMonth = _context.CarSales
                            .Where(s => s.StoreListing.StoreLocationId == sl.StoreLocationId
                                 && s.SaleDate >= DateTime.Now.AddMonths(-1))
                            .Count(),
                        Revenue = _context.CarSales
                            .Where(s => s.StoreListing.StoreLocationId == sl.StoreLocationId
                                 && s.SaleDate >= DateTime.Now.AddMonths(-1))
                            .Sum(s => s.FinalPrice),
                        Sellers = _context.Users
                            .Where(u => u.StoreLocationId == sl.StoreLocationId)
                            .Select(u => new SellersDto
                            {
                                SellerId = u.UserId,
                                FullName = u.FullName,
                                Email = u.Email,
                                PhoneNumber = u.Mobile
                            })
                            .ToList()
                    })
                    .ToListAsync();

                var result = showrooms.Select(s => new ShowroomDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Location = s.Location,
                    TotalCars = s.TotalCars,
                    SoldThisMonth = s.SoldThisMonth,
                    Revenue = s.Revenue,
                    MainSeller = s.Sellers.FirstOrDefault(),
                    AllSellers = s.Sellers,
                    RevenueGrowth = GetRevenueGrowth(s.Id),
                    Brands = GetBrandPerformance(s.Id),
                    SalesData = GetMonthlySalesData(s.Id),
                    Inventory = GetRecentInventory(s.Id),
                    PopularModels = GetPopularModels(s.Id)
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("showrooms/{id}")]
        public async Task<IActionResult> GetShowroomDetail(int id)
        {
            try
            {
                var showroom = await _context.StoreLocations
                    .Where(sl => sl.StoreLocationId == id)
                    .Select(sl => new ShowroomDto
                    {
                        Id = sl.StoreLocationId,
                        Name = sl.Name,
                        Location = sl.Address,
                        TotalCars = sl.StoreListings.Sum(sl => sl.CurrentQuantity),
                        SoldThisMonth = _context.CarSales
                            .Where(s => s.StoreListing.StoreLocationId == id
                                && s.SaleDate >= DateTime.Now.AddMonths(-1))
                            .Count(),
                        Revenue = _context.CarSales
                            .Where(s => s.StoreListing.StoreLocationId == id
                                && s.SaleDate >= DateTime.Now.AddMonths(-1))
                            .Sum(s => s.FinalPrice),
                        RevenueGrowth = GetRevenueGrowth(id),
                        Brands = GetBrandPerformance(id),
                        SalesData = GetMonthlySalesData(id),
                        Inventory = GetRecentInventory(id),
                        PopularModels = GetPopularModels(id),
                        MainSeller = _context.Users
                            .Where(u => u.StoreLocationId == id)
                            .Select(u => new SellersDto
                            {
                                SellerId = u.UserId,
                                FullName = u.FullName,
                                Email = u.Email,
                                PhoneNumber = u.Mobile
                            })
                            .FirstOrDefault(),
                        AllSellers = _context.Users
                            .Where(u => u.StoreLocationId == id)
                            .Select(u => new SellersDto
                            {
                                SellerId = u.UserId,
                                FullName = u.FullName,
                                Email = u.Email,
                                PhoneNumber = u.Mobile
                            })
                            .ToList()
                    })
                    .FirstOrDefaultAsync();

                if (showroom == null)
                    return NotFound();

                return Ok(showroom);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("showrooms/{id}/sales")]
        public async Task<IActionResult> GetShowroomSales(int id)
        {
            try
            {
                // Solution 1: Using conditional logic instead of null-conditional operator
                var sales = await _context.CarSales
                    .Where(s => s.StoreListing.StoreLocationId == id)
                    .OrderByDescending(s => s.SaleDate)
                    .Select(s => new
                    {
                        s.SaleId,
                        s.SaleDate,
                        s.FinalPrice,
                        s.SaleStatus.StatusName,
                        Car = new
                        {
                            s.StoreListing.CarListing.ListingId,
                            s.StoreListing.CarListing.ModelId,
                            Model = s.StoreListing.CarListing.Model.Name,
                            Manufacturer = s.StoreListing.CarListing.Model.Manufacturer.Name,
                            s.StoreListing.CarListing.Year,
                            s.StoreListing.CarListing.Mileage,
                            s.StoreListing.CarListing.Price,
                            s.StoreListing.CarListing.Condition,
                            s.StoreListing.CarListing.RentSell,
                            s.StoreListing.CarListing.Vin,
                            Transmission = s.StoreListing.CarListing.Specifications.Any()
                                ? s.StoreListing.CarListing.Specifications.FirstOrDefault().Transmission
                                : "Automatic",
                            SeatingCapacity = s.StoreListing.CarListing.Specifications.Any()
                                ? s.StoreListing.CarListing.Specifications.FirstOrDefault().SeatingCapacity
                                : 5,
                            Certified = s.StoreListing.CarListing.Certified,
                            Images = _context.CarImages.Where(img => img.ListingId == s.StoreListing.CarListing.ListingId).Select(img => img.Url).ToList(),
                            Available_Units = _context.CarListings.Count(x => x.ModelId == s.StoreListing.CarListing.ModelId)
                        },
                        s.CreatedAt,
                        s.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(sales);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("showrooms/{id}/inventory")]
        public async Task<IActionResult> GetShowroomInventory(int id)
        {
            try
            {
                var inventory = await _context.CarInventories
                    .Where(i => i.StoreListing.StoreLocationId == id)
                    .OrderByDescending(i => i.CreatedAt)
                    .Select(i => new
                    {
                        i.InventoryId,
                        i.TransactionType,
                        i.Quantity,
                        i.UnitPrice,
                        i.ReferenceId,
                        i.Notes,
                        i.CreatedBy,
                        i.TransactionDate,
                        i.CreatedAt,
                        Car = new
                        {
                            i.StoreListing.CarListing.ListingId,
                            ModelName = i.StoreListing.CarListing.Model.Name,
                            ManufacturerName = i.StoreListing.CarListing.Model.Manufacturer.Name,
                            i.StoreListing.CarListing.Year,
                            i.StoreListing.CarListing.Mileage,
                            i.StoreListing.CarListing.Price
                        }
                    })
                    .ToListAsync();
                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("showrooms/{id}/brands")]
        public async Task<IActionResult> GetShowroomBrands(int id)
        {
            try
            {
                var brands = await _context.StoreListings
                    .Where(sl => sl.StoreLocationId == id)
                    .GroupBy(sl => sl.CarListing.Model.Manufacturer.Name)
                    .Select(g => new
                    {
                        Name = g.Key,
                        Count = g.Sum(sl => sl.CurrentQuantity),
                        Revenue = _context.CarSales
                            .Where(s => s.StoreListing.StoreLocationId == id 
                                && s.StoreListing.CarListing.Model.Manufacturer.Name == g.Key)
                            .Sum(s => s.FinalPrice)
                    })
                    .OrderByDescending(b => b.Count)
                    .ToListAsync();

                return Ok(brands);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("showrooms/export")]
        public async Task<IActionResult> ExportReport([FromBody] ExportReportDto model)
        {
            try
            {
                var showroom = await _context.StoreLocations.FindAsync(model.ShowroomId);
                if (showroom == null)
                    return NotFound();

                // Generate report data
                var reportData = new
                {
                    Showroom = new
                    {
                        showroom.Name,
                        showroom.Address,
                        DateGenerated = DateTime.Now
                    },
                    Summary = await GetShowroomSummary(model.ShowroomId),
                    Sales = GetMonthlySalesData(model.ShowroomId),        // Remove await
                    Inventory = GetRecentInventory(model.ShowroomId),      // Remove await
                    Brands = GetBrandPerformance(model.ShowroomId)         // Remove await
                };

                // Generate Excel file
                var memoryStream = new MemoryStream();
                using (var package = new ExcelPackage(memoryStream))
                {
                    // Add worksheet and populate data
                    var worksheet = package.Workbook.Worksheets.Add("Report");

                    // Example of how to populate the worksheet with data
                    // You can expand this based on your actual data structure

                    // Add headers
                    worksheet.Cells[1, 1].Value = "Showroom Report";
                    worksheet.Cells[2, 1].Value = "Name:";
                    worksheet.Cells[2, 2].Value = reportData.Showroom.Name;
                    worksheet.Cells[3, 1].Value = "Address:";
                    worksheet.Cells[3, 2].Value = reportData.Showroom.Address;
                    worksheet.Cells[4, 1].Value = "Date Generated:";
                    worksheet.Cells[4, 2].Value = reportData.Showroom.DateGenerated;

                    // Add summary section
                    int row = 6;
                    worksheet.Cells[row, 1].Value = "Summary";
                    // Add summary data here based on your Summary object structure

                    // Add sales data
                    row += 3;
                    worksheet.Cells[row, 1].Value = "Sales Data";
                    // Add sales data here based on your Sales list structure

                    // Add inventory data
                    row += 3;
                    worksheet.Cells[row, 1].Value = "Inventory Data";
                    // Add inventory data here based on your Inventory list structure

                    // Add brands data
                    row += 3;
                    worksheet.Cells[row, 1].Value = "Brand Performance";
                    // Add brands data here based on your Brands list structure

                    package.Save();
                }

                memoryStream.Position = 0;
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"showroom_report_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private decimal GetRevenueGrowth(int showroomId)
        {
            var currentMonthRevenue = _context.CarSales
                .Where(s => s.StoreListing.StoreLocationId == showroomId
                     && s.SaleDate >= DateTime.Now.AddMonths(-1))
                .Sum(s => s.FinalPrice);

            var previousMonthRevenue = _context.CarSales
                .Where(s => s.StoreListing.StoreLocationId == showroomId
                     && s.SaleDate >= DateTime.Now.AddMonths(-2)
                    && s.SaleDate < DateTime.Now.AddMonths(-1))
                .Sum(s => s.FinalPrice);

            if (previousMonthRevenue == 0)
                return 0;

            return ((currentMonthRevenue - previousMonthRevenue) / previousMonthRevenue) * 100;
        }

        private List<BrandPerformanceDto> GetBrandPerformance(int showroomId)
        {
            return _context.StoreListings
                .Where(sl => sl.StoreLocationId == showroomId)
                .GroupBy(sl => sl.CarListing.Model.Manufacturer.Name)
                .Select(g => new BrandPerformanceDto
                {
                    Name = g.Key,
                    Count = g.Sum(sl => sl.CurrentQuantity),
                    Revenue = _context.CarSales
                        .Where(s => s.StoreListing.StoreLocationId == showroomId 
                            && s.StoreListing.CarListing.Model.Manufacturer.Name == g.Key)
                        .Sum(s => s.FinalPrice)
                })
                .OrderByDescending(b => b.Count)
                .ToList();
        }

        private List<SalesDataDto> GetMonthlySalesData(int showroomId)
        {
            var startDate = DateTime.Now.AddMonths(-1);
            var endDate = DateTime.Now;
            var salesData = new List<SalesDataDto>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dailySales = _context.CarSales
                    .Where(s => s.StoreListing.StoreLocationId == showroomId
                        && s.SaleDate.HasValue
                        && s.SaleDate.Value.Date == date.Date)
                    .Count();

                salesData.Add(new SalesDataDto
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Sold = dailySales
                });
            }

            return salesData;
        }

        private List<InventoryItemDto> GetRecentInventory(int showroomId)
        {
            return _context.CarInventories
                .Where(i => i.StoreListing.StoreLocationId == showroomId)
                .OrderByDescending(i => i.CreatedAt)
                .Take(10)
                .Select(i => new InventoryItemDto
                {
                    Model = i.StoreListing.CarListing.Model.Name,
                    Date = i.CreatedAt.ToString("yyyy-MM-dd"),
                    Quantity = i.Quantity,
                    Type = i.TransactionType == 1 ? "Nhập hàng" : 
                           i.TransactionType == 2 ? "Xuất hàng" : "Điều chỉnh"
                })
                .ToList();
        }

        private List<ModelPerformanceDto> GetPopularModels(int showroomId)
        {
            return _context.StoreListings
                .Where(sl => sl.StoreLocationId == showroomId)
                .GroupBy(sl => new
                {
                    ModelName = sl.CarListing.Model.Name,
                    ManufacturerName = sl.CarListing.Model.Manufacturer.Name
                })
                .Select(g => new ModelPerformanceDto
                {
                    Name = g.Key.ModelName,
                    Brand = g.Key.ManufacturerName,
                    ImageUrl = null, // Or set to a default image URL
                    Count = g.Sum(sl => sl.CurrentQuantity),
                    Sold = _context.CarSales
                        .Where(s => s.StoreListing.StoreLocationId == showroomId
                            && s.StoreListing.CarListing.Model.Name == g.Key.ModelName)
                        .Count()
                })
                .OrderByDescending(m => m.Count)
                .Take(5)
                .ToList();
        }

        private async Task<ShowroomSummaryDto> GetShowroomSummary(int showroomId)
        {
            var showroom = await _context.StoreLocations.FindAsync(showroomId);
            if (showroom == null) return null;

            return new ShowroomSummaryDto
            {
                Name = showroom.Name,
                Location = showroom.Address,
                TotalCars = _context.StoreListings
                    .Where(sl => sl.StoreLocationId == showroomId)
                    .Sum(sl => sl.CurrentQuantity),
                Revenue = _context.CarSales
                    .Where(s => s.StoreListing.StoreLocationId == showroomId 
                        && s.SaleDate >= DateTime.Now.AddMonths(-1))
                    .Sum(s => s.FinalPrice),
                RevenueGrowth = GetRevenueGrowth(showroomId),
                SoldThisMonth = _context.CarSales
                    .Where(s => s.StoreListing.StoreLocationId == showroomId 
                        && s.SaleDate >= DateTime.Now.AddMonths(-1))
                    .Count()
            };
        }

        public class ExportReportDto
        {
            public int ShowroomId { get; set; }
            public string DateRange { get; set; } // "thisMonth", "lastMonth", "thisYear"
        }

        public class ShowroomSummaryDto
        {
            public string Name { get; set; }
            public string Location { get; set; }
            public int TotalCars { get; set; }
            public decimal Revenue { get; set; }
            public decimal RevenueGrowth { get; set; }
            public int SoldThisMonth { get; set; }
        }

        [HttpGet("transactions/{id}")]
        public async Task<ActionResult<object>> GetTransactionDetail(int id)
        {
            try
            {
                var transaction = await (
                    from sale in _context.CarSales
                    join storelisting in _context.StoreListings on sale.StoreListingId equals storelisting.StoreListingId
                    join listing in _context.CarListings on storelisting.ListingId equals listing.ListingId
                    join model in _context.CarModels on listing.ModelId equals model.ModelId
                    join manu in _context.CarManufacturers on model.ManufacturerId equals manu.ManufacturerId
                    join status in _context.SaleStatus on sale.SaleStatusId equals status.SaleStatusId
                    join customer in _context.Users on sale.CustomerId equals customer.UserId into customerGroup
                    from customer in customerGroup.DefaultIfEmpty()
                    join seller in _context.Users on listing.UserId equals seller.UserId into sellerGroup
                    from seller in sellerGroup.DefaultIfEmpty()
                    where sale.SaleId == id
                    select new
                    {
                        SaleId = sale.SaleId,
                        SaleDate = sale.SaleDate,
                        FinalPrice = sale.FinalPrice,
                        SaleStatus = status.StatusName,
                        CreatedAt = sale.CreatedAt,
                        UpdatedAt = sale.UpdatedAt,

                        // Customer Information
                        Customer = customer != null ? new
                        {
                            UserId = customer.UserId,
                            Name = customer.Name,
                            FullName = customer.FullName,
                            Email = customer.Email,
                            Mobile = customer.Mobile,
                            Role = customer.Role,
                            Province = customer.Province,
                            Status = customer.Status
                        } : null,

                        // Seller Information
                        Seller = seller != null ? new
                        {
                            UserId = seller.UserId,
                            Name = seller.Name,
                            FullName = seller.FullName,
                            Email = seller.Email,
                            Mobile = seller.Mobile,
                            Role = seller.Role,
                            Province = seller.Province
                        } : null,

                        // Car Information
                        Car = new
                        {
                            ListingId = listing.ListingId,
                            ModelId = listing.ModelId,
                            Model = model.Name,
                            Manufacturer = manu.Name,
                            Year = listing.Year,
                            Mileage = listing.Mileage,
                            Price = listing.Price,
                            Location = storelisting.StoreLocation,
                            Condition = listing.Condition,
                            RentSell = listing.RentSell,
                            Vin = listing.Vin,
                            Description = listing.Description,
                            Certified = listing.Certified,

                            // Specifications
                            Transmission = listing.Specifications != null && listing.Specifications.Any()
                                ? listing.Specifications.First().Transmission
                                : "Automatic",
                            SeatingCapacity = listing.Specifications != null && listing.Specifications.Any()
                                ? listing.Specifications.First().SeatingCapacity
                                : 5,
                            FuelType = listing.Specifications != null && listing.Specifications.Any()
                                ? listing.Specifications.First().FuelType
                                : "Gasoline",

                            // Images
                            Images = _context.CarImages
                                .Where(img => img.ListingId == listing.ListingId)
                                .Select(img => img.Url)
                                .ToList()
                        }
                    }
                ).FirstOrDefaultAsync();

                if (transaction == null)
                {
                    return NotFound(new { message = "Transaction not found" });
                }

                return Ok(transaction);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("transactions")]
        public async Task<ActionResult<object>> GetTransactions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var query = from sale in _context.CarSales
                            join storelisting in _context.StoreListings on sale.StoreListingId equals storelisting.StoreListingId
                            join listing in _context.CarListings on storelisting.ListingId equals listing.ListingId
                            join model in _context.CarModels on listing.ModelId equals model.ModelId
                            join manu in _context.CarManufacturers on model.ManufacturerId equals manu.ManufacturerId
                            join statusEntity in _context.SaleStatus on sale.SaleStatusId equals statusEntity.SaleStatusId
                            join customer in _context.Users on sale.CustomerId equals customer.UserId into customerGroup
                            from customer in customerGroup.DefaultIfEmpty()
                            select new
                            {
                                SaleId = sale.SaleId,
                                SaleDate = sale.SaleDate,
                                FinalPrice = sale.FinalPrice,
                                SaleStatus = statusEntity.StatusName,
                                CreatedAt = sale.CreatedAt,
                                UpdatedAt = sale.UpdatedAt,

                                CustomerName = customer != null ? customer.FullName ?? customer.Name : "N/A",
                                CustomerEmail = customer != null ? customer.Email : "N/A",

                                CarInfo = $"{manu.Name} {model.Name} ({listing.Year})",
                                CarPrice = listing.Price,
                                CarVin = listing.Vin
                            };

                // Apply filters
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(t => t.SaleStatus.ToLower().Contains(status.ToLower()));
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(t => t.SaleDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(t => t.SaleDate <= toDate.Value);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(t =>
                        t.CustomerName.ToLower().Contains(search.ToLower()) ||
                        t.CustomerEmail.ToLower().Contains(search.ToLower()) ||
                        t.CarInfo.ToLower().Contains(search.ToLower()) ||
                        t.CarVin.ToLower().Contains(search.ToLower())
                    );
                }

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var transactions = await query
                    .OrderByDescending(t => t.SaleDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new
                {
                    data = transactions,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = totalPages,
                        hasNextPage = page < totalPages,
                        hasPreviousPage = page > 1
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPut("transactions/{id}/status")]
        public async Task<ActionResult> UpdateTransactionStatus(int id, [FromBody] UpdateTransactionStatusRequest request)
        {
            try
            {
                var transaction = await _context.CarSales.FindAsync(id);
                if (transaction == null)
                {
                    return NotFound(new { message = "Transaction not found" });
                }

                // Validate status exists
                var statusExists = await _context.SaleStatus
                    .AnyAsync(s => s.SaleStatusId == request.StatusId);

                if (!statusExists)
                {
                    return BadRequest(new { message = "Invalid status ID" });
                }

                transaction.SaleStatusId = request.StatusId;
                transaction.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Transaction status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpDelete("transactions/{id}")]
        public async Task<ActionResult> DeleteTransaction(int id)
        {
            try
            {
                var transaction = await _context.CarSales.FindAsync(id);
                if (transaction == null)
                {
                    return NotFound(new { message = "Transaction not found" });
                }

                _context.CarSales.Remove(transaction);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Transaction deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // Request model for updating transaction status
        public class UpdateTransactionStatusRequest
        {
            public int StatusId { get; set; }
        }
    } 

}