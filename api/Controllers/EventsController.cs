using Microsoft.AspNetCore.Mvc;
using CampusConnect.Api.Services;

namespace CampusConnect.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly ILogger<EventsController> _logger;
    private readonly IEventService _eventService;
    private readonly IClubService _clubService;

    public EventsController(ILogger<EventsController> logger, IEventService eventService, IClubService clubService)
    {
        _logger = logger;
        _eventService = eventService;
        _clubService = clubService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllEvents()
    {
        try
        {
            _logger.LogInformation("Getting all events");
            var events = await _eventService.GetAllEventsAsync();
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("{eventId:int}")]
    public async Task<IActionResult> GetEventById(int eventId)
    {
        try
        {
            _logger.LogInformation($"Getting event with ID {eventId}");
            var eventDto = await _eventService.GetEventByIdAsync(eventId);

            if (eventDto == null)
            {
                return NotFound();
            }
            return Ok(eventDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving event with ID {eventId}");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("clubs/{clubId:int}/events")]
    public async Task<IActionResult> GetEventsByClub(int clubId)
    {
        try
        {
            _logger.LogInformation($"Getting events for club ID {clubId}");
            // Verify club exists
            var club = await _clubService.GetClubByIdAsync(clubId);
            if (club == null)
            {
                return NotFound("Club not found");
            }

            var events = await _eventService.GetEventsByClubIdAsync(clubId);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving events for club ID {clubId}");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("{eventId:int}/rsvp")]
    public async Task<IActionResult> RsvpToEvent(int eventId, [FromQuery] int userId)
    {
        try
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid or missing userId");
            }

            var result = await _eventService.RsvpToEventAsync(eventId, userId);
            return result switch
            {
                IEventService.RsvpResult.Success => Ok(),
                IEventService.RsvpResult.EventNotFound => NotFound("Event not found"),
                IEventService.RsvpResult.UserNotFound => NotFound("User not found"),
                IEventService.RsvpResult.AlreadyRsvped => Conflict("User has already RSVPed"),
                _ => StatusCode(StatusCodes.Status500InternalServerError),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error RSVPing to event ID {eventId}");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}