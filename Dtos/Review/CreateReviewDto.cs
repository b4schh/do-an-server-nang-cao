using System.ComponentModel.DataAnnotations;

namespace FootballField.API.Dtos.Review
{
    public class CreateReviewDto
    {
        [Required(ErrorMessage = "FieldId là bắt buộc")]
        public int FieldId { get; set; }

        [Required(ErrorMessage = "CustomerId là bắt buộc")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "BookingId là bắt buộc")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Điểm đánh giá là bắt buộc")]
        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Bình luận không được quá 1000 ký tự")]
        public string? Comment { get; set; }
    }
}
