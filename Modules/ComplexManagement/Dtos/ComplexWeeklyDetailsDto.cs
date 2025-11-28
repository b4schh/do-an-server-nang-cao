using FootballField.API.Modules.FieldManagement.Dtos;

namespace FootballField.API.Modules.ComplexManagement.Dtos
{
    /// <summary>
    /// DTO chứa thông tin complex và availability của các field theo từng ngày trong tuần
    /// </summary>
    public class ComplexWeeklyDetailsDto
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string Name { get; set; } = null!;
        public string? Street { get; set; }
        public string? Ward { get; set; }
        public string? Province { get; set; }
        public string? Phone { get; set; }
        public TimeSpan? OpeningTime { get; set; }
        public TimeSpan? ClosingTime { get; set; }
        public string? Description { get; set; }
        public byte Status { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// Danh sách các field với availability theo từng ngày
        /// </summary>
        public IEnumerable<FieldWeeklyAvailabilityDto> Fields { get; set; } = new List<FieldWeeklyAvailabilityDto>();
    }

    /// <summary>
    /// DTO chứa thông tin field và danh sách availability theo từng ngày
    /// </summary>
    public class FieldWeeklyAvailabilityDto
    {
        public int Id { get; set; }
        public int ComplexId { get; set; }
        public string Name { get; set; } = null!;
        public string? SurfaceType { get; set; }
        public string? FieldSize { get; set; }
        public string? FieldType { get; set; }
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Danh sách timeslots với availability theo từng ngày
        /// Key: Date (yyyy-MM-dd), Value: List of TimeSlots
        /// </summary>
        public Dictionary<string, IEnumerable<DailyTimeSlotDto>> DailyTimeSlots { get; set; } = new Dictionary<string, IEnumerable<DailyTimeSlotDto>>();
    }

    /// <summary>
    /// DTO chứa thông tin timeslot và availability cho một ngày cụ thể
    /// </summary>
    public class DailyTimeSlotDto
    {
        public int Id { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Trạng thái availability cho ngày này
        /// true = đã được book, false = còn trống
        /// </summary>
        public bool IsAvailable { get; set; }
    }
}
