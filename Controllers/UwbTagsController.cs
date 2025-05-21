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
    public class UwbTagsController : ControllerBase
    {
        private readonly MottuContext _context;

        public UwbTagsController(MottuContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UwbTag>>> GetTags(
            [FromQuery] TagStatus? status)
        {
            var query = _context.UwbTags.AsQueryable();

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            var tags = await query.ToListAsync();
            return Ok(tags);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUwbTag(int id, UwbTag uwbTag)
        {
            if (id != uwbTag.Id)
            {
                return BadRequest();
            }

            _context.Entry(uwbTag).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UwbTagExists(id))
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
        public async Task<ActionResult<UwbTag>> PostUwbTag(UwbTag uwbTag)
        {
            _context.UwbTags.Add(uwbTag);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUwbTag", new { id = uwbTag.Id }, uwbTag);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUwbTag(int id)
        {
            var uwbTag = await _context.UwbTags.FindAsync(id);
            if (uwbTag == null)
            {
                return NotFound();
            }

            _context.UwbTags.Remove(uwbTag);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UwbTagExists(int id)
        {
            return _context.UwbTags.Any(e => e.Id == id);
        }
    }
}
