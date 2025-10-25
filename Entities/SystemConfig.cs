namespace FootballField.API.Entities;

public class SystemConfig
{
    public int Id { get; set; }
    public string ConfigKey { get; set; } = null!;
    public string? ConfigValue { get; set; }
    public string DataType { get; set; } = "string";
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
