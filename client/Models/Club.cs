namespace Client.Models
{
    public class Club
    {
        public int ClubId { get; set; }
        public string ClubName { get; set; } = "";
        public string? ClubImgURL { get; set; }
        public string? ClubDescription { get; set; }
        // public string? Category { get; set; }
        //public DateTime CreatedAt { get; set; }
        //public bool Active { get; set; }
    }
}