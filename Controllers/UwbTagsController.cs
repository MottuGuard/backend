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
using backend.DTOs.UwbTag;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class UwbTagsController : ControllerBase
    {
        private readonly MottuContext _context;

        public UwbTagsController(MottuContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<UwbTagResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedResponse<UwbTagResponseDto>>> GetTags(
            [FromQuery] TagStatus? status,
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

            var query = _context.UwbTags.Include(t => t.Moto).AsQueryable();

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            var totalCount = await query.CountAsync();

            var tags = await query
                .OrderBy(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new PagedResponse<UwbTagResponseDto>
            {
                Data = tags.Select(t => new UwbTagResponseDto
                {
                    Id = t.Id,
                    Eui64 = t.Eui64,
                    Status = t.Status.ToString(),
                    MotoId = t.MotoId,
                    Moto = t.Moto != null ? new UwbTagResponseDto.MotoInfo
                    {
                        Id = t.Moto.Id,
                        Placa = t.Moto.Placa ?? string.Empty,
                        Modelo = t.Moto.Modelo.ToString(),
                        Status = t.Moto.Status.ToString()
                    } : null
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
        [ProducesResponseType(typeof(UwbTagResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UwbTagResponseDto>> GetUwbTag(int id)
        {
            var tag = await _context.UwbTags
                .Include(t => t.Moto)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tag == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "NOT_FOUND",
                    Message = $"UWB tag with ID {id} not found",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            var response = new UwbTagResponseDto
            {
                Id = tag.Id,
                Eui64 = tag.Eui64,
                Status = tag.Status.ToString(),
                MotoId = tag.MotoId,
                Moto = tag.Moto != null ? new UwbTagResponseDto.MotoInfo
                {
                    Id = tag.Moto.Id,
                    Placa = tag.Moto.Placa ?? string.Empty,
                    Modelo = tag.Moto.Modelo.ToString(),
                    Status = tag.Moto.Status.ToString()
                } : null
            };

            return Ok(response);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> PutUwbTag(int id, [FromBody] UpdateUwbTagDto dto)
        {
            var tag = await _context.UwbTags.FindAsync(id);
            if (tag == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "NOT_FOUND",
                    Message = $"UWB tag with ID {id} not found",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            if (dto.Eui64 != null && dto.Eui64 != tag.Eui64)
            {
                if (await _context.UwbTags.AnyAsync(t => t.Eui64 == dto.Eui64 && t.Id != id))
                {
                    return Conflict(new ErrorResponse
                    {
                        Error = "DUPLICATE_EUI64",
                        Message = $"A UWB tag with Eui64 '{dto.Eui64}' already exists",
                        TraceId = HttpContext.TraceIdentifier
                    });
                }
                tag.Eui64 = dto.Eui64;
            }

            if (dto.MotoId.HasValue && dto.MotoId.Value != tag.MotoId)
            {
                if (!await _context.Motos.AnyAsync(m => m.Id == dto.MotoId.Value))
                {
                    return UnprocessableEntity(new ErrorResponse
                    {
                        Error = "INVALID_MOTO_ID",
                        Message = $"Motorcycle with ID {dto.MotoId.Value} does not exist",
                        TraceId = HttpContext.TraceIdentifier
                    });
                }
                tag.MotoId = dto.MotoId.Value;
            }

            if (dto.Status.HasValue)
                tag.Status = dto.Status.Value;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.UwbTags.AnyAsync(t => t.Id == id))
                {
                    return NotFound(new ErrorResponse
                    {
                        Error = "NOT_FOUND",
                        Message = $"UWB tag with ID {id} not found",
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                return Conflict(new ErrorResponse
                {
                    Error = "CONCURRENCY_CONFLICT",
                    Message = "The UWB tag has been modified by another user. Please refresh and try again.",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(typeof(UwbTagResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<UwbTagResponseDto>> PostUwbTag([FromBody] CreateUwbTagDto dto)
        {
            if (await _context.UwbTags.AnyAsync(t => t.Eui64 == dto.Eui64))
            {
                return Conflict(new ErrorResponse
                {
                    Error = "DUPLICATE_EUI64",
                    Message = $"A UWB tag with Eui64 '{dto.Eui64}' already exists",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            if (!await _context.Motos.AnyAsync(m => m.Id == dto.MotoId))
            {
                return UnprocessableEntity(new ErrorResponse
                {
                    Error = "INVALID_MOTO_ID",
                    Message = $"Motorcycle with ID {dto.MotoId} does not exist",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            var tag = new UwbTag
            {
                Eui64 = dto.Eui64,
                MotoId = dto.MotoId,
                Status = dto.Status
            };

            _context.UwbTags.Add(tag);
            await _context.SaveChangesAsync();

            await _context.Entry(tag).Reference(t => t.Moto).LoadAsync();

            var response = new UwbTagResponseDto
            {
                Id = tag.Id,
                Eui64 = tag.Eui64,
                Status = tag.Status.ToString(),
                MotoId = tag.MotoId,
                Moto = tag.Moto != null ? new UwbTagResponseDto.MotoInfo
                {
                    Id = tag.Moto.Id,
                    Placa = tag.Moto.Placa ?? string.Empty,
                    Modelo = tag.Moto.Modelo.ToString(),
                    Status = tag.Moto.Status.ToString()
                } : null
            };

            return CreatedAtAction(nameof(GetUwbTag), new { id = tag.Id }, response);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUwbTag(int id)
        {
            var tag = await _context.UwbTags.FindAsync(id);
            if (tag == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "NOT_FOUND",
                    Message = $"UWB tag with ID {id} not found",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            _context.UwbTags.Remove(tag);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
