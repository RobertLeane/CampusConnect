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
    public async Task<IActionResult> GetClubsAsync()
    {
        try
        {
            _logger.LogInformation("Getting clubs from database");
            var clubs = await _clubService.GetActiveClubsAsync().ConfigureAwait(false);
            _logger.LogInformation($"Retrieved {clubs.Count} active clubs from database");
            return Ok(clubs);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while retrieving clubs from database");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while retrieving clubs");
        }
        catch
        {
            throw;
        }
    }

    [HttpGet("{clubId:int}")]
    public async Task<IActionResult> GetClubByIdAsync(int clubId)
    {
        try
        {
            _logger.LogInformation($"Getting club {clubId} from database");
            var club = await _clubService.GetClubByIdAsync(clubId).ConfigureAwait(false);
            
            if (club == null)
            {
                return NotFound();
            }

            return Ok(club);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, $"Invalid operation while retrieving club {clubId} from database");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while retrieving the club");
        }
        catch
        {
            throw;
        }
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetUserJoinedClubsAsync(int userId)
    {
        try
        {
            _logger.LogInformation($"Getting clubs for user ID {userId}");
            var clubs = await _clubService.GetUserJoinedClubsAsync(userId).ConfigureAwait(false);
            return Ok(clubs);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, $"Invalid operation while retrieving clubs for user ID {userId}");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while retrieving clubs for the user");
        }
        catch
        {
            throw;
        }
    }
}