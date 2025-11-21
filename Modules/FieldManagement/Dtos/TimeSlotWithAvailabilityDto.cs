namespace FootballField.API.Modules.FieldManagement.Dtos
{
    public class TimeSlotWithAvailabilityDto
    {
        public int Id { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public bool IsBooked { get; set; } // Trạng thái đã được đặt cho ngày cụ thể
    }
}