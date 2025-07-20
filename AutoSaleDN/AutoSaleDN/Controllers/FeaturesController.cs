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
    public class FeaturesController : ControllerBase
    {
        private readonly AutoSaleDbContext _context;
        public FeaturesController(AutoSaleDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CarFeature>>> GetCarFeatures()
        {
            return await _context.CarFeatures.ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<CarFeature>> GetCarFeature(int id)
        {
            var carFeature = await _context.CarFeatures.FindAsync(id);

            if (carFeature == null)
            {
                return NotFound(new { message = $"Feature with ID {id} not found." });
            }

            return carFeature;
        }
        [HttpPost]
        public async Task<ActionResult<CarFeature>> PostCarFeature(CarFeature carFeature)
        {
            // Basic validation: ensure FeatureName is not null or empty
            if (string.IsNullOrWhiteSpace(carFeature.Name))
            {
                return BadRequest(new { message = "Feature name cannot be empty." });
            }

            // Optional: Check if a feature with the same name already exists to prevent duplicates
            if (await _context.CarFeatures.AnyAsync(f => f.Name == carFeature.Name))
            {
                return Conflict(new { message = $"Feature '{carFeature.Name}' already exists." });
            }

            _context.CarFeatures.Add(carFeature);
            await _context.SaveChangesAsync();

            // Return 201 Created status with the newly created resource
            return CreatedAtAction(nameof(GetCarFeature), new { id = carFeature.FeatureId }, carFeature);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCarFeature(int id, CarFeature carFeature)
        {
            if (id != carFeature.FeatureId)
            {
                return BadRequest(new { message = "Feature ID in URL does not match ID in body." });
            }

            // Basic validation
            if (string.IsNullOrWhiteSpace(carFeature.Name))
            {
                return BadRequest(new { message = "Feature name cannot be empty." });
            }

            // Optional: Check if another feature with the same name already exists
            if (await _context.CarFeatures.AnyAsync(f => f.Name == carFeature.Name && f.FeatureId != id))
            {
                return Conflict(new { message = $"Feature '{carFeature.Name}' already exists with a different ID." });
            }

            _context.Entry(carFeature).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarFeatureExists(id))
                {
                    return NotFound(new { message = $"Feature with ID {id} not found." });
                }
                else
                {
                    throw; // Re-throw if it's not a "not found" concurrency issue
                }
            }

            return NoContent(); // 204 No Content for successful update
        }

        [HttpPut("{id}/toggle-status")]
        public async Task<IActionResult> ChangeStatusCarFeature(int id, CarFeatureDto model)
        {
            var feature = await _context.CarFeatures.FirstOrDefaultAsync(u => u.FeatureId == id);

            if (feature == null)
            {
                return NotFound($"Customer with ID {id} not found.");
            }

            feature.Status = model.Status.Value;

            try
            {
                await _context.SaveChangesAsync();
                string action = feature.Status ? "activated" : "deactivated";
                return Ok(new { message = $"CarFeatures {action} successfully" });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("A concurrency error occurred. The CarFeatures might have been updated or deleted by another user.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private bool CarFeatureExists(int id)
        {
            return _context.CarFeatures.Any(e => e.FeatureId == id);
        }
    }
}
