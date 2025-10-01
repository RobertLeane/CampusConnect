namespace Client.Models
{
    public class User
    {
        // User properties to match the API UserDto

        public int UserID { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";

        // Remove Password from client model for security
        // public string Password { get; set; } = "";
    }
}