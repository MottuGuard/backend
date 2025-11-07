using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using backend.DTO;
using backend.IntegrationTests.Helpers;
using backend.Models.ApiResponses;
using FluentAssertions;

namespace backend.IntegrationTests.Controllers;

public class AuthControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsCreated()
    {
        // Arrange
        var registerDto = new RegisterDTO
        {
            Email = $"newuser{Guid.NewGuid():N}@test.com", // Unique email
            Password = "NewUser@123",
            Name = "New Test User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        result.GetProperty("id").GetInt32().Should().BeGreaterThan(0);
        result.GetProperty("email").GetString().Should().Be(registerDto.Email);
        result.GetProperty("name").GetString().Should().Be(registerDto.Name);
        result.GetProperty("message").GetString().Should().Contain("successfully");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        var loginDto = new AuthDTO
        {
            Email = "testuser@test.com", // Seeded user in TestWebApplicationFactory
            Password = "Test@123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        result.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
        result.GetProperty("refreshToken").GetString().Should().NotBeNullOrEmpty();

        var user = result.GetProperty("user");
        user.GetProperty("id").GetInt32().Should().BeGreaterThan(0);
        user.GetProperty("email").GetString().Should().Be(loginDto.Email);
        user.GetProperty("name").GetString().Should().Be("Test User");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new AuthDTO
        {
            Email = "testuser@test.com",
            Password = "WrongPassword@123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);

        errorResponse.Should().NotBeNull();
        errorResponse!.Error.Should().Be("AUTHENTICATION_FAILED");
        errorResponse.Message.Should().Contain("Invalid");
    }
}
