using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

namespace Client.Services;

public class AuthService
{
    private readonly IJSRuntime _jsRuntime;
    private UserSession? _currentUser;

    public AuthService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public UserSession? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;

    public event Action? AuthStateChanged;

    public async Task<bool> LoginAsync(string email, string password, HttpClient httpClient)
    {
        try
        {
            var loginRequest = new { Email = email, Password = password };
            var response = await httpClient.PostAsJsonAsync("/api/auth/login", loginRequest).ConfigureAwait(false);;

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);;
                var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (authResponse != null)
                {
                    _currentUser = new UserSession
                    {
                        UserId = authResponse.UserId,
                        Email = authResponse.Email,
                        IsAuthenticated = true
                    };

                    // Store in browser localStorage for persistence
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "user", JsonSerializer.Serialize(_currentUser)).ConfigureAwait(false);
                    
                    AuthStateChanged?.Invoke();
                    return true;
                }
            }
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (JsonException)
        {
            throw;
        }
        catch (Exception)
        {
            throw;
        }

        return false;
    }

    public async Task LogoutAsync()
    {
        _currentUser = null;
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "user").ConfigureAwait(false);
        AuthStateChanged?.Invoke();
    }

    public async Task<bool> InitializeAsync()
    {
        try
        {
            var userJson = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "user").ConfigureAwait(false);
            if (!string.IsNullOrEmpty(userJson))
            {
                _currentUser = JsonSerializer.Deserialize<UserSession>(userJson);
                return _currentUser?.IsAuthenticated == true;
            }
        }
        catch (JsonException)
        {
            throw;
        }
        catch (JSException)
        {
            throw;
        }

        return false;
    }
}

public class UserSession
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsAuthenticated { get; set; }
}

public class AuthResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}