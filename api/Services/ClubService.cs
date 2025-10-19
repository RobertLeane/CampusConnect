using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CampusConnect.Api.Models;

namespace CampusConnect.Api.Services;

public class ClubService : IClubService
{
    private readonly ILogger<ClubService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _server;
    private readonly string _database;

    public ClubService(ILogger<ClubService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        // Try different configuration sources for database settings
        _server = _configuration["Values:Sql__Server"]
               ?? _configuration["Sql__Server"]
               ?? _configuration["Sql:Server"]
               ?? Environment.GetEnvironmentVariable("Sql__Server") 
               ?? Environment.GetEnvironmentVariable("SQL_SERVER")
               ?? "campusconnect-server.database.windows.net";
               
        _database = _configuration["Values:Sql__Database"]
                 ?? _configuration["Sql__Database"]
                 ?? _configuration["Sql:Database"]
                 ?? Environment.GetEnvironmentVariable("Sql__Database") 
                 ?? Environment.GetEnvironmentVariable("SQL_DATABASE") 
                 ?? "campusconnect-database";
                 
        _logger.LogInformation($"Database config - Server: {_server}, Database: {_database}");
    }

    public async Task<List<ClubDto>> GetActiveClubsAsync()
    {
        try
        {
            _logger.LogInformation("Starting to retrieve active clubs");
            _logger.LogInformation($"Using server: {_server}, database: {_database}");
            
            if (string.IsNullOrEmpty(_server) || string.IsNullOrEmpty(_database))
            {
                var error = "Database configuration is missing";
                _logger.LogError(error);
                throw new InvalidOperationException(error);
            }

            await using var conn = await SqlService.OpenTokenConnectionAsync(_server, _database, _configuration);
            _logger.LogInformation("Database connection established successfully");
            
            await using var cmd = new SqlCommand(
                "SELECT ClubId, ClubName, ClubImgURL, ClubDescription FROM dbo.Clubs", 
                conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            var clubs = new List<ClubDto>();
            while (await reader.ReadAsync())
            {
                clubs.Add(new ClubDto
                {
                    ClubId = reader.GetInt32(0),
                    ClubName = reader.GetString(1),
                    ClubImgURL = reader.IsDBNull(2) ? null : reader.GetString(2),
                    ClubDescription = reader.IsDBNull(3) ? null : reader.GetString(3),
                });
            }

            _logger.LogInformation($"Successfully retrieved {clubs.Count} clubs");
            return clubs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active clubs: {ErrorMessage}", ex.Message);
            throw new InvalidOperationException($"An error occurred while retrieving clubs: {ex.Message}", ex);
        }
    }

    public async Task<ClubDto?> GetClubByIdAsync(int clubId)
    {
        try
        {
            if (string.IsNullOrEmpty(_server) || string.IsNullOrEmpty(_database))
            {
                _logger.LogError("Database configuration is missing");
                throw new InvalidOperationException("Database configuration is missing");
            }

            await using var conn = await SqlService.OpenTokenConnectionAsync(_server, _database, _configuration);
            await using var cmd = new SqlCommand(
                "SELECT ClubId, ClubName, ClubImgURL, ClubDescription FROM dbo.Clubs WHERE ClubId = @clubId", 
                conn);
            cmd.Parameters.AddWithValue("@clubId", clubId);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new ClubDto
                {
                    ClubId = reader.GetInt32(0),  // ClubId
                    ClubName = reader.GetString(1),  // ClubName
                    ClubImgURL = reader.IsDBNull(2) ? null : reader.GetString(2),  // ClubImgURL
                    ClubDescription = reader.IsDBNull(3) ? null : reader.GetString(3),  // ClubDescription
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving club {ClubId}", clubId);
            throw;
        }
    }

    public Task<ClubDto> CreateClubAsync(ClubDto club)
    {
        // TODO: Implement club creation
        throw new NotImplementedException("Club creation not yet implemented");
    }

    public async Task<List<ClubDto>> GetUserJoinedClubsAsync(int userId)
    {
        try
        {
            _logger.LogInformation($"Getting clubs for user ID {userId}");
            
            if (string.IsNullOrEmpty(_server) || string.IsNullOrEmpty(_database))
            {
                _logger.LogError("Database configuration is missing");
                throw new InvalidOperationException("Database configuration is missing");
            }

            const string sql = @"
                SELECT c.ClubId, c.ClubName, c.ClubImgURL, c.ClubDescription
                FROM dbo.Clubs c
                INNER JOIN dbo.Members m ON c.ClubId = m.ClubId
                WHERE m.UserId = @UserId";

            await using var conn = await SqlService.OpenTokenConnectionAsync(_server, _database, _configuration);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            await using var reader = await cmd.ExecuteReaderAsync();

            var clubs = new List<ClubDto>();
            while (await reader.ReadAsync())
            {
                clubs.Add(new ClubDto
                {
                    ClubId = reader.GetInt32(0),
                    ClubName = reader.GetString(1),
                    ClubImgURL = reader.IsDBNull(2) ? null : reader.GetString(2),
                    ClubDescription = reader.IsDBNull(3) ? null : reader.GetString(3),
                });
            }

            _logger.LogInformation($"Successfully retrieved {clubs.Count} clubs for user {userId}");
            return clubs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving clubs for user {UserId}", userId);
            throw;
        }
    }

    public Task<ClubDto?> UpdateClubAsync(int clubId, ClubDto club)
    {
        // TODO: Implement club update
        throw new NotImplementedException("Club update not yet implemented");
    }

    public Task<bool> DeleteClubAsync(int clubId)
    {
        // TODO: Implement club deletion
        throw new NotImplementedException("Club deletion not yet implemented");
    }
}