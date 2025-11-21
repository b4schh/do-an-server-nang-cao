namespace FootballField.API.Modules.ReviewManagement.Dtos
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public byte Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsVisible { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}