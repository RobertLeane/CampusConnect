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
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        
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
                 
        _logger.LogInformation("Database config - Server: {Server}, Database: {Database}", _server, _database);
    }

    public async Task<List<ClubDto>> GetActiveClubsAsync()
    {
        try
        {
            _logger.LogInformation("Starting to retrieve active clubs");
            _logger.LogInformation("Using server: {Server}, database: {Database}", _server, _database);
            
            if (string.IsNullOrEmpty(_server) || string.IsNullOrEmpty(_database))
            {
                var error = "Database configuration is missing";
                _logger.LogError(error);
                throw new InvalidOperationException(error);
            }

            var conn = await SqlService.OpenTokenConnectionAsync(_server, _database, _configuration).ConfigureAwait(false);
            await using (conn.ConfigureAwait(false))
            {
                _logger.LogInformation("Database connection established successfully");
                
                using var cmd = new SqlCommand(
                    "SELECT ClubId, ClubName, ClubImgURL, ClubDescription FROM dbo.Clubs", 
                    conn);
                var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                await using (reader.ConfigureAwait(false))
                {
                    var clubs = new List<ClubDto>();
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var isImgNull = await reader.IsDBNullAsync(2).ConfigureAwait(false);
                        var isDescNull = await reader.IsDBNullAsync(3).ConfigureAwait(false);
                        
                        clubs.Add(new ClubDto
                        {
                            ClubId = reader.GetInt32(0),
                            ClubName = reader.GetString(1),
                            ClubImgURL = isImgNull ? null : reader.GetString(2),
                            ClubDescription = isDescNull ? null : reader.GetString(3),
                        });
                    }

                    _logger.LogInformation("Successfully retrieved {Count} clubs", clubs.Count);
                    return clubs;
                }
            }
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

            var conn = await SqlService.OpenTokenConnectionAsync(_server, _database, _configuration).ConfigureAwait(false);
            await using (conn.ConfigureAwait(false))
            {
                using var cmd = new SqlCommand(
                    "SELECT ClubId, ClubName, ClubImgURL, ClubDescription FROM dbo.Clubs WHERE ClubId = @clubId", 
                    conn);
                cmd.Parameters.AddWithValue("@clubId", clubId);
                
                var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                await using (reader.ConfigureAwait(false))
                {
                    if (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var isImgNull = await reader.IsDBNullAsync(2).ConfigureAwait(false);
                        var isDescNull = await reader.IsDBNullAsync(3).ConfigureAwait(false);
                        
                        return new ClubDto
                        {
                            ClubId = reader.GetInt32(0),
                            ClubName = reader.GetString(1),
                            ClubImgURL = isImgNull ? null : reader.GetString(2),
                            ClubDescription = isDescNull ? null : reader.GetString(3),
                        };
                    }

                    return null;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving club {ClubId}", clubId);
            throw new InvalidOperationException($"An error occurred while retrieving club {clubId}: {ex.Message}", ex);
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
            _logger.LogInformation("Getting clubs for user ID {UserId}", userId);
            
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

            var conn = await SqlService.OpenTokenConnectionAsync(_server, _database, _configuration).ConfigureAwait(false);
            await using (conn.ConfigureAwait(false))
            {
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                
                var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                await using (reader.ConfigureAwait(false))
                {
                    var clubs = new List<ClubDto>();
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var isImgNull = await reader.IsDBNullAsync(2).ConfigureAwait(false);
                        var isDescNull = await reader.IsDBNullAsync(3).ConfigureAwait(false);
                        
                        clubs.Add(new ClubDto
                        {
                            ClubId = reader.GetInt32(0),
                            ClubName = reader.GetString(1),
                            ClubImgURL = isImgNull ? null : reader.GetString(2),
                            ClubDescription = isDescNull ? null : reader.GetString(3),
                        });
                    }

                    _logger.LogInformation("Successfully retrieved {Count} clubs for user {UserId}", clubs.Count, userId);
                    return clubs;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving clubs for user {UserId}", userId);
            throw new InvalidOperationException($"An error occurred while retrieving clubs for user {userId}: {ex.Message}", ex);
        }
    }
}