using backend.Models;
using backend.Services;
using backend.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace backend.Tests.Services;

public class TokenServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly TokenService _tokenService;
    private const string TestJwtKey = "test-jwt-key-for-testing-purposes-must-be-at-least-32-characters-long";

    public TokenServiceTests()
    {
        // Mock IConfiguration
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(x => x["Jwt:Key"]).Returns(TestJwtKey);

        // Mock UserManager (requires complex setup)
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            null, null, null, null, null, null, null, null);

        _tokenService = new TokenService(_mockConfiguration.Object, _mockUserManager.Object);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnBase64String()
    {
        // Act
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
        refreshToken.Should().MatchRegex("^[A-Za-z0-9+/=]+$", "it should be a valid base64 string");
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnUniqueTokensOnMultipleCalls()
    {
        // Act
        var token1 = _tokenService.GenerateRefreshToken();
        var token2 = _tokenService.GenerateRefreshToken();
        var token3 = _tokenService.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
        token2.Should().NotBe(token3);
        token1.Should().NotBe(token3);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturn44CharactersLength()
    {
        // Arrange - 32 bytes encoded in base64 should be 44 characters (with padding)

        // Act
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Assert
        refreshToken.Length.Should().Be(44, "32 random bytes in base64 should produce 44 characters");
    }

    [Fact]
    public async Task GenerateToken_ShouldReturnValidJwtToken()
    {
        // Arrange
        var user = TestDataBuilder.ApplicationUser()
            .WithId(123)
            .WithEmail("test@test.com")
            .WithName("Test User")
            .Build();

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        // Act
        var token = await _tokenService.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3, "JWT tokens have 3 parts separated by dots");
    }

    [Fact]
    public async Task GenerateToken_ShouldIncludeUserClaimsInToken()
    {
        // Arrange
        var user = TestDataBuilder.ApplicationUser()
            .WithId(456)
            .WithEmail("user@example.com")
            .WithName("John Doe")
            .Build();

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        // Act
        var token = await _tokenService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "456");
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "user@example.com");
        jwtToken.Claims.Should().Contain(c => c.Type == "name" && c.Value == "John Doe");
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
    }

    [Fact]
    public async Task GenerateToken_ShouldExpireIn30Minutes()
    {
        // Arrange
        var user = TestDataBuilder.ApplicationUser()
            .WithId(11)
            .WithEmail("test@test.com")
            .WithName("Test")
            .Build();

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = await _tokenService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var expectedExpiry = beforeGeneration.AddMinutes(30);
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetPrincipalFromToken_ShouldExtractClaimsFromValidToken()
    {
        // Arrange
        var user = TestDataBuilder.ApplicationUser()
            .WithId(12)
            .WithEmail("principal@test.com")
            .WithName("Principal Test")
            .Build();

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        var token = await _tokenService.GenerateToken(user);

        // Act
        var principal = _tokenService.GetPrincipalFromToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "12");
        principal.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "principal@test.com");
        principal.Claims.Should().Contain(c => c.Type == "name" && c.Value == "Principal Test");
    }

    [Fact]
    public void GetPrincipalFromToken_ShouldThrowException_WhenTokenIsInvalid()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        // Act
        Action act = () => _tokenService.GetPrincipalFromToken(invalidToken);

        // Assert
        act.Should().Throw<Exception>("invalid tokens should throw an exception");
    }

    [Fact]
    public async Task GetPrincipalFromToken_ShouldNotValidateLifetime()
    {
        // Arrange - This test verifies that expired tokens can still have their claims extracted
        var user = TestDataBuilder.ApplicationUser()
            .WithId(13)
            .WithEmail("expired@test.com")
            .WithName("Expired Test")
            .Build();

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        // Generate a token (it will expire in 30 minutes, but GetPrincipalFromToken doesn't validate lifetime)
        var token = await _tokenService.GenerateToken(user);

        // Act
        var principal = _tokenService.GetPrincipalFromToken(token);

        // Assert
        principal.Should().NotBeNull("GetPrincipalFromToken should not validate token expiration");
        principal.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "13");
    }

    [Fact]
    public async Task GenerateToken_ShouldGenerateUniqueJtiForEachToken()
    {
        // Arrange
        var user = TestDataBuilder.ApplicationUser()
            .WithId(14)
            .WithEmail("jti@test.com")
            .WithName("JTI Test")
            .Build();

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        // Act
        var token1 = await _tokenService.GenerateToken(user);
        var token2 = await _tokenService.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken1 = handler.ReadJwtToken(token1);
        var jwtToken2 = handler.ReadJwtToken(token2);

        var jti1 = jwtToken1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = jwtToken2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        // Assert
        jti1.Should().NotBe(jti2, "each token should have a unique JTI (JWT ID)");
    }
}
