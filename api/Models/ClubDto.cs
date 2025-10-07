namespace CampusConnect.Api.Models;
using System.ComponentModel.DataAnnotations;

public class ClubDto
{
    public int ClubId { get; set; }

    [Required]
    [MaxLength(25)]
    public string ClubName { get; set; } = "";
    
    [MaxLength(2048)]
    public string? ClubImgURL { get; set; }
    
    [MaxLength(100)]
    public string? ClubDescription { get; set; }
}