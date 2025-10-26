using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CampusConnect.Api.Models;

namespace CampusConnect.Api.Services;

public class MemberService : IMemberService
{
    private readonly ILogger<MemberService> _logger;
    private readonly IConfiguration _configuration;

    public MemberService(ILogger<MemberService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<List<MemberDto>> GetMembersByClubIdAsync(int clubId)
    {
        try
        {
            _logger.LogInformation("Getting members for club {ClubId}", clubId);

            await using var conn = await SqlService.CreateConnection(_configuration);
            await using var cmd = new SqlCommand("SELECT MemberId, UserId, ClubId FROM dbo.Members WHERE ClubId = @ClubId", conn);
            cmd.Parameters.Add(new SqlParameter("@ClubId", System.Data.SqlDbType.Int) { Value = clubId });
            await using var reader = await cmd.ExecuteReaderAsync();

            var list = new List<MemberDto>();
            while (await reader.ReadAsync())
            {
                list.Add(new MemberDto
                {
                    MemberId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    ClubId = reader.GetInt32(2)
                });
            }

            _logger.LogInformation("Retrieved {Count} members for club {ClubId}", list.Count, clubId);
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving members for club {ClubId}", clubId);
            throw;
        }
    }

    public async Task<List<MemberDto>> GetMembersByUserIdAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Getting members for user {UserId}", userId);

            await using var conn = await SqlService.CreateConnection(_configuration);
            await using var cmd = new SqlCommand("SELECT MemberId, UserId, ClubId FROM dbo.Members WHERE UserId = @UserId", conn);
            cmd.Parameters.Add(new SqlParameter("@UserId", System.Data.SqlDbType.Int) { Value = userId });
            await using var reader = await cmd.ExecuteReaderAsync();

            var list = new List<MemberDto>();
            while (await reader.ReadAsync())
            {
                list.Add(new MemberDto
                {
                    MemberId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    ClubId = reader.GetInt32(2)
                });
            }

            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving members for user {UserId}", userId);
            throw;
        }
    }

    public async Task<MemberDto?> GetMemberByIdAsync(int memberId)
    {
        try
        {
            _logger.LogInformation("Getting member {MemberId}", memberId);

            await using var conn = await SqlService.CreateConnection(_configuration);
            await using var cmd = new SqlCommand("SELECT MemberId, UserId, ClubId FROM dbo.Members WHERE MemberId = @MemberId", conn);
            cmd.Parameters.Add(new SqlParameter("@MemberId", System.Data.SqlDbType.Int) { Value = memberId });
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var member = new MemberDto
                {
                    MemberId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    ClubId = reader.GetInt32(2)
                };
                return member;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting member by id {MemberId}", memberId);
            throw;
        }
    }

    public async Task<MemberDto> AddMemberToClubAsync(int userId, int clubId, string role)
    {
        try
        {
            _logger.LogInformation("Adding member for user {UserId} to club {ClubId}", userId, clubId);

            await using var conn = await SqlService.CreateConnection(_configuration);

            // Validate user exists
            await using (var checkUserCmd = new SqlCommand("SELECT COUNT(1) FROM dbo.Users WHERE UserId = @UserId", conn))
            {
                checkUserCmd.Parameters.Add(new SqlParameter("@UserId", System.Data.SqlDbType.Int) { Value = userId });
                var userExists = await checkUserCmd.ExecuteScalarAsync();
                if (userExists == null || Convert.ToInt32(userExists) == 0)
                {
                    _logger.LogError("User {UserId} does not exist", userId);
                    throw new InvalidOperationException($"User with ID {userId} does not exist");
                }
            }

            // Validate club exists
            await using (var checkClubCmd = new SqlCommand("SELECT COUNT(1) FROM dbo.Clubs WHERE ClubId = @ClubId", conn))
            {
                checkClubCmd.Parameters.Add(new SqlParameter("@ClubId", System.Data.SqlDbType.Int) { Value = clubId });
                var clubExists = await checkClubCmd.ExecuteScalarAsync();
                if (clubExists == null || Convert.ToInt32(clubExists) == 0)
                {
                    _logger.LogError("Club {ClubId} does not exist", clubId);
                    throw new InvalidOperationException($"Club with ID {clubId} does not exist");
                }
            }

            // Prevent duplicate membership
            await using (var checkCmd = new SqlCommand("SELECT MemberId FROM dbo.Members WHERE UserId = @UserId AND ClubId = @ClubId", conn))
            {
                checkCmd.Parameters.Add(new SqlParameter("@UserId", System.Data.SqlDbType.Int) { Value = userId });
                checkCmd.Parameters.Add(new SqlParameter("@ClubId", System.Data.SqlDbType.Int) { Value = clubId });
                var exists = await checkCmd.ExecuteScalarAsync();
                if (exists != null && exists != DBNull.Value)
                {
                    var existingId = Convert.ToInt32(exists);
                    var existing = await GetMemberByIdAsync(existingId);
                    if (existing != null) return existing;
                }
            }

            await using (var insertCmd = new SqlCommand("INSERT INTO dbo.Members (UserId, ClubId) OUTPUT INSERTED.MemberId VALUES (@UserId, @ClubId)", conn))
            {
                insertCmd.Parameters.Add(new SqlParameter("@UserId", System.Data.SqlDbType.Int) { Value = userId });
                insertCmd.Parameters.Add(new SqlParameter("@ClubId", System.Data.SqlDbType.Int) { Value = clubId });
                var scalar = await insertCmd.ExecuteScalarAsync();
                var newId = Convert.ToInt32(scalar);
                var created = await GetMemberByIdAsync(newId);
                if (created == null) throw new InvalidOperationException("Failed to retrieve created member");
                return created;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding member to club {ClubId} for user {UserId}", clubId, userId);
            throw;
        }
    }
}