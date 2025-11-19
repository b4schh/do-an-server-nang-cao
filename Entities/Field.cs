namespace FootballField.API.Entities;

public class Field
{
    public int Id { get; set; }
    public int ComplexId { get; set; }
    public string Name { get; set; } = null!;
    public string? SurfaceType { get; set; }
    public string? FieldSize { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public Complex Complex { get; set; } = null!;
    public ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<FavoriteComplex> FavoritedBy { get; set; } = new List<FavoriteComplex>();
}
