namespace FootballField.API.Modules.ComplexManagement.Entities;

public class ComplexImage
{
    public int Id { get; set; }
    public int ComplexId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public bool IsMain { get; set; } = false;

    // Navigation properties
    public Complex Complex { get; set; } = null!;
}
