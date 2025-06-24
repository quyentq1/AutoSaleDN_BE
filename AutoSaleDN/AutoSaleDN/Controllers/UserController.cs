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
            return BadRequest("Username or Email already exists.");

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
        return Ok(new { message = "Register successful" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var user = await _context.Users.SingleOrDefaultAsync(x => x.Email == model.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            return Unauthorized("Invalid Email or password.");

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

