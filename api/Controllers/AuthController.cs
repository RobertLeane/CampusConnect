using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using CampusConnect.Api.Models;
using CampusConnect.Api.Services;

namespace CampusConnect.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IUserService _userService;

    public AuthController(ILogger<AuthController> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest model)
    {
        try
        {
            if (model is null)
                return BadRequest("Invalid JSON payload.");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();
                return BadRequest(new { Message = "Validation failed", Errors = errors });
            }

            if (await _userService.EmailExistsAsync(model.Email).ConfigureAwait(false))
                return Conflict(new { Message = "Email already exists" });

            var userId = await _userService.RegisterAsync(model).ConfigureAwait(false);
            var resp = new AuthResponse
            {
                UserId = userId,
                Email = model.Email,
                Message = "User registered"
            };

            return Ok(resp);
        }
        catch (InvalidOperationException dup) when (dup.Message.Contains("exists", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new { Message = dup.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            throw;
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest model)
    {
        try
        {
            if (model is null)
                return BadRequest("Invalid JSON payload.");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();
                return BadRequest(new { Message = "Validation failed", Errors = errors });
            }

            var authResponse = await _userService.LoginAsync(model).ConfigureAwait(false);
            if (authResponse == null)
            {
                return Unauthorized(new { Message = "Invalid email or password" });
            }

            return Ok(authResponse);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}