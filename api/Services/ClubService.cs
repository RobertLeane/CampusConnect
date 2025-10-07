using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using CampusConnect.Api.Models;

namespace CampusConnect.Api.Services;

public class ClubService : IClubService
{
    private readonly ILogger<ClubService> _logger;
    private readonly string _server;
    private readonly string _database;

    public ClubService(ILogger<ClubService> logger)
    {
        _logger = logger;
        _server = Environment.GetEnvironmentVariable("Sql__Server")!;
        _database = Environment.GetEnvironmentVariable("Sql__Database")!;
    }

    public async Task<List<ClubDto>> GetActiveClubsAsync()
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

            return clubs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active clubs");
            throw;
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

            await using var conn = await SqlService.OpenTokenConnectionAsync(_server, _database);
            await using var cmd = new SqlCommand(
                "SELECT ClubID, ClubName, Description, Active FROM dbo.Club WHERE ClubID = @clubId", 
                conn);
            cmd.Parameters.AddWithValue("@clubId", clubId);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new ClubDto
                {
                    ClubId = reader.GetInt32(0),  // ClubId
                    ClubName = reader.GetString(1),  // ClubName
                    ClubDescription = reader.IsDBNull(2) ? null : reader.GetString(2),  // Description
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