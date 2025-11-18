namespace FootballField.API.Entities;

// Enums
public enum NotificationType : byte
{
    System = 0,
    Booking = 1,
    Payment = 2,
    Review = 3,
    Other = 4
}

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? SenderId { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public NotificationType Type { get; set; } = NotificationType.System;
    public string? RelatedTable { get; set; }
    public int? RelatedId { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public User? Sender { get; set; }
}
