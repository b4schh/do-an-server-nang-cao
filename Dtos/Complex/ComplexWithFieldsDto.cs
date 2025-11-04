using FootballField.API.Dtos.Field;
using FootballField.API.Entities;

namespace FootballField.API.Dtos.Complex
{
    public class ComplexWithFieldsDto
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
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Description { get; set; }
        public ComplexStatus Status { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Nested DTOs
        public IEnumerable<FieldDto> Fields { get; set; } = new List<FieldDto>();
    }
}