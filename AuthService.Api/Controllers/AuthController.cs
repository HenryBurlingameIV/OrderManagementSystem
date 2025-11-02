using AuthService.Application;
using AuthService.Application.Contracts;
using AuthService.Application.DTO;
using AuthService.Infrastructure;
using AuthService.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;

        public AuthController(IAuthenticationService authService, IPasswordHasher passwordHasher, AuthDbContext context)
        {
            _authService = authService;
        }

        [HttpPost]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            var result = await _authService.LoginAsync(request, ct);
            return Ok(result);
        }
    }


}
