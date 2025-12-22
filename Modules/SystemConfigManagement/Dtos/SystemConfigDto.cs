namespace FootballField.API.Modules.SystemConfigManagement.Dtos
{
    public class SystemConfigDto
    {
        public int Id { get; set; }
        public string ConfigKey { get; set; } = null!;
        public string? ConfigValue { get; set; }
        public string DataType { get; set; } = "string";
        public string? Description { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
