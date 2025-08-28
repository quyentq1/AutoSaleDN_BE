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
using System.Globalization;
using System.Text.Json;

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
        if (await _context.Users.AnyAsync(x => x.Email == model.Email))
            return BadRequest("Email already exists.");

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
            Province = model.Province,
            Status = true
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
    [FromQuery] string? fuelType = null,
    [FromQuery] string? powerUnit = null,
    [FromQuery] double? powerFrom = null,
    [FromQuery] double? powerTo = null,
    [FromQuery] string? vehicleType = null,
    [FromQuery] bool? driveType4x4 = null,
    [FromQuery] string? color = null,
    [FromQuery] List<string>? features = null,
    [FromQuery] string? sortBy = null,
    [FromQuery] int page = 1,
    [FromQuery] int perPage = 5 
)
{
    try
    {
            var query = _context.CarListings.AsQueryable();

            // Apply filters with null checks
            if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(c =>
                (c.Model != null && c.Model.Name.Contains(keyword)) ||
                (c.Model != null && c.Model.CarManufacturer != null && c.Model.CarManufacturer.Name.Contains(keyword)) ||
                (c.Description != null && c.Description.Contains(keyword)) ||
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
            query = query.Where(c => c.CarPricingDetails != null && c.CarPricingDetails.Any());
        }

        // TODO: Implement discountedCars filter
        if (discountedCars.HasValue && discountedCars.Value)
        {
            // Add your discount logic here
        }

        // TODO: Implement premiumPartners filter  
        if (premiumPartners.HasValue && premiumPartners.Value)
        {
            // Add your premium partners logic here
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
            query = query.Where(c => c.Specifications != null && 
                c.Specifications.Any(s => s.Transmission == transmission));
        }

        if (!string.IsNullOrEmpty(fuelType))
        {
            query = query.Where(c => c.Specifications != null && 
                c.Specifications.Any(s => s.FuelType == fuelType));
        }

        if (!string.IsNullOrEmpty(vehicleType))
        {
            query = query.Where(c => c.Specifications != null && 
                c.Specifications.Any(s => s.CarType == vehicleType));
        }

        if (driveType4x4.HasValue && driveType4x4.Value)
        {
            query = query.Where(c => c.CarListingFeatures != null && 
                c.CarListingFeatures.Any(clf => clf.Feature != null && clf.Feature.Name == "4x4"));
        }

        // Fix color filter logic
        if (!string.IsNullOrEmpty(color))
        {
            query = query.Where(c => c.Specifications != null && 
                c.Specifications.Any(s => 
                    (s.ExteriorColor != null && s.ExteriorColor.ToLower().Contains(color.ToLower())) ||
                    (s.InteriorColor != null && s.InteriorColor.ToLower().Contains(color.ToLower()))
                ));
        }

        if (features != null && features.Any())
        {
            foreach (var featureName in features)
            {
                var currentFeature = featureName; // Capture for closure
                query = query.Where(c => c.CarListingFeatures != null && 
                    c.CarListingFeatures.Any(clf => clf.Feature != null && clf.Feature.Name == currentFeature));
            }
        }

            query = query.Where(c => !c.StoreListings.SelectMany(sl => sl.CarSales)
                                      .Any(cs => cs.SaleStatus.StatusName == "Payment Complete" || cs.SaleStatus.StatusName == "Sold"));

            var availableStatuses = new List<string> { "Available", "Pending Deposit" };

            var orderedQuery = query.OrderByDescending(c =>
                availableStatuses.Contains(
                    c.StoreListings
                     .SelectMany(sl => sl.CarSales)
                     .OrderByDescending(cs => cs.CreatedAt)
                     .Select(cs => cs.SaleStatus.StatusName)
                     .FirstOrDefault() ?? "Available"
                )
            );

            switch (sortBy?.ToLower())
            {
                case "price_asc":
                    orderedQuery = orderedQuery.ThenBy(c => c.Price);
                    break;
                case "price_desc":
                    orderedQuery = orderedQuery.ThenByDescending(c => c.Price);
                    break;
                case "year_desc":
                    orderedQuery = orderedQuery.ThenByDescending(c => c.Year);
                    break;
                case "year_asc":
                    orderedQuery = orderedQuery.ThenBy(c => c.Year);
                    break;
                case "mileage_desc":
                    orderedQuery = orderedQuery.ThenByDescending(c => c.Mileage);
                    break;
                case "mileage_asc":
                    orderedQuery = orderedQuery.ThenBy(c => c.Mileage);
                    break;
                default:
                    orderedQuery = orderedQuery.ThenByDescending(c => c.DatePosted);
                    break;
            }

            var totalResults = await orderedQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalResults / (double)perPage);

            var cars = await orderedQuery
                .Skip((page - 1) * perPage)
                .Take(perPage)
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
                            c.Model.CarManufacturer.ManufacturerId,
                            c.Model.CarManufacturer.Name
                        }
                    },
                    Specifications = c.Specifications.Select(s => new {
                        s.SpecificationId,
                        s.Engine,
                        s.Transmission,
                        s.FuelType,
                        s.SeatingCapacity,
                        s.CarType
                    }).ToList(),
                    Images = c.CarImages.Select(i => new { i.ImageId, i.Url, i.Filename }).ToList(),
                    Features = c.CarListingFeatures.Select(f => new { f.Feature.FeatureId, f.Feature.Name }).ToList(),
                    ServiceHistory = c.CarServiceHistories.Select(sh => new { sh.RecentServicing, sh.NoAccidentHistory, sh.Modifications }).ToList(),
                    Pricing = c.CarPricingDetails.Select(p => new { p.TaxRate, p.RegistrationFee }).ToList(),
                    SalesHistory = c.CarSales.Select(s => new { s.SaleId, s.FinalPrice, s.SaleDate, SaleStatus = s.SaleStatus.StatusName }).ToList(),
                    Reviews = c.Reviews.Select(r => new { r.ReviewId, r.UserId, r.Rating, r.User.FullName, r.CreatedAt }).ToList(),
                    Showrooms = c.StoreListings.Select(cs => new { cs.StoreLocation.StoreLocationId, cs.StoreLocation.Name, cs.StoreLocation.Address }).ToList(),
                    CurrentSaleStatus = c.StoreListings
                                         .SelectMany(sl => sl.CarSales)
                                         .OrderByDescending(s => s.CreatedAt)
                                         .Select(s => s.SaleStatus.StatusName)
                                         .FirstOrDefault() ?? "Available"
                })
                .ToListAsync();

            return Ok(new
            {
                cars,
                totalResults,
                totalPages,
                currentPage = page,
                perPage
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetCars: {ex.Message} \n {ex.StackTrace}");
            return StatusCode(500, new { message = "An error occurred while processing your request." });
        }
    }

    [HttpGet("cars/{id}")]
    public async Task<IActionResult> GetCarDetail(int id)
    {
        try
        {
            var car = await _context.CarListings
                .Include(c => c.Model)
                    .ThenInclude(m => m.CarManufacturer)
                .Include(c => c.Specifications)
                .Include(c => c.CarImages)
                .Include(c => c.CarVideos)
                .Include(c => c.CarListingFeatures)
                    .ThenInclude(clf => clf.Feature)
                .Include(c => c.CarServiceHistories)
                .Include(c => c.CarPricingDetails)
                .AsSplitQuery() 
                .FirstOrDefaultAsync(c => c.ListingId == id);

            if (car == null)
            {
                return NotFound(new { message = "Car not found." });
            }

            var showroomAndSalesInfo = await _context.StoreListings
                .Include(sl => sl.StoreLocation)
                    .ThenInclude(sloc => sloc.Users)
                .Include(sl => sl.CarSales)
                    .ThenInclude(cs => cs.SaleStatus)
                .Include(sl => sl.CarSales)
                    .ThenInclude(cs => cs.DepositPayment)
                .Include(sl => sl.CarSales)
                    .ThenInclude(cs => cs.FullPayment)
                .AsSplitQuery()
                .Where(sl => sl.ListingId == id)
                .ToListAsync();

            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ListingId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var firstShowroom = showroomAndSalesInfo.FirstOrDefault()?.StoreLocation;
            var seller = firstShowroom?.Users?.FirstOrDefault();

            var allCarSalesForThisCar = showroomAndSalesInfo.SelectMany(sl => sl.CarSales ?? new List<CarSale>()).ToList();
            var latestRelevantSale = allCarSalesForThisCar
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefault(s =>
                    s.SaleStatus?.StatusName == "Deposit Paid" ||
                    s.SaleStatus?.StatusName == "Payment Complete" ||
                    s.SaleStatus?.StatusName == "Pending Deposit" ||
                    s.SaleStatus?.StatusName == "Pending Full Payment"
                );

            string saleStatusDisplay = "Available";
            string paymentStatusDisplay = null;
            if (latestRelevantSale != null)
            {
                if (latestRelevantSale.SaleStatus?.StatusName == "Payment Complete")
                {
                    saleStatusDisplay = "Sold";
                    paymentStatusDisplay = "Full Payment Made";
                }
                else if (latestRelevantSale.SaleStatus?.StatusName == "Deposit Paid")
                {
                    saleStatusDisplay = "On Hold";
                    paymentStatusDisplay = "Deposit Made";
                }
                else if (latestRelevantSale.SaleStatus?.StatusName == "Pending Deposit")
                {
                    saleStatusDisplay = "Pending Deposit";
                    paymentStatusDisplay = "Pending Deposit Payment";
                }
                else if (latestRelevantSale.SaleStatus?.StatusName == "Pending Full Payment")
                {
                    saleStatusDisplay = "Pending Full Payment";
                    paymentStatusDisplay = "Pending Full Payment";
                }
            }

            var carDetail = new
            {
                car.ListingId,
                car.ModelId,
                UserId = seller?.UserId,
                SellerName = seller?.FullName,
                SellerEmail = seller?.Email,
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
                        car.Model.CarManufacturer.ManufacturerId,
                        car.Model.CarManufacturer.Name
                    }
                },
                Specification = car.Specifications?.Select(s => new
                {
                    s.SpecificationId,
                    s.Engine,
                    s.Transmission,
                    s.FuelType,
                    s.SeatingCapacity,
                    s.CarType
                }).ToList(),
                Images = car.CarImages?.Select(i => new { i.ImageId, i.Url, i.Filename }).ToList(),
                CarVideo = car.CarVideos?.Select(v => new { v.VideoId, v.Url, v.ListingId }).ToList(),
                Features = car.CarListingFeatures?.Select(f => new { f.Feature.FeatureId, f.Feature.Name }).ToList(),
                ServiceHistory = car.CarServiceHistories?.Select(sh => new { sh.RecentServicing, sh.NoAccidentHistory, sh.Modifications }).ToList(),
                Pricing = car.CarPricingDetails?.Select(p => new { p.TaxRate, p.RegistrationFee }).ToList(),
                SalesHistory = allCarSalesForThisCar.Select(s => new { s.SaleId, s.FinalPrice, s.SaleDate, s.SaleStatus.StatusName }).ToList(),
                Reviews = reviews.Select(r => new { r.ReviewId, r.UserId, r.Rating, r.User.FullName, r.CreatedAt }).ToList(),
                Showrooms = showroomAndSalesInfo.Select(sl => new
                {
                    sl.StoreListingId,
                    sl.StoreLocation.StoreLocationId,
                    sl.StoreLocation.Name,
                    sl.StoreLocation.Address
                }).ToList(),
                CurrentSaleStatus = saleStatusDisplay,
                CurrentPaymentStatus = paymentStatusDisplay
            };

            return Ok(carDetail);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An internal server error occurred: {ex.Message}");
        }
    }

    [HttpGet("cars/{id}/similar")]
    public async Task<IActionResult> GetSimilarCars(int id)
    {
        var car = await _context.CarListings
            .Select(c => new { c.ListingId, c.Model.ManufacturerId })
            .FirstOrDefaultAsync(c => c.ListingId == id);

        if (car == null)
        {
            return NotFound(new { message = "Car not found." });
        }

        var similarCars = await _context.CarListings
            .Where(c => c.Model.ManufacturerId == car.ManufacturerId && c.ListingId != id)
            .OrderBy(c => c.DatePosted)
            .Take(3)
            .Select(c => new
            {
                c.ListingId,
                Name = c.Model.CarManufacturer.Name + " " + c.Model.Name,
                Image = c.CarImages.Select(i => i.Url).FirstOrDefault(),
                c.Price,
                Details = c.Specifications.Select(s => new
                {
                    s.Engine,
                    s.Transmission,
                    s.FuelType
                }).FirstOrDefault(),
                Tags = c.CarListingFeatures.Select(f => f.Feature.Name).Take(2).ToList()
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
    [HttpGet("cars/years")]
    public async Task<IActionResult> GetDistinctRegistrationYears()
    {
        var years = await _context.CarListings
            .Select(c => c.Year)
            .Distinct()
            .OrderByDescending(y => y)
            .ToListAsync();
        return Ok(years);
    }
    [HttpGet("cars/features")]
    public async Task<IActionResult> GetDistinctFeatures()
    {
        var features = await _context.CarFeatures
            .Select(f => f.Name)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync();
        return Ok(features);
    }
    [HttpGet("cars/vehicle-types")]
    public async Task<IActionResult> GetDistinctVehicleTypes()
    {
        var vehicleTypes = await _context.CarListings
            .Where(c => c.Specifications.Any())
            .SelectMany(c => c.Specifications)
            .Select(s => s.CarType)
            .Where(type => type != null && type != "")
            .Distinct()
            .OrderBy(type => type)
            .ToListAsync();
        return Ok(vehicleTypes);
    }
    [HttpGet("cars/fuel-types")]
    public async Task<IActionResult> GetDistinctFuelTypes()
    {
        var fuelTypes = await _context.CarListings
            .Where(c => c.Specifications.Any())
            .SelectMany(c => c.Specifications)
            .Select(s => s.FuelType)
            .Where(type => type != null && type != "")
            .Distinct()
            .OrderBy(type => type)
            .ToListAsync();
        return Ok(fuelTypes);
    }

    [HttpGet("cars/mileage-ranges")]
    public async Task<IActionResult> GetMileageRanges()
    {
        var minMileage = await _context.CarListings
            .MinAsync(c => (int?)c.Mileage);
        var maxMileage = await _context.CarListings
            .MaxAsync(c => (int?)c.Mileage);

        if (!minMileage.HasValue || !maxMileage.HasValue)
        {
            return Ok(new List<object>());
        }

        var breakpoints = new List<int> { 10000, 50000, 100000, 150000 };
        breakpoints.Sort();

        var ranges = new List<object>();

        int currentMin = 0;

        if (minMileage.Value > 0)
        {
            ranges.Add(new { value = $"0-{minMileage.Value}", label = $"0 - {minMileage.Value:N0} km" });
            currentMin = minMileage.Value;
        }


        foreach (var bp in breakpoints)
        {
            if (currentMin < bp)
            {
                int rangeTo = Math.Min(bp, maxMileage.Value);
                if (currentMin < rangeTo)
                {
                    ranges.Add(new { value = $"{currentMin}-{rangeTo}", label = $"{currentMin:N0} - {rangeTo:N0} km" });
                }
            }
            currentMin = bp + 1;
        }

        if (maxMileage.Value >= currentMin)
        {
            ranges.Add(new { value = $"{currentMin}-max", label = $"Over {currentMin:N0} km" });
        }

        if (ranges.Count == 0 && minMileage.HasValue && maxMileage.HasValue)
        {
            ranges.Add(new { value = $"{minMileage.Value}-{maxMileage.Value}", label = $"{minMileage.Value:N0} - {maxMileage.Value:N0} km" });
        }


        return Ok(ranges);
    }

    [HttpGet("cars/price-ranges")]
    public async Task<IActionResult> GetPriceRanges()
    {
        var minPrice = await _context.CarListings
            .MinAsync(c => (decimal?)c.Price);
        var maxPrice = await _context.CarListings
            .MaxAsync(c => (decimal?)c.Price);

        if (!minPrice.HasValue || !maxPrice.HasValue)
        {
            return Ok(new List<object>());
        }

        var ranges = new List<object>();
        decimal currentMin = minPrice.Value;

        decimal[] potentialBreakpoints = new decimal[]
        {
        0m,
        50_000_000m,
        100_000_000m,
        200_000_000m,
        300_000_000m,
        500_000_000m,
        700_000_000m,
        1_000_000_000m,
        1_500_000_000m,
        2_000_000_000m,
        3_000_000_000m,
        5_000_000_000m,

        };

        var relevantBreakpoints = potentialBreakpoints
            .Where(bp => bp >= 0m && bp >= currentMin && bp <= maxPrice.Value)
            .OrderBy(bp => bp)
            .ToList();

        if (currentMin > 0m && (relevantBreakpoints.Count == 0 || relevantBreakpoints[0] > currentMin))
        {
            decimal firstRangeTo = relevantBreakpoints.Any() ? Math.Min(relevantBreakpoints[0], maxPrice.Value) : maxPrice.Value;
            if (0m < firstRangeTo)
            {
                ranges.Add(new { value = $"0-{firstRangeTo}", label = $"0 - {FormatCurrency(firstRangeTo)} VND" });
                currentMin = firstRangeTo + 1m;
            }
        }
        else if (currentMin == 0m && relevantBreakpoints.Any())
        {

            currentMin = 0m;
        }

        foreach (var bp in relevantBreakpoints)
        {
            if (currentMin < bp)
            {
                ranges.Add(new { value = $"{currentMin}-{bp}", label = $"{FormatCurrency(currentMin)} - {FormatCurrency(bp)} VND" });
                currentMin = bp + 1m;
            }
        }

        if (maxPrice.Value >= currentMin)
        {
            ranges.Add(new { value = $"{currentMin}-max", label = $"Above {FormatCurrency(currentMin)} VND" });
        }

        if (ranges.Count == 0 && minPrice.HasValue && maxPrice.HasValue)
        {
            ranges.Add(new { value = $"{minPrice.Value}-{maxPrice.Value}", label = $"{FormatCurrency(minPrice.Value)} - {FormatCurrency(maxPrice.Value)} VND" });
        }

        return Ok(ranges);
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

            var query = _context.BlogPosts

              .Include(p => p.Category)

              .Include(p => p.BlogPostTags)

                .ThenInclude(pt => pt.Tag)


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

    [HttpGet("reviews")]
    public async Task<IActionResult> GetAllReviews()
    {
        try
        {
            var reviews = await _context.Reviews
                .Include(r => r.User) // nếu có navigation property tới User
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var result = reviews.Select(r => new
            {
                SaleId = r.SaleId,
                rating = r.Rating,
                content = r.Content,
                images = string.IsNullOrEmpty(r.Reply)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(r.Reply) ?? new List<string>(),
                createdAt = r.CreatedAt,
                userName = r.User != null ? r.User.FullName : "Anonymous"
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }

    [HttpGet("posts/slug/{slug}")]
    public async Task<IActionResult> GetBlogPostBySlug(string slug)
    {
        try
        {
            var post = await _context.BlogPosts
                .Include(p => p.Category)
                .Include(p => p.BlogPostTags)
                    .ThenInclude(pt => pt.Tag)
                .Where(p => p.Slug == slug && p.IsPublished == true) // Lấy bài viết bằng slug và chỉ lấy bài đã publish
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

            if (post == null)
            {
                return NotFound(new { message = "Blog post not found" });
            }

            // Tăng ViewCount nếu bạn muốn theo dõi lượt xem
            // post.ViewCount = (post.ViewCount ?? 0) + 1;
            // await _context.SaveChangesAsync();

            return Ok(post);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the blog post", error = ex.Message });
        }


    }



    private string FormatCurrency(decimal amount)
    {
        if (amount >= 1_000_000_000m)
        {
            decimal billion = amount / 1_000_000_000m;
            return $"{billion:0.#} billion";
        }
        else if (amount >= 1_000_000m)
        {
            decimal million = amount / 1_000_000m;
            return $"{million:0.#} million";
        }
        else if (amount >= 1_000m)
        {
            decimal thousand = amount / 1_000m;
            return $"{thousand:0.#} thousand";
        }

        return amount.ToString("N0", CultureInfo.InvariantCulture);
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