using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoSaleDN.Models;
using BCrypt.Net;
using static AutoSaleDN.DTO.Auth;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly AutoSaleDbContext _context;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;

    public UserController(AutoSaleDbContext context, IConfiguration config, IMemoryCache cache)
    {
        _context = context;
        _config = config;
        _cache = cache;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        if (await _context.Users.AnyAsync(x => x.Name == model.Name || x.Email == model.Email))
            return BadRequest("Username or email already exists.");

        var user = new User
        {
            Name = model.Name,
            Email = model.Email,
            FullName = model.FullName,
            Mobile = model.Mobile,
            Role = model.Role,
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Province = model.Province
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Registration successful" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var user = await _context.Users.SingleOrDefaultAsync(x => x.Email == model.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
        {
            return Unauthorized("Invalid email or password.");
        }
        if (!user.Status)
        {
            return Unauthorized("Your account has been deactivated by the administrator.");
        }

        var token = GenerateJwtToken(user);
        return Ok(new
        {
            token,
            role = user.Role
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users
            .Where(u => u.UserId == Int32.Parse(userId))
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                Name = u.Name,
                Email = u.Email,
                FullName = u.FullName,
                Mobile = u.Mobile,
                Province = u.Province,
                Role = u.Role
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost("forgotpassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
    {
        var user = await _context.Users.SingleOrDefaultAsync(x => x.Email == model.Email);
        if (user == null)
            return NotFound("Email not found.");

        var otp = new Random().Next(100_000, 999_999).ToString();

        _cache.Set(
            $"reset_otp_{user.Email}",
            otp,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            }
        );

        await SendEmailAsync(
            user.Email,
            "Your Password Reset Code",
            $"Your password reset code is: {otp}. It expires in 10 minutes."
        );

        return Ok(new { message = "Reset code sent to your email." });
    }

    [HttpPost("verify-reset-otp")]
    public IActionResult VerifyResetOtp([FromBody] VerifyOtpDto model)
    {
        if (!_cache.TryGetValue($"reset_otp_{model.Email}", out string cachedOtp) || cachedOtp != model.Otp)
        {
            return BadRequest("Invalid or expired code.");
        }
        return Ok(new { message = "OTP valid, you can reset password now." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
    {
        if (!_cache.TryGetValue($"reset_otp_{model.Email}", out string cachedOtp) || cachedOtp != model.Otp)
        {
            return BadRequest("Invalid or expired code.");
        }

        var user = await _context.Users.SingleOrDefaultAsync(x => x.Email == model.Email);
        if (user == null)
            return NotFound("Email not found.");

        user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        await _context.SaveChangesAsync();

        _cache.Remove($"reset_otp_{model.Email}");

        return Ok(new { message = "Password reset successful." });
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
        [FromQuery] string? color = null,
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
            .Include(c => c.StoreListings)
                .ThenInclude(cs => cs.StoreLocation)
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
            // Implement discount logic if applicable
        }
        if (premiumPartners.HasValue && premiumPartners.Value)
        {
            // Implement premium partner logic if applicable
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
        if (!string.IsNullOrEmpty(color))
        {
            query = query.Where(c => c.Specifications.Any(s => s.ExteriorColor != null && s.InteriorColor != null));
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
                c.Description,
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
                    s.CarType
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
                Pricing = c.CarPricingDetails != null ? c.CarPricingDetails.Select(shh => new
                {
                    shh.TaxRate,
                    shh.RegistrationFee
                }).ToList() : null,
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
                }) : null,
                Showrooms = c.StoreListings != null ? c.StoreListings.Select(cs => new
                {
                    cs.StoreLocation.StoreLocationId,
                    cs.StoreLocation.Name,
                    cs.StoreLocation.Address,
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
            .Include(c => c.StoreListings)
                .ThenInclude(cs => cs.StoreLocation)
            .FirstOrDefaultAsync(c => c.ListingId == id);

        if (car == null)
            return NotFound(new { message = "Car not found." });

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
            car.Description,
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
            }) : null,
            Showrooms = car.StoreListings != null ? car.StoreListings.Select(cs => new
            {
                cs.StoreLocation.StoreLocationId,
                cs.StoreLocation.Name,
                cs.StoreLocation.Address,
            }) : null
        };

        return Ok(carDetail);
    }

    [HttpGet("cars/{id}/similar")]
    public async Task<IActionResult> GetSimilarCars(int id)
    {
        var car = await _context.CarListings
            .Include(c => c.Model)
                .ThenInclude(m => m.Manufacturer)
            .FirstOrDefaultAsync(c => c.ListingId == id);

        if (car == null)
            return NotFound(new { message = "Car not found." });

        var similarCars = await _context.CarListings
            .Include(c => c.Model)
                .ThenInclude(m => m.Manufacturer)
            .Include(c => c.Specifications)
            .Include(c => c.CarImages)
            .Include(c => c.CarListingFeatures)
                .ThenInclude(clf => clf.Feature)
            .Where(c => c.Model.ManufacturerId == car.Model.ManufacturerId && c.ListingId != id)
            .Take(3)
            .Select(c => new
            {
                c.ListingId,
                Name = $"{c.Model.Manufacturer.Name} {c.Model.Name}",
                Image = c.CarImages != null ? c.CarImages.Select(i => i.Url).FirstOrDefault() : null,
                c.Price,
                Details = c.Specifications != null ? c.Specifications.Select(s => new
                {
                    s.Engine,
                    s.Transmission,
                    s.FuelType
                }).FirstOrDefault() : null,
                Tags = c.CarListingFeatures != null ? c.CarListingFeatures.Select(f => f.Feature.Name).Take(2).ToList() : new List<string>()
            })
            .ToListAsync();

        return Ok(similarCars);
    }


    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var emailSettings = _config.GetSection("EmailSettings");
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(emailSettings["SenderName"], emailSettings["SenderEmail"]));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;
        email.Body = new TextPart("plain") { Text = body };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["Port"]), false);
        await smtp.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["SenderPassword"]);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}

public class TestDriveRequestDto
{
    public int ShowroomId { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public DateTime PreferredDate { get; set; }
}

public class TestDrive
{
    public int TestDriveId { get; set; }
    public int CarListingId { get; set; }
    public int ShowroomId { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public DateTime PreferredDate { get; set; }
    public DateTime CreatedAt { get; set; }
}