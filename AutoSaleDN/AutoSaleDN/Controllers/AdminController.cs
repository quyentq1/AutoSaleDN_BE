using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSaleDN.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;

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
                .Select(u => new {
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
                .Select(u => new {
                    u.UserId,
                    u.Name,
                    u.Email,
                    u.FullName,
                    u.Mobile,
                    u.Role,
                    u.CreatedAt,
                    u.UpdatedAt
                }).FirstOrDefaultAsync();

            if (user == null)
                return NotFound();
            return Ok(user);
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
        public async Task<ActionResult<IEnumerable<object>>> GetCars()
        {
            var cars = await _context.CarListings
                .Select(c => new {
                    c.ListingId,
                    c.ModelId,
                    c.UserId,
                    c.Year,
                    c.Mileage,
                    c.Price,
                    c.Location,
                    c.Condition,
                    c.RentSell,
                    c.ListingStatus,
                    c.DatePosted,
                    c.DateUpdated
                }).ToListAsync();
            return Ok(cars);
        }

        [HttpGet("cars/{id}")]
        public async Task<ActionResult<object>> GetCar(int id)
        {
            var car = await _context.CarListings
                .Where(c => c.ListingId == id)
                .Select(c => new {
                    c.ListingId,
                    c.ModelId,
                    c.UserId,
                    c.Year,
                    c.Mileage,
                    c.Price,
                    c.Location,
                    c.Condition,
                    c.RentSell,
                    c.ListingStatus,
                    c.DatePosted,
                    c.DateUpdated
                }).FirstOrDefaultAsync();

            if (car == null)
                return NotFound();
            return Ok(car);
        }

        [HttpPost("cars")]
        public async Task<ActionResult> CreateCar([FromBody] CarListing model)
        {
            var car = new CarListing
            {
                ModelId = model.ModelId,
                UserId = model.UserId,
                Year = model.Year,
                Mileage = model.Mileage,
                Price = model.Price,
                Location = model.Location,
                Condition = model.Condition,
                RentSell = model.RentSell,
                ListingStatus = model.ListingStatus ?? "active",
                DatePosted = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                Certified = model.Certified,
                Vin = model.Vin,
                Description = model.Description
            };
            _context.CarListings.Add(car);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Car created successfully" });
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
            car.Location = model.Location;
            car.Condition = model.Condition;
            car.RentSell = model.RentSell;
            car.ListingStatus = model.ListingStatus ?? car.ListingStatus;
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
    }
}