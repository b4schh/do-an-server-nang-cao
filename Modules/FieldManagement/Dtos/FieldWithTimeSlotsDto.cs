namespace FootballField.API.Modules.FieldManagement.Dtos
{
    public class FieldWithTimeSlotsDto
    {
        public int Id { get; set; }
        public int ComplexId { get; set; }
        public string Name { get; set; } = null!;
        public string? SurfaceType { get; set; }
        public string? FieldSize { get; set; }
        public bool IsActive { get; set; }
        
        public IEnumerable<TimeSlotWithAvailabilityDto> TimeSlots { get; set; } = new List<TimeSlotWithAvailabilityDto>();
    }
}
