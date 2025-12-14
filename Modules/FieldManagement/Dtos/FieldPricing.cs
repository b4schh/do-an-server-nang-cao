namespace FootballField.API.Modules.FieldManagement.Dtos
{

public class FieldPricing
{
    public int FieldId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
}