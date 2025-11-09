namespace FootballField.API.Entities;

// Enums
public enum ComplexStatus : byte
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public class Complex
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string Name { get; set; } = null!;
    public string? Street { get; set; }
    public string? Ward { get; set; }
    public string? Province { get; set; }
    public string? Phone { get; set; }
    public TimeSpan? OpeningTime { get; set; }
    public TimeSpan? ClosingTime { get; set; }
    public string? Description { get; set; }
    public ComplexStatus Status { get; set; } = ComplexStatus.Pending;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public User Owner { get; set; } = null!;
    public ICollection<Field> Fields { get; set; } = new List<Field>();
    public ICollection<ComplexImage> ComplexImages { get; set; } = new List<ComplexImage>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

}
