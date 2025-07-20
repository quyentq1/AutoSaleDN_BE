using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSaleDN.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using AutoSaleDN.DTO;
using System.Reflection;
using OfficeOpenXml;

namespace AutoSaleDN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ColorsController : ControllerBase
    {
        private readonly AutoSaleDbContext _context;
        public ColorsController(AutoSaleDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CarColor>>> GetCarColors()
        {
            return await _context.CarColors.ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<CarColor>> GetCarColor(int id)
        {
            var carColor = await _context.CarColors.FindAsync(id);

            if (carColor == null)
            {
                return NotFound(new { message = $"Color with ID {id} not found." });
            }

            return carColor;
        }
        [HttpPost]
        public async Task<ActionResult<CarColor>> PostCarFeature(CarColor carColor)
        {
            // Basic validation: ensure FeatureName is not null or empty
            if (string.IsNullOrWhiteSpace(carColor.Name))
            {
                return BadRequest(new { message = "Feature name cannot be empty." });
            }

            // Optional: Check if a feature with the same name already exists to prevent duplicates
            if (await _context.CarColors.AnyAsync(f => f.Name == carColor.Name))
            {
                return Conflict(new { message = $"Color '{carColor.Name}' already exists." });
            }

            _context.CarColors.Add(carColor);
            await _context.SaveChangesAsync();

            // Return 201 Created status with the newly created resource
            return CreatedAtAction(nameof(GetCarColor), new { id = carColor.ColorId }, carColor);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCarColor(int id, CarColor carColor)
        {
            if (id != carColor.ColorId)
            {
                return BadRequest(new { message = "Color ID in URL does not match ID in body." });
            }

            // Basic validation
            if (string.IsNullOrWhiteSpace(carColor.Name))
            {
                return BadRequest(new { message = "Color name cannot be empty." });
            }

            // Optional: Check if another feature with the same name already exists
            if (await _context.CarColors.AnyAsync(f => f.Name == carColor.Name && f.ColorId != id))
            {
                return Conflict(new { message = $"Color '{carColor.Name}' already exists with a different ID." });
            }

            _context.Entry(carColor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarColorExists(id))
                {
                    return NotFound(new { message = $"Color with ID {id} not found." });
                }
                else
                {
                    throw; // Re-throw if it's not a "not found" concurrency issue
                }
            }

            return NoContent(); // 204 No Content for successful update
        }

        [HttpPut("{id}/toggle-status")]
        public async Task<IActionResult> ChangeStatusCarColor(int id, CarColorDto model)
        {
            var color = await _context.CarColors.FirstOrDefaultAsync(u => u.ColorId == id);

            if (color == null)
            {
                return NotFound($"Color with ID {id} not found.");
            }

            color.Status = model.Status.Value;

            try
            {
                await _context.SaveChangesAsync();
                string action = color.Status ? "activated" : "deactivated";
                return Ok(new { message = $"Color {action} successfully" });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("A concurrency error occurred. The Color might have been updated or deleted by another user.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private bool CarColorExists(int id)
        {
            return _context.CarColors.Any(e => e.ColorId == id);
        }
    }
}
