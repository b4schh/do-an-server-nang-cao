namespace FootballField.API.Modules.LocationManagement.Entities;

public class Ward
{
    public int Id { get; set; }
    public int Code { get; set; }
    public string Name { get; set; } = null!;
    public string Codename { get; set; } = null!;
    public string DivisionType { get; set; } = null!;
    public int ProvinceCode { get; set; }

    // Navigation properties
    public Province Province { get; set; } = null!;
}
