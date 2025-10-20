using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using CampusConnect.Api.Models;

namespace CampusConnect.Api.Services;

public class MemberService : IMemberService
{
    private readonly ILogger<MemberService> _logger;
    private readonly string _server;
    private readonly string _database;

    public MemberService(ILogger<MemberService> logger)
    {
        _logger = logger;
     // Try different environment variable names like ClubService does to be robust for local and Azure environments
     _server = Environment.GetEnvironmentVariable("Sql__Server") 
         ?? Environment.GetEnvironmentVariable("SQL_SERVER")
         ?? "svdb-campusconnect.database.windows.net";
     _database = Environment.GetEnvironmentVariable("Sql__Database") 
           ?? Environment.GetEnvironmentVariable("SQL_DATABASE")
           ?? "DB-CampusConnect";

     _logger.LogInformation($"MemberService database config - Server: {_server}, Database: {_database}");
    }

    // TODO: Implement all methods from IMemberService interface
    // Use the ClubService.cs as a reference for database operations
    // Remember to:
    // 1. Check for null/empty server and database
    // 2. Use SqlService.OpenTokenConnectionAsync for database connections
    // 3. Use parameterized queries to prevent SQL injection
    // 4. Handle exceptions and log errors
    // 5. Return appropriate data types as defined in the interface

    public async Task<List<MemberDto>> GetMembersByClubIdAsync(int clubId)
    {
        try
        {
            _logger.LogInformation("Getting members for club {ClubId}", clubId);

            if (string.IsNullOrWhiteSpace(_server) || string.IsNullOrWhiteSpace(_database))
            {
                _logger.LogError("Database configuration is missing");
                throw new InvalidOperationException("Database configuration is missing");
            }

            await using var conn = await SqlService.OpenTokenConnectionAsync(_server, _database);
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

            if (string.IsNullOrWhiteSpace(_server) || string.IsNullOrWhiteSpace(_database))
            {
                _logger.LogError("Database configuration is missing");
                throw new InvalidOperationException("Database configuration is missing");
            }

            await using var conn = await SqlService.OpenTokenConnectionAsync(_server, _database);
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

            if (string.IsNullOrWhiteSpace(_server) || string.IsNullOrWhiteSpace(_database))
            {
                _logger.LogError("Database configuration is missing");
                throw new InvalidOperationException("Database configuration is missing");
            }

            await using var conn = await SqlService.OpenTokenConnectionAsync(_server, _database);
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

            if (string.IsNullOrWhiteSpace(_server) || string.IsNullOrWhiteSpace(_database))
            {
                _logger.LogError("Database configuration is missing");
                throw new InvalidOperationException("Database configuration is missing");
            }

            await using var conn = await SqlService.OpenTokenConnectionAsync(_server, _database);

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

   /* public Task<bool> RemoveMemberFromClubAsync(int memberId)
    {
        _logger.LogInformation("RemoveMemberFromClubAsync called for member {MemberId}", memberId);
        throw new NotImplementedException("RemoveMemberFromClubAsync is not implemented yet");
    }
    */
}