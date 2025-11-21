using FootballField.API.Modules.FieldManagement.Entities;
using FootballField.API.Modules.UserManagement.Entities;

namespace FootballField.API.Modules.ComplexManagement.Entities;

public class FavoriteComplex
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ComplexId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Field Field { get; set; } = null!;
}
