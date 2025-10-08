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
        
        // Try different environment variable patterns for Azure Static Web Apps
        _server = Environment.GetEnvironmentVariable("Sql__Server") 
               ?? Environment.GetEnvironmentVariable("SQL_SERVER")
               ?? "svdb-campusconnect.database.windows.net";
               
        _database = Environment.GetEnvironmentVariable("Sql__Database") 
                 ?? Environment.GetEnvironmentVariable("SQL_DATABASE") 
                 ?? "DB-CampusConnect";
                 
        _logger.LogInformation($"Database config - Server: {_server}, Database: {_database}");
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        try
        {
            _logger.LogInformation("Starting to retrieve all users");
            
            if (string.IsNullOrEmpty(_server) || string.IsNullOrEmpty(_database))
            {
                var error = "Database configuration is missing";
                _logger.LogError(error);
                throw new InvalidOperationException(error);
            }

            await using var connection = await SqlService.OpenTokenConnectionAsync(_server, _database);
            _logger.LogInformation("Database connection established successfully");
            
            await using var command = new SqlCommand("SELECT UserID, FirstName, LastName, Email FROM Users", connection);
            await using var reader = await command.ExecuteReaderAsync();
            
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
            
            _logger.LogInformation($"Successfully retrieved {users.Count} users");
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users: {ErrorMessage}", ex.Message);
            throw new InvalidOperationException($"An error occurred while retrieving users: {ex.Message}", ex);
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

            await using var connection = await SqlService.OpenTokenConnectionAsync(_server, _database);
            await using var command = new SqlCommand("SELECT UserID, FirstName, LastName, Email FROM Users WHERE UserID = @UserId", connection);
            command.Parameters.AddWithValue("@UserId", userId);
            
            await using var reader = await command.ExecuteReaderAsync();
            
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
            _logger.LogError(ex, "Error retrieving user {UserId}", userId);
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

            await using var connection = await SqlService.OpenTokenConnectionAsync(_server, _database);
            await using var command = new SqlCommand("SELECT UserID, FirstName, LastName, Email FROM Users WHERE Email = @Email", connection);
            command.Parameters.AddWithValue("@Email", email);
            
            await using var reader = await command.ExecuteReaderAsync();
            
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

    public async Task<UserDto> CreateUserAsync(UserDto user)
    {
        try
        {
            if (string.IsNullOrEmpty(_server) || string.IsNullOrEmpty(_database))
            {
                _logger.LogError("Database configuration is missing");
                throw new InvalidOperationException("Database configuration is missing");
            }

            await using var connection = await SqlService.OpenTokenConnectionAsync(_server, _database);
            await using var command = new SqlCommand(
                "INSERT INTO Users (FirstName, LastName, Email) OUTPUT INSERTED.UserID VALUES (@FirstName, @LastName, @Email)", 
                connection);
            
            command.Parameters.AddWithValue("@FirstName", user.FirstName);
            command.Parameters.AddWithValue("@LastName", user.LastName);
            command.Parameters.AddWithValue("@Email", user.Email);
            
            var userId = (int)(await command.ExecuteScalarAsync() ?? 0);
            user.UserID = userId;
            
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            throw;
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(_server) || string.IsNullOrEmpty(_database))
            {
                _logger.LogError("Database configuration is missing");
                throw new InvalidOperationException("Database configuration is missing");
            }

            // Check if email already exists - use separate connection
            await using var checkConnection = await SqlService.OpenTokenConnectionAsync(_server, _database);
            await using var checkCommand = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email = @Email", checkConnection);
            checkCommand.Parameters.AddWithValue("@Email", request.Email);
            
            _logger.LogInformation($"Checking if email exists: {request.Email}");
            var emailCount = await checkCommand.ExecuteScalarAsync();
            _logger.LogInformation($"Email count query returned: {emailCount}");
            
            var emailExists = (int)(emailCount ?? 0) > 0;
            _logger.LogInformation($"Email exists check result: {emailExists}");
            
            if (emailExists)
            {
                _logger.LogWarning($"Registration failed - email already exists: {request.Email}");
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email already exists"
                };
            }

            _logger.LogInformation($"Email is available, proceeding with user creation: {request.Email}");

            // Create new user - use new connection to avoid any issues
            await using var insertConnection = await SqlService.OpenTokenConnectionAsync(_server, _database);
            await using var insertCommand = new SqlCommand(
                "INSERT INTO Users (FirstName, LastName, Email, PasswordHash) OUTPUT INSERTED.UserID VALUES (@FirstName, @LastName, @Email, @PasswordHash)", 
                insertConnection);
            
            insertCommand.Parameters.AddWithValue("@FirstName", request.FirstName);
            insertCommand.Parameters.AddWithValue("@LastName", request.LastName);
            insertCommand.Parameters.AddWithValue("@Email", request.Email);
            insertCommand.Parameters.AddWithValue("@PasswordHash", request.Password); // TODO: Hash password
            
            var userId = (int)(await insertCommand.ExecuteScalarAsync() ?? 0);
            
            var user = new UserDto
            {
                UserID = userId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email
            };

            return new AuthResponse
            {
                Success = true,
                Message = "User registered successfully",
                User = user
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return new AuthResponse
            {
                Success = false,
                Message = $"Registration failed: {ex.Message}"
            };
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(_server) || string.IsNullOrEmpty(_database))
            {
                _logger.LogError("Database configuration is missing");
                throw new InvalidOperationException("Database configuration is missing");
            }

            await using var connection = await SqlService.OpenTokenConnectionAsync(_server, _database);
            _logger.LogInformation($"Attempting login for email: {request.Email}");
            
            // Try different possible ID column names
            await using var command = new SqlCommand(
                "SELECT * FROM Users WHERE Email = @Email", 
                connection);
            
            command.Parameters.AddWithValue("@Email", request.Email);
            
            await using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                _logger.LogInformation($"User found in database for email: {request.Email}");
                
                // Get column indices safely
                int idIndex = -1;
                int firstNameIndex = -1;
                int lastNameIndex = -1;
                int emailIndex = -1;
                int passwordIndex = -1;
                
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string columnName = reader.GetName(i).ToLower();
                    switch (columnName)
                    {
                        case "userid":
                        case "id":
                        case "user_id":
                            idIndex = i;
                            break;
                        case "firstname":
                        case "first_name":
                            firstNameIndex = i;
                            break;
                        case "lastname":
                        case "last_name":
                            lastNameIndex = i;
                            break;
                        case "email":
                            emailIndex = i;
                            break;
                        case "passwordhash":
                        case "password_hash":
                        case "password":
                            passwordIndex = i;
                            break;
                    }
                }
                
                if (passwordIndex == -1)
                {
                    _logger.LogError("PasswordHash column not found in Users table");
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Database configuration error"
                    };
                }
                
                var storedPassword = reader.GetString(passwordIndex);
                _logger.LogInformation($"Password check for user: {request.Email}");
                
                // Simple password check (should use proper hashing in production)
                if (storedPassword == request.Password)
                {
                    var user = new UserDto
                    {
                        UserID = idIndex >= 0 ? reader.GetInt32(idIndex) : 0,
                        FirstName = firstNameIndex >= 0 ? reader.GetString(firstNameIndex) : "",
                        LastName = lastNameIndex >= 0 ? reader.GetString(lastNameIndex) : "",
                        Email = emailIndex >= 0 ? reader.GetString(emailIndex) : request.Email
                    };

                    _logger.LogInformation($"Login successful for user: {request.Email}");
                    return new AuthResponse
                    {
                        Success = true,
                        Message = "Login successful",
                        User = user
                    };
                }
                else
                {
                    _logger.LogWarning($"Invalid password for user: {request.Email}");
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid password"
                    };
                }
            }
            else
            {
                _logger.LogWarning($"User not found: {request.Email}");
                return new AuthResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return new AuthResponse
            {
                Success = false,
                Message = $"Login failed: {ex.Message}"
            };
        }
    }
}