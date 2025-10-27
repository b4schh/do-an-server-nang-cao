namespace FootballField.API.Entities;

public class OwnerSetting
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public decimal? DepositRate { get; set; }
    public int? MinBookingNotice { get; set; }
    public bool AllowReview { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    public User Owner { get; set; } = null!;
}
