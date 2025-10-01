using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CampusConnect.Api.Services;

namespace CampusConnect.Api.Functions;

public class UserFunctions
{
    private readonly ILogger<UserFunctions> _logger;
    private readonly IUserService _userService;

    public UserFunctions(ILogger<UserFunctions> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    // Azure Functions for User operations
    [Function("GetUsers")]
    public async Task<IActionResult> GetUsers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users")] HttpRequest req)
    {
        try
        {
            _logger.LogInformation("Getting users from database");
            var users = await _userService.GetAllUsersAsync();
            _logger.LogInformation($"Retrieved {users.Count} users from database");
            return new OkObjectResult(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users from database");
            return new ObjectResult("An error occurred while retrieving users")
            {
                StatusCode = 500
            };
        }
    }

    [Function("GetUserById")]
    public async Task<IActionResult> GetUserById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/{userId:int}")] HttpRequest req,
        int userId)
    {
        try
        {
            _logger.LogInformation($"Getting user {userId} from database");
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving user {userId} from database");
            return new ObjectResult("An error occurred while retrieving the user")
            {
                StatusCode = 500
            };
        }
    }


    [Function("CreateUser")]
    public async Task<IActionResult> CreateUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users")] HttpRequest req)
    {
        try
        {
            _logger.LogInformation("Creating a new user in the database");
            using var reader = new StreamReader(req.Body);
            var body = await reader.ReadToEndAsync();
            var userDto = System.Text.Json.JsonSerializer.Deserialize<CampusConnect.Api.Models.UserDto>(body);
            if (userDto == null)
            {
                return new BadRequestObjectResult("Invalid user data.");
            }
            var user = await _userService.CreateUserAsync(userDto);
            _logger.LogInformation($"Created user with ID {user.UserID}");
            return new CreatedAtRouteResult("GetUserById", new { userId = user.UserID }, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating a new user in the database");
            return new ObjectResult("An error occurred while creating the user")
            {
                StatusCode = 500
            };
        }

    }

    [Function("GetUserByEmail")]
    public async Task<IActionResult> GetUserByEmail(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/email/{email}")] HttpRequest req,
        string email)
    {
        try
        {
            _logger.LogInformation($"Getting user by email {email} from database");
            var user = await _userService.GetUserByEmailAsync(email);

            if (user == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving user by email {email} from database");
            return new ObjectResult("An error occurred while retrieving the user")
            {
                StatusCode = 500
            };
        }
    }
}