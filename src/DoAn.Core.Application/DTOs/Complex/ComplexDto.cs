namespace DoAn.Core.Application.DTOs.Complex;

public class ComplexDto
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
        public ComplexStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? MainImageUrl { get; set; }
    public int FieldCount { get; set; }
}
