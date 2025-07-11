using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSaleDN.Models;
using Microsoft.AspNetCore.Authorization;

namespace AutoSaleDN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class CustomerController : ControllerBase
    {
        private readonly AutoSaleDbContext _context;
        public CustomerController(AutoSaleDbContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var user = await _context.Users.FindAsync(userId);
            return user == null ? NotFound() : Ok(user);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] User model)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Mobile = model.Mobile;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated" });
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.Password))
                return BadRequest("Old password incorrect");
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Password changed" });
        }

        [HttpGet("addresses")]
        public async Task<IActionResult> GetAddresses()
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var addresses = await _context.DeliveryAddresses.Where(a => a.UserId == userId).ToListAsync();
            return Ok(addresses);
        }

        [HttpPost("addresses")]
        public async Task<IActionResult> AddAddress([FromBody] DeliveryAddress model)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            model.UserId = userId;
            _context.DeliveryAddresses.Add(model);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Address added" });
        }

        [HttpPut("addresses/{id}")]
        public async Task<IActionResult> UpdateAddress(int id, [FromBody] DeliveryAddress model)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var address = await _context.DeliveryAddresses.FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId);
            if (address == null) return NotFound();
            address.Address = model.Address;
            address.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Address updated" });
        }

        [HttpGet("cars")]
        public async Task<IActionResult> GetCars(
             [FromQuery] string? keyword = null,
             [FromQuery] string? paymentType = null,
             [FromQuery] decimal? priceFrom = null,
             [FromQuery] decimal? priceTo = null,
             [FromQuery] bool? vatDeduction = null,
             [FromQuery] bool? discountedCars = null,
             [FromQuery] bool? premiumPartners = null,
             [FromQuery] int? registrationFrom = null,
             [FromQuery] int? registrationTo = null,
             [FromQuery] int? mileageFrom = null,
             [FromQuery] int? mileageTo = null,
             [FromQuery] string? transmission = null,
             [FromQuery] string? fuel = null,
             [FromQuery] string? powerUnit = null,
             [FromQuery] double? powerFrom = null,
             [FromQuery] double? powerTo = null,
             [FromQuery] string? vehicleType = null,
             [FromQuery] bool? driveType4x4 = null,
             [FromQuery] string? exteriorColor = null,
             [FromQuery] List<string>? features = null
         )
        {
            var query = _context.CarListings
                .Include(c => c.Model)
                .ThenInclude(m => m.Manufacturer)
                .Include(c => c.Specifications)
                .Include(c => c.CarImages)
                .Include(c => c.CarListingFeatures)
                    .ThenInclude(clf => clf.Feature)
                .Include(c => c.CarServiceHistories)
                .Include(c => c.CarPricingDetails)
                .Include(c => c.CarSales)
                .ThenInclude(s => s.SaleStatus)
                .Include(c => c.Reviews)
                .ThenInclude(r => r.User)
                .AsQueryable();

            // Apply Keyword Filter
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(c =>
                    c.Model.Name.Contains(keyword) ||
                    c.Model.Manufacturer.Name.Contains(keyword) ||
                    c.Description.Contains(keyword) ||
                    c.Year.ToString().Contains(keyword)
                );
            }
            if (priceFrom.HasValue)
            {
                query = query.Where(c => c.Price >= priceFrom.Value);
            }
            if (priceTo.HasValue)
            {
                query = query.Where(c => c.Price <= priceTo.Value);
            }
            if (vatDeduction.HasValue && vatDeduction.Value)
            {
                query = query.Where(c => c.CarPricingDetails.Any());
            }
            if (discountedCars.HasValue && discountedCars.Value)
            {
            }
            if (premiumPartners.HasValue && premiumPartners.Value)
            {
            }

            if (registrationFrom.HasValue)
            {
                query = query.Where(c => c.Year >= registrationFrom.Value);
            }
            if (registrationTo.HasValue)
            {
                query = query.Where(c => c.Year <= registrationTo.Value);
            }

            if (mileageFrom.HasValue)
            {
                query = query.Where(c => c.Mileage >= mileageFrom.Value);
            }
            if (mileageTo.HasValue)
            {
                query = query.Where(c => c.Mileage <= mileageTo.Value);
            }
            if (!string.IsNullOrEmpty(transmission))
            {
                query = query.Where(c => c.Specifications.Any(s => s.Transmission == transmission));
            }

            if (!string.IsNullOrEmpty(fuel))
            {
                query = query.Where(c => c.Specifications.Any(s => s.FuelType == fuel));
            }

            


            if (!string.IsNullOrEmpty(vehicleType))
            {
                query = query.Where(c => c.Specifications.Any(s => s.CarType == vehicleType));
            }

            if (driveType4x4.HasValue && driveType4x4.Value)
            {
                query = query.Where(c => c.CarListingFeatures.Any(clf => clf.Feature.Name == "4x4"));
            }

            if (!string.IsNullOrEmpty(exteriorColor))
            {
                query = query.Where(c => c.Specifications.Any(s => s.ExteriorColor == exteriorColor));
            }

            if (features != null && features.Any())
            {
                foreach (var featureName in features)
                {
                    query = query.Where(c => c.CarListingFeatures.Any(clf => clf.Feature.Name == featureName));
                }
            }


            var cars = await query
                .Select(c => new
                {
                    c.ListingId,
                    c.ModelId,
                    c.UserId,
                    c.Year,
                    c.Mileage,
                    c.Price,
                    c.Condition,
                    c.DatePosted,
                    Model = new
                    {
                        c.Model.ModelId,
                        c.Model.Name,
                        Manufacturer = new
                        {
                            c.Model.Manufacturer.ManufacturerId,
                            c.Model.Manufacturer.Name
                        }
                    },
                    Specifications = c.Specifications != null ? c.Specifications.Select(s => new
                    {
                        s.SpecificationId,
                        s.Engine,
                        s.Transmission,
                        s.FuelType,
                        s.SeatingCapacity,
                        s.InteriorColor,
                        s.ExteriorColor,
                        s.CarType,
                    }).ToList() : null,
                    Images = c.CarImages != null ? c.CarImages.Select(i => new
                    {
                        i.ImageId,
                        i.Url,
                        i.Filename
                    }) : null,
                    Features = c.CarListingFeatures != null ? c.CarListingFeatures.Select(f => new
                    {
                        f.Feature.FeatureId,
                        f.Feature.Name
                    }) : null,
                    ServiceHistory = c.CarServiceHistories != null ? c.CarServiceHistories.Select(sh => new
                    {
                        sh.RecentServicing,
                        sh.NoAccidentHistory,
                        sh.Modifications
                    }) : null,
                    Pricing = c.CarPricingDetails != null ? c.CarPricingDetails.Select(
                        shh => new
                        {
                            shh.TaxRate,
                            shh.RegistrationFee,
                        }
                        ).ToList() : null,
                    SalesHistory = c.CarSales != null ? c.CarSales.Select(s => new
                    {
                        s.SaleId,
                        s.FinalPrice,
                        s.SaleDate,
                        s.SaleStatus.StatusName
                    }) : null,
                    Reviews = c.Reviews != null ? c.Reviews.Select(r => new
                    {
                        r.ReviewId,
                        r.UserId,
                        r.Rating,
                        r.User.FullName,
                        r.CreatedAt
                    }) : null
                })
                .ToListAsync();

            return Ok(cars);
        }
        [HttpGet("cars/{id}")]
        public async Task<IActionResult> GetCarDetail(int id)
        {
            var car = await _context.CarListings
                .Include(c => c.Model)
                    .ThenInclude(m => m.Manufacturer)
                .Include(c => c.Specifications)
                .Include(c => c.CarImages)
                .Include(c => c.CarListingFeatures)
                    .ThenInclude(clf => clf.Feature)
                .Include(c => c.CarServiceHistories)
                .Include(c => c.CarPricingDetails)
                .Include(c => c.CarSales)
                    .ThenInclude(clf => clf.SaleStatus)
                .Include(c => c.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.ListingId == id);

            if (car == null)
                return NotFound();

            var carDetail = new
            {
                car.ListingId,
                car.ModelId,
                car.UserId,
                car.Year,
                car.Mileage,
                car.Price,
                car.Condition,
                car.DatePosted,
                Model = new
                {
                    car.Model.ModelId,
                    car.Model.Name,
                    Manufacturer = new
                    {
                        car.Model.Manufacturer.ManufacturerId,
                        car.Model.Manufacturer.Name
                    }
                },
                Specification = car.Specifications != null ? car.Specifications.Select(s => new

                    {
                        s.SpecificationId,
                        s.Engine,
                        s.Transmission,
                        s.FuelType,
                        s.SeatingCapacity,
                        s.InteriorColor,
                        s.ExteriorColor,
                        s.CarType
                    }).ToList() : null,
                Images = car.CarImages != null ? car.CarImages.Select(i => new
                {
                    i.ImageId,
                    i.Url,
                    i.Filename
                }) : null,
                Features = car.CarListingFeatures != null ? car.CarListingFeatures.Select(f => new
                {
                    f.Feature.FeatureId,
                    f.Feature.Name
                }) : null,
                ServiceHistory = car.CarServiceHistories != null ? car.CarServiceHistories.Select(sh => new
                {
                    sh.RecentServicing,
                    sh.NoAccidentHistory,
                    sh.Modifications
                }) : null,
                Pricing = car.CarPricingDetails != null ? car.CarPricingDetails.Select(shh => new
                    {
                        shh.TaxRate,
                        shh.RegistrationFee
                    }).ToList() : null,
                SalesHistory = car.CarSales != null ? car.CarSales.Select(s => new
                {
                    s.SaleId,
                    s.FinalPrice,
                    s.SaleDate,
                    s.SaleStatus.StatusName
                }) : null,
                Reviews = car.Reviews != null ? car.Reviews.Select(r => new
                {
                    r.ReviewId,
                    r.UserId,
                    r.Rating,
                    r.User.FullName,
                    r.CreatedAt
                }) : null
            };

            return Ok(carDetail);
        }

        [HttpPost("orders")]
        public async Task<IActionResult> CreateOrder([FromBody] CarSale model)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            model.CustomerId = userId;
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;
            _context.CarSales.Add(model);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Order created" });
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders([FromQuery] int? statusId = null)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var query = _context.CarSales.Where(s => s.CustomerId == userId);
            if (statusId.HasValue)
                query = query.Where(s => s.SaleStatusId == statusId.Value);
            var orders = await query.ToListAsync();
            return Ok(orders);
        }

        [HttpGet("orders/{id}")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var order = await _context.CarSales.FirstOrDefaultAsync(s => s.SaleId == id && s.CustomerId == userId);
            return order == null ? NotFound() : Ok(order);
        }

        [HttpPut("orders/{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var sale = await _context.CarSales.FirstOrDefaultAsync(s => s.SaleId == id && s.CustomerId == userId);
            if (sale == null) return NotFound();
            sale.SaleStatusId = 3;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Order cancelled" });
        }

        [HttpPost("reviews")]
        public async Task<IActionResult> AddReview([FromBody] Review model)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            model.UserId = userId;
            model.CreatedAt = DateTime.UtcNow;
            _context.Reviews.Add(model);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Review added" });
        }

        [HttpGet("blogs")]
        public async Task<IActionResult> GetBlogs()
        {
            var blogs = await _context.BlogPosts.ToListAsync();
            return Ok(blogs);
        }

        [HttpGet("blogs/{id}")]
        public async Task<IActionResult> GetBlogDetail(int id)
        {
            var blog = await _context.BlogPosts.FindAsync(id);
            return blog == null ? NotFound() : Ok(blog);
        }

        [HttpGet("chats")]
        public IActionResult GetChats() => Ok(new { message = "Chat list (implement as needed)" });

        [HttpGet("chats/{id}")]
        public IActionResult GetChatDetail(int id) => Ok(new { message = "Chat detail (implement as needed)" });

        [HttpPost("chats/{id}/send")]
        public IActionResult SendChat(int id, [FromBody] string message) => Ok(new { message = "Message sent (implement as needed)" });

        // 13. Xem khuyến mãi
        [HttpGet("promotions")]
        public async Task<IActionResult> GetPromotions()
        {
            var promotions = await _context.Promotions.ToListAsync();
            return Ok(promotions);
        }
    }
}
