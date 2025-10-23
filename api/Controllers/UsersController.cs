using Microsoft.AspNetCore.Mvc;
using CampusConnect.Api.Services;

namespace CampusConnect.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetUserCount()
    {
        try
        {
            _logger.LogInformation("Getting user count");
            var count = await _userService.GetUserCountAsync();
            _logger.LogInformation($"Returning user count: {count}");
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user count");
            return StatusCode(500, "Internal server error");
        }
    }
}