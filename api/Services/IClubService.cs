using CampusConnect.Api.Models;

namespace CampusConnect.Api.Services;

public interface IClubService
{
    Task<List<ClubDto>> GetActiveClubsAsync();
    Task<ClubDto?> GetClubByIdAsync(int clubId);
    Task<ClubDto> CreateClubAsync(ClubDto club);
    Task<List<ClubDto>> GetUserJoinedClubsAsync(int userId);
}