using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CampusConnect.Api.Services;

namespace CampusConnect.Api.Functions;

public class EventFunctions
{
    private readonly ILogger<EventFunctions> _logger;
    private readonly IEventService _eventService;

    private readonly IClubService _clubService;

    public EventFunctions(ILogger<EventFunctions> logger, IEventService eventService, IClubService clubService)
    {
        _logger = logger;
        _eventService = eventService;
        _clubService = clubService;
    }

    [Function("GetEvents")]
    public async Task<IActionResult> GetAllEvents(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "events")] HttpRequest req)
    {
        try
        {
            _logger.LogInformation("Getting all events");
            var events = await _eventService.GetAllEventsAsync();
            return new OkObjectResult(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
    // [Function("GetEventById")]
    [Function("GetEventById")]
    public async Task<IActionResult> GetEventById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "events/{eventId:int}")] HttpRequest req,
        int eventId)
    {
        try
        {
            _logger.LogInformation($"Getting event with ID {eventId}");
            var eventDto = await _eventService.GetEventByIdAsync(eventId);

            if (eventDto == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(eventDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving event with ID {eventId}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
    // [Function("GetEventsByClub")]
    [Function("GetEventsByClub")]
    public async Task<IActionResult> GetEventsByClub(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "clubs/{clubId:int}/events")] HttpRequest req,
        int clubId)
    {
        try
        {
            _logger.LogInformation($"Getting events for club ID {clubId}");
            // Verify club exists
            var club = await _clubService.GetClubByIdAsync(clubId);
            if (club == null)
            {
                return new NotFoundObjectResult("Club not found");
            }

            var events = await _eventService.GetEventsByClubIdAsync(clubId);
            return new OkObjectResult(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving events for club ID {clubId}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    [Function("RsvpToEvent")]
    public async Task<IActionResult> RsvpToEvent(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "events/{eventId:int}/rsvp")] HttpRequest req,
        int eventId)
    {
        try
        {
            // For simplicity, assume userId is passed as a query parameter
            if (!int.TryParse(req.Query["userId"], out var userId))
            {
                return new BadRequestObjectResult("Invalid or missing userId");
            }

            var result = await _eventService.RsvpToEventAsync(eventId, userId);
            return result switch
            {
                IEventService.RsvpResult.Success => new OkResult(),
                IEventService.RsvpResult.EventNotFound => new NotFoundObjectResult("Event not found"),
                IEventService.RsvpResult.UserNotFound => new NotFoundObjectResult("User not found"),
                IEventService.RsvpResult.AlreadyRsvped => new ConflictObjectResult("User has already RSVPed"),
                _ => new StatusCodeResult(StatusCodes.Status500InternalServerError),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error RSVPing to event ID {eventId}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}

    // [Function("CreateEvent")]
    // 
    // Use ClubFunctions.cs as a reference for the structure
    // Remember to:
    // 1. Use appropriate HTTP triggers (GET, POST, PUT, DELETE)
    // 2. Define proper routes (e.g., "events", "events/{eventId:int}", "clubs/{clubId:int}/events")
    // 3. Handle exceptions and return appropriate HTTP status codes
    // 4. Log information and errors appropriately
