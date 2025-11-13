namespace FootballField.API.Entities;

public class SystemLog
{
    public long Id { get; set; }
    public string? LogLevel { get; set; }
    public string? Source { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
}
