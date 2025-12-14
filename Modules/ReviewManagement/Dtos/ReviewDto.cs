namespace FootballField.API.Modules.ReviewManagement.Dtos
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public ReviewUserDto User { get; set; } = null!;
        public byte Rating { get; set; }
        public string? Comment { get; set; }
        public List<string> Images { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Helpful { get; set; }
        public bool IsVotedByCurrentUser { get; set; }
    }
}