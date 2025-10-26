using CampusConnect.Api.Models;

namespace CampusConnect.Api.Services;

public interface IMemberService
{
    Task<List<MemberDto>> GetMembersByClubIdAsync(int clubId);
    Task<List<MemberDto>> GetMembersByUserIdAsync(int userId);
    Task<MemberDto?> GetMemberByIdAsync(int memberId);
    Task<MemberDto> AddMemberToClubAsync(int userId, int clubId, string role);
}