using CampusConnect.Api.Models;

namespace CampusConnect.Api.Services;

public interface IUserService
{
    Task<bool> EmailExistsAsync(string email);
    Task<int> RegisterAsync(RegisterRequest req);
    Task<AuthResponse?> LoginAsync(LoginRequest req);
    Task<int> GetUserCountAsync();
}
