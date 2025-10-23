using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CampusConnect.Api.Models;

namespace CampusConnect.Api.Services;

public class EventParticipantService : IEventParticipantService
{
    private readonly ILogger<EventParticipantService> _logger;
    private readonly IConfiguration _configuration;

    public EventParticipantService(ILogger<EventParticipantService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    // TODO: Implement all methods from IEventParticipantService interface  

    public async Task<List<EventParticipantDto>> GetActiveEventParticipantsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving active event participants");

            await using var conn = await SqlService.CreateConnection(_configuration);
            await using var cmd = new SqlCommand("SELECT MemberId, EventId, ClubId FROM dbo.EventParticipants", conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            var list = new List<EventParticipantDto>();
            while (await reader.ReadAsync())
            {
                list.Add(new EventParticipantDto
                {
                    MemberId = reader.GetInt32(0),
                    EventId = reader.GetInt32(1),
                    ClubId = reader.GetInt32(2)
                });
            }
            _logger.LogInformation($"Retrieved {list.Count} event participants");
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active event participants");
            throw;
        }
    }

    public async Task<int?> GetUserByMemberIdAsync(int memberId)
    {
        try
        {
            _logger.LogInformation("Getting UserId for MemberId {MemberId}", memberId);
            await using var conn = await SqlService.CreateConnection(_configuration);
            await using var cmd = new SqlCommand("SELECT UserId FROM dbo.Members WHERE MemberId = @MemberId", conn);
            cmd.Parameters.Add(new SqlParameter("@MemberId", SqlDbType.Int) { Value = memberId });
            var scalar = await cmd.ExecuteScalarAsync();
            return scalar == null || scalar == DBNull.Value ? (int?)null : Convert.ToInt32(scalar);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting UserId for MemberId {MemberId}", memberId);
            throw;
        }
    }

    public async Task<List<int>> GetMemberIdsByUserIdAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Getting member ids for UserId {UserId}", userId);
            await using var conn = await SqlService.CreateConnection(_configuration);
            await using var cmd = new SqlCommand("SELECT MemberId FROM dbo.Members WHERE UserId = @UserId", conn);
            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
            await using var reader = await cmd.ExecuteReaderAsync();

            var memberIds = new List<int>();
            while (await reader.ReadAsync())
            {
                memberIds.Add(reader.GetInt32(0));
            }
            return memberIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting member ids for UserId {UserId}", userId);
            throw;
        }
    }

    public async Task<List<EventDto>> GetEventsByMemberIdsAsync(IEnumerable<int> memberIds)
    {
        try
        {
            _logger.LogInformation("Getting events for member ids");
            var eventDetails = new List<EventDto>();

            // If no member ids provided return empty list
            var memberIdList = memberIds?.ToList() ?? new List<int>();
            if (memberIdList.Count == 0) return eventDetails;

            await using var conn = await SqlService.CreateConnection(_configuration);

            // Efficient single query: join EventParticipants -> Events
            // Build parameterized IN clause
            var paramNames = new List<string>();
            var cmd = new SqlCommand();
            for (var i = 0; i < memberIdList.Count; i++)
            {
                var name = "@m" + i;
                paramNames.Add(name);
                cmd.Parameters.Add(new SqlParameter(name, SqlDbType.Int) { Value = memberIdList[i] });
            }

            var inClause = string.Join(",", paramNames);
            cmd.Connection = conn;
            cmd.CommandText = $@"
                SELECT e.EventId, e.EventName, e.EventDescription, e.EventDateTime, e.EventImgURL, e.ClubId
                FROM dbo.Events e
                INNER JOIN dbo.EventParticipants ep ON e.EventId = ep.EventId
                WHERE ep.MemberId IN ({inClause})
                ORDER BY e.EventDateTime";

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                eventDetails.Add(new EventDto
                {
                    EventId = reader.GetInt32(0),
                    EventName = reader.GetString(1),
                    EventDescription = reader.IsDBNull(2) ? null : reader.GetString(2),
                    EventDateTime = reader.GetDateTime(3),
                    EventImgURL = reader.IsDBNull(4) ? null : reader.GetString(4),
                    ClubId = reader.GetInt32(5)
                });
            }

            return eventDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting events for member ids");
            throw;
        }
    }

    public async Task<int?> GetClubIdByEventIdAsync(int eventId)
    {
        try
        {
            _logger.LogInformation("Getting club id for EventId {EventId}", eventId);
            await using var conn = await SqlService.CreateConnection(_configuration);
            await using var cmd = new SqlCommand("SELECT ClubId FROM dbo.Events WHERE EventId = @EventId", conn);
            cmd.Parameters.Add(new SqlParameter("@EventId", SqlDbType.Int) { Value = eventId });
            var scalar = await cmd.ExecuteScalarAsync();
            return scalar == null || scalar == DBNull.Value ? (int?)null : Convert.ToInt32(scalar);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting club id for EventId {EventId}", eventId);
            throw;
        }
    }

    public async Task<int?> GetMemberIdByUserIdAndClubIdAsync(int userId, int clubId)
    {
        try
        {
            _logger.LogInformation("Getting MemberId for UserId {UserId} and ClubId {ClubId}", userId, clubId);
            await using var conn = await SqlService.CreateConnection(_configuration);
            await using var cmd = new SqlCommand("SELECT MemberId FROM dbo.Members WHERE UserId = @UserId AND ClubId = @ClubId", conn);
            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
            cmd.Parameters.Add(new SqlParameter("@ClubId", SqlDbType.Int) { Value = clubId });
            var scalar = await cmd.ExecuteScalarAsync();
            return scalar == null || scalar == DBNull.Value ? (int?)null : Convert.ToInt32(scalar);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MemberId for UserId {UserId} and ClubId {ClubId}", userId, clubId);
            throw;
        }
    }

    public async Task RegisterUserForEventAsync(int memberId, int clubId, int eventId)
    {
        try
        {
            _logger.LogInformation("Registering MemberId {MemberId} for EventId {EventId} in ClubId {ClubId}", memberId, eventId, clubId);
            await using var conn = await SqlService.CreateConnection(_configuration);

            await using (var checkCmd = new SqlCommand("SELECT MemberId FROM dbo.EventParticipants WHERE EventId = @EventId AND MemberId = @MemberId", conn))
            {
                checkCmd.Parameters.Add(new SqlParameter("@EventId", SqlDbType.Int) { Value = eventId });
                checkCmd.Parameters.Add(new SqlParameter("@MemberId", SqlDbType.Int) { Value = memberId });
                var exists = await checkCmd.ExecuteScalarAsync();
                if (exists != null && exists != DBNull.Value)
                {
                    throw new InvalidOperationException("Member is already registered for this event");
                }
            }

            await using (var insertCmd = new SqlCommand("INSERT INTO dbo.EventParticipants (MemberId, ClubId, EventId) VALUES (@MemberId, @ClubId, @EventId)", conn))
            {
                insertCmd.Parameters.Add(new SqlParameter("@MemberId", SqlDbType.Int) { Value = memberId });
                insertCmd.Parameters.Add(new SqlParameter("@ClubId", SqlDbType.Int) { Value = clubId });
                insertCmd.Parameters.Add(new SqlParameter("@EventId", SqlDbType.Int) { Value = eventId });
                await insertCmd.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering member for event");
            throw;
        }
    }
}
