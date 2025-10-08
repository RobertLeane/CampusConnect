using System.Net.Http.Json;
using Client.Models;

namespace Client.Services;

public class AuthApi(HttpClient http)
{
    public async Task<AuthResponse> RegisterAsync(UserRegistration registration, CancellationToken ct = default)
    {
        try
        {
            Console.WriteLine($"Making registration API call to: {http.BaseAddress}api/auth/register");
            var response = await http.PostAsJsonAsync("/api/auth/register", registration, ct);
            Console.WriteLine($"Registration response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: ct);
                Console.WriteLine($"Registration successful for user: {result?.User?.Email}");
                return result ?? new AuthResponse { Success = false, Message = "Unknown error" };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Registration failed with status {response.StatusCode}: {errorContent}");
                return new AuthResponse { Success = false, Message = $"Registration failed: {errorContent}" };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Registration exception: {ex.Message}");
            return new AuthResponse { Success = false, Message = $"Registration error: {ex.Message}" };
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest loginRequest, CancellationToken ct = default)
    {
        try
        {
            Console.WriteLine($"Making login API call to: {http.BaseAddress}api/auth/login");
            var response = await http.PostAsJsonAsync("/api/auth/login", loginRequest, ct);
            Console.WriteLine($"Login response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: ct);
                Console.WriteLine($"Login successful for user: {result?.User?.Email}");
                return result ?? new AuthResponse { Success = false, Message = "Unknown error" };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Login failed with status {response.StatusCode}: {errorContent}");
                return new AuthResponse { Success = false, Message = $"Login failed: {errorContent}" };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login exception: {ex.Message}");
            return new AuthResponse { Success = false, Message = $"Login error: {ex.Message}" };
        }
    }
}