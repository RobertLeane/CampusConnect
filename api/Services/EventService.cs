using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CampusConnect.Api.Models;
using System.Data.Common;
using System.Linq.Expressions;

namespace CampusConnect.Api.Services
{
    public class EventService : IEventService
    {
        private readonly ILogger<EventService> _logger;
        private readonly IConfiguration _configuration;

        public EventService(ILogger<EventService> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<List<EventDto>> GetAllEventsAsync()
        {
            const string sql = @"
                SELECT *
                FROM dbo.Events
                ORDER BY EventDateTime";

            var conn = await SqlService.CreateConnectionAsync(_configuration).ConfigureAwait(false);
            await using (conn.ConfigureAwait(false))
            {
                using var cmd = new SqlCommand(sql, conn);
                var rdr = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                await using (rdr.ConfigureAwait(false))
                {
                    var events = new List<EventDto>();
                    while (await rdr.ReadAsync().ConfigureAwait(false))
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
            }
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
                var conn = await SqlService.CreateConnectionAsync(_configuration).ConfigureAwait(false);
                await using (conn.ConfigureAwait(false))
                {
                    using var cmd = new SqlCommand(
                        "SELECT * FROM dbo.Events WHERE EventId = @EventId",
                        conn);
                    cmd.Parameters.Add(new SqlParameter("@EventId", SqlDbType.Int) { Value = eventId });
                    var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                    await using (reader.ConfigureAwait(false))
                    {
                        if (await reader.ReadAsync().ConfigureAwait(false))
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
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving event {EventId}", eventId);
                throw new InvalidOperationException($"An error occurred while retrieving event {eventId}", ex);
            }
        }

        public async Task<List<EventDto>> GetEventsByClubIdAsync(int clubId)
        {
            const string sql = @"
                SELECT *
                FROM dbo.Events WHERE ClubId = @ClubId
                ORDER BY EventDateTime";

            var conn = await SqlService.CreateConnectionAsync(_configuration).ConfigureAwait(false);
            await using (conn.ConfigureAwait(false))
            {
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@ClubId", SqlDbType.Int) { Value = clubId });

                var rdr = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                await using (rdr.ConfigureAwait(false))
                {
                    var list = new List<EventDto>();
                    while (await rdr.ReadAsync().ConfigureAwait(false))
                    {
                        list.Add(MapEvent(rdr));
                    }
                    return list;
                }
            }
        }
        public Task<IEventService.RsvpResult> RsvpToEventAsync(int eventId, int userId)
        {
            return RsvpAsync(eventId, userId);
        }

        public async Task<List<EventDto>> GetUserRsvpEventsAsync(int userId)
        {
            const string sql = @"
                SELECT e.*
                FROM dbo.Events e
                INNER JOIN dbo.EventParticipants ep ON e.EventId = ep.EventId
                INNER JOIN dbo.Members m ON ep.MemberId = m.MemberId
                WHERE m.UserId = @UserId
                ORDER BY e.EventDateTime";

            var conn = await SqlService.CreateConnectionAsync(_configuration).ConfigureAwait(false);
            await using (conn.ConfigureAwait(false))
            {
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });

                var rdr = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                await using (rdr.ConfigureAwait(false))
                {
                    var events = new List<EventDto>();
                    while (await rdr.ReadAsync().ConfigureAwait(false))
                    {
                        events.Add(MapEvent(rdr));
                    }
                    return events;
                }
            }
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

            const string checkRsvpSql = "SELECT COUNT(1) FROM dbo.EventParticipants WHERE EventId = @EventId AND MemberId IN (SELECT MemberId FROM dbo.Members WHERE UserId = @UserId)";
            const string insertRsvpSql = @"
            INSERT INTO dbo.EventParticipants (EventId, MemberId, ClubId)
            SELECT @EventId, m.MemberId, e.ClubId 
            FROM dbo.Members m
            INNER JOIN dbo.Events e ON e.EventId = @EventId
            WHERE m.UserId = @UserId AND m.ClubId = e.ClubId";
            try
            {
                var conn = await SqlService.CreateConnectionAsync(_configuration).ConfigureAwait(false);
                await using (conn.ConfigureAwait(false))
                {
                    // Event exists?
                    using (var checkEvent = new SqlCommand(checkEventSql, conn))
                    {
                        checkEvent.Parameters.Add(new SqlParameter("@EventId", SqlDbType.Int) { Value = eventId });
                        if ((int)(await checkEvent.ExecuteScalarAsync().ConfigureAwait(false) ?? 0) == 0)
                            return IEventService.RsvpResult.EventNotFound;
                    }

                    // User exists?
                    using (var checkUser = new SqlCommand(checkUserSql, conn))
                    {
                        checkUser.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        if ((int)(await checkUser.ExecuteScalarAsync().ConfigureAwait(false) ?? 0) == 0)
                            return IEventService.RsvpResult.UserNotFound;
                    }

                    // Is member of the event's club?
                    using (var checkMember = new SqlCommand(checkMemberSql, conn))
                    {
                        checkMember.Parameters.Add(new SqlParameter("@EventId", SqlDbType.Int) { Value = eventId });
                        checkMember.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        if ((int)(await checkMember.ExecuteScalarAsync().ConfigureAwait(false) ?? 0) == 0)
                            return IEventService.RsvpResult.NotClubMember;
                    }

                    // Already RSVPed?
                    using (var checkRsvp = new SqlCommand(checkRsvpSql, conn))
                    {
                        checkRsvp.Parameters.Add(new SqlParameter("@EventId", SqlDbType.Int) { Value = eventId });
                        checkRsvp.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        if ((int)(await checkRsvp.ExecuteScalarAsync().ConfigureAwait(false) ?? 0) > 0)
                            return IEventService.RsvpResult.AlreadyRsvped;
                    }

                    // Insert RSVP
                    using (var insert = new SqlCommand(insertRsvpSql, conn))
                    {
                        insert.Parameters.Add(new SqlParameter("@EventId", SqlDbType.Int) { Value = eventId });
                        insert.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        var rows = await insert.ExecuteNonQueryAsync().ConfigureAwait(false);
                        return rows == 1 ? IEventService.RsvpResult.Success : IEventService.RsvpResult.Error;
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error processing RSVP for EventId {EventId} and UserId {UserId}", eventId, userId);
                return IEventService.RsvpResult.Error;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing RSVP for EventId {EventId} and UserId {UserId}", eventId, userId);
                throw new InvalidOperationException($"An error occurred while processing RSVP for event {eventId}", ex);
            }
        }
    }
}
