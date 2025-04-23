using backend.Models;
using System.Security.Claims;

public interface ITokenService
{
    Task<string> GenerateToken(ApplicationUser user);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromToken(string token);
}