using System.ComponentModel.DataAnnotations;

namespace CampusConnect.Api.Models;

public class RegisterRequest
{
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(255)]
    public string Password { get; set; } = string.Empty;
}
