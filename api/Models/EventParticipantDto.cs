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
}