using backend.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.IntegrationTests.Helpers;

/// <summary>
/// Helper class for generating JWT tokens for integration tests
/// </summary>
public static class AuthenticationHelper
{
    private const string TestJwtKey = "test-jwt-key-for-testing-purposes-must-be-at-least-32-characters-long";

    /// <summary>
    /// Generates a JWT token for a test user
    /// </summary>
    public static string GenerateJwtToken(ApplicationUser user, string[]? roles = null)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("name", user.Name ?? string.Empty)
        };

        if (roles != null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a JWT token for the default test user
    /// </summary>
    public static string GenerateJwtToken()
    {
        var user = new ApplicationUser
        {
            Id = 999,
            Email = "testuser@test.com",
            Name = "Test User"
        };

        return GenerateJwtToken(user, new[] { "User" });
    }

    /// <summary>
    /// Generates a JWT token for an admin user
    /// </summary>
    public static string GenerateAdminJwtToken()
    {
        var user = new ApplicationUser
        {
            Id = 1000,
            Email = "admin@test.com",
            Name = "Admin User"
        };

        return GenerateJwtToken(user, new[] { "Admin", "User" });
    }
}
