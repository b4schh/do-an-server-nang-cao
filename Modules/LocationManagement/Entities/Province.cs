namespace FootballField.API.Modules.LocationManagement.Entities;

public class Province
{
    public int Id { get; set; }
    public int Code { get; set; }
    public string Name { get; set; } = null!;
    public string Codename { get; set; } = null!;
    public string DivisionType { get; set; } = null!;

    // Navigation properties
    public ICollection<Ward> Wards { get; set; } = new List<Ward>();
}
