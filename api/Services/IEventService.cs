using CampusConnect.Api.Models;

namespace CampusConnect.Api.Services;

public interface IEventService
{
    // TODO: Add method signatures for Event operations
    // Examples:
    // Task<List<EventDto>> GetAllEventsAsync();
        Task<List<EventDto>> GetAllEventsAsync();
    // Task<List<EventDto>> GetEventsByClubIdAsync(int clubId);

    // Task<EventDto?> GetEventByIdAsync(int eventId);
    Task<EventDto?> GetEventByIdAsync(int eventId);
    // Task<EventDto> CreateEventAsync(EventDto eventDto);
    Task<List<EventDto>> GetEventsByClubIdAsync(int clubId);
    Task<EventDto> CreateEventAsync(EventDto eventDto);
}