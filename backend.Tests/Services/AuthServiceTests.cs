using backend.DTO;
using backend.Models;
using backend.Models.ApiResponses;
using backend.Services;
using backend.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace backend.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        _mockConfiguration = new Mock<IConfiguration>();
        _mockTokenService = new Mock<ITokenService>();

        _authService = new AuthService(
            _mockUserManager.Object,
            _mockConfiguration.Object,
            _mockTokenService.Object);
    }

    #region Login Tests (Authenticate with Email/Password)

    [Fact]
    public async Task Authenticate_WithValidCredentials_ShouldReturnOkWithTokens()
    {
        var authDTO = new AuthDTO { Email = "test@test.com", Password = "Test@123" };
        var user = TestDataBuilder.ApplicationUser()
            .WithId(1)
            .WithEmail(authDTO.Email)
            .WithName("Test User")
            .Build();

        _mockUserManager.Setup(x => x.FindByEmailAsync(authDTO.Email))
            .ReturnsAsync(user);
        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, authDTO.Password))
            .ReturnsAsync(true);
        _mockTokenService.Setup(x => x.GenerateToken(user))
            .ReturnsAsync("jwt-token");
        _mockTokenService.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        var result = await _authService.Authenticate(authDTO);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Authenticate_WithInvalidEmail_ShouldReturnUnauthorized()
    {
        var authDTO = new AuthDTO { Email = "nonexistent@test.com", Password = "Test@123" };

        _mockUserManager.Setup(x => x.FindByEmailAsync(authDTO.Email))
            .ReturnsAsync((ApplicationUser)null);

        var result = await _authService.Authenticate(authDTO);

        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        var errorResponse = unauthorizedResult.Value as ErrorResponse;
        errorResponse.Error.Should().Be("AUTHENTICATION_FAILED");
        errorResponse.Message.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task Authenticate_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        var authDTO = new AuthDTO { Email = "test@test.com", Password = "WrongPassword" };
        var user = TestDataBuilder.ApplicationUser()
            .WithId(2)
            .WithEmail(authDTO.Email)
            .Build();

        _mockUserManager.Setup(x => x.FindByEmailAsync(authDTO.Email))
            .ReturnsAsync(user);
        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, authDTO.Password))
            .ReturnsAsync(false);

        var result = await _authService.Authenticate(authDTO);

        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        var errorResponse = unauthorizedResult.Value as ErrorResponse;
        errorResponse.Error.Should().Be("AUTHENTICATION_FAILED");
    }

    [Fact]
    public async Task Authenticate_ShouldGenerateAndStoreRefreshToken()
    {
        var authDTO = new AuthDTO { Email = "test@test.com", Password = "Test@123" };
        var user = TestDataBuilder.ApplicationUser()
            .WithEmail(authDTO.Email)
            .Build();

        _mockUserManager.Setup(x => x.FindByEmailAsync(authDTO.Email))
            .ReturnsAsync(user);
        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, authDTO.Password))
            .ReturnsAsync(true);
        _mockTokenService.Setup(x => x.GenerateToken(user))
            .ReturnsAsync("jwt-token");
        _mockTokenService.Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");

        var result = await _authService.Authenticate(authDTO);

        user.RefreshToken.Should().Be("new-refresh-token");
        user.RefreshTokenExpiryTime.Should().BeCloseTo(DateTime.UtcNow.AddDays(1), TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Registration Tests

    [Fact]
    public async Task Register_WithValidData_ShouldCreateUser()
    {
        var registerDTO = new RegisterDTO
        {
            Email = "newuser@test.com",
            Name = "New User",
            Password = "NewUser@123"
        };

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDTO.Password))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _authService.Register(registerDTO);

        result.Should().BeOfType<CreatedResult>();
        var createdResult = result as CreatedResult;
        createdResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Register_ShouldSetUsernameToEmail()
    {
        var registerDTO = new RegisterDTO
        {
            Email = "test@example.com",
            Name = "Test",
            Password = "Test@123"
        };

        ApplicationUser capturedUser = null;
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Callback<ApplicationUser, string>((u, p) => capturedUser = u)
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);

        await _authService.Register(registerDTO);

        capturedUser.Should().NotBeNull();
        capturedUser.UserName.Should().Be(registerDTO.Email);
        capturedUser.Email.Should().Be(registerDTO.Email);
        capturedUser.Name.Should().Be(registerDTO.Name);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        var registerDTO = new RegisterDTO
        {
            Email = "existing@test.com",
            Name = "Test",
            Password = "Test@123"
        };

        var identityErrors = new[]
        {
            new IdentityError { Description = "Email 'existing@test.com' is already taken." }
        };

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDTO.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        var result = await _authService.Register(registerDTO);

        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        var errorResponse = badRequestResult.Value as ErrorResponse;
        errorResponse.Error.Should().Be("REGISTRATION_FAILED");
        errorResponse.Message.Should().Be("Failed to create user");
    }

    [Fact]
    public async Task Register_ShouldAddUserToUserRole()
    {
        var registerDTO = new RegisterDTO
        {
            Email = "test@test.com",
            Name = "Test",
            Password = "Test@123"
        };

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerDTO.Password))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);

        await _authService.Register(registerDTO);

        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"), Times.Once);
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public async Task Authenticate_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        var refreshDTO = new RefreshDTO
        {
            Token = "valid-jwt-token",
            RefreshToken = "valid-refresh-token"
        };

        var user = TestDataBuilder.ApplicationUser()
            .WithId(10)
            .WithEmail("test@test.com")
            .Build();

        user.RefreshToken = "valid-refresh-token";
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);

        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "10")
        }));

        _mockTokenService.Setup(x => x.GetPrincipalFromToken(refreshDTO.Token))
            .Returns(principal);
        _mockUserManager.Setup(x => x.FindByIdAsync("10"))
            .ReturnsAsync(user);
        _mockTokenService.Setup(x => x.GenerateToken(user))
            .ReturnsAsync("new-jwt-token");
        _mockTokenService.Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");
        _mockUserManager.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _authService.Authenticate(refreshDTO);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Authenticate_WithInvalidToken_ShouldReturnUnauthorized()
    {
        var refreshDTO = new RefreshDTO
        {
            Token = "invalid-token",
            RefreshToken = "refresh-token"
        };

        _mockTokenService.Setup(x => x.GetPrincipalFromToken(refreshDTO.Token))
            .Throws(new Exception("Invalid token"));

        var result = await _authService.Authenticate(refreshDTO);

        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        var errorResponse = unauthorizedResult.Value as ErrorResponse;
        errorResponse.Error.Should().Be("INVALID_TOKEN");
    }

    [Fact]
    public async Task Authenticate_WithExpiredRefreshToken_ShouldReturnUnauthorized()
    {
        var refreshDTO = new RefreshDTO
        {
            Token = "valid-jwt-token",
            RefreshToken = "expired-refresh-token"
        };

        var user = TestDataBuilder.ApplicationUser()
            .WithId(10)
            .Build();

        user.RefreshToken = "expired-refresh-token";
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1); // Expired yesterday

        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "10")
        }));

        _mockTokenService.Setup(x => x.GetPrincipalFromToken(refreshDTO.Token))
            .Returns(principal);
        _mockUserManager.Setup(x => x.FindByIdAsync("10"))
            .ReturnsAsync(user);

        var result = await _authService.Authenticate(refreshDTO);

        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        var errorResponse = unauthorizedResult.Value as ErrorResponse;
        errorResponse.Error.Should().Be("INVALID_REFRESH_TOKEN");
        errorResponse.Message.Should().Be("Refresh token is invalid or expired");
    }

    [Fact]
    public async Task Authenticate_WithMismatchedRefreshToken_ShouldReturnUnauthorized()
    {
        var refreshDTO = new RefreshDTO
        {
            Token = "valid-jwt-token",
            RefreshToken = "wrong-refresh-token"
        };

        var user = TestDataBuilder.ApplicationUser()
            .WithId(10)
            .Build();

        user.RefreshToken = "correct-refresh-token";
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);

        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "10")
        }));

        _mockTokenService.Setup(x => x.GetPrincipalFromToken(refreshDTO.Token))
            .Returns(principal);
        _mockUserManager.Setup(x => x.FindByIdAsync("10"))
            .ReturnsAsync(user);

        var result = await _authService.Authenticate(refreshDTO);

        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        var errorResponse = unauthorizedResult.Value as ErrorResponse;
        errorResponse.Error.Should().Be("INVALID_REFRESH_TOKEN");
    }

    [Fact]
    public async Task Authenticate_WithNonExistentUser_ShouldReturnNotFound()
    {
        var refreshDTO = new RefreshDTO
        {
            Token = "valid-jwt-token",
            RefreshToken = "refresh-token"
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "nonexistent-user-id")
        }));

        _mockTokenService.Setup(x => x.GetPrincipalFromToken(refreshDTO.Token))
            .Returns(principal);
        _mockUserManager.Setup(x => x.FindByIdAsync("nonexistent-user-id"))
            .ReturnsAsync((ApplicationUser)null);

        var result = await _authService.Authenticate(refreshDTO);

        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        var errorResponse = notFoundResult.Value as ErrorResponse;
        errorResponse.Error.Should().Be("USER_NOT_FOUND");
    }

    [Fact]
    public async Task Authenticate_RefreshFlow_ShouldUpdateUserWithNewRefreshToken()
    {
        var refreshDTO = new RefreshDTO
        {
            Token = "valid-jwt-token",
            RefreshToken = "old-refresh-token"
        };

        var user = TestDataBuilder.ApplicationUser()
            .WithId(10)
            .Build();

        user.RefreshToken = "old-refresh-token";
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);

        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "10")
        }));

        _mockTokenService.Setup(x => x.GetPrincipalFromToken(refreshDTO.Token))
            .Returns(principal);
        _mockUserManager.Setup(x => x.FindByIdAsync("10"))
            .ReturnsAsync(user);
        _mockTokenService.Setup(x => x.GenerateToken(user))
            .ReturnsAsync("new-jwt-token");
        _mockTokenService.Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");
        _mockUserManager.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        await _authService.Authenticate(refreshDTO);

        user.RefreshToken.Should().Be("new-refresh-token");
        user.RefreshTokenExpiryTime.Should().BeCloseTo(DateTime.UtcNow.AddDays(1), TimeSpan.FromSeconds(5));
        _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    #endregion
}
