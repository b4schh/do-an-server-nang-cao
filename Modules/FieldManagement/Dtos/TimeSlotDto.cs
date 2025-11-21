namespace FootballField.API.Modules.FieldManagement.Dtos
{
    public class TimeSlotDto
    {
        public int Id { get; set; }
        public int FieldId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
