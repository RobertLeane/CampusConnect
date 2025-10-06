using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using CampusConnect.Api.Models;

namespace CampusConnect.Api.Services;

public class EventService : IEventService
{
    private readonly ILogger<EventService> _logger;
    private readonly string _server;
    private readonly string _database;

    public EventService(ILogger<EventService> logger)
    {
        _logger = logger;
        _server = Environment.GetEnvironmentVariable("Sql__Server")!;
        _database = Environment.GetEnvironmentVariable("Sql__Database")!;
    }

    // TODO: Implement all methods from IEventService interface
    // Use the ClubService.cs as a reference for database operations
    // Remember to:
    // 1. Check for null/empty server and database
    // 2. Use SqlService.OpenTokenConnectionAsync for database connections
    // 3. Use parameterized queries to prevent SQL injection
    // 4. Handle exceptions and log errors
    // 5. Return appropriate data types as defined in the interface
     private static readonly List<EventDto> _data = new()
        {
            new EventDto { EventId=1, EventName="AI for Beginners — Hands-on", Description="Python workshop", EventDateTime=DateTime.Today.AddDays(2).AddHours(18), ClubId=2 },
            new EventDto { EventId=2, EventName="Sunset Photo Walk", Description="Python workshop",EventDateTime=DateTime.Today.AddDays(3).AddHours(17), ClubId=1 },
            new EventDto { EventId=3, EventName="Campus Food Drive", Description="Python workshop",EventDateTime=DateTime.Today.AddDays(4).AddHours(10), ClubId=3 }
        };

        public Task<List<EventDto>> GetAllEventsAsync()
            => Task.FromResult(_data.ToList());

        public Task<EventDto?> GetEventByIdAsync(int eventId)
            => Task.FromResult(_data.FirstOrDefault(e => e.EventId == eventId));

        public Task<List<EventDto>> GetEventsByClubIdAsync(int clubId)
            => Task.FromResult(_data.Where(e => e.ClubId == clubId).ToList());

        public Task<EventDto> CreateEventAsync(EventDto eventDto)
        {
            eventDto.EventId = (_data.LastOrDefault()?.EventId ?? 0) + 1;
            _data.Add(eventDto);
            return Task.FromResult(eventDto);
        }
    }
