using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using backend.DTO;
using backend.DTOs.Moto;
using backend.IntegrationTests.Helpers;
using backend.Models;
using backend.Models.ApiResponses;
using FluentAssertions;

namespace backend.IntegrationTests.Controllers;

public class MotosControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public MotosControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var loginDto = new AuthDTO
        {
            Email = "testuser@test.com",
            Password = "Test@123"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth/login", loginDto);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

        return result.GetProperty("token").GetString()!;
    }

    private async Task<MotoResponseDto> CreateMotoAsync(string token, string placa, string chassi)
    {
        var createDto = new CreateMotoDto
        {
            Chassi = chassi,
            Placa = placa,
            Modelo = ModeloMoto.MottuSportESD,
            Status = MotoStatus.Disponivel
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJsonAsync("/api/v1/Motos", createDto);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<MotoResponseDto>(content, _jsonOptions)!;
    }

    [Fact]
    public async Task GetMotos_WithValidAuth_ReturnsPagedList()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/Motos?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResponse<MotoResponseDto>>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Pagination.Should().NotBeNull();
        result.Pagination.Page.Should().Be(1);
        result.Pagination.PageSize.Should().Be(10);
        result.Pagination.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CreateMoto_WithValidData_ReturnsCreated()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var uniquePlaca = $"ABC{Random.Shared.Next(1, 9)}D{Random.Shared.Next(10, 99)}";
        var uniqueChassi = $"CHASSI-{Guid.NewGuid():N}";

        var createDto = new CreateMotoDto
        {
            Chassi = uniqueChassi,
            Placa = uniquePlaca,
            Modelo = ModeloMoto.MottuSportESD,
            Status = MotoStatus.Disponivel
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Motos", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        var moto = JsonSerializer.Deserialize<MotoResponseDto>(content, _jsonOptions);

        moto.Should().NotBeNull();
        moto!.Id.Should().BeGreaterThan(0);
        moto.Chassi.Should().Be(uniqueChassi);
        moto.Placa.Should().Be(uniquePlaca);
        moto.Modelo.Should().Be("MottuSportESD");
        moto.Status.Should().Be("Disponivel");
    }

    [Fact]
    public async Task CreateMoto_WithDuplicatePlaca_ReturnsConflict()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var placa = $"ABC{Random.Shared.Next(1, 9)}D{Random.Shared.Next(10, 99)}";

        // Create first moto
        await CreateMotoAsync(token, placa, $"CHASSI-{Guid.NewGuid():N}");

        // Attempt to create second moto with same placa
        var duplicateDto = new CreateMotoDto
        {
            Chassi = $"CHASSI-{Guid.NewGuid():N}",
            Placa = placa,
            Modelo = ModeloMoto.MottuSport,
            Status = MotoStatus.Disponivel
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Motos", duplicateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);

        errorResponse.Should().NotBeNull();
        errorResponse!.Error.Should().Contain("DUPLICATE");
        errorResponse.Message.Should().Contain("Placa");
    }

    [Fact]
    public async Task GetMoto_ById_ReturnsDetail()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var placa = $"ABC{Random.Shared.Next(1, 9)}D{Random.Shared.Next(10, 99)}";
        var chassi = $"CHASSI-{Guid.NewGuid():N}";

        var createdMoto = await CreateMotoAsync(token, placa, chassi);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/api/v1/Motos/{createdMoto.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var moto = JsonSerializer.Deserialize<MotoResponseDto>(content, _jsonOptions);

        moto.Should().NotBeNull();
        moto!.Id.Should().Be(createdMoto.Id);
        moto.Chassi.Should().Be(chassi);
        moto.Placa.Should().Be(placa);
        moto.Modelo.Should().Be("MottuSportESD");
        moto.Status.Should().Be("Disponivel");
    }

    [Fact]
    public async Task GetMotos_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        // Ensure no Authorization header is set
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync("/api/v1/Motos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
