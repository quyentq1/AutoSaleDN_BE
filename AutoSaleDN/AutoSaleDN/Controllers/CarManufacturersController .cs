using AutoSaleDN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoSaleDN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requires authentication for all actions in this controller
    public class CarManufacturersController : ControllerBase
    {
        private readonly AutoSaleDbContext _context;

        public CarManufacturersController(AutoSaleDbContext context)
        {
            _context = context;
        }

        // GET: api/CarManufacturers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CarManufacturer>>> GetCarManufacturers()
        {
            if (_context.CarManufacturers == null)
            {
                return NotFound("No car manufacturers found.");
            }
            return await _context.CarManufacturers.ToListAsync();
        }

        // GET: api/CarManufacturers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CarManufacturer>> GetCarManufacturer(int id)
        {
            if (_context.CarManufacturers == null)
            {
                return NotFound();
            }
            var carManufacturer = await _context.CarManufacturers.FindAsync(id);

            if (carManufacturer == null)
            {
                return NotFound();
            }

            return carManufacturer;
        }

        // POST: api/CarManufacturers
        [HttpPost]
        public async Task<ActionResult<CarManufacturer>> PostCarManufacturer(CarManufacturer carManufacturer)
        {
            if (_context.CarManufacturers == null)
            {
                return Problem("Entity set 'ApplicationDbContext.CarManufacturers' is null.");
            }

            // Basic validation for name uniqueness (case-insensitive)
            if (await _context.CarManufacturers.AnyAsync(m => m.Name.ToLower() == carManufacturer.Name.ToLower()))
            {
                return Conflict("A manufacturer with this name already exists.");
            }

            _context.CarManufacturers.Add(carManufacturer);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCarManufacturer", new { id = carManufacturer.ManufacturerId }, carManufacturer);
        }

        // PUT: api/CarManufacturers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCarManufacturer(int id, CarManufacturer carManufacturer)
        {
            if (id != carManufacturer.ManufacturerId)
            {
                return BadRequest("Manufacturer ID mismatch.");
            }

            // Basic validation for name uniqueness (case-insensitive) excluding the current entity
            if (await _context.CarManufacturers.AnyAsync(m => m.Name.ToLower() == carManufacturer.Name.ToLower() && m.ManufacturerId != id))
            {
                return Conflict("A manufacturer with this name already exists.");
            }

            _context.Entry(carManufacturer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarManufacturerExists(id))
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
                // Log the exception (e.g., using ILogger)
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating manufacturer: {ex.Message}");
            }

            return NoContent();
        }

        // DELETE: api/CarManufacturers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCarManufacturer(int id)
        {
            if (_context.CarManufacturers == null)
            {
                return NotFound();
            }
            var carManufacturer = await _context.CarManufacturers.FindAsync(id);
            if (carManufacturer == null)
            {
                return NotFound();
            }

            // Check if there are associated CarModels before deleting
            var hasModels = await _context.CarModels.AnyAsync(m => m.ManufacturerId == id);
            if (hasModels)
            {
                // Return a conflict or bad request if deletion is not allowed due to associated models
                // Or remove this check if OnDelete(DeleteBehavior.Cascade) is desired and handled by DB
                return BadRequest("Cannot delete manufacturer because there are associated car models. Please delete the models first.");
            }

            _context.CarManufacturers.Remove(carManufacturer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CarManufacturerExists(int id)
        {
            return (_context.CarManufacturers?.Any(e => e.ManufacturerId == id)).GetValueOrDefault();
        }
    }
}