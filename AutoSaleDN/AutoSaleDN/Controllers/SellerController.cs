using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSaleDN.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AutoSaleDN.DTO;

namespace AutoSaleDN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Seller")]
    public class SellerController : ControllerBase
    {
        private readonly AutoSaleDbContext _context;
        public SellerController(AutoSaleDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User ID claim not found.");
            }
            return int.Parse(userIdClaim);
        }

        // 1. Xem & cập nhật profile
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

        // 2. Đổi mật khẩu
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
        // 4. Quản lý xe của mình
        [HttpGet("cars")]
        public async Task<IActionResult> GetMyCars()
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var cars = await _context.CarListings.Where(c => c.UserId == userId).ToListAsync();
            return Ok(cars);
        }

        [HttpPost("cars")]
        public async Task<IActionResult> AddCar([FromBody] CarListing model)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            model.UserId = userId;
            model.DatePosted = DateTime.UtcNow;
            model.DateUpdated = DateTime.UtcNow;
            _context.CarListings.Add(model);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Car added" });
        }

        [HttpPut("cars/{id}")]
        public async Task<IActionResult> UpdateCar(int id, [FromBody] CarListing model)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var car = await _context.CarListings.FirstOrDefaultAsync(c => c.ListingId == id && c.UserId == userId);
            if (car == null) return NotFound();
            car.ModelId = model.ModelId;
            car.Year = model.Year;
            car.Mileage = model.Mileage;
            car.Price = model.Price;
            car.Condition = model.Condition;
            car.Status = model.Status;
            car.Description = model.Description;
            car.DateUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Car updated" });
        }

        [HttpDelete("cars/{id}")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var car = await _context.CarListings.FirstOrDefaultAsync(c => c.ListingId == id && c.UserId == userId);
            if (car == null) return NotFound();
            _context.CarListings.Remove(car);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Car deleted" });
        }
        [HttpGet("orders")]
        public async Task<IActionResult> GetSellerOrders()
        {
            try
            {
                var userId = GetUserId();
                var seller = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.Role == "Seller");

                if (seller == null || !seller.StoreLocationId.HasValue)
                {
                    return Unauthorized(new { message = "Seller profile or store location not found." });
                }

                var storeLocationId = seller.StoreLocationId.Value;

                var orders = await _context.CarSales
                    .Include(cs => cs.SaleStatus)
                    .Include(cs => cs.Customer)
                    .Include(cs => cs.StoreListing)
                        .ThenInclude(sl => sl.CarListing)
                            .ThenInclude(cl => cl.Model)
                                .ThenInclude(cm => cm.CarManufacturer)
                    .Include(cs => cs.StoreListing)
                        .ThenInclude(sl => sl.CarListing)
                            .ThenInclude(cl => cl.CarImages)
                    .Include(cs => cs.StatusHistory) // Thêm dòng này để lấy lịch sử trạng thái
                        .ThenInclude(sh => sh.SaleStatus) // Lấy tên của trạng thái
                    .Where(s => s.StoreListing.StoreLocationId == storeLocationId)
                    .OrderByDescending(cs => cs.CreatedAt)
                    .Select(cs => new
                    {
                        OrderId = cs.SaleId,
                        cs.OrderNumber,
                        cs.FinalPrice,
                        cs.DepositAmount,
                        cs.RemainingBalance,
                        OrderDate = cs.CreatedAt,
                        cs.DeliveryOption,
                        cs.ExpectedDeliveryDate,
                        cs.ActualDeliveryDate,
                        cs.OrderType,
                        Notes = cs.Notes,

                        CurrentSaleStatus = new
                        {
                            Id = cs.SaleStatus.SaleStatusId,
                            Name = cs.SaleStatus.StatusName
                        },

                        // Thêm lịch sử trạng thái vào đây
                        StatusHistory = cs.StatusHistory.Select(sh => new
                        {
                            Id = sh.SaleStatusId,
                            Name = sh.SaleStatus.StatusName,
                            Date = sh.Timestamp,
                            Notes = sh.Notes
                        }).OrderBy(sh => sh.Date).ToList(),

                        CustomerInfo = cs.Customer != null ? new
                        {
                            Name = cs.Customer.FullName,
                            Email = cs.Customer.Email,
                            Phone = cs.Customer.Mobile,
                        } : null,

                        CarDetails = cs.StoreListing.CarListing != null ? new
                        {
                            ListingId = cs.StoreListing.CarListing.ListingId,
                            Make = cs.StoreListing.CarListing.Model.CarManufacturer.Name,
                            Model = cs.StoreListing.CarListing.Model.Name,
                            Year = cs.StoreListing.CarListing.Year,
                            ImageUrl = cs.StoreListing.CarListing.CarImages.FirstOrDefault().Url
                        } : null,

                        // ... các trường khác
                    })
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("orders/{id}")]
        public async Task<IActionResult> GetSellerOrderDetail(int id)
        {
            try
            {
                var userId = GetUserId();
                var seller = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.Role == "Seller");

                if (seller == null || !seller.StoreLocationId.HasValue)
                {
                    return Unauthorized(new { message = "Seller profile or store location not found." });
                }

                var storeLocationId = seller.StoreLocationId.Value;

                var order = await _context.CarSales
                    .Include(cs => cs.SaleStatus)
                    .Include(cs => cs.Customer)
                    .Include(cs => cs.StoreListing)
                        .ThenInclude(sl => sl.CarListing)
                            .ThenInclude(cl => cl.Model)
                                .ThenInclude(cm => cm.CarManufacturer)
                    .Include(cs => cs.StatusHistory)
                        .ThenInclude(sh => sh.SaleStatus)
                    .Include(cs => cs.StoreListing)
                        .ThenInclude(sl => sl.CarListing)
                            .ThenInclude(cl => cl.CarImages)
                    .FirstOrDefaultAsync(s => s.SaleId == id && s.StoreListing.StoreLocationId == storeLocationId);

                if (order == null) return NotFound("Order not found or you are not authorized to view this order.");

                return Ok(new
                {
                    OrderId = order.SaleId,
                    order.OrderNumber,
                    order.FinalPrice,
                    order.DepositAmount,
                    order.RemainingBalance,
                    order.ExpectedDeliveryDate,
                    order.ActualDeliveryDate,
                    order.Notes,
                    OrderDate = order.SaleDate,
                    CurrentSaleStatus = new
                    {
                        Id = order.SaleStatus.SaleStatusId,
                        Name = order.SaleStatus.StatusName
                    },
                    StatusHistory = order.StatusHistory.Select(sh => new
                    {
                        Id = sh.SaleStatusId,
                        Name = sh.SaleStatus.StatusName,
                        Date = sh.Timestamp,
                        Notes = sh.Notes
                    }).OrderBy(sh => sh.Date).ToList(),
                    CustomerInfo = new
                    {
                        Name = order.Customer?.FullName,
                        Phone = order.Customer?.Mobile,
                        Email = order.Customer?.Email
                    },
                    CarDetails = new
                    {
                        ListingId = order.StoreListing?.CarListing?.ListingId,
                        Make = order.StoreListing?.CarListing?.Model?.CarManufacturer?.Name,
                        Model = order.StoreListing?.CarListing?.Model?.Name,
                        Year = order.StoreListing?.CarListing?.Year,
                        ImageUrl = order.StoreListing?.CarListing?.CarImages?.FirstOrDefault()?.Url
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
        // DTO để nhận dữ liệu từ request
        public class UpdateOrderDeliveryStatusDto
        {
            public int SaleStatusId { get; set; }
            public DateTime? ExpectedDeliveryDate { get; set; }
            public DateTime? ActualDeliveryDate { get; set; }
            public string Notes { get; set; }
        }

        [HttpPut("orders/{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderDeliveryStatusDto dto)
        {
            // ... (Giữ nguyên phần xác thực và tìm order)
            var userId = GetUserId();
            var seller = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.Role == "Seller");

            if (seller == null || !seller.StoreLocationId.HasValue)
            {
                return Unauthorized(new { message = "Seller profile or store location not found." });
            }

            var storeLocationId = seller.StoreLocationId.Value;

            var order = await _context.CarSales
                .Include(o => o.StoreListing)
                .FirstOrDefaultAsync(o => o.SaleId == id && o.StoreListing.StoreLocationId == storeLocationId);

            if (order == null)
            {
                return NotFound("Order not found or you are not authorized to update this order.");
            }

            // Kiểm tra SaleStatusId có hợp lệ không
            var newStatus = await _context.SaleStatus.FindAsync(dto.SaleStatusId);
            if (newStatus == null)
            {
                return BadRequest(new { message = "Invalid status ID." });
            }

            // Cập nhật các trường
            order.SaleStatusId = dto.SaleStatusId;
            order.ExpectedDeliveryDate = dto.ExpectedDeliveryDate;
            order.ActualDeliveryDate = dto.ActualDeliveryDate;
            order.Notes = dto.Notes;
            order.UpdatedAt = DateTime.UtcNow;

           var statusHistoryEntry = new SaleStatusHistory
           {
               SaleId = order.SaleId,
               SaleStatusId = dto.SaleStatusId,
               UserId = userId,
               Notes = dto.Notes,
               Timestamp = DateTime.UtcNow
           };
            _context.SaleStatusHistory.Add(statusHistoryEntry);

           await _context.SaveChangesAsync();

            return Ok(new { message = "Order status updated successfully." });
        }

        private async Task<int?> GetSellerShowroomId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return null;

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return null;
            }
            var user = await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => u.StoreLocationId)
                .FirstOrDefaultAsync();

            return user;
        }

        [HttpGet("reports/revenue/daily")]
        public async Task<IActionResult> GetDailyRevenueReport(DateTime? date = null)
        {
            var showroomId = await GetSellerShowroomId();
            if (showroomId == null)
            {
                return Unauthorized("Seller has no assigned showroom.");
            }

            var targetDate = date?.Date ?? DateTime.UtcNow.Date;
            var sales = await _context.Payments
                .Where(p => p.PaymentStatus == "completed")
                .Where(p => p.DateOfPayment.Date == targetDate)
                .Where(p => p.PaymentForSale != null && p.PaymentForSale.StoreListing.StoreLocationId == showroomId)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            return Ok(new { date = targetDate, totalRevenue = sales });
        }

        // Lấy doanh thu hàng tháng của showroom seller
        [HttpGet("reports/revenue/monthly")]
        public async Task<IActionResult> GetMonthlyRevenueReport(int? year = null, int? month = null)
        {
            var showroomId = await GetSellerShowroomId();
            if (showroomId == null)
            {
                return Unauthorized("Seller has no assigned showroom.");
            }

            var y = year ?? DateTime.UtcNow.Year;
            var m = month ?? DateTime.UtcNow.Month;
            var sales = await _context.Payments
                .Where(p => p.PaymentStatus == "completed")
                .Where(p => p.DateOfPayment.Year == y && p.DateOfPayment.Month == m)
                .Where(p => p.PaymentForSale != null && p.PaymentForSale.StoreListing.StoreLocationId == showroomId)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            return Ok(new { year = y, month = m, totalRevenue = sales });
        }

        // Lấy doanh thu hàng năm của showroom seller
        [HttpGet("reports/revenue/yearly")]
        public async Task<IActionResult> GetYearlyRevenueReport(int? year = null)
        {
            var showroomId = await GetSellerShowroomId();
            if (showroomId == null)
            {
                return Unauthorized("Seller has no assigned showroom.");
            }

            var y = year ?? DateTime.UtcNow.Year;
            var sales = await _context.Payments
                .Where(p => p.PaymentStatus == "completed")
                .Where(p => p.DateOfPayment.Year == y)
                .Where(p => p.PaymentForSale != null && p.PaymentForSale.StoreListing.StoreLocationId == showroomId)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            return Ok(new { year = y, totalRevenue = sales });
        }

        // Lấy danh sách xe bán chạy nhất của showroom seller
        [HttpGet("reports/top-selling-cars")]
        public async Task<ActionResult<IEnumerable<TopSellingCarDto>>> GetTopSellingCars()
        {
            var showroomId = await GetSellerShowroomId();
            if (showroomId == null)
            {
                return Unauthorized("Seller has no assigned showroom.");
            }

            var topCars = await _context.CarListings
                .Include(cl => cl.Model)
                .ThenInclude(m => m.CarManufacturer)
                .Include(cl => cl.CarImages)
                .Where(cl => cl.StoreListings.Any(sl => sl.StoreLocationId == showroomId))
                .GroupBy(cl => new
                {
                    cl.ModelId,
                    ModelName = cl.Model.Name,
                    ManufacturerName = cl.Model.CarManufacturer.Name
                })
                .Select(g => new TopSellingCarDto
                {
                    ModelId = g.Key.ModelId,
                    ModelName = g.Key.ModelName,
                    ManufacturerName = g.Key.ManufacturerName,
                    ImageUrl = g.FirstOrDefault().CarImages.FirstOrDefault().Url,
                    // Cập nhật để tính TotalSold dựa trên các đơn hàng đã thanh toán hoàn tất
                    TotalSold = _context.CarSales
                        .Where(cs => cs.StoreListing.StoreLocationId == showroomId &&
                                     cs.StoreListing.CarListing.ModelId == g.Key.ModelId)
                        // Đếm các đơn hàng có ít nhất một khoản thanh toán full_payment hoặc remaining_payment đã hoàn tất
                        .Count(cs => cs.FullPayment != null && cs.FullPayment.PaymentStatus == "completed" ||
                                     cs.DepositPayment != null && cs.DepositPayment.PaymentStatus == "completed" && cs.RemainingBalance == 0),
                    // Cập nhật để tính Revenue từ các khoản thanh toán đã hoàn tất
                    Revenue = _context.Payments
                        .Where(p => (p.PaymentPurpose == "full_payment" || p.PaymentPurpose == "remaining_payment") &&
                                     p.PaymentStatus == "completed")
                        .Where(p => p.PaymentForSale != null &&
                                     p.PaymentForSale.StoreListing.StoreLocationId == showroomId &&
                                     p.PaymentForSale.StoreListing.CarListing.ModelId == g.Key.ModelId)
                        .Sum(p => (decimal?)p.Amount) ?? 0,
                    AverageRating = _context.Reviews
                        .Where(r => r.Listing.StoreListings.Any(sl => sl.StoreLocationId == showroomId) &&
                                    r.Listing.ModelId == g.Key.ModelId)
                        .Any() ? (int)_context.Reviews
                        .Where(r => r.Listing.StoreListings.Any(sl => sl.StoreLocationId == showroomId) &&
                                    r.Listing.ModelId == g.Key.ModelId)
                        .Average(r => r.Rating) : 0,
                    TotalReviews = _context.Reviews
                        .Where(r => r.Listing.StoreListings.Any(sl => sl.StoreLocationId == showroomId) &&
                                    r.Listing.ModelId == g.Key.ModelId)
                        .Count()
                })
                .OrderByDescending(c => c.Revenue)
                .Take(10)
                .ToListAsync();

            return Ok(topCars);
        }

        [HttpGet("cars-in-showroom")]
        public async Task<ActionResult<ShowroomInventoryDto>> GetCarsInShowroom()
        {
            var showroomId = await GetSellerShowroomId();
            if (showroomId == null)
            {
                return Unauthorized("Seller has no assigned showroom.");
            }

            var showroom = await _context.StoreLocations
                .FirstOrDefaultAsync(sl => sl.StoreLocationId == showroomId);
            if (showroom == null)
            {
                return NotFound("Showroom not found.");
            }

            // Lấy tất cả các listing đang 'IN_STOCK' của showroom
            var listings = await _context.StoreListings
                .Include(sl => sl.CarListing)
                    .ThenInclude(cl => cl.Model)
                        .ThenInclude(m => m.CarManufacturer)
                .Include(sl => sl.CarListing)
                    .ThenInclude(cl => cl.CarImages)
                .Where(sl => sl.StoreLocationId == showroomId && sl.Status == "IN_STOCK")
                .ToListAsync();

            // Tính tổng số xe
            var totalCars = listings.Sum(sl => sl.AvailableQuantity);

            // Grouping và tạo danh sách Brands
            var brands = listings
                .GroupBy(sl => sl.CarListing.Model.CarManufacturer.Name)
                .Select(g => new CarBrandStatsDto
                {
                    BrandName = g.Key,
                    TotalCars = g.Sum(sl => sl.AvailableQuantity)
                })
                .ToList();

            // Tạo danh sách Models, bao gồm hình ảnh
            var models = listings
                .Select(sl => new CarModelStatsDto
                {
                    ModelName = sl.CarListing.Model.Name,
                    ManufacturerName = sl.CarListing.Model.CarManufacturer.Name,
                    CurrentQuantity = sl.CurrentQuantity,
                    AvailableQuantity = sl.AvailableQuantity,
                })
                .GroupBy(m => new { m.ModelName, m.ManufacturerName })
                .Select(g => new CarModelStatsDto
                {
                    ModelName = g.Key.ModelName,
                    ManufacturerName = g.Key.ManufacturerName,
                    CurrentQuantity = g.Sum(m => m.CurrentQuantity),
                    AvailableQuantity = g.Sum(m => m.AvailableQuantity),
                })
                .ToList();

            // Khởi tạo và trả về một đối tượng ShowroomInventoryDto duy nhất
            var inventory = new ShowroomDetailsDto
            {
                TotalCars = totalCars,
                AvailableCars = totalCars,
                Brands = brands,
                Models = models
            };

            return Ok(inventory);
        }

        [HttpGet("reports/my-showroom-inventory")]
    public async Task<ActionResult<ShowroomInventoryDto>> GetMyShowroomInventory()
    {
        var showroomId = await GetSellerShowroomId();
        if (showroomId == null)
        {
            return Unauthorized("Seller has no assigned showroom.");
        }

        var showroom = await _context.StoreLocations
            .FirstOrDefaultAsync(sl => sl.StoreLocationId == showroomId);
        if (showroom == null)
        {
            return NotFound("Showroom not found.");
        }

        var listings = await _context.StoreListings
            .Include(sl => sl.CarListing)
                .ThenInclude(cl => cl.Model)
                    .ThenInclude(m => m.CarManufacturer)
            .Include(sl => sl.CarListing)
                .ThenInclude(cl => cl.CarImages)
            .Where(sl => sl.StoreLocationId == showroomId && sl.Status == "IN_STOCK")
            .ToListAsync();

        var totalCars = listings.Sum(sl => sl.AvailableQuantity);

        var brands = listings
            .GroupBy(sl => sl.CarListing.Model.CarManufacturer.Name)
            .Select(g => new CarBrandStatsDto
            {
                BrandName = g.Key,
                TotalCars = g.Sum(sl => sl.AvailableQuantity)
            })
            .ToList();

        var models = listings
            .Select(sl => new
            {
                ModelName = sl.CarListing.Model.Name,
                ManufacturerName = sl.CarListing.Model.CarManufacturer.Name,
                CurrentQuantity = sl.CurrentQuantity,
                AvailableQuantity = sl.AvailableQuantity,
                ImageUrl = sl.CarListing.CarImages.FirstOrDefault().Url
            })
            .GroupBy(m => new { m.ModelName, m.ManufacturerName })
            .Select(g => new CarModelStatsDto
            {
                ModelName = g.Key.ModelName,
                ManufacturerName = g.Key.ManufacturerName,
                CurrentQuantity = g.Sum(m => m.CurrentQuantity),
                AvailableQuantity = g.Sum(m => m.AvailableQuantity),
            })
            .ToList();

        var inventory = new ShowroomDetailsDto
        {
            TotalCars = totalCars,
            AvailableCars = totalCars,
            Brands = brands,
            Models = models
        };

        return Ok(inventory);
    }

        // 7. Xem, trả lời đánh giá
        [HttpGet("reviews")]
        public async Task<IActionResult> GetReviews()
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var reviews = await _context.Reviews
                .Include(r => r.Listing)
                .Where(r => r.Listing.UserId == userId)
                .ToListAsync();
            return Ok(reviews);
        }

        [HttpPost("reviews/{id}/reply")]
        public async Task<IActionResult> ReplyReview(int id, [FromBody] string reply)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();
            review.Reply = reply;
            review.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Reply added" });
        }

        // 8. Chat (giả lập)
        [HttpGet("chats")]
        public IActionResult GetChats() => Ok(new { message = "Chat list (implement as needed)" });

        [HttpGet("chats/{id}")]
        public IActionResult GetChatDetail(int id) => Ok(new { message = "Chat detail (implement as needed)" });

        [HttpPost("chats/{id}/reply")]
        public IActionResult ReplyChat(int id, [FromBody] string message) => Ok(new { message = "Reply sent (implement as needed)" });

        // 9. Xem danh sách khuyến mãi
        [HttpGet("promotions")]
        public async Task<IActionResult> GetPromotions()
        {
            var promotions = await _context.Promotions.ToListAsync();
            return Ok(promotions);
        }
    }

    public class ChangePasswordDto
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}