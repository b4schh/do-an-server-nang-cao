using System.ComponentModel.DataAnnotations;

namespace FootballField.API.Dtos.Booking
{
    public class UploadPaymentProofDto
    {
        [Required(ErrorMessage = "File ảnh là bắt buộc")]
        public IFormFile PaymentProofImage { get; set; } = null!;

        [MaxLength(255, ErrorMessage = "Ghi chú không được vượt quá 255 ký tự")]
        public string? PaymentNote { get; set; }
    }
}
