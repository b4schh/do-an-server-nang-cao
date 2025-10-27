namespace FootballField.API.Entities;

public class TimeSlot
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    public Field Field { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
