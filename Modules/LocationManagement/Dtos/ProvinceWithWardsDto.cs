namespace FootballField.API.Modules.LocationManagement.Dtos;

public class ProvinceWithWardsDto
{
    public int Code { get; set; }
    public string Name { get; set; } = null!;
    public string Codename { get; set; } = null!;
    public string DivisionType { get; set; } = null!;
    public IEnumerable<WardDto> Wards { get; set; } = new List<WardDto>();
}
