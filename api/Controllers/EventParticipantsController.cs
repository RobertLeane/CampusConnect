using Microsoft.AspNetCore.Mvc;
using CampusConnect.Api.Services;

namespace CampusConnect.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventParticipantsController : ControllerBase
{
    private readonly ILogger<EventParticipantsController> _logger;
    private readonly IEventParticipantService _eventParticipantService;

    public EventParticipantsController(ILogger<EventParticipantsController> logger, IEventParticipantService eventParticipantService)
    {
        _logger = logger;
        _eventParticipantService = eventParticipantService;
    }

    [HttpGet]
    public async Task<IActionResult> GetEventParticipants()
    {
        try
        {
            _logger.LogInformation("Getting Event Participants from database");
            var eventparticipants = await _eventParticipantService.GetActiveEventParticipantsAsync();
            _logger.LogInformation($"Retrieved {eventparticipants.Count} active EventParticipants from database");
            return Ok(eventparticipants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event participants from database");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while retrieving event participants");
        }
    }

    [HttpGet("users/{memberId}/events")]
    public async Task<IActionResult> GetUserEventRegistrations(int memberId)
    {
        try
        {
            _logger.LogInformation($"Getting event registrations for member {memberId} from database");
            var userId = await _eventParticipantService.GetUserByMemberIdAsync(memberId);
            if (userId == null)
            {
                return NotFound("Member not found");
            }

            var memberIds = await _eventParticipantService.GetMemberIdsByUserIdAsync(userId.Value);
            var events = await _eventParticipantService.GetEventsByMemberIdsAsync(memberIds);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event registrations");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while retrieving the event registrations");
        }
    }
    
    [HttpPost("users/{userId}/events/{eventId}/register")]
    public async Task<IActionResult> RegisterForEvent(int userId, int eventId)
    {
        try
        {
            _logger.LogInformation("Registering user {UserId} for event {EventId}", userId, eventId);

            var clubId = await _eventParticipantService.GetClubIdByEventIdAsync(eventId);
            if (clubId == null)
                return NotFound("Event not found or club not found");

            var memberId = await _eventParticipantService.GetMemberIdByUserIdAndClubIdAsync(userId, clubId.Value);
            if (memberId == null)
                return NotFound("User is not a member of the club associated with the event.");

            await _eventParticipantService.RegisterUserForEventAsync(memberId.Value, clubId.Value, eventId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user for event");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while registering for the event");
        }
    }
}