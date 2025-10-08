namespace CampusConnect.Api.Models;

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public UserDto? User { get; set; }
    public string? Token { get; set; } // For future JWT implementation
}