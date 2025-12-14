namespace FootballField.API.Modules.ComplexManagement.Dtos
{
    public class AvailabilityDto
    {
        public int ComplexId { get; set; }
        public List<AvailabilityDayDto> Days { get; set; } = new List<AvailabilityDayDto>();
    }

    public class AvailabilityDayDto
    {
        public string Date { get; set; } = null!; // Format: yyyy-MM-dd
        public List<AvailabilityTimeSlotDto> TimeSlots { get; set; } = new List<AvailabilityTimeSlotDto>();
    }

    public class AvailabilityTimeSlotDto
    {
        public string StartTime { get; set; } = null!; // Format: HH:mm
        public string EndTime { get; set; } = null!; // Format: HH:mm
        public List<AvailabilityFieldDto> Fields { get; set; } = new List<AvailabilityFieldDto>();
    }

    public class AvailabilityFieldDto
    {
        public int FieldId { get; set; }
        public string FieldName { get; set; } = null!;
        public string? FieldSize { get; set; }
        public string? SurfaceType { get; set; }
        public int TimeSlotId { get; set; }
        public decimal Price { get; set; }
        public bool IsBooked { get; set; }
        public bool IsPast { get; set; }
    }
}
