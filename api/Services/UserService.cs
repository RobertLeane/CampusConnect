using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using CampusConnect.Api.Models;

namespace CampusConnect.Api.Services;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var normalizedEmail = (email ?? string.Empty).Trim().ToLowerInvariant();
        const string sql = "SELECT COUNT(1) FROM dbo.Users WHERE Email = @Email";
        await using var conn = await SqlService.CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Value = normalizedEmail });

        var result = await cmd.ExecuteScalarAsync();
        var count = (result is int i) ? i : Convert.ToInt32(result);
        return count > 0;
    }

    public async Task<int> RegisterAsync(RegisterRequest req)
    {
        var first = (req.FirstName ?? string.Empty).Trim();
        var last = (req.LastName ?? string.Empty).Trim();
        var email = (req.Email ?? string.Empty).Trim().ToLowerInvariant();
        var password = req.Password ?? string.Empty;

        if (await EmailExistsAsync(email))
        {
            _logger.LogInformation("Attempted registration with existing email: {Email}", email);
            throw new InvalidOperationException("Email already exists");
        }

        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        const string sql = @"INSERT INTO dbo.Users (FirstName, LastName, Email, PasswordHash)
VALUES (@FirstName, @LastName, @Email, @PasswordHash);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

    await using var conn = await SqlService.CreateConnection();
    await using var cmd = new SqlCommand(sql, conn);
    cmd.Parameters.Add(new SqlParameter("@FirstName", SqlDbType.NVarChar, 50) { Value = first });
    cmd.Parameters.Add(new SqlParameter("@LastName", SqlDbType.NVarChar, 50) { Value = last });
    cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Value = email });
    cmd.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 255) { Value = hash });

        try
        {
            var result = await cmd.ExecuteScalarAsync();
            var newId = (result is int i) ? i : Convert.ToInt32(result);
            if (newId <= 0)
            {
                _logger.LogError("Failed to insert user record for {Email}", email);
                throw new Exception("Registration failed");
            }
            return newId;
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            // Unique constraint violation (duplicate Email)
            _logger.LogWarning(ex, "Unique constraint violation for email {Email}", email);
            throw new InvalidOperationException("Email already exists");
        }
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest req)
    {
        var email = (req.Email ?? string.Empty).Trim().ToLowerInvariant();
        var password = req.Password ?? string.Empty;

        const string sql = "SELECT UserId, FirstName, LastName, Email, PasswordHash FROM dbo.Users WHERE Email = @Email";
        await using var conn = await SqlService.CreateConnection();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Value = email });

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            _logger.LogInformation("Login attempt with non-existent email: {Email}", email);
            return null; // User not found
        }

        var userId = reader.GetInt32("UserId");
        var firstName = reader.GetString("FirstName");
        var lastName = reader.GetString("LastName");
        var storedEmail = reader.GetString("Email");
        var passwordHash = reader.GetString("PasswordHash");

        // Verify password using BCrypt
        if (!BCrypt.Net.BCrypt.Verify(password, passwordHash))
        {
            _logger.LogInformation("Invalid password for email: {Email}", email);
            return null; // Invalid password
        }

        _logger.LogInformation("Successful login for user: {UserId}", userId);
        return new AuthResponse
        {
            UserId = userId,
            Email = storedEmail,
            Message = $"Welcome back, {firstName}!"
        };
    }
}
