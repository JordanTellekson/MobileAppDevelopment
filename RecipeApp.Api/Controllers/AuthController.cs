using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RecipeApp.Shared.DTOs;
using RecipeApp.Shared.Services;
using System;
using System.Threading.Tasks;

namespace RecipeApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request == null)
                return BadRequest("Registration data is missing.");

            try
            {
                var response = await _authService.RegisterAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {Username}", request.Username);
                return StatusCode(500, "Internal server error.");
            }
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            if (request == null)
                return BadRequest("Login data is missing.");

            try
            {
                var response = await _authService.LoginAsync(request);
                if (response == null)
                    return Unauthorized("Invalid credentials.");

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in user {Username}", request.Username);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}