namespace CampusConnect.Api.Models;

public class EventDto
{
    // TODO: Add Event properties (EventId, EventName, Description, EventDate, Location, MaxParticipants, ClubId, CreatedAt, Active)
    // Hint: Match your database Event table schema
    public int EventId { get; set; }
    public string? EventName { get; set; } = "";
    public string? Description { get; set; } = "";
    public DateTime EventDateTime { get; set; }
    public int ClubId { get; set; }

}