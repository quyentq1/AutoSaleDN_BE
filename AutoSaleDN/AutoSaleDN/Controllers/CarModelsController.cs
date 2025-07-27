using AutoSaleDN.DTO;
using AutoSaleDN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AutoSaleDN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requires authentication for all actions in this controller
    public class CarModelsController : ControllerBase
    {
        private readonly AutoSaleDbContext _context;

        public CarModelsController(AutoSaleDbContext context)
        {
            _context = context;
        }

        // GET: api/CarModels
        // Includes Manufacturer data for richer client-side display
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CarModel>>> GetCarModels()
        {
            if (_context.CarModels == null)
            {
                return NotFound("No car models found.");
            }
            return await _context.CarModels
                                 .Include(cm => cm.CarManufacturer) // Eager load the Manufacturer data
                                 .ToListAsync();
        }

        // GET: api/CarModels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CarModel>> GetCarModel(int id)
        {
            if (_context.CarModels == null)
            {
                return NotFound();
            }
            var carModel = await _context.CarModels
                                         .Include(cm => cm.CarManufacturer) // Eager load the Manufacturer data
                                         .FirstOrDefaultAsync(cm => cm.ModelId == id);

            if (carModel == null)
            {
                return NotFound();
            }

            return carModel;
        }

        // GET: api/CarManufacturers/{manufacturerId}/Models - Optional, if you want to get models by manufacturer
        [HttpGet("~/api/CarManufacturers/{manufacturerId}/Models")]
        public async Task<ActionResult<IEnumerable<CarModel>>> GetModelsByManufacturer(int manufacturerId)
        {
            if (_context.CarModels == null)
            {
                return NotFound();
            }

            var models = await _context.CarModels
                                        .Where(cm => cm.ManufacturerId == manufacturerId)
                                        .Include(cm => cm.CarManufacturer)
                                        .ToListAsync();

            if (!models.Any() && !await _context.CarManufacturers.AnyAsync(m => m.ManufacturerId == manufacturerId))
            {
                return NotFound($"Manufacturer with ID {manufacturerId} not found or has no models.");
            }

            return models;
        }

        // POST: api/CarModels
        [HttpPost]
        public async Task<ActionResult<CarModel>> PostCarModel(CarModel carModel)
        {
            if (_context.CarModels == null)
            {
                return Problem("Entity set 'ApplicationDbContext.CarModels' is null.");
            }

            // Validate that the Manufacturer exists
            var manufacturerExists = await _context.CarManufacturers.AnyAsync(m => m.ManufacturerId == carModel.ManufacturerId);
            if (!manufacturerExists)
            {
                return BadRequest($"Manufacturer with ID {carModel.ManufacturerId} does not exist.");
            }

            // Basic validation for name uniqueness under the same manufacturer (case-insensitive)
            if (await _context.CarModels.AnyAsync(m => m.ManufacturerId == carModel.ManufacturerId && m.Name.ToLower() == carModel.Name.ToLower()))
            {
                return Conflict("A model with this name already exists for this manufacturer.");
            }

            _context.CarModels.Add(carModel);
            await _context.SaveChangesAsync();

            // Reload the model with Manufacturer data before returning
            await _context.Entry(carModel).Reference(cm => cm.CarManufacturer).LoadAsync();

            return CreatedAtAction("GetCarModel", new { id = carModel.ModelId }, carModel);
        }

        // PUT: api/CarModels/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCarModel(int id, CarModel carModel)
        {
            if (id != carModel.ModelId)
            {
                return BadRequest("Model ID mismatch.");
            }

            // Ensure the ManufacturerId is not changed or is valid if changed (though typically not changed in frontend)
            if (!await _context.CarManufacturers.AnyAsync(m => m.ManufacturerId == carModel.ManufacturerId))
            {
                return BadRequest($"Manufacturer with ID {carModel.ManufacturerId} does not exist.");
            }

            // Basic validation for name uniqueness under the same manufacturer (case-insensitive)
            if (await _context.CarModels.AnyAsync(m => m.ManufacturerId == carModel.ManufacturerId && m.Name.ToLower() == carModel.Name.ToLower() && m.ModelId != id))
            {
                return Conflict("A model with this name already exists for this manufacturer.");
            }

            _context.Entry(carModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarModelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating model: {ex.Message}");
            }

            return NoContent();
        }

        // PUT: api/CarModels/5/toggle-status
        [HttpPut("{id}/toggle-status")]
        public async Task<IActionResult> ToggleCarModelStatus(int id, [FromBody] CarModelStatusUpdateDto statusUpdate)
        {
            var carModel = await _context.CarModels.FindAsync(id);

            if (carModel == null)
            {
                return NotFound($"Model with ID {id} not found.");
            }

            // Validate new status value
            if (statusUpdate.Status != "Active" && statusUpdate.Status != "Inactive")
            {
                return BadRequest("Invalid status value. Must be 'Active' or 'Inactive'.");
            }

            carModel.Status = statusUpdate.Status;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarModelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error toggling model status: {ex.Message}");
            }

            return NoContent();
        }

        // DELETE: api/CarModels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCarModel(int id)
        {
            if (_context.CarModels == null)
            {
                return NotFound();
            }
            var carModel = await _context.CarModels.FindAsync(id);
            if (carModel == null)
            {
                return NotFound();
            }

            _context.CarModels.Remove(carModel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CarModelExists(int id)
        {
            return (_context.CarModels?.Any(e => e.ModelId == id)).GetValueOrDefault();
        }
    }

    // DTO for status update, as seen in the frontend
    public class CarModelStatusUpdateDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}