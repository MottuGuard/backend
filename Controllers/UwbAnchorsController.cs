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
    public class UwbAnchorsController : ControllerBase
    {
        private readonly MottuContext _context;

        public UwbAnchorsController(MottuContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UwbAnchor>>> GetAnchors(
            [FromQuery] string? nameContains,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.UwbAnchors.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nameContains))
                query = query.Where(a => a.Name.Contains(nameContains));

            var anchors = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(anchors);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UwbAnchor>> GetUwbAnchor(int id)
        {
            var uwbAnchor = await _context.UwbAnchors.FindAsync(id);

            if (uwbAnchor == null)
            {
                return NotFound();
            }

            return uwbAnchor;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUwbAnchor(int id, UwbAnchor uwbAnchor)
        {
            if (id != uwbAnchor.Id)
            {
                return BadRequest();
            }

            _context.Entry(uwbAnchor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UwbAnchorExists(id))
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
        public async Task<ActionResult<UwbAnchor>> PostUwbAnchor(UwbAnchor uwbAnchor)
        {
            _context.UwbAnchors.Add(uwbAnchor);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUwbAnchor", new { id = uwbAnchor.Id }, uwbAnchor);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUwbAnchor(int id)
        {
            var uwbAnchor = await _context.UwbAnchors.FindAsync(id);
            if (uwbAnchor == null)
            {
                return NotFound();
            }

            _context.UwbAnchors.Remove(uwbAnchor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UwbAnchorExists(int id)
        {
            return _context.UwbAnchors.Any(e => e.Id == id);
        }
    }
}
