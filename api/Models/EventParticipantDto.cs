using System.ComponentModel.DataAnnotations;

namespace CampusConnect.Api.Models;

public class EventParticipantDto
{
    [Required]
    public int MemberId { get; set; }

    [Required]
    public int EventId { get; set; }
    
    [Required]
    public int ClubId { get; set; }

    /*[Optional]
    [MaxLength(25)]
    public string UserName { get; set; } = ;

    [Optional]
    [MaxLength(50)]
    public string ClubName { get; set; } = ;
    
    [Optional]
    [MaxLength(50)]
    public string EventName { get; set; } = ;*/
   
    // To get the optional fields, calls will need to made to respective functions? Or new functions will need to be written???
}