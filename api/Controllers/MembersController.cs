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
    public async Task<IActionResult> GetClubMembersAsync(int clubId)
    {
        try
        {
            _logger.LogInformation($"Getting members for club {clubId} from database");
            var members = await _memberService.GetMembersByClubIdAsync(clubId).ConfigureAwait(false);
            _logger.LogInformation($"Retrieved {members?.Count ?? 0} members for club {clubId}");
            return Ok(members);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while retrieving members for club {ClubId}", clubId);
            return BadRequest("Invalid operation: " + ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving members for club {ClubId}", clubId);
            throw; // Rethrow to let ASP.NET Core handle unexpected exceptions
        }
    }

    [HttpGet("users/{userId:int}/memberships")]
    public async Task<IActionResult> GetUserMembershipsAsync(int userId)
    {
        try
        {
            _logger.LogInformation($"Getting memberships for user {userId} from database");
            var memberships = await _memberService.GetMembersByUserIdAsync(userId).ConfigureAwait(false);
            _logger.LogInformation($"Retrieved {memberships?.Count ?? 0} memberships for user {userId}");
            return Ok(memberships);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while retrieving memberships for user {UserId}", userId);
            return BadRequest("Invalid operation: " + ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving memberships for user {UserId}", userId);
            throw; // Rethrow to let ASP.NET Core handle unexpected exceptions
        }
    }

    [HttpPost("clubs/{clubId:int}/members")]
    public async Task<IActionResult> AddMemberToClubAsync(int clubId, [FromBody] AddMemberRequest request)
    {
        try
        {
            if (request == null || request.UserId <= 0)
            {
                return BadRequest("Invalid request body. Expecting { userId: int, role: string }");
            }

            _logger.LogInformation($"Adding user {request.UserId} to club {clubId} with role '{request.Role}'");
            var member = await _memberService.AddMemberToClubAsync(request.UserId, clubId, request.Role ?? "Member").ConfigureAwait(false);
            
            // Use Created instead of CreatedAtAction to avoid route generation issues
            return Created($"/api/members/clubs/{clubId}/members", member);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while adding member to club {ClubId}", clubId);
            return BadRequest("Invalid operation: " + ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding member to club {ClubId}", clubId);
            throw; // Rethrow to let ASP.NET Core handle unexpected exceptions
        }
    }

    // DTO for adding members
    public class AddMemberRequest
    {
        public int UserId { get; set; }
        public string? Role { get; set; }
    }
}