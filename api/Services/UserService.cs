using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using CampusConnect.Api.Models;

namespace CampusConnect.Api.Services;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly string _server;
    private readonly string _database;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
        _server = Environment.GetEnvironmentVariable("Sql__Server")!;
        _database = Environment.GetEnvironmentVariable("Sql__Database")!;
    }


    public async Task<List<UserDto>> GetAllUsersAsync()
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
                "SELECT UserID, FirstName, LastName, Email FROM dbo.User",
                conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            var users = new List<UserDto>();
            while (await reader.ReadAsync())
            {
                users.Add(new UserDto
                {
                    UserID = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Email = reader.GetString(3)
                });
            }
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            throw;
        }
    }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
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
                "SELECT UserID, FirstName, LastName, Email FROM dbo.User WHERE UserID = @userId",
                conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new UserDto
                {
                    UserID = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Email = reader.GetString(3)
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving user {userId}");
            throw;
        }
    }

    //Insert a user into the database
    public async Task<UserDto> CreateUserAsync(UserDto user)
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
                "INSERT INTO dbo.User (FirstName, LastName, Email) OUTPUT INSERTED.UserID VALUES (@FirstName, @LastName, @Email)",
                conn);
            cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
            cmd.Parameters.AddWithValue("@LastName", user.LastName);
            cmd.Parameters.AddWithValue("@Email", user.Email);

            var insertedId = (int)await cmd.ExecuteScalarAsync();
            user.UserID = insertedId;
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            throw;
        }
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
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
            "SELECT UserID, FirstName, LastName, Email FROM dbo.User WHERE Email = @email",
            conn);
        cmd.Parameters.AddWithValue("@email", email);
        await using var reader = await cmd.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return new UserDto
            {
                UserID = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                Email = reader.GetString(3)
            };
        }
        return null;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving user by email {Email}", email);
        throw;
    }
}
}
    