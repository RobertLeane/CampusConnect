using CampusConnect.Api.Models;

namespace CampusConnect.Api.Services;

public interface IEventService
{
    Task<List<EventDto>> GetAllEventsAsync();
    Task<EventDto?> GetEventByIdAsync(int eventId);
    Task<List<EventDto>> GetEventsByClubIdAsync(int clubId);
    Task<RsvpResult> RsvpToEventAsync(int eventId, int userId);
    Task<List<EventDto>> GetUserRsvpEventsAsync(int userId);
    
    public enum RsvpResult
    {
        Success,
        EventNotFound,
        UserNotFound,
        NotClubMember,    
        AlreadyRsvped,
        Error
    }
}
