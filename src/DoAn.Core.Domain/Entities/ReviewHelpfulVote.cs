namespace DoAn.Core.Domain.Entities;

public class ReviewHelpfulVote
{
    public int Id { get; set; }
    public int ReviewId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Review Review { get; set; } = null!;
    public User User { get; set; } = null!;
}
