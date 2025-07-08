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

        // 3. Địa chỉ giao hàng
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

        // 4. Xem, tìm kiếm, lọc xe
        [HttpGet("cars")]
        public async Task<IActionResult> GetCars([FromQuery] string? keyword = null)
        {
            var query = _context.CarListings.AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(c => c.Description.Contains(keyword) || c.Location.Contains(keyword));
            var cars = await query.ToListAsync();
            return Ok(cars);
        }

        // 5. Xem chi tiết xe
        [HttpGet("cars/{id}")]
        public async Task<IActionResult> GetCarDetail(int id)
        {
            var car = await _context.CarListings.FindAsync(id);
            return car == null ? NotFound() : Ok(car);
        }

        // 6. Đặt xe (tạo đơn hàng)
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

        // 7. Xem lịch sử đơn hàng, lọc theo trạng thái
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

        // 8. Xem chi tiết đơn hàng
        [HttpGet("orders/{id}")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var order = await _context.CarSales.FirstOrDefaultAsync(s => s.SaleId == id && s.CustomerId == userId);
            return order == null ? NotFound() : Ok(order);
        }

        // 9. Hủy đơn hàng
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

        // 10. Đánh giá xe
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

        // 11. Xem blog
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

        // 12. Chat (giả lập)
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
