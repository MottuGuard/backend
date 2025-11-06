using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Models.ApiResponses;
using backend.DTOs.UwbAnchor;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class UwbAnchorsController : ControllerBase
    {
        private readonly MottuContext _context;

        public UwbAnchorsController(MottuContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<UwbAnchorResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedResponse<UwbAnchorResponseDto>>> GetAnchors(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1)
                return BadRequest(new ErrorResponse
                {
                    Error = "INVALID_PAGE",
                    Message = "Page must be greater than 0",
                    TraceId = HttpContext.TraceIdentifier
                });

            if (pageSize < 1 || pageSize > 100)
                return BadRequest(new ErrorResponse
                {
                    Error = "INVALID_PAGE_SIZE",
                    Message = "PageSize must be between 1 and 100",
                    TraceId = HttpContext.TraceIdentifier
                });

            var totalCount = await _context.UwbAnchors.CountAsync();

            var anchors = await _context.UwbAnchors
                .OrderBy(a => a.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new PagedResponse<UwbAnchorResponseDto>
            {
                Data = anchors.Select(a => new UwbAnchorResponseDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    X = a.X,
                    Y = a.Y,
                    Z = a.Z
                }),
                Pagination = new PaginationMetadata
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    HasNextPage = page * pageSize < totalCount,
                    HasPreviousPage = page > 1
                }
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UwbAnchorResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UwbAnchorResponseDto>> GetUwbAnchor(int id)
        {
            var anchor = await _context.UwbAnchors.FindAsync(id);

            if (anchor == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "NOT_FOUND",
                    Message = $"UWB anchor with ID {id} not found",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            var response = new UwbAnchorResponseDto
            {
                Id = anchor.Id,
                Name = anchor.Name,
                X = anchor.X,
                Y = anchor.Y,
                Z = anchor.Z
            };

            return Ok(response);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutUwbAnchor(int id, [FromBody] UpdateUwbAnchorDto dto)
        {
            var anchor = await _context.UwbAnchors.FindAsync(id);
            if (anchor == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "NOT_FOUND",
                    Message = $"UWB anchor with ID {id} not found",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            if (dto.Name != null)
                anchor.Name = dto.Name;

            if (dto.X.HasValue)
                anchor.X = dto.X.Value;

            if (dto.Y.HasValue)
                anchor.Y = dto.Y.Value;

            if (dto.Z.HasValue)
                anchor.Z = dto.Z.Value;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(typeof(UwbAnchorResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UwbAnchorResponseDto>> PostUwbAnchor([FromBody] CreateUwbAnchorDto dto)
        {
            var anchor = new UwbAnchor
            {
                Name = dto.Name,
                X = dto.X,
                Y = dto.Y,
                Z = dto.Z
            };

            _context.UwbAnchors.Add(anchor);
            await _context.SaveChangesAsync();

            var response = new UwbAnchorResponseDto
            {
                Id = anchor.Id,
                Name = anchor.Name,
                X = anchor.X,
                Y = anchor.Y,
                Z = anchor.Z
            };

            return CreatedAtAction(nameof(GetUwbAnchor), new { id = anchor.Id }, response);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUwbAnchor(int id)
        {
            var anchor = await _context.UwbAnchors.FindAsync(id);
            if (anchor == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "NOT_FOUND",
                    Message = $"UWB anchor with ID {id} not found",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            _context.UwbAnchors.Remove(anchor);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
