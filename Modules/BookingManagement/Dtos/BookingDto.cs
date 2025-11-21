using FootballField.API.Modules.BookingManagement.Entities;

namespace FootballField.API.Shared.Dtos.BookingManagement
{
    public class BookingDto
    {
        public int Id { get; set; }
        public int FieldId { get; set; }
        public string? FieldName { get; set; }
        public int ComplexId { get; set; }
        public string? ComplexName { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public int OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public int TimeSlotId { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime HoldExpiresAt { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DepositAmount { get; set; }
        public string? PaymentProofUrl { get; set; }
        public string? Note { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public string? BookingStatusText { get; set; }
        public int? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? CancelledBy { get; set; }
        public string? CancelledByName { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
