namespace FootballField.API.Modules.LocationManagement.Dtos;

public class WardDto
{
    public int Code { get; set; }
    public string Name { get; set; } = null!;
    public string Codename { get; set; } = null!;
    public string DivisionType { get; set; } = null!;
    public int ProvinceCode { get; set; }
}
