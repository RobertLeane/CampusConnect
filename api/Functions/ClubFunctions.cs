using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CampusConnect.Api.Services;

namespace CampusConnect.Api.Functions;

public class ClubFunctions
{
    private readonly ILogger<ClubFunctions> _logger;
    private readonly IClubService _clubService;

    public ClubFunctions(ILogger<ClubFunctions> logger, IClubService clubService)
    {
        _logger = logger;
        _clubService = clubService;
    }

    [Function("GetClubs")]
    public async Task<IActionResult> GetClubs(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "clubs")] HttpRequest req)
    {
        try
        {
            _logger.LogInformation("Getting clubs from database");
            var clubs = await _clubService.GetActiveClubsAsync();
            _logger.LogInformation($"Retrieved {clubs.Count} active clubs from database");
            return new OkObjectResult(clubs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving clubs from database");
            return new ObjectResult("An error occurred while retrieving clubs")
            {
                StatusCode = 500
            };
        }
    }

    [Function("GetClubById")]
    public async Task<IActionResult> GetClubById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "clubs/{clubId:int}")] HttpRequest req,
        int clubId)
    {
        try
        {
            _logger.LogInformation($"Getting club {clubId} from database");
            var club = await _clubService.GetClubByIdAsync(clubId);
            
            if (club == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(club);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving club {clubId} from database");
            return new ObjectResult("An error occurred while retrieving the club")
            {
                StatusCode = 500
            };
        }
    }
}