using System.ComponentModel.DataAnnotations;

namespace FootballField.API.Dtos.Review
{
    public class UpdateReviewDto
    {
        [Required(ErrorMessage = "Điểm đánh giá là bắt buộc")]
        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5")]
        public byte Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Bình luận không được quá 1000 ký tự")]
        public string? Comment { get; set; }
    }
}