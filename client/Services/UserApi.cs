using System.Net.Http.Json;
using Client.Models;

namespace Client.Services;

public class UserApi(HttpClient http)
{
    // Methods to call the User API endpoints

    public async Task<List<User>> GetUsersAsync(CancellationToken ct = default)
        => await http.GetFromJsonAsync<List<User>>("/api/users", ct) ?? [];

    public async Task<User?> GetUserByIdAsync(int userId, CancellationToken ct = default)
        => await http.GetFromJsonAsync<User?>($"/api/users/{userId}", ct);

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default)
        => await http.GetFromJsonAsync<User?>($"/api/users/email/{email}", ct);

    public async Task<User> CreateUserAsync(User user, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("/api/users", user, ct);
        if (response.IsSuccessStatusCode)
        {
            var createdUser = await response.Content.ReadFromJsonAsync<User>(cancellationToken: ct);
            return createdUser ?? new User();
        }
        return new User();
    }
}
