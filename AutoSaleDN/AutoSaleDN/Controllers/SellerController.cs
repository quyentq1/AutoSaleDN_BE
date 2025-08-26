using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSaleDN.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AutoSaleDN.DTO;
using System.ComponentModel.DataAnnotations;

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

        private IQueryable<Payment> GetBaseRevenueQuery(int showroomId)
        {
            return _context.Payments
                .Where(p => p.PaymentStatus == "completed" &&
                            p.PaymentForSale != null &&
                            p.PaymentForSale.StoreListing.StoreLocationId == showroomId);
        }

        // --- CÁC API HIỆN TẠI ĐÃ ĐƯỢC LÀM GỌN ---

        [HttpGet("reports/revenue/daily")]
        public async Task<IActionResult> GetDailyRevenueReport(DateTime? date = null)
        {
            var showroomId = await GetSellerShowroomId();
            if (showroomId == null) return Unauthorized("Seller has no assigned showroom.");

            var targetDate = date?.Date ?? DateTime.UtcNow.Date;

            var totalRevenue = await GetBaseRevenueQuery(showroomId.Value)
                .Where(p => p.DateOfPayment.Date == targetDate)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            return Ok(new { date = targetDate, totalRevenue });
        }

        [HttpGet("reports/revenue/monthly")]
        public async Task<IActionResult> GetMonthlyRevenueReport(int? year = null, int? month = null)
        {
            var showroomId = await GetSellerShowroomId();
            if (showroomId == null) return Unauthorized("Seller has no assigned showroom.");

            var y = year ?? DateTime.UtcNow.Year;
            var m = month ?? DateTime.UtcNow.Month;
            var currentMonthStart = new DateTime(y, m, 1);
            var prevMonthStart = currentMonthStart.AddMonths(-1);

            var baseQuery = GetBaseRevenueQuery(showroomId.Value);

            var currentMonthRevenue = await baseQuery
                .Where(p => p.DateOfPayment.Year == y && p.DateOfPayment.Month == m)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            // TÍNH TOÁN SO SÁNH: Lấy doanh thu tháng trước
            var previousMonthRevenue = await baseQuery
                .Where(p => p.DateOfPayment >= prevMonthStart && p.DateOfPayment < currentMonthStart)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            // Tính tăng trưởng
            decimal? growth = previousMonthRevenue > 0
                ? Math.Round(((currentMonthRevenue - previousMonthRevenue) / previousMonthRevenue) * 100, 2)
                : (currentMonthRevenue > 0 ? 100.0m : 0m);

            return Ok(new
            {
                year = y,
                month = m,
                totalRevenue = currentMonthRevenue,
                previousMonthRevenue, // Trả về thêm dữ liệu so sánh
                growthPercentage = growth
            });
        }

        [HttpGet("reports/revenue/yearly")]
        public async Task<IActionResult> GetYearlyRevenueReport(int? year = null)
        {
            var showroomId = await GetSellerShowroomId();
            if (showroomId == null)
            {
                return Unauthorized("Seller has no assigned showroom.");
            }

            var y = year ?? DateTime.UtcNow.Year;

            // Sử dụng lại hàm helper để code gọn hơn
            var baseQuery = GetBaseRevenueQuery(showroomId.Value);

            // Tính doanh thu năm hiện tại
            var currentYearRevenue = await baseQuery
                .Where(p => p.DateOfPayment.Year == y)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            // TÍNH TOÁN SO SÁNH: Lấy doanh thu năm trước
            var previousYearRevenue = await baseQuery
                .Where(p => p.DateOfPayment.Year == y - 1)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            // Tính tăng trưởng
            decimal? growth = previousYearRevenue > 0
                ? Math.Round(((currentYearRevenue - previousYearRevenue) / previousYearRevenue) * 100, 2)
                : (currentYearRevenue > 0 ? 100.0m : 0m);

            return Ok(new
            {
                year = y,
                totalRevenue = currentYearRevenue,
                previousYearRevenue, // Dữ liệu so sánh
                yearGrowth = growth // Tỷ lệ tăng trưởng
            });
        }

        // --- API MỚI ĐƯỢC BỔ SUNG ---

        [HttpGet("reports/revenue/range")]
        public async Task<IActionResult> GetRevenueReportByRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var showroomId = await GetSellerShowroomId();
            if (showroomId == null) return Unauthorized("Seller has no assigned showroom.");

            // Đảm bảo endDate tính đến hết ngày
            var inclusiveEndDate = endDate.Date.AddDays(1);

            var totalRevenue = await GetBaseRevenueQuery(showroomId.Value)
                .Where(p => p.DateOfPayment >= startDate.Date && p.DateOfPayment < inclusiveEndDate)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            return Ok(new { startDate = startDate.Date, endDate = endDate.Date, totalRevenue });
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

            // === BƯỚC 1: LẤY DỮ LIỆU BÁN HÀNG CỦA TOP XE ===
            // Truy vấn này chỉ tập trung vào việc tính toán doanh số và doanh thu
            var topCarsSalesData = await _context.CarSales
                .Where(sale =>
                    sale.StoreListing.StoreLocationId == showroomId &&
                    sale.FullPayment != null &&
                    sale.FullPayment.PaymentStatus == "completed"
                )
                .GroupBy(sale => new
                {
                    sale.StoreListing.CarListing.ModelId,
                    ModelName = sale.StoreListing.CarListing.Model.Name,
                    ManufacturerName = sale.StoreListing.CarListing.Model.CarManufacturer.Name,
                    ImageUrl = sale.StoreListing.CarListing.CarImages.FirstOrDefault().Url
                })
                .Select(g => new TopSellingCarDto
                {
                    ModelId = g.Key.ModelId,
                    ModelName = g.Key.ModelName,
                    ManufacturerName = g.Key.ManufacturerName,
                    ImageUrl = g.Key.ImageUrl,
                    TotalSold = g.Count(),
                    Revenue = g.Sum(sale =>
                        (sale.DepositPayment != null && sale.DepositPayment.PaymentStatus == "completed" ? (decimal?)sale.DepositPayment.Amount : 0m) +
                        (sale.FullPayment != null && sale.FullPayment.PaymentStatus == "completed" ? (decimal?)sale.FullPayment.Amount : 0m)
                    ) ?? 0m
                })
                .OrderByDescending(c => c.TotalSold)
                .ThenByDescending(c => c.Revenue)
                .Take(10)
                .ToListAsync();

            if (!topCarsSalesData.Any())
            {
                return Ok(new List<TopSellingCarDto>());
            }

            // === BƯỚC 2: LẤY DỮ LIỆU REVIEW CHO TOP XE ĐÓ ===
            // Lấy danh sách ID của các model đã lọt vào top
            var topModelIds = topCarsSalesData.Select(c => c.ModelId).ToList();

            // Chạy một truy vấn duy nhất để lấy tất cả review stats cho các model này
            var reviewStats = await _context.Reviews
                .Where(r => r.ListingId.HasValue && topModelIds.Contains(r.Listing.ModelId))
                .GroupBy(r => r.Listing.ModelId)
                .Select(g => new
                {
                    ModelId = g.Key,
                    TotalReviews = g.Count(),
                    AverageRating = g.Average(r => r.Rating)
                })
                .ToDictionaryAsync(x => x.ModelId);

            // === BƯỚC 3: GỘP KẾT QUẢ LẠI ===
            // Gán thông tin review vào danh sách top xe (thao tác này cực nhanh vì thực hiện trong bộ nhớ)
            foreach (var car in topCarsSalesData)
            {
                if (reviewStats.ContainsKey(car.ModelId))
                {
                    var stats = reviewStats[car.ModelId];
                    car.TotalReviews = stats.TotalReviews;
                    car.AverageRating = (int)Math.Round(stats.AverageRating);
                }
            }

            return Ok(topCarsSalesData);
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

        [HttpGet("my-showroom-profile")]
        public async Task<IActionResult> GetMyShowroomProfile()
        {
            var showroomId = await GetSellerShowroomId();
            if (showroomId == null)
            {
                return Unauthorized("Seller has no assigned showroom.");
            }

            // Truy vấn CSDL để lấy tên và địa chỉ của showroom
            var showroomProfile = await _context.StoreLocations
                .Where(sl => sl.StoreLocationId == showroomId)
                .Select(sl => new
                {
                    sl.Name,
                    sl.Address
                })
                .FirstOrDefaultAsync();

            if (showroomProfile == null)
            {
                return NotFound("Showroom not found.");
            }

            return Ok(showroomProfile);
        }

        [HttpGet("reports/my-showroom-inventory")]
        public async Task<ActionResult<ShowroomInventoryDto>> GetMyShowroomInventory()
        {
            var showroomId = await GetSellerShowroomId();
            if (showroomId == null)
            {
                return Unauthorized("Seller has no assigned showroom.");
            }

            // Truy vấn cực kỳ đơn giản và hiệu quả để lấy tổng số xe
            var totalCars = await _context.StoreListings
                .CountAsync(listing => listing.StoreLocationId == showroomId && !listing.IsCurrent);

            return Ok(new { totalCars = totalCars });
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
        [HttpGet("posts")]

        public async Task<IActionResult> GetBlogPosts(

  [FromQuery] int page = 1,

  [FromQuery] int pageSize = 10,

  [FromQuery] string search = "",

  [FromQuery] int? categoryId = null,

  [FromQuery] bool? isPublished = null)

        {

            try

            {

                var sellerId = GetUserId();

                var query = _context.BlogPosts

                  .Include(p => p.Category)

                  .Include(p => p.BlogPostTags)

                    .ThenInclude(pt => pt.Tag)

                  .Where(p => p.UserId == sellerId)

                  .AsQueryable();



                if (!string.IsNullOrEmpty(search))

                {

                    query = query.Where(p =>

                      p.Title.Contains(search) ||

                      p.Slug.Contains(search) ||

                      p.Content.Contains(search));

                }



                if (categoryId.HasValue)

                {

                    query = query.Where(p => p.CategoryId == categoryId);

                }



                if (isPublished.HasValue)

                {

                    query = query.Where(p => p.IsPublished == isPublished);

                }



                var totalCount = await query.CountAsync();



                var posts = await query

                  .OrderByDescending(p => p.CreatedAt)

                  .Skip((page - 1) * pageSize)

                  .Take(pageSize)

                  .Select(p => new

                  {

                      p.PostId,

                      p.Title,

                      p.Slug,

                      p.Content,

                      p.Excerpt,

                      p.FeaturedImage,

                      p.IsPublished,

                      p.PublishedDate,

                      p.ViewCount,

                      p.CreatedAt,

                      p.UpdatedAt,

                      Category = new { p.Category.CategoryId, p.Category.Name },

                      Tags = p.BlogPostTags.Select(pt => new { pt.Tag.TagId, pt.Tag.Name })

                  })

                  .ToListAsync();



                return Ok(new

                {

                    Items = posts,

                    TotalCount = totalCount,

                    PageNumber = page,

                    PageSize = pageSize,

                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)

                });

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { message = "An error occurred while retrieving blog posts", error = ex.Message });

            }

        }



        [HttpGet("posts/{id}")]
        public async Task<IActionResult> GetBlogPost(int id)
        {
            try
            {
                var sellerId = GetUserId();

                var post = await _context.BlogPosts
                    .Include(p => p.Category)
                    .Include(p => p.BlogPostTags)
                        .ThenInclude(pt => pt.Tag)
                    .Where(p => p.PostId == id && p.UserId == sellerId)
                    .Select(p => new
                    {
                        p.PostId,
                        p.Title,
                        p.Slug,
                        p.Content,
                        p.Excerpt,
                        p.FeaturedImage,
                        p.IsPublished,
                        p.PublishedDate,
                        p.ViewCount,
                        p.CreatedAt,
                        p.UpdatedAt,
                        Category = new
                        {
                            p.Category.CategoryId,
                            p.Category.Name,
                            p.Category.Slug,
                        },
                        Tags = p.BlogPostTags.Select(pt => new
                        {
                            pt.Tag.TagId,
                            pt.Tag.Name,
                            pt.Tag.Slug
                        }).ToList(),
                    })
                    .FirstOrDefaultAsync();

                if (post == null)
                {
                    return NotFound(new { message = "Blog post not found" });
                }

                return Ok(post);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the blog post", error = ex.Message });
            }
        }



        [HttpPost("posts")]
        public async Task<IActionResult> CreateBlogPost([FromBody] BlogPostCreateModel model)
        {
            try
            {
                var sellerId = GetUserId();

                // Validate category exists
                var category = await _context.BlogCategories
                    .FirstOrDefaultAsync(c => c.CategoryId == model.CategoryId);

                if (category == null)
                {
                    return BadRequest(new { message = "Invalid category ID" });
                }

                // --- NEW TAGS LOGIC ---
                var allTagIds = new List<int>();

                // 1. Process existing tags from TagIds
                if (model.TagIds != null && model.TagIds.Any())
                {
                    var existingTagIds = await _context.BlogTags
                        .Where(t => model.TagIds.Contains(t.TagId))
                        .Select(t => t.TagId)
                        .ToListAsync();

                    if (existingTagIds.Count != model.TagIds.Count)
                    {
                        var invalidTagIds = model.TagIds.Except(existingTagIds).ToList();
                        return BadRequest(new { message = $"Invalid tag ID(s): {string.Join(", ", invalidTagIds)}" });
                    }
                    allTagIds.AddRange(existingTagIds);
                }

                // 2. Process new tags from NewTagNames
                if (model.NewTagNames != null && model.NewTagNames.Any())
                {
                    foreach (var newTagName in model.NewTagNames.Distinct())
                    {
                        var existingTag = await _context.BlogTags
                            .FirstOrDefaultAsync(t => t.Name.ToLower() == newTagName.ToLower());

                        if (existingTag != null)
                        {
                            // Tag with this name already exists, use its ID
                            allTagIds.Add(existingTag.TagId);
                        }
                        else
                        {
                            // Create new tag
                            var newTag = new BlogTag { Name = newTagName, Slug = newTagName };
                            _context.BlogTags.Add(newTag);
                            await _context.SaveChangesAsync(); // Save to get the new TagId
                            allTagIds.Add(newTag.TagId);
                        }
                    }
                }
                // --- END OF NEW TAGS LOGIC ---

                // Create new post
                var post = new BlogPost
                {
                    Title = model.Title,
                    Slug = model.Slug,
                    Content = model.Content,
                    Excerpt = model.Excerpt,
                    FeaturedImage = model.FeaturedImage,
                    IsPublished = model.IsPublished,
                    PublishedDate = model.IsPublished ? DateTime.UtcNow : null,
                    CategoryId = model.CategoryId,
                    UserId = sellerId,
                    CreatedAt = DateTime.UtcNow
                };

                // Add BlogPostTag entries for all tags
                post.BlogPostTags = allTagIds.Select(tagId => new BlogPostTag
                {
                    TagId = tagId
                }).ToList();

                _context.BlogPosts.Add(post);
                await _context.SaveChangesAsync();

                // Return the created post with related data
                var createdPost = await _context.BlogPosts
                    .Include(p => p.Category)
                    .Include(p => p.BlogPostTags)
                        .ThenInclude(pt => pt.Tag)
                    .Where(p => p.PostId == post.PostId)
                    .Select(p => new
                    {
                        p.PostId,
                        p.Title,
                        p.Slug,
                        p.Content,
                        p.Excerpt,
                        p.FeaturedImage,
                        p.IsPublished,
                        p.PublishedDate,
                        p.ViewCount,
                        p.CreatedAt,
                        p.UpdatedAt,
                        Category = new { p.Category.CategoryId, p.Category.Name },
                        Tags = p.BlogPostTags.Select(pt => new { pt.Tag.TagId, pt.Tag.Name })
                    })
                    .FirstOrDefaultAsync();

                return CreatedAtAction(nameof(GetBlogPost), new { id = post.PostId }, createdPost);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the blog post", error = ex.Message });
            }
        }


        // PUT: api/seller/posts/5

        [HttpPut("posts/{id}")]
        public async Task<IActionResult> UpdateBlogPost(int id, [FromBody] BlogPostUpdateModel model)
        {
            try
            {
                var sellerId = GetUserId();

                var post = await _context.BlogPosts
                    .Include(p => p.BlogPostTags)
                    .FirstOrDefaultAsync(p => p.PostId == id && p.UserId == sellerId);

                if (post == null)
                {
                    return NotFound(new { message = "Blog post not found" });
                }

                // Update post properties
                if (model.Title != null) post.Title = model.Title;
                if (model.Slug != null) post.Slug = model.Slug;
                if (model.Content != null) post.Content = model.Content;
                if (model.Excerpt != null) post.Excerpt = model.Excerpt;
                if (model.FeaturedImage != null) post.FeaturedImage = model.FeaturedImage;
                post.UpdatedAt = DateTime.UtcNow;

                // Update category if provided
                if (model.CategoryId.HasValue)
                {
                    var category = await _context.BlogCategories
                        .FirstOrDefaultAsync(c => c.CategoryId == model.CategoryId);
                    if (category == null)
                    {
                        return BadRequest(new { message = "Invalid category ID" });
                    }
                    post.CategoryId = category.CategoryId;
                }

                // --- NEW TAGS LOGIC ---
                if (model.TagIds != null || model.NewTagNames != null)
                {
                    var allTagIds = new List<int>();

                    // 1. Process existing tags from TagIds
                    if (model.TagIds != null && model.TagIds.Any())
                    {
                        var existingTagIds = await _context.BlogTags
                            .Where(t => model.TagIds.Contains(t.TagId))
                            .Select(t => t.TagId)
                            .ToListAsync();
                        allTagIds.AddRange(existingTagIds);
                    }

                    // 2. Process new tags from NewTagNames
                    if (model.NewTagNames != null && model.NewTagNames.Any())
                    {
                        foreach (var newTagName in model.NewTagNames.Distinct())
                        {
                            var existingTag = await _context.BlogTags
                                .FirstOrDefaultAsync(t => t.Name.ToLower() == newTagName.ToLower());

                            if (existingTag != null)
                            {
                                allTagIds.Add(existingTag.TagId);
                            }
                            else
                            {
                                var newTag = new BlogTag { Name = newTagName, Slug = newTagName };
                                _context.BlogTags.Add(newTag);
                                await _context.SaveChangesAsync();
                                allTagIds.Add(newTag.TagId);
                            }
                        }
                    }

                    // Remove existing tags and add new ones
                    _context.BlogPostTags.RemoveRange(post.BlogPostTags);
                    post.BlogPostTags = allTagIds.Distinct().Select(tagId => new BlogPostTag
                    {
                        PostId = post.PostId,
                        TagId = tagId
                    }).ToList();
                }
                // --- END OF NEW TAGS LOGIC ---

                // Update published status if provided
                if (model.IsPublished.HasValue)
                {
                    post.IsPublished = model.IsPublished.Value;
                    if (model.IsPublished.Value && !post.PublishedDate.HasValue)
                    {
                        post.PublishedDate = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();

                // Return the updated post
                var updatedPost = await _context.BlogPosts
                    .Include(p => p.Category)
                    .Include(p => p.BlogPostTags)
                        .ThenInclude(pt => pt.Tag)
                    .Where(p => p.PostId == id)
                    .Select(p => new
                    {
                        p.PostId,
                        p.Title,
                        p.Slug,
                        p.Content,
                        p.Excerpt,
                        p.FeaturedImage,
                        p.IsPublished,
                        p.PublishedDate,
                        p.ViewCount,
                        p.CreatedAt,
                        p.UpdatedAt,
                        Category = new { p.Category.CategoryId, p.Category.Name },
                        Tags = p.BlogPostTags.Select(pt => new { pt.Tag.TagId, pt.Tag.Name })
                    })
                    .FirstOrDefaultAsync();

                return Ok(updatedPost);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the blog post", error = ex.Message });
            }
        }

        // Paste this entire method into your SellerController.cs file

        // Đặt trong file SellerController.cs

        [HttpPut("orders/{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusDto dto)
        {
            // Bắt đầu một transaction để đảm bảo tất cả các thay đổi đều thành công hoặc không có gì cả
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var sellerId = GetUserId();

                var carSale = await _context.CarSales
                    .Include(cs => cs.StoreListing)
                    .FirstOrDefaultAsync(cs => cs.SaleId == orderId && cs.StoreListing.StoreLocation.Users.Any(u => u.UserId == sellerId));

                if (carSale == null)
                {
                    return NotFound(new { message = "Order not found or you do not have permission to modify it." });
                }

                var newStatus = await _context.SaleStatus.FindAsync(dto.NewStatusId);
                if (newStatus == null)
                {
                    return BadRequest(new { message = "Invalid status ID." });
                }

                var paymentCompleteStatus = await _context.SaleStatus.FirstOrDefaultAsync(s => s.StatusName == "Payment Complete");
                if (paymentCompleteStatus == null)
                {
                    return StatusCode(500, new { message = "Critical status 'Payment Complete' not found." });
                }

                // --- LOGIC ĐẶC BIỆT KHI GIAO HÀNG ---
                if (newStatus.StatusName == "Delivered")
                {
                    bool needsPayment = (carSale.RemainingBalance.HasValue && carSale.RemainingBalance > 0) || carSale.FullPaymentId == null;

                    if (needsPayment)
                    {
                        // 1. Tự động tạo bản ghi thanh toán cuối cùng
                        var finalPayment = new Payment
                        {
                            UserId = carSale.CustomerId,
                            ListingId = carSale.StoreListing.ListingId,
                            PaymentForSaleId = carSale.SaleId,
                            TransactionId = $"FINAL-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                            Amount = carSale.RemainingBalance ?? carSale.FinalPrice,
                            PaymentMethod = "internal_settlement", // Ghi nhận thanh toán nội bộ
                            PaymentStatus = "completed",
                            PaymentPurpose = "final_settlement_on_delivery",
                            DateOfPayment = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.Payments.Add(finalPayment);
                        await _context.SaveChangesAsync();

                        // 2. Cập nhật đơn hàng với thông tin thanh toán
                        carSale.FullPaymentId = finalPayment.PaymentId;
                        carSale.RemainingBalance = 0;

                        // Cập nhật trạng thái thành "Payment Complete" trước
                        carSale.SaleStatusId = paymentCompleteStatus.SaleStatusId;
                        carSale.UpdatedAt = DateTime.UtcNow;
                        _context.SaleStatusHistory.Add(new SaleStatusHistory
                        {
                            SaleId = carSale.SaleId,
                            SaleStatusId = paymentCompleteStatus.SaleStatusId,
                            UserId = sellerId, // Người bán đã thực hiện hành động
                            Notes = "Payment automatically settled upon delivery.",
                            Timestamp = DateTime.UtcNow
                        });
                        await _context.SaveChangesAsync();
                    }

                    // 3. Cập nhật ngày giao hàng thực tế
                    carSale.ActualDeliveryDate = DateTime.UtcNow;
                }

                // 4. Cập nhật trạng thái cuối cùng và lưu lịch sử
                carSale.SaleStatusId = dto.NewStatusId;
                carSale.UpdatedAt = DateTime.UtcNow;

                _context.SaleStatusHistory.Add(new SaleStatusHistory
                {
                    SaleId = carSale.SaleId,
                    SaleStatusId = dto.NewStatusId,
                    UserId = sellerId,
                    Notes = $"Status updated by seller.", // Bạn có thể thêm ghi chú từ DTO nếu muốn
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                // Hoàn tất transaction
                await transaction.CommitAsync();

                return Ok(new { message = "Order status updated successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error in UpdateOrderStatus: {ex.Message}");
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
        public class UpdateOrderStatusDto
        {
            public int NewStatusId { get; set; }
        }

        [HttpDelete("posts/{id}")]

        public async Task<IActionResult> DeleteBlogPost(int id)

        {

            try

            {

                var sellerId = GetUserId();



                var post = await _context.BlogPosts

                  .FirstOrDefaultAsync(p => p.PostId == id && p.UserId == sellerId);



                if (post == null)

                {

                    return NotFound(new { message = "Blog post not found" });

                }



                _context.BlogPosts.Remove(post);

                await _context.SaveChangesAsync();



                return NoContent();

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { message = "An error occurred while deleting the blog post", error = ex.Message });

            }

        }


        [HttpPatch("posts/{id}/publish")]

        public async Task<IActionResult> TogglePublishStatus(int id, [FromBody] bool isPublished)

        {

            try

            {

                var sellerId = GetUserId();



                var post = await _context.BlogPosts

                  .FirstOrDefaultAsync(p => p.PostId == id && p.UserId == sellerId);



                if (post == null)

                {

                    return NotFound(new { message = "Blog post not found" });

                }



                post.IsPublished = isPublished;

                post.PublishedDate = isPublished ? DateTime.UtcNow : post.PublishedDate;

                post.UpdatedAt = DateTime.UtcNow;



                await _context.SaveChangesAsync();



                return Ok(new

                {

                    message = isPublished ? "Post published successfully" : "Post unpublished successfully",

                    isPublished = post.IsPublished,

                    publishedDate = post.PublishedDate

                });

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { message = "An error occurred while updating the publish status", error = ex.Message });

            }

        }


        [HttpGet("posts/categories")]

        public async Task<IActionResult> GetBlogCategories()

        {

            var categories = await _context.BlogCategories

              .OrderBy(c => c.Name)

              .Select(c => new BlogCategoryModel

              {

                  CategoryId = c.CategoryId,

                  Name = c.Name,

                  Slug = c.Slug,

                  Description = c.Description

              })

              .ToListAsync();



            return Ok(categories);

        }
        [HttpGet("test-drives")]
        public async Task<IActionResult> GetBookingsForSeller()
        {
            var sellerId = GetUserId();
            if (sellerId == null)
            {
                return Unauthorized("Seller is not authenticated.");
            }

            // Lấy danh sách các StoreLocationId mà seller này quản lý
            var managedStoreLocationIds = await _context.StoreLocations
                .Where(sl => sl.Users.Any(u => u.UserId == sellerId))
                .Select(sl => sl.StoreLocationId)
                .ToListAsync();

            if (!managedStoreLocationIds.Any())
            {
                return Ok(new List<object>()); // Trả về danh sách rỗng nếu seller không quản lý showroom nào
            }

            // Lấy tất cả các lịch hẹn thuộc các showroom mà seller quản lý
            var bookings = await _context.TestDriveBookings
                .Include(b => b.User) // Thông tin khách hàng đặt lịch
                .Include(b => b.StoreListing)
                    .ThenInclude(sl => sl.StoreLocation)
                .Include(b => b.StoreListing)
                    .ThenInclude(sl => sl.CarListing)
                        .ThenInclude(cl => cl.Model)
                            .ThenInclude(m => m.CarManufacturer)
                .Where(b => managedStoreLocationIds.Contains(b.StoreListing.StoreLocationId))
                .OrderByDescending(b => b.BookingDate)
                .Select(b => new
                {
                    // Thông tin lịch hẹn
                    b.BookingId,
                    b.BookingDate,
                    b.Status,
                    b.Notes,
                    b.HasLicense,

                    // Thông tin khách hàng
                    Customer = new
                    {
                        b.User.UserId,
                        b.User.FullName,
                        b.User.Email,
                        b.User.Mobile
                    },

                    // Thông tin xe
                    Car = new
                    {
                        b.StoreListing.CarListing.ListingId,
                        ManufacturerName = b.StoreListing.CarListing.Model.CarManufacturer.Name,
                        ModelName = b.StoreListing.CarListing.Model.Name,
                        b.StoreListing.CarListing.Year
                    },

                    // Thông tin showroom
                    Showroom = new
                    {
                        ShowroomName = b.StoreListing.StoreLocation.Name,
                        b.StoreListing.StoreLocation.Address
                    }
                })
                .ToListAsync();

            return Ok(bookings);
        }

        // === API 2: LẤY CHI TIẾT MỘT LỊCH HẸN ===
        [HttpGet("test-drives/{id}")]
        public async Task<IActionResult> GetBookingDetailForSeller(int id)
        {
            var sellerId = GetUserId();
            if (sellerId == null)
            {
                return Unauthorized("Seller is not authenticated.");
            }

            var booking = await _context.TestDriveBookings
                .Include(b => b.User)
                .Include(b => b.StoreListing)
                    .ThenInclude(sl => sl.StoreLocation)
                        .ThenInclude(loc => loc.Users)
                .Include(b => b.StoreListing)
                    .ThenInclude(sl => sl.CarListing)
                        .ThenInclude(cl => cl.Model)
                            .ThenInclude(m => m.CarManufacturer)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound("Booking not found.");
            }

            // === KIỂM TRA QUYỀN: Đảm bảo seller này có quyền xem lịch hẹn này ===
            bool isAuthorized = booking.StoreListing.StoreLocation.Users.Any(u => u.UserId == sellerId);
            if (!isAuthorized)
            {
                return Forbid("You do not have permission to view this booking.");
            }

            var bookingDetail = new
            {
                booking.BookingId,
                booking.BookingDate,
                booking.Status,
                booking.Notes,
                booking.HasLicense,
                Customer = new { booking.User.UserId, booking.User.FullName, booking.User.Email, booking.User.Mobile },
                Car = new
                {
                    booking.StoreListing.CarListing.ListingId,
                    ManufacturerName = booking.StoreListing.CarListing.Model.CarManufacturer.Name,
                    ModelName = booking.StoreListing.CarListing.Model.Name,
                    booking.StoreListing.CarListing.Year,
                    booking.StoreListing.CarListing.Price,
                    booking.StoreListing.CarListing.Mileage,
                    booking.StoreListing.CarListing.Condition,
                },
                Showroom = new
                {
                    ShowroomName = booking.StoreListing.StoreLocation.Name,
                    booking.StoreListing.StoreLocation.Address
                }
            };

            return Ok(bookingDetail);
        }

        // === API 3: CẬP NHẬT TRẠNG THÁI LỊCH HẸN ===
        [HttpPut("test-drives/{id}/status")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusDto statusDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sellerId = GetUserId();
            if (sellerId == null)
            {
                return Unauthorized("Seller is not authenticated.");
            }

            // Các trạng thái hợp lệ mà seller có thể cập nhật
            var allowedStatuses = new[] { "Confirmed", "Completed", "Canceled" };
            if (!allowedStatuses.Contains(statusDto.Status))
            {
                return BadRequest("Invalid status value.");
            }

            var booking = await _context.TestDriveBookings
                .Include(b => b.StoreListing)
                    .ThenInclude(sl => sl.StoreLocation)
                        .ThenInclude(loc => loc.Users)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound("Booking not found.");
            }

            // KIỂM TRA QUYỀN
            bool isAuthorized = booking.StoreListing.StoreLocation.Users.Any(u => u.UserId == sellerId);
            if (!isAuthorized)
            {
                return Forbid("You do not have permission to update this booking.");
            }

            // Cập nhật trạng thái và lưu vào DB
            booking.Status = statusDto.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Booking status successfully updated to '{statusDto.Status}'." });
        }
        public class UpdateBookingStatusDto
        {
            [Required]
            public string Status { get; set; }
        }

        [HttpGet("posts/tags")]

        public async Task<IActionResult> GetBlogTags()

        {

            var tags = await _context.BlogTags

              .OrderBy(t => t.Name)

              .Select(t => new BlogTagModel

              {

                  TagId = t.TagId,

                  Name = t.Name,

                  Slug = t.Slug

              })

              .ToListAsync();



            return Ok(tags);

        }

    }


}
public class BlogPostCreateModel

{

    [Required]

    public string Title { get; set; }



    [Required]

    public string Slug { get; set; }



    [Required]

    public string Content { get; set; }



    public string? Excerpt { get; set; }

    public string? FeaturedImage { get; set; }

    public bool IsPublished { get; set; } = false;

    public int CategoryId { get; set; }

    public List<string> NewTagNames { get; set; }

    public List<int>? TagIds { get; set; }

}
public class BlogPostUpdateModel

{

    public string? Title { get; set; }

    public string? Slug { get; set; }

    public string? Content { get; set; }

    public string? Excerpt { get; set; }

    public string? FeaturedImage { get; set; }

    public bool? IsPublished { get; set; }

    public int? CategoryId { get; set; }

    public List<int>? TagIds { get; set; }

    public List<string> NewTagNames { get; set; }

}

public class BlogCategoryModel

{

    public int CategoryId { get; set; }

    public string Name { get; set; }

    public string? Slug { get; set; }

    public string? Description { get; set; }

}



public class BlogTagModel

{

    public int TagId { get; set; }

    public string Name { get; set; }

    public string? Slug { get; set; }

}
public class ChangePasswordDto
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}
