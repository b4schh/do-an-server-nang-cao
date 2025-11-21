using FootballField.API.Modules.ReviewManagement.Dtos;
using FootballField.API.Modules.ReviewManagement.Services;
using FootballField.API.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FootballField.API.Modules.ReviewManagement.Controllers
{
    [ApiController]
    [Route("api/reviews")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// [PUBLIC] Lấy tất cả review (admin only - có thể thấy review ẩn)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var reviews = await _reviewService.GetAllReviewsAsync();
                return Ok(ApiResponse<IEnumerable<ReviewDto>>.Ok(reviews, "Lấy danh sách đánh giá thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"Lỗi: {ex.Message}", 500));
            }
        }

        /// <summary>
        /// [PUBLIC] Lấy review theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var review = await _reviewService.GetReviewByIdAsync(id);
                if (review == null)
                    return NotFound(ApiResponse<string>.Fail("Không tìm thấy đánh giá", 404));

                return Ok(ApiResponse<ReviewDto>.Ok(review, "Lấy thông tin đánh giá thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"Lỗi: {ex.Message}", 500));
            }
        }

        /// <summary>
        /// [PUBLIC] Lấy danh sách review theo Field ID (chỉ hiển thị review visible)
        /// </summary>
        [HttpGet("field/{fieldId}")]
        public async Task<IActionResult> GetByFieldId(int fieldId)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByFieldIdAsync(fieldId);
                return Ok(ApiResponse<IEnumerable<ReviewDto>>.Ok(reviews, "Lấy danh sách đánh giá thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"Lỗi: {ex.Message}", 500));
            }
        }

        /// <summary>
        /// [PUBLIC] Lấy danh sách review theo Complex ID (chỉ hiển thị review visible)
        /// </summary>
        [HttpGet("complex/{complexId}")]
        public async Task<IActionResult> GetByComplexId(int complexId)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByComplexIdAsync(complexId);
                return Ok(ApiResponse<IEnumerable<ReviewDto>>.Ok(reviews, "Lấy danh sách đánh giá thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"Lỗi: {ex.Message}", 500));
            }
        }

        /// <summary>
        /// [PUBLIC] Lấy điểm trung bình của Field
        /// </summary>
        [HttpGet("field/{fieldId}/average-rating")]
        public async Task<IActionResult> GetAverageRatingByField(int fieldId)
        {
            try
            {
                var avgRating = await _reviewService.GetAverageRatingByFieldIdAsync(fieldId);
                return Ok(ApiResponse<double>.Ok(avgRating, "Lấy điểm trung bình thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"Lỗi: {ex.Message}", 500));
            }
        }

        /// <summary>
        /// [PUBLIC] Lấy điểm trung bình của Complex
        /// </summary>
        [HttpGet("complex/{complexId}/average-rating")]
        public async Task<IActionResult> GetAverageRatingByComplex(int complexId)
        {
            try
            {
                var avgRating = await _reviewService.GetAverageRatingByComplexIdAsync(complexId);
                return Ok(ApiResponse<double>.Ok(avgRating, "Lấy điểm trung bình thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"Lỗi: {ex.Message}", 500));
            }
        }

        /// <summary>
        /// [USER] Lấy tất cả review của mình
        /// </summary>
        [HttpGet("my-reviews")]
        [Authorize]
        public async Task<IActionResult> GetMyReviews()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<string>.Fail("Không thể xác thực người dùng", 401));
                }

                var reviews = await _reviewService.GetMyReviewsAsync(userId);
                return Ok(ApiResponse<IEnumerable<ReviewDto>>.Ok(reviews, "Lấy danh sách đánh giá của bạn thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"Lỗi: {ex.Message}", 500));
            }
        }

        /// <summary>
        /// [USER] Tạo review mới (chỉ được tạo sau khi hoàn thành booking)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateReviewDto createReviewDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<string>.Fail("Không thể xác thực người dùng", 401));
                }

                var created = await _reviewService.CreateReviewAsync(userId, createReviewDto);
                return Ok(ApiResponse<ReviewDto>.Ok(created, "Tạo đánh giá thành công", 201));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<string>.Fail(ex.Message, 403));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }

        /// <summary>
        /// [USER] Cập nhật review của mình
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewDto updateReviewDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<string>.Fail("Không thể xác thực người dùng", 401));
                }

                await _reviewService.UpdateReviewAsync(id, userId, updateReviewDto);
                return Ok(ApiResponse<string>.Ok(null, "Cập nhật đánh giá thành công"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<string>.Fail(ex.Message, 403));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }

        /// <summary>
        /// [USER] Xóa review của mình
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<string>.Fail("Không thể xác thực người dùng", 401));
                }

                await _reviewService.DeleteReviewAsync(id, userId);
                return Ok(ApiResponse<string>.Ok(null, "Xóa đánh giá thành công"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<string>.Fail(ex.Message, 403));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }

        /// <summary>
        /// [ADMIN] Xóa bất kỳ review nào
        /// </summary>
        [HttpDelete("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDelete(int id)
        {
            try
            {
                await _reviewService.AdminDeleteReviewAsync(id);
                return Ok(ApiResponse<string>.Ok(null, "Admin đã xóa đánh giá thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }

        /// <summary>
        /// [ADMIN] Ẩn/Hiện review
        /// </summary>
        [HttpPatch("admin/{id}/visibility")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminToggleVisibility(int id, [FromBody] bool isVisible)
        {
            try
            {
                await _reviewService.AdminToggleVisibilityAsync(id, isVisible);
                var message = isVisible ? "Hiển thị đánh giá thành công" : "Ẩn đánh giá thành công";
                return Ok(ApiResponse<string>.Ok(null, message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }
    }
}