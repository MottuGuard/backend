using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using backend.DTO;
using backend.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration, ITokenService tokenService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _tokenService = tokenService;
        }
        public async Task<IActionResult> Authenticate(AuthDTO authDTO)
        {
            var user = await _userManager.FindByEmailAsync(authDTO.Email);
            if (user == null)
            {
                return new BadRequestObjectResult(new
                {
                    Message = "Email não encontrado"
                });
            }
            var result = await _userManager.CheckPasswordAsync(user, authDTO.Password);
            if (!result)
            {
                return new BadRequestObjectResult(new
                {
                    Message = "Senha Incorreta"
                });
            }
            var token = await _tokenService.GenerateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);

            return new OkObjectResult(new
            {
                token,
                refreshToken,
                user = new
                {
                    user.Id,
                    user.Name,
                    user.Email
                }
            });
        }
        public async Task<IActionResult> Authenticate(RefreshDTO refreshDTO)
        {
            ClaimsPrincipal principal;
            try
            {
                principal = _tokenService.GetPrincipalFromToken(refreshDTO.Token);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { Message = "Access token inválido", Error = ex.Message });
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return new BadRequestObjectResult(new { Message = "Token não contém o identificador do usuário." });
            }

            var user = await _userManager.FindByIdAsync(userIdClaim.Value);
            if (user == null)
            {
                return new NotFoundObjectResult(new { Message = "Usuário não encontrado." });
            }

            if (user.RefreshToken != refreshDTO.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return new BadRequestObjectResult(new { Message = "Refresh token inválido ou expirado." });
            }

            var newToken = await _tokenService.GenerateToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);
            await _userManager.UpdateAsync(user);


            return new OkObjectResult(new
            {
                token = newToken,
                refreshToken = newRefreshToken,
                user = new
                {
                    user.Id,
                    user.Name,
                    user.Email
                }
            });
        }
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Name = dto.Name,
                Email = dto.Email,
            };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return new BadRequestObjectResult(new
                {
                    Message = "Erro ao criar usuário",
                    Errors = result.Errors.Select(e => e.Description)
                });
            }
            await _userManager.AddToRoleAsync(user, "User");
            return new OkObjectResult(new
            {
                Message = "Usuário criado com sucesso"
            });
        }
    }
}