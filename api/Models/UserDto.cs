namespace CampusConnect.Api.Models;
using System.ComponentModel.DataAnnotations;

public class UserDto
{
    public int UserID { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";
    
    [Required]
    [MaxLength(20)]
    public string FirstName { get; set; } = "";
    
    [Required]
    [MaxLength(20)]
    public string LastName { get; set; } = "";

    // Remove Password from DTO - handle separately for security
    // public string Password { get; set; } = "";
}