namespace FootballField.API.Dtos.Review
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int FieldId { get; set; }
        public int CustomerId { get; set; }
        public int BookingId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsVisible { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
