using System.ComponentModel.DataAnnotations;

namespace CampusConnect.Api.Models;

public class EventDto
{
    public int EventId { get; set; }

    [Required, MaxLength(25)]
    public string EventName { get; set; } = "";

    [MaxLength(100)]
    public string? EventDescription { get; set; } = "";

    public DateTime EventDateTime { get; set; }

    [MaxLength(2048)]
    public string? EventImgURL { get; set; } = "";

    public int ClubId { get; set; }
}
