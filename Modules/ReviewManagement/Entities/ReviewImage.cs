namespace FootballField.API.Modules.ReviewManagement.Entities;

public class ReviewImage
{
    public int Id { get; set; }
    public int ReviewId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Review Review { get; set; } = null!;
}
