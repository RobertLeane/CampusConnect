using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CampusConnect.Api.Models;
using CampusConnect.Api.Services;

namespace CampusConnect.Api.Functions;

public class AuthFunctions
{
    private readonly ILogger<AuthFunctions> _logger;
    private readonly IUserService _userService;

    public AuthFunctions(ILogger<AuthFunctions> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [Function("RegisterUser")]
    public async Task<IActionResult> Register(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/register")] HttpRequest req)
    {
        try
        {
            using var reader = new StreamReader(req.Body);
            var body = await reader.ReadToEndAsync();

            var jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var model = JsonSerializer.Deserialize<RegisterRequest>(body, jsonOpts);
            if (model is null)
                return new BadRequestObjectResult("Invalid JSON payload.");

            var ctx = new ValidationContext(model);
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(model, ctx, results, true))
            {
                var errors = results.Select(r => r.ErrorMessage).ToArray();
                return new BadRequestObjectResult(new { Message = "Validation failed", Errors = errors });
            }

            if (await _userService.EmailExistsAsync(model.Email))
                return new ConflictObjectResult(new { Message = "Email already exists" });

            var userId = await _userService.RegisterAsync(model);
            var resp = new AuthResponse
            {
                UserId = userId,
                Email = model.Email,
                Message = "User registered"
            };

            return new CreatedResult($"/api/users/{userId}", resp);
        }
        catch (InvalidOperationException dup) when (dup.Message.Contains("exists", StringComparison.OrdinalIgnoreCase))
        {
            return new ConflictObjectResult(new { Message = dup.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
