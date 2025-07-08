using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSaleDN.Models;
using Microsoft.AspNetCore.Authorization;

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

        // 3. Quản lý cửa hàng
        [HttpGet("store")]
        public async Task<IActionResult> GetStore()
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var store = await _context.StoreLocations.FirstOrDefaultAsync(s => s.UserId == userId);
            return store == null ? NotFound() : Ok(store);
        }

        [HttpPut("store")]
        public async Task<IActionResult> UpdateStore([FromBody] StoreLocation model)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var store = await _context.StoreLocations.FirstOrDefaultAsync(s => s.UserId == userId);
            if (store == null) return NotFound();
            store.Name = model.Name;
            store.Address = model.Address;
            store.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Store updated" });
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
            car.Location = model.Location;
            car.Condition = model.Condition;
            car.RentSell = model.RentSell;
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

        // 5. Quản lý đơn hàng liên quan đến xe của mình
        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders()
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var orders = await _context.CarSales
                .Include(s => s.Listing)
                .Where(s => s.Listing.UserId == userId)
                .ToListAsync();
            return Ok(orders);
        }

        [HttpPut("orders/{id}/accept")]
        public async Task<IActionResult> AcceptOrder(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var sale = await _context.CarSales.Include(s => s.Listing)
                .FirstOrDefaultAsync(s => s.SaleId == id && s.Listing.UserId == userId);
            if (sale == null) return NotFound();
            sale.SaleStatusId = 1;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Order accepted" });
        }

        [HttpPut("orders/{id}/reject")]
        public async Task<IActionResult> RejectOrder(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var sale = await _context.CarSales.Include(s => s.Listing)
                .FirstOrDefaultAsync(s => s.SaleId == id && s.Listing.UserId == userId);
            if (sale == null) return NotFound();
            sale.SaleStatusId = 3;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Order rejected" });
        }

        // 6. Quản lý kho xe của mình
        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory()
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var inventory = await _context.CarInventories
                .Where(i => _context.CarListings.Any(c => c.UserId == userId && c.ModelId == i.ModelId))
                .ToListAsync();
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