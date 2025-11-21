using FootballField.API.Modules.UserManagement.Entities;

namespace FootballField.API.Modules.OwnerSettingsManagement.Entities;

public class OwnerSetting
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public decimal? DepositRate { get; set; }
    public int? MinBookingNotice { get; set; }
    public bool AllowReview { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public User Owner { get; set; } = null!;
}
