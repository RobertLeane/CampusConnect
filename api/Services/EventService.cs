using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using CampusConnect.Api.Models;
using System.Data.Common;
using System.Linq.Expressions;

namespace CampusConnect.Api.Services
{
    public class EventService : IEventService
    {
        private readonly ILogger<EventService> _logger;
        private readonly string _server;
        private readonly string _database;

        public EventService(ILogger<EventService> logger)
        {
            _logger = logger;
            _server = Environment.GetEnvironmentVariable("Sql__Server") ?? string.Empty;
            _database = Environment.GetEnvironmentVariable("Sql__Database") ?? string.Empty;

            if (string.IsNullOrWhiteSpace(_server) || string.IsNullOrWhiteSpace(_database))
            {
                _logger.LogWarning("Database configuration is missing (Sql__Server / Sql__Database).");
            }
        }

        public async Task<List<EventDto>> GetAllEventsAsync()
        {
            const string sql = @"
                SELECT *
                FROM dbo.Events
                ORDER BY EventDateTime";

            using var conn = await SqlService.OpenTokenConnectionAsync(_server, _database);
            using var cmd = new SqlCommand(sql, conn);
            using var rdr = await cmd.ExecuteReaderAsync();

            var events = new List<EventDto>();
            while (await rdr.ReadAsync())
            {
                events.Add(new EventDto
                {
                    EventId = (int)rdr["EventId"],
                    EventName = (string)rdr["EventName"],
                    EventDescription = rdr["EventDescription"] as string ?? "",
                    EventDateTime = (DateTime)rdr["EventDateTime"],
                    EventImgURL = rdr["EventImgURL"] as string ?? "",
                    ClubId = (int)rdr["ClubId"]
                });
            }
            return events;
     
        }
        private static EventDto MapEvent(SqlDataReader rdr)
        {
            return new EventDto
            {
                EventId = (int)rdr["EventId"],
                EventName = (string)rdr["EventName"],
                EventDescription = rdr["EventDescription"] as string ?? "",
                EventDateTime = (DateTime)rdr["EventDateTime"],
                EventImgURL = rdr["EventImgURL"] as string ?? "",
                ClubId = (int)rdr["ClubId"]
            };
        }

        public async Task<EventDto?> GetEventByIdAsync(int eventId)
        {
            try
            {
                if (string.IsNullOrEmpty(_server) || string.IsNullOrEmpty(_database))
                {
                    _logger.LogError("Database configuration is missing");
                    throw new InvalidOperationException("Database configuration is missing");
                }
                await using var conn = await SqlService.OpenTokenConnectionAsync(_server, _database);
                await using var cmd = new SqlCommand(
                    "SELECT * FROM dbo.Events WHERE EventId = @EventId",
                    conn);
                cmd.Parameters.Add(new SqlParameter("@EventId", SqlDbType.Int) { Value = eventId });
                await using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new EventDto
                    {
                        EventId = (int)reader["EventId"],
                        EventName = (string)reader["EventName"],
                        EventDescription = reader["EventDescription"] as string ?? "",
                        EventDateTime = (DateTime)reader["EventDateTime"],
                        EventImgURL = reader["EventImgURL"] as string ?? "",
                        ClubId = (int)reader["ClubId"]
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving event {eventId}");
                throw;
            }
            
        }

        public async Task<List<EventDto>> GetEventsByClubIdAsync(int clubId)
        {
            const string sql = @"
                SELECT *
                FROM dbo.Events WHERE ClubId = @ClubId
                ORDER BY EventDateTime";

            using var conn = await SqlService.OpenTokenConnectionAsync(_server, _database);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@ClubId", SqlDbType.Int) { Value = clubId });

            using var rdr = await cmd.ExecuteReaderAsync();

            var list = new List<EventDto>();
            while (await rdr.ReadAsync())
            {
                list.Add(MapEvent(rdr));
            }
            return list;
        }
        public Task<IEventService.RsvpResult> RsvpToEventAsync(int eventId, int userId)
        {
            // FIX: call the real worker
            return RsvpAsync(eventId, userId);
        }

        public async Task<List<EventDto>> GetUserRsvpEventsAsync(int userId)
        {
            const string sql = @"
                SELECT e.*
                FROM dbo.Events e
                INNER JOIN dbo.EventRsvps r ON e.EventId = r.EventId
                WHERE r.UserId = @UserId
                ORDER BY e.EventDateTime";

            using var conn = await SqlService.OpenTokenConnectionAsync(_server, _database);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });

