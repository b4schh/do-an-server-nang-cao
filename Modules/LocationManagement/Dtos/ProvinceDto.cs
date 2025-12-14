namespace FootballField.API.Modules.LocationManagement.Dtos;

public class ProvinceDto
{
    public int Code { get; set; }
    public string Name { get; set; } = null!;
    public string Codename { get; set; } = null!;
    public string DivisionType { get; set; } = null!;
}
