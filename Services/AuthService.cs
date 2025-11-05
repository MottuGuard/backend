using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using backend.DTO;
using backend.Models;
using backend.Models.ApiResponses;
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
                return new UnauthorizedObjectResult(new ErrorResponse
                {
                    Error = "AUTHENTICATION_FAILED",
                    Message = "Invalid email or password"
                });
            }
            var result = await _userManager.CheckPasswordAsync(user, authDTO.Password);
            if (!result)
            {
                return new UnauthorizedObjectResult(new ErrorResponse
                {
                    Error = "AUTHENTICATION_FAILED",
                    Message = "Invalid email or password"
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
            catch (Exception)
            {
                return new UnauthorizedObjectResult(new ErrorResponse
                {
                    Error = "INVALID_TOKEN",
                    Message = "Access token is invalid or expired"
                });
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return new UnauthorizedObjectResult(new ErrorResponse
                {
                    Error = "INVALID_TOKEN",
                    Message = "Token does not contain user identifier"
                });
            }

            var user = await _userManager.FindByIdAsync(userIdClaim.Value);
            if (user == null)
            {
                return new NotFoundObjectResult(new ErrorResponse
                {
                    Error = "USER_NOT_FOUND",
                    Message = "User not found"
                });
            }

            if (user.RefreshToken != refreshDTO.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return new UnauthorizedObjectResult(new ErrorResponse
                {
                    Error = "INVALID_REFRESH_TOKEN",
                    Message = "Refresh token is invalid or expired"
                });
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
                return new BadRequestObjectResult(new ErrorResponse
                {
                    Error = "REGISTRATION_FAILED",
                    Message = "Failed to create user",
                    Errors = new Dictionary<string, string[]>
                    {
                        { "user", result.Errors.Select(e => e.Description).ToArray() }
                    }
                });
            }
            await _userManager.AddToRoleAsync(user, "User");

            return new CreatedResult($"/api/users/{user.Id}", new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email,
                message = "User created successfully"
            });
        }
    }
}