            using var rdr = await cmd.ExecuteReaderAsync();

            var events = new List<EventDto>();
            while (await rdr.ReadAsync())
            {
                events.Add(MapEvent(rdr));
            }
            return events;
        }

        private async Task<IEventService.RsvpResult> RsvpAsync(int eventId, int userId)
        {
            const string checkEventSql = "SELECT COUNT(1) FROM dbo.Events WHERE EventId = @EventId";
            const string checkUserSql = "SELECT COUNT(1) FROM dbo.Users  WHERE UserId  = @UserId";

            // membership: user must belong to the club that owns the event
            const string checkMemberSql = @"
            SELECT COUNT(1)
            FROM dbo.Members m
            INNER JOIN dbo.Events e ON e.ClubId = m.ClubId
            WHERE e.EventId = @EventId AND m.UserId = @UserId";

            const string checkRsvpSql = "SELECT COUNT(1) FROM dbo.EventRsvps WHERE EventId = @EventId AND UserId = @UserId";
            const string insertRsvpSql = @"
            INSERT INTO dbo.EventRsvps (EventId, UserId, RsvpDateTime)
            VALUES (@EventId, @UserId, GETUTCDATE())";
            try
            {
                using var conn = await SqlService.OpenTokenConnectionAsync(_server, _database);

                // Event exists?
                using (var checkEvent = new SqlCommand(checkEventSql, conn))
                {
                    checkEvent.Parameters.Add(new SqlParameter("@EventId", SqlDbType.Int) { Value = eventId });
                    if ((int)(await checkEvent.ExecuteScalarAsync() ?? 0) == 0)
                        return IEventService.RsvpResult.EventNotFound;
                }

                // User exists?
                using (var checkUser = new SqlCommand(checkUserSql, conn))
                {
                    checkUser.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                    if ((int)(await checkUser.ExecuteScalarAsync() ?? 0) == 0)
                        return IEventService.RsvpResult.UserNotFound;
                }

                // Is member of the event's club?
                using (var checkMember = new SqlCommand(checkMemberSql, conn))
                {
                    checkMember.Parameters.Add(new SqlParameter("@EventId", SqlDbType.Int) { Value = eventId });
                    checkMember.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                    if ((int)(await checkMember.ExecuteScalarAsync() ?? 0) == 0)
                        return IEventService.RsvpResult.NotClubMember;
                }

                // Already RSVPed?
                using (var checkRsvp = new SqlCommand(checkRsvpSql, conn))
                {
                    checkRsvp.Parameters.Add(new SqlParameter("@EventId", SqlDbType.Int) { Value = eventId });
                    checkRsvp.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                    if ((int)(await checkRsvp.ExecuteScalarAsync() ?? 0) > 0)
                        return IEventService.RsvpResult.AlreadyRsvped;
                }

                // Insert RSVP
                using (var insert = new SqlCommand(insertRsvpSql, conn))
                {
                    insert.Parameters.Add(new SqlParameter("@EventId", SqlDbType.Int) { Value = eventId });
                    insert.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                    var rows = await insert.ExecuteNonQueryAsync();
                    return rows == 1 ? IEventService.RsvpResult.Success : IEventService.RsvpResult.Error;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing RSVP");
                return IEventService.RsvpResult.Error;
            }
        }
    }
}
