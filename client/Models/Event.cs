using System.Text.Json.Serialization;

namespace Client.Models
{
     public class Event
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = "";
        public string? EventDescription { get; set; } = "";
        public DateTime EventDateTime { get; set; }
        public string? EventImgURL { get; set; } = "";
        public int ClubId { get; set; }
    }
}
