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
    public class PositionRecordsController : ControllerBase
    {
        private readonly MottuContext _context;

        public PositionRecordsController(MottuContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PositionRecord>>> GetPositionRecords()
        {
            return await _context.PositionRecords.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PositionRecord>> GetPositionRecord(int id)
        {
            var positionRecord = await _context.PositionRecords.FindAsync(id);

            if (positionRecord == null)
            {
                return NotFound();
            }

            return positionRecord;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPositionRecord(int id, PositionRecord positionRecord)
        {
            if (id != positionRecord.Id)
            {
                return BadRequest();
            }

            _context.Entry(positionRecord).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PositionRecordExists(id))
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
        public async Task<ActionResult<PositionRecord>> PostPositionRecord(PositionRecord positionRecord)
        {
            _context.PositionRecords.Add(positionRecord);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPositionRecord", new { id = positionRecord.Id }, positionRecord);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePositionRecord(int id)
        {
            var positionRecord = await _context.PositionRecords.FindAsync(id);
            if (positionRecord == null)
            {
                return NotFound();
            }

            _context.PositionRecords.Remove(positionRecord);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PositionRecordExists(int id)
        {
            return _context.PositionRecords.Any(e => e.Id == id);
        }
    }
}
