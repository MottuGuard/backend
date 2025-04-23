using backend.DTO;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(AuthDTO authDTO)
    {
        return await _authService.Authenticate(authDTO);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDTO registerDTO)
    {
        return await _authService.Register(registerDTO);
    }
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken(RefreshDTO refreshDTO)
    {
        return await _authService.Authenticate(refreshDTO);
    }
}