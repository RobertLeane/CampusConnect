using Microsoft.Data.SqlClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CampusConnect.Api.Services;

namespace api;

public class GetClubs
{
    private readonly ILogger<GetClubs> _logger;

    public GetClubs(ILogger<GetClubs> logger)
    {
        _logger = logger;
    }

    [Function("GetClubs")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "clubs")] HttpRequest req)
    {
        try
        {
            _logger.LogInformation("Getting clubs from database");
            
            var server = Environment.GetEnvironmentVariable("Sql__Server");
            var database = Environment.GetEnvironmentVariable("Sql__Database");

            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(database))
            {
                _logger.LogError("Database configuration is missing");
                return new ObjectResult("Database configuration is missing")
                {
                    StatusCode = 500
                };
            }

            await using var conn = await SqlService.OpenTokenConnectionAsync(server, database);
            
            // Updated SQL query to match your actual table schema
            await using var cmd = new SqlCommand("SELECT ClubId, ClubName, Description, Active FROM dbo.Club WHERE Active = 1", conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            var clubs = new List<object>();
            while (await reader.ReadAsync())
            {
                clubs.Add(new {
                    ClubId = reader.GetInt32(0),  // ClubId
                    ClubName = reader.GetString(1),  // ClubName
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),  // Description
                    Active = reader.GetBoolean(3)  // Active
                });
            }

            _logger.LogInformation($"Retrieved {clubs.Count} active clubs from database");
            return new OkObjectResult(clubs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving clubs from database");
            return new ObjectResult("An error occurred while retrieving clubs")
            {
                StatusCode = 500
            };
        }
    }
}
