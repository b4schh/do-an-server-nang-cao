using FootballField.API.Modules.BookingManagement.Entities;
using FootballField.API.Modules.ComplexManagement.Entities;
using FootballField.API.Modules.NotificationManagement.Entities;
using FootballField.API.Modules.OwnerSettingsManagement.Entities;
using FootballField.API.Modules.ReviewManagement.Entities;

namespace FootballField.API.Modules.UserManagement.Entities;

// Enums
public enum UserRole : byte
{
    Customer = 0,
    Owner = 1,
    Admin = 2
}

public enum UserStatus : byte
{
    Inactive = 0,
    Active = 1,
    Banned = 2
}

public class User
{
    public int Id { get; set; }
    public string LastName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Password { get; set; }
    public UserRole Role { get; set; } = UserRole.Customer;
    public string? AvatarUrl { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public bool IsDeleted { get; set; } = false;
    public int? DeletedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public User? DeletedByUser { get; set; }
    public ICollection<Complex> OwnedComplexes { get; set; } = new List<Complex>();
    public ICollection<Booking> CustomerBookings { get; set; } = new List<Booking>();
    public ICollection<Booking> OwnerBookings { get; set; } = new List<Booking>();
    public ICollection<FavoriteComplex> FavoriteComplexes { get; set; } = new List<FavoriteComplex>();
    public ICollection<Notification> ReceivedNotifications { get; set; } = new List<Notification>();
    public ICollection<Notification> SentNotifications { get; set; } = new List<Notification>();
    public OwnerSetting? OwnerSetting { get; set; }
}
