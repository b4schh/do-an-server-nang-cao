namespace FootballField.API.Dtos.Field
{
    public class FieldDto
    {
        public int Id { get; set; }
        public int ComplexId { get; set; }
        public string Name { get; set; } = null!;
<<<<<<< HEAD
        public string? SurfaceType { get; set; }
        public string? FieldSize { get; set; }
=======
        public string FieldType { get; set; } = null!;
        public int PricePerHour { get; set; }
>>>>>>> origin/Vu
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
