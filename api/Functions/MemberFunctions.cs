using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CampusConnect.Api.Services;

namespace CampusConnect.Api.Functions;

public class MemberFunctions
{
    private readonly ILogger<MemberFunctions> _logger;
    private readonly IMemberService _memberService;

    public MemberFunctions(ILogger<MemberFunctions> logger, IMemberService memberService)
    {
        _logger = logger;
        _memberService = memberService;
    }
    [Function("GetClubMembers")]
    public async Task<IActionResult> GetClubMembers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "clubs/{clubId:int}/members")] HttpRequest req,
        int clubId)
    {
        try
        {
            _logger.LogInformation($"Getting members for club {clubId} from database");
            var members = await _memberService.GetMembersByClubIdAsync(clubId);
            _logger.LogInformation($"Retrieved {members?.Count ?? 0} members for club {clubId}");
            return new OkObjectResult(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving members for club {ClubId}", clubId);
            return new ObjectResult("An error occurred while retrieving club members") { StatusCode = 500 };
        }
    }

    [Function("GetUserMemberships")]
    public async Task<IActionResult> GetUserMemberships(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/{userId:int}/memberships")] HttpRequest req,
        int userId)
    {
        try
        {
            _logger.LogInformation($"Getting memberships for user {userId} from database");
            var memberships = await _memberService.GetMembersByUserIdAsync(userId);
            _logger.LogInformation($"Retrieved {memberships?.Count ?? 0} memberships for user {userId}");
            return new OkObjectResult(memberships);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving memberships for user {UserId}", userId);
            return new ObjectResult("An error occurred while retrieving user memberships") { StatusCode = 500 };
        }
    }

    [Function("AddMemberToClub")]
    public async Task<IActionResult> AddMemberToClub(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "clubs/{clubId:int}/members")] HttpRequest req,
        int clubId)
    {
        try
        {
            // Expect a simple JSON body like { "userId": 123, "role": "Member" }
            var body = await System.Text.Json.JsonSerializer.DeserializeAsync<AddMemberRequest>(req.Body, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (body == null || body.UserId <= 0)
            {
                return new BadRequestObjectResult("Invalid request body. Expecting { userId: int, role: string }");
            }

            _logger.LogInformation($"Adding user {body.UserId} to club {clubId} with role '{body.Role}'");
            var member = await _memberService.AddMemberToClubAsync(body.UserId, clubId, body.Role ?? "Member");
            return new CreatedResult($"/clubs/{clubId}/members/{member?.MemberId}", member);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding member to club {ClubId}", clubId);
            return new ObjectResult("An error occurred while adding the member to the club") { StatusCode = 500 };
        }
    }

    // Small DTO used for deserializing AddMember requests
    private class AddMemberRequest
    {
        public int UserId { get; set; }
        public string? Role { get; set; }
    }
}