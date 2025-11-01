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
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<List<MemberDto>> GetMembersByClubIdAsync(int clubId)
    {
        try
        {
            _logger.LogInformation("Getting members for club {ClubId}", clubId);

            var conn = await SqlService.CreateConnectionAsync(_configuration).ConfigureAwait(false);
            await using (conn.ConfigureAwait(false))
            {
                using var cmd = new SqlCommand("SELECT MemberId, UserId, ClubId FROM dbo.Members WHERE ClubId = @ClubId", conn);
                cmd.Parameters.Add(new SqlParameter("@ClubId", System.Data.SqlDbType.Int) { Value = clubId });
                var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                await using (reader.ConfigureAwait(false))
                {
                    var list = new List<MemberDto>();
                    while (await reader.ReadAsync().ConfigureAwait(false))
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
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving members for club {ClubId}", clubId);
            throw new InvalidOperationException($"An error occurred while retrieving members for club {clubId}", ex);
        }
    }

    public async Task<List<MemberDto>> GetMembersByUserIdAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Getting members for user {UserId}", userId);

            var conn = await SqlService.CreateConnectionAsync(_configuration).ConfigureAwait(false);
            await using (conn.ConfigureAwait(false))
            {
                using var cmd = new SqlCommand("SELECT MemberId, UserId, ClubId FROM dbo.Members WHERE UserId = @UserId", conn);
                cmd.Parameters.Add(new SqlParameter("@UserId", System.Data.SqlDbType.Int) { Value = userId });
                var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                await using (reader.ConfigureAwait(false))
                {
                    var list = new List<MemberDto>();
                    while (await reader.ReadAsync().ConfigureAwait(false))
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
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving members for user {UserId}", userId);
            throw new InvalidOperationException($"An error occurred while retrieving members for user {userId}", ex);
        }
    }

    public async Task<MemberDto?> GetMemberByIdAsync(int memberId)
    {
        try
        {
            _logger.LogInformation("Getting member {MemberId}", memberId);

            var conn = await SqlService.CreateConnectionAsync(_configuration).ConfigureAwait(false);
            await using (conn.ConfigureAwait(false))
            {
                using var cmd = new SqlCommand("SELECT MemberId, UserId, ClubId FROM dbo.Members WHERE MemberId = @MemberId", conn);
                cmd.Parameters.Add(new SqlParameter("@MemberId", System.Data.SqlDbType.Int) { Value = memberId });
                var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                await using (reader.ConfigureAwait(false))
                {
                    if (await reader.ReadAsync().ConfigureAwait(false))
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
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting member by id {MemberId}", memberId);
            throw new InvalidOperationException($"An error occurred while getting member {memberId}", ex);
        }
    }

    public async Task<MemberDto> AddMemberToClubAsync(int userId, int clubId, string role)
    {
        try
        {
            _logger.LogInformation("Adding member for user {UserId} to club {ClubId}", userId, clubId);

            var conn = await SqlService.CreateConnectionAsync(_configuration).ConfigureAwait(false);
            await using (conn.ConfigureAwait(false))
            {
                // Validate user exists
                using (var checkUserCmd = new SqlCommand("SELECT COUNT(1) FROM dbo.Users WHERE UserId = @UserId", conn))
                {
                    checkUserCmd.Parameters.Add(new SqlParameter("@UserId", System.Data.SqlDbType.Int) { Value = userId });
                    var userExists = await checkUserCmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (userExists == null || Convert.ToInt32(userExists) == 0)
                    {
                        _logger.LogError("User {UserId} does not exist", userId);
                        throw new InvalidOperationException($"User with ID {userId} does not exist");
                    }
                }

                // Validate club exists
                using (var checkClubCmd = new SqlCommand("SELECT COUNT(1) FROM dbo.Clubs WHERE ClubId = @ClubId", conn))
                {
                    checkClubCmd.Parameters.Add(new SqlParameter("@ClubId", System.Data.SqlDbType.Int) { Value = clubId });
                    var clubExists = await checkClubCmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (clubExists == null || Convert.ToInt32(clubExists) == 0)
                    {
                        _logger.LogError("Club {ClubId} does not exist", clubId);
                        throw new InvalidOperationException($"Club with ID {clubId} does not exist");
                    }
                }

                // Prevent duplicate membership
                using (var checkCmd = new SqlCommand("SELECT MemberId FROM dbo.Members WHERE UserId = @UserId AND ClubId = @ClubId", conn))
                {
                    checkCmd.Parameters.Add(new SqlParameter("@UserId", System.Data.SqlDbType.Int) { Value = userId });
                    checkCmd.Parameters.Add(new SqlParameter("@ClubId", System.Data.SqlDbType.Int) { Value = clubId });
                    var exists = await checkCmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (exists != null && exists != DBNull.Value)
                    {
                        var existingId = Convert.ToInt32(exists);
                        var existing = await GetMemberByIdAsync(existingId).ConfigureAwait(false);
                        if (existing != null) return existing;
                    }
                }

                using (var insertCmd = new SqlCommand("INSERT INTO dbo.Members (UserId, ClubId) OUTPUT INSERTED.MemberId VALUES (@UserId, @ClubId)", conn))
                {
                    insertCmd.Parameters.Add(new SqlParameter("@UserId", System.Data.SqlDbType.Int) { Value = userId });
                    insertCmd.Parameters.Add(new SqlParameter("@ClubId", System.Data.SqlDbType.Int) { Value = clubId });
                    var scalar = await insertCmd.ExecuteScalarAsync().ConfigureAwait(false);
                    var newId = Convert.ToInt32(scalar);
                    var created = await GetMemberByIdAsync(newId).ConfigureAwait(false);
                    if (created == null) throw new InvalidOperationException("Failed to retrieve created member");
                    return created;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding member to club {ClubId} for user {UserId}", clubId, userId);
            throw new InvalidOperationException($"An error occurred while adding member to club {clubId} for user {userId}", ex);
        }
    }
}