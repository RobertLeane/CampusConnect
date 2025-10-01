using System.ComponentModel.DataAnnotations;

namespace Client.Models
{
    public class UserRegistration
    {
        [Required]
        [MaxLength(20)]
        public string FirstName { get; set; } = "";
        
        [Required]
        [MaxLength(20)]
        public string LastName { get; set; } = "";
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
        
        [Required]
        [MinLength(8)]
        public string Password { get; set; } = "";
        
        // Convert to User for API call (excluding password)
        public User ToUser()
        {
            return new User
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                Email = this.Email
            };
        }
    }
}