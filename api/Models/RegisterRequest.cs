namespace CampusConnect.Api.Models;
using System.ComponentModel.DataAnnotations;

public class RegisterRequest
{
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = "";
    
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = "";
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = "";
    
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = "";
}