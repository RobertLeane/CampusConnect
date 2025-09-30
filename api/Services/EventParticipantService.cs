using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using CampusConnect.Api.Models;

namespace CampusConnect.Api.Services;

public class EventParticipantService : IEventParticipantService
{
    private readonly ILogger<EventParticipantService> _logger;
    private readonly string _server;
    private readonly string _database;

    public EventParticipantService(ILogger<EventParticipantService> logger)
    {
        _logger = logger;
        _server = Environment.GetEnvironmentVariable("Sql__Server")!;
        _database = Environment.GetEnvironmentVariable("Sql__Database")!;
    }

    // TODO: Implement all methods from IEventParticipantService interface
    // Use the ClubService.cs as a reference for database operations
    // Remember to:
    // 1. Check for null/empty server and database
    // 2. Use SqlService.OpenTokenConnectionAsync for database connections
    // 3. Use parameterized queries to prevent SQL injection
    // 4. Handle exceptions and log errors
    // 5. Return appropriate data types as defined in the interface
}