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
    public async Task<IActionResult> Register([FromBody] RegisterRequest model)
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

            if (await _userService.EmailExistsAsync(model.Email))
                return Conflict(new { Message = "Email already exists" });

            var userId = await _userService.RegisterAsync(model);
            var resp = new AuthResponse
            {
                UserId = userId,
                Email = model.Email,
                Message = "User registered"
            };

            return CreatedAtAction(nameof(GetUser), "Users", new { id = userId }, resp);
        }
        catch (InvalidOperationException dup) when (dup.Message.Contains("exists", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new { Message = dup.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
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

            var authResponse = await _userService.LoginAsync(model);
            if (authResponse == null)
            {
                return Unauthorized(new { Message = "Invalid email or password" });
            }

            return Ok(authResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    // Placeholder for GetUser action referenced in CreatedAtAction
    private IActionResult GetUser(int id)
    {
        // This would be implemented in UsersController
        return Ok();
    }
}