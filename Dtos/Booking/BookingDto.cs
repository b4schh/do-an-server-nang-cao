using FootballField.API.Entities;

namespace FootballField.API.Dtos.Booking
{
    public class BookingDto
    {
        public int Id { get; set; }
        public int FieldId { get; set; }
        public int CustomerId { get; set; }
        public int OwnerId { get; set; }
        public DateTime BookingDate { get; set; }
        public int TimeSlotId { get; set; }
        public int DepositAmount { get; set; }
        public int TotalAmount { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public string? Note { get; set; }
        public DateTime? CancelledAt { get; set; }
        public int? CancelledBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
