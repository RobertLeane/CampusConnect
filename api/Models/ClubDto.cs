namespace CampusConnect.Api.Models;
using System.ComponentModel.DataAnnotations;

public class ClubDto
{
    public int ClubID { get; set; }

    [Required]
    [MaxLength(25)]
    public string ClubName { get; set; } = "";
    [Required]
    [MaxLength(25)]
    public string? Description { get; set; }
    // public string? Category { get; set; }


    //public DateTime CreatedAt { get; set; }
    //public bool Active { get; set; }
}