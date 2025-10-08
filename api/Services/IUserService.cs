using CampusConnect.Api.Models;

namespace CampusConnect.Api.Services
{
    public interface IUserService
    {
        // Method signatures for User operations
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<UserDto> CreateUserAsync(UserDto user);
        
        // Authentication methods
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }
}