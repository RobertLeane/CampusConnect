using Microsoft.AspNetCore.Mvc;
using CampusConnect.Api.Services;

namespace CampusConnect.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClubsController : ControllerBase
{
    private readonly ILogger<ClubsController> _logger;
    private readonly IClubService _clubService;

    public ClubsController(ILogger<ClubsController> logger, IClubService clubService)
    {
        _logger = logger;
        _clubService = clubService;
    }

    [HttpGet]
    public async Task<IActionResult> GetClubs()
    {
        try
        {
            _logger.LogInformation("Getting clubs from database");
            var clubs = await _clubService.GetActiveClubsAsync();
            _logger.LogInformation($"Retrieved {clubs.Count} active clubs from database");
            return Ok(clubs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving clubs from database");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while retrieving clubs");
        }
    }

    [HttpGet("{clubId:int}")]
    public async Task<IActionResult> GetClubById(int clubId)
    {
        try
        {
            _logger.LogInformation($"Getting club {clubId} from database");
            var club = await _clubService.GetClubByIdAsync(clubId);
            
            if (club == null)
            {
                return NotFound();
            }

            return Ok(club);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving club {clubId} from database");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while retrieving the club");
        }
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetUserJoinedClubs(int userId)
    {
        try
        {
            _logger.LogInformation($"Getting clubs for user ID {userId}");
            var clubs = await _clubService.GetUserJoinedClubsAsync(userId);
            return Ok(clubs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving clubs for user ID {userId}");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}