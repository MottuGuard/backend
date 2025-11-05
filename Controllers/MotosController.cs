using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Models.ApiResponses;
using backend.DTOs.Moto;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class MotosController : ControllerBase
    {
        private readonly MottuContext _context;

        public MotosController(MottuContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<MotoResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedResponse<MotoResponseDto>>> GetMotos(
            [FromQuery] MotoStatus? status,
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

            var query = _context.Motos.Include(m => m.UwbTag).AsQueryable();

            if (status.HasValue)
                query = query.Where(m => m.Status == status.Value);

            var totalCount = await query.CountAsync();

            var motos = await query
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new PagedResponse<MotoResponseDto>
            {
                Data = motos.Select(m => new MotoResponseDto
                {
                    Id = m.Id,
                    Chassi = m.Chassi ?? string.Empty,
                    Placa = m.Placa ?? string.Empty,
                    Modelo = m.Modelo.ToString(),
                    Status = m.Status.ToString(),
                    LastX = m.LastX,
                    LastY = m.LastY,
                    LastSeenAt = m.LastSeenAt,
                    UwbTag = m.UwbTag != null ? new MotoResponseDto.UwbTagInfo
                    {
                        Id = m.UwbTag.Id,
                        Eui64 = m.UwbTag.Eui64
                    } : null,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
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
        [ProducesResponseType(typeof(MotoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MotoResponseDto>> GetMoto(int id)
        {
            var moto = await _context.Motos
                .Include(m => m.UwbTag)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (moto == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "NOT_FOUND",
                    Message = $"Motorcycle with ID {id} not found",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            var response = new MotoResponseDto
            {
                Id = moto.Id,
                Chassi = moto.Chassi ?? string.Empty,
                Placa = moto.Placa ?? string.Empty,
                Modelo = moto.Modelo.ToString(),
                Status = moto.Status.ToString(),
                LastX = moto.LastX,
                LastY = moto.LastY,
                LastSeenAt = moto.LastSeenAt,
                UwbTag = moto.UwbTag != null ? new MotoResponseDto.UwbTagInfo
                {
                    Id = moto.UwbTag.Id,
                    Eui64 = moto.UwbTag.Eui64
                } : null,
                CreatedAt = moto.CreatedAt,
                UpdatedAt = moto.UpdatedAt
            };

            return Ok(response);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PutMoto(int id, [FromBody] UpdateMotoDto dto)
        {
            var moto = await _context.Motos.FindAsync(id);
            if (moto == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "NOT_FOUND",
                    Message = $"Motorcycle with ID {id} not found",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            if (dto.Placa != null && dto.Placa != moto.Placa)
            {
                if (await _context.Motos.AnyAsync(m => m.Placa == dto.Placa && m.Id != id))
                {
                    return Conflict(new ErrorResponse
                    {
                        Error = "DUPLICATE_PLACA",
                        Message = $"A motorcycle with placa '{dto.Placa}' already exists",
                        TraceId = HttpContext.TraceIdentifier
                    });
                }
                moto.Placa = dto.Placa;
            }

            if (dto.Chassi != null && dto.Chassi != moto.Chassi)
            {
                if (await _context.Motos.AnyAsync(m => m.Chassi == dto.Chassi && m.Id != id))
                {
                    return Conflict(new ErrorResponse
                    {
                        Error = "DUPLICATE_CHASSI",
                        Message = $"A motorcycle with chassi '{dto.Chassi}' already exists",
                        TraceId = HttpContext.TraceIdentifier
                    });
                }
                moto.Chassi = dto.Chassi;
            }

            if (dto.Modelo.HasValue)
                moto.Modelo = dto.Modelo.Value;

            if (dto.Status.HasValue)
                moto.Status = dto.Status.Value;

            moto.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Motos.AnyAsync(m => m.Id == id))
                {
                    return NotFound(new ErrorResponse
                    {
                        Error = "NOT_FOUND",
                        Message = $"Motorcycle with ID {id} not found",
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                return Conflict(new ErrorResponse
                {
                    Error = "CONCURRENCY_CONFLICT",
                    Message = "The motorcycle has been modified by another user. Please refresh and try again.",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(typeof(MotoResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<MotoResponseDto>> PostMoto([FromBody] CreateMotoDto dto)
        {
            if (await _context.Motos.AnyAsync(m => m.Placa == dto.Placa))
            {
                return Conflict(new ErrorResponse
                {
                    Error = "DUPLICATE_PLACA",
                    Message = $"A motorcycle with placa '{dto.Placa}' already exists",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            if (await _context.Motos.AnyAsync(m => m.Chassi == dto.Chassi))
            {
                return Conflict(new ErrorResponse
                {
                    Error = "DUPLICATE_CHASSI",
                    Message = $"A motorcycle with chassi '{dto.Chassi}' already exists",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            var moto = new Moto
            {
                Chassi = dto.Chassi,
                Placa = dto.Placa,
                Modelo = dto.Modelo,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Motos.Add(moto);
            await _context.SaveChangesAsync();

            var response = new MotoResponseDto
            {
                Id = moto.Id,
                Chassi = moto.Chassi,
                Placa = moto.Placa,
                Modelo = moto.Modelo.ToString(),
                Status = moto.Status.ToString(),
                LastX = moto.LastX,
                LastY = moto.LastY,
                LastSeenAt = moto.LastSeenAt,
                CreatedAt = moto.CreatedAt,
                UpdatedAt = moto.UpdatedAt
            };

            return CreatedAtAction(nameof(GetMoto), new { id = moto.Id }, response);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteMoto(int id)
        {
            var moto = await _context.Motos.FindAsync(id);
            if (moto == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "NOT_FOUND",
                    Message = $"Motorcycle with ID {id} not found",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            if (moto.Status == MotoStatus.Reservada)
            {
                return Conflict(new ErrorResponse
                {
                    Error = "MOTORCYCLE_RESERVED",
                    Message = "Cannot delete a motorcycle that is currently reserved",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            _context.Motos.Remove(moto);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
