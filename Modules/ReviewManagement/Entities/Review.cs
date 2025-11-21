using System.Numerics;
using FootballField.API.Modules.BookingManagement.Entities;
using FootballField.API.Modules.FieldManagement.Entities;
using FootballField.API.Modules.UserManagement.Entities;

namespace FootballField.API.Modules.ReviewManagement.Entities;

public class Review
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public byte Rating { get; set; } // 1-5
    public string? Comment { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public Booking Booking { get; set; } = null!;
}
