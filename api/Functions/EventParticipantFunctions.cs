using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CampusConnect.Api.Services;

namespace CampusConnect.Api.Functions;

public class EventParticipantFunctions
{
    private readonly ILogger<EventParticipantFunctions> _logger;
    private readonly IEventParticipantService _eventParticipantService;

    public EventParticipantFunctions(ILogger<EventParticipantFunctions> logger, IEventParticipantService eventParticipantService)
    {
        _logger = logger;
        _eventParticipantService = eventParticipantService;
    }

    [Function("GetEventParticipants")]
    public async Task<IActionResult> GetEventParticipants(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "eventparticipants")] HttpRequest req)
    {
        try
        {
            _logger.LogInformation("Getting Event Participants from database");
            var eventparticipants = await _eventParticipantService.GetActiveEventParticipantsAsync();
            _logger.LogInformation($"Retrieved {eventparticipants.Count} active EventParticipants from database");
            return new OkObjectResult(eventparticipants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event participants from database");
            return new ObjectResult("An error occurred while retrieving event participants")
            {
                StatusCode = 500
            };
        }
    }

    [Function("GetUserEventRegistrations")]
    public async Task<IActionResult> GetUserEventRegistrations(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/{memberId}/events")] HttpRequest req,
        int memberId)
    {
        try
        {
            _logger.LogInformation($"Getting event registrations for member {memberId} from database");
            var userId = await _eventParticipantService.GetUserByMemberIdAsync(memberId);
            if (userId == null)
            {
                return new NotFoundObjectResult("Member not found");
            }

            var memberIds = await _eventParticipantService.GetMemberIdsByUserIdAsync(userId.Value);
            var events = await _eventParticipantService.GetEventsByMemberIdsAsync(memberIds);
            return new OkObjectResult(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event registrations");
            return new ObjectResult("An error occurred while retrieving the event registrations")
            {
                StatusCode = 500
            };
        }
    }
    
    [Function("RegisterForEvent")]
    public async Task<IActionResult> RegisterForEvent(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users/{userId}/events/{eventId}/register")] HttpRequest req,
    int userId,int eventId)
    {
        try
        {
            _logger.LogInformation("Registering user {UserId} for event {EventId}", userId, eventId);

            var clubId = await _eventParticipantService.GetClubIdByEventIdAsync(eventId);
            if (clubId == null)
                return new NotFoundObjectResult("Event not found or club not found");

            var memberId = await _eventParticipantService.GetMemberIdByUserIdAndClubIdAsync(userId, clubId.Value);
            if (memberId == null)
                return new NotFoundObjectResult("User is not a member of the club associated with the event.");

            await _eventParticipantService.RegisterUserForEventAsync(memberId.Value, clubId.Value, eventId);
            return new OkResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event participants from database");
            return new ObjectResult("An error occurred while retrieving event participants")
            {
                StatusCode = 500
            };
        }
    }
  

    
    // Use ClubFunctions.cs as a reference for the structure
    // Remember to:
    // 1. Use appropriate HTTP triggers (GET, POST, PUT, DELETE)
    // 2. Define proper routes (e.g., "events/{eventId:int}/participants", "users/{userId:int}/events")
    // 3. Handle exceptions and return appropriate HTTP status codes
    // 4. Log information and errors appropriately
}