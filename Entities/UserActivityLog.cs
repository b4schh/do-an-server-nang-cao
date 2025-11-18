namespace FootballField.API.Entities;

public class UserActivityLog
{
    public long Id { get; set; }
    public int? UserId { get; set; }
    public string? Action { get; set; }
    public string? TargetTable { get; set; }
    public int? TargetId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
