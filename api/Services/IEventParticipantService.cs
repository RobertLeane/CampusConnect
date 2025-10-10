using CampusConnect.Api.Models;

namespace CampusConnect.Api.Services;

public interface IEventParticipantService
{
    Task<List<EventParticipantDto>> GetActiveEventParticipantsAsync();
    Task<int?> GetUserByMemberIdAsync(int memberId);
    Task<List<int>> GetMemberIdsByUserIdAsync(int userId);
    Task<List<EventDto>> GetEventsByMemberIdsAsync(IEnumerable<int> memberIds);
    Task<int?> GetClubIdByEventIdAsync(int eventId);
    Task<int?> GetMemberIdByUserIdAndClubIdAsync(int userId, int clubId);
    Task RegisterUserForEventAsync(int memberId, int clubId, int eventId);
    
    // TODO: Add method signatures for EventParticipant operations
    // Examples:
    // Task<List<EventParticipantDto>> GetParticipantsByEventIdAsync(int eventId);
    // Task<List<EventParticipantDto>> GetEventsByUserIdAsync(int userId);
    // Task<EventParticipantDto?> GetParticipantByIdAsync(int participantId);
    // Task<EventParticipantDto> RegisterUserForEventAsync(int userId, int eventId);
}