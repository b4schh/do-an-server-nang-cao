namespace FootballField.API.Modules.FieldManagement.Dtos
{
public class TimeSlotPriceDto
{
    public int FieldId { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal Price { get; set; }
}
}