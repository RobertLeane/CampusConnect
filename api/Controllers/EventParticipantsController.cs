using Microsoft.AspNetCore.Mvc;
using CampusConnect.Api.Services;
using CampusConnect.Api.Models;

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
    public async Task<IActionResult> GetEventParticipantsAsync()
    {
        try
        {
            _logger.LogInformation("Getting Event Participants from database");
            var eventparticipants = await _eventParticipantService.GetActiveEventParticipantsAsync().ConfigureAwait(false);
            _logger.LogInformation($"Retrieved {eventparticipants.Count} active EventParticipants from database");
            return Ok(eventparticipants);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while retrieving event participants from database");
            return BadRequest("Invalid operation: " + ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving event participants from database");
            throw;
        }
    }

    [HttpGet("users/{memberId}/events")]
    public async Task<IActionResult> GetUserEventRegistrationsAsync(int memberId)
    {
        try
        {
            _logger.LogInformation($"Getting event registrations for member {memberId} from database");
            var userId = await _eventParticipantService.GetUserByMemberIdAsync(memberId).ConfigureAwait(false);
            if (userId == null)
            {
                return NotFound("Member not found");
            }

            var memberIds = await _eventParticipantService.GetMemberIdsByUserIdAsync(userId.Value).ConfigureAwait(false);
            var events = await _eventParticipantService.GetEventsByMemberIdsAsync(memberIds).ConfigureAwait(false);
            return Ok(events);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while retrieving event registrations");
            return BadRequest("Invalid operation: " + ex.Message);
        }
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> RegisterForEventAsync([FromBody] RegisterEventRequest request)
    {
        try
        {
            _logger.LogInformation("Registering user {UserId} for event {EventId} in club {ClubId}", 
                request.UserId, request.EventId, request.ClubId);

            var memberId = await _eventParticipantService.GetMemberIdByUserIdAndClubIdAsync(request.UserId, request.ClubId).ConfigureAwait(false);
            if (memberId == null)
            {
                return BadRequest("User is not a member of this club");
            }

            await _eventParticipantService.RegisterUserForEventAsync(memberId.Value, request.ClubId, request.EventId).ConfigureAwait(false);
            
            var result = new EventParticipantDto
            {
                MemberId = memberId.Value,
                EventId = request.EventId,
                ClubId = request.ClubId
            };
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Registration conflict for UserId={UserId}, EventId={EventId}", 
                request.UserId, request.EventId);
            return Conflict(ex.Message);
        }
    }

    public class RegisterEventRequest
    {
        public int UserId { get; set; }
        public int EventId { get; set; }
        public int ClubId { get; set; }
    }
    
    [HttpPost("users/{userId}/events/{eventId}/register")]
    public async Task<IActionResult> RegisterForEventAsync(int userId, int eventId)
    {
        try
        {
            _logger.LogInformation("Registering user {UserId} for event {EventId}", userId, eventId);

            var clubId = await _eventParticipantService.GetClubIdByEventIdAsync(eventId).ConfigureAwait(false);
            if (clubId == null)
                return NotFound("Event not found or club not found");

            var memberId = await _eventParticipantService.GetMemberIdByUserIdAndClubIdAsync(userId, clubId.Value).ConfigureAwait(false);
            if (memberId == null)
                return NotFound("User is not a member of the club associated with the event.");

            await _eventParticipantService.RegisterUserForEventAsync(memberId.Value, clubId.Value, eventId).ConfigureAwait(false);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while registering user for event");
            return BadRequest("Invalid operation: " + ex.Message);
        }
    }
}