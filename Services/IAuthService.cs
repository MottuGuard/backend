using backend.DTO;
using Microsoft.AspNetCore.Mvc;

namespace backend.Services
{
    public interface IAuthService
    {
        public Task<IActionResult> Authenticate(AuthDTO authDTO);
        public Task<IActionResult> Authenticate(RefreshDTO refreshDTO);
        public Task<IActionResult> Register(RegisterDTO registerDTO);
    }
}
