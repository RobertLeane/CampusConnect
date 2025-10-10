namespace CampusConnect.Api.Models;

public class MemberDto
{
    // TODO: Add Member properties (MemberId, UserId, ClubId)
    public int MemberId { get; set; }
    public int UserId { get; set; }
    public int ClubId { get; set; }
    // Optional: Add navigation properties like UserName, ClubName for display purposes
}