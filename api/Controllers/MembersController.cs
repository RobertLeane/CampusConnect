using Microsoft.AspNetCore.Mvc;
using CampusConnect.Api.Services;

namespace CampusConnect.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly ILogger<MembersController> _logger;
    private readonly IMemberService _memberService;

    public MembersController(ILogger<MembersController> logger, IMemberService memberService)
    {
        _logger = logger;
        _memberService = memberService;
    }

    [HttpGet("clubs/{clubId:int}/members")]
    public async Task<IActionResult> GetClubMembers(int clubId)
    {
        try
        {
            _logger.LogInformation($"Getting members for club {clubId} from database");
            var members = await _memberService.GetMembersByClubIdAsync(clubId);
            _logger.LogInformation($"Retrieved {members?.Count ?? 0} members for club {clubId}");
            return Ok(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving members for club {ClubId}", clubId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while retrieving club members");
        }
    }

    [HttpGet("users/{userId:int}/memberships")]
    public async Task<IActionResult> GetUserMemberships(int userId)
    {
        try
        {
            _logger.LogInformation($"Getting memberships for user {userId} from database");
            var memberships = await _memberService.GetMembersByUserIdAsync(userId);
            _logger.LogInformation($"Retrieved {memberships?.Count ?? 0} memberships for user {userId}");
            return Ok(memberships);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving memberships for user {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while retrieving user memberships");
        }
    }

    [HttpPost("clubs/{clubId:int}/members")]
    public async Task<IActionResult> AddMemberToClub(int clubId, [FromBody] AddMemberRequest request)
    {
        try
        {
            if (request == null || request.UserId <= 0)
            {
                return BadRequest("Invalid request body. Expecting { userId: int, role: string }");
            }

            _logger.LogInformation($"Adding user {request.UserId} to club {clubId} with role '{request.Role}'");
            var member = await _memberService.AddMemberToClubAsync(request.UserId, clubId, request.Role ?? "Member");
            return CreatedAtAction(nameof(GetClubMembers), new { clubId = clubId }, member);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding member to club {ClubId}", clubId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while adding the member to the club");
        }
    }

    // DTO for adding members
    public class AddMemberRequest
    {
        public int UserId { get; set; }
        public string? Role { get; set; }
    }
}