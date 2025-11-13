namespace FootballField.API.Entities;

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
