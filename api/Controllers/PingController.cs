using Microsoft.AspNetCore.Mvc;

namespace CampusConnect.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    private readonly ILogger<PingController> _logger;

    public PingController(ILogger<PingController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [HttpPost]
    public IActionResult Ping()
    {
        _logger.LogInformation("Ping endpoint was called");
        return Ok("Welcome to CampusConnect API!");
    }
}