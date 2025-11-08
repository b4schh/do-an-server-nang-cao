namespace FootballField.API.Entities;

public class Review
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int CustomerId { get; set; }
    public int ComplexId { get; set; }
    public byte Rating { get; set; } 
    public int FieldId { get; set; }
    public string? Comment { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public Booking Booking { get; set; } = null!;
    public User Customer { get; set; } = null!;
    public Complex Complex { get; set; } = null!;
    public Field Field { get; set; } = null!;
}
