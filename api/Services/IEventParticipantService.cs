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

}