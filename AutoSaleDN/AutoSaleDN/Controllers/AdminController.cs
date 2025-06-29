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
                    u.Province,
                    u.CreatedAt,
                    u.UpdatedAt
                }).FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            // Get sales transaction history for this customer (as buyer)
            var transactions = await (
                from sale in _context.CarSales
                join listing in _context.CarListings on sale.ListingId equals listing.ListingId
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
                        listing.Location,
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
                    listing.Location,
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
                Location = dto.Location,
                Condition = dto.Condition,
                RentSell = dto.RentSell,
                Description = dto.Description,
                Certified = dto.Certified,
                Vin = dto.Vin,
                DatePosted = DateTime.Now,
                DateUpdated = DateTime.Now,
                ListingStatus = "Available"
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

            // 4. CarInventory
            var inventory = new CarInventory
            {
                ModelId = dto.ModelId,
                ColorId = dto.ColorId,
                QuantityImported = dto.QuantityImported,
                QuantityAvailable = dto.QuantityImported,
                QuantitySold = 0,
                ImportDate = dto.ImportDate,
                ImportPrice = dto.ImportPrice,
                Notes = dto.Notes
            };
            _context.CarInventories.Add(inventory);

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
        [HttpGet("transactions/{id}")]
        public async Task<ActionResult<object>> GetTransactionDetail(int id)
        {
            var transaction = await (
                from sale in _context.CarSales
                join listing in _context.CarListings on sale.ListingId equals listing.ListingId
                join model in _context.CarModels on listing.ModelId equals model.ModelId
                join manu in _context.CarManufacturers on model.ManufacturerId equals manu.ManufacturerId
                join status in _context.SaleStatus on sale.SaleStatusId equals status.SaleStatusId
                join customer in _context.Users on listing.UserId equals customer.UserId
                from spec in _context.CarSpecifications.Where(x => x.ListingId == listing.ListingId).DefaultIfEmpty()
                where sale.SaleId == id
                select new
                {
                    sale.SaleId,
                    sale.SaleDate,
                    sale.FinalPrice,
                    SaleStatus = status.StatusName,
                    CreatedAt = sale.CreatedAt,
                    UpdatedAt = sale.UpdatedAt,
                    Customer = new
                    {
                        customer.UserId,
                        customer.FullName,
                        customer.Email,
                        customer.Mobile,
                        customer.Role
                    },
                    Car = new
                    {
                        listing.ListingId,
                        Manufacturer = manu.Name,
                        Model = model.Name,
                        listing.Year,
                        listing.Mileage,
                        listing.Price,
                        listing.Location,
                        listing.Condition,
                        listing.RentSell,
                        Vin = listing.Vin,
                        Description = listing.Description,
                        Certified = listing.Certified,
                        Color = spec.ExteriorColor,
                        Transmission = spec.Transmission,
                        Images = _context.CarImages
                            .Where(img => img.ListingId == listing.ListingId)
                            .Select(img => img.Url)
                            .ToList()
                    }
                }
            ).FirstOrDefaultAsync();

            if (transaction == null)
                return NotFound();

            return Ok(transaction);
        }

        [HttpGet("cars/add-form-data")]
        public async Task<IActionResult> GetAddCarFormData()
        {
            var models = await _context.CarModels.Include(x => x.Manufacturer).ToListAsync();
            var colors = await _context.CarColors.ToListAsync();
            var features = await _context.CarFeatures.ToListAsync();
            return Ok(new
            {
                models = models.Select(m => new {
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
    }

}