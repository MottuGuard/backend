using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UwbMeasurementsController : ControllerBase
    {
        private readonly MottuContext _context;

        public UwbMeasurementsController(MottuContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UwbMeasurement>>> GetMeasurements(
            [FromQuery] int? tagId,
            [FromQuery] int? anchorId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var query = _context.UwbMeasurements.AsQueryable();

            if (tagId.HasValue)
                query = query.Where(m => m.UwbTagId == tagId.Value);
            if (anchorId.HasValue)
                query = query.Where(m => m.UwbAnchorId == anchorId.Value);
            if (from.HasValue)
                query = query.Where(m => m.Timestamp >= from.Value);
            if (to.HasValue)
                query = query.Where(m => m.Timestamp <= to.Value);

            var result = await query
                .OrderBy(m => m.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<UwbMeasurement>> GetUwbMeasurement(int id)
        {
            var uwbMeasurement = await _context.UwbMeasurements.FindAsync(id);

            if (uwbMeasurement == null)
            {
                return NotFound();
            }

            return uwbMeasurement;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUwbMeasurement(int id, UwbMeasurement uwbMeasurement)
        {
            if (id != uwbMeasurement.Id)
            {
                return BadRequest();
            }

            _context.Entry(uwbMeasurement).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UwbMeasurementExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<UwbMeasurement>> PostUwbMeasurement(UwbMeasurement uwbMeasurement)
        {
            _context.UwbMeasurements.Add(uwbMeasurement);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUwbMeasurement", new { id = uwbMeasurement.Id }, uwbMeasurement);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUwbMeasurement(int id)
        {
            var uwbMeasurement = await _context.UwbMeasurements.FindAsync(id);
            if (uwbMeasurement == null)
            {
                return NotFound();
            }

            _context.UwbMeasurements.Remove(uwbMeasurement);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UwbMeasurementExists(int id)
        {
            return _context.UwbMeasurements.Any(e => e.Id == id);
        }
    }
}
