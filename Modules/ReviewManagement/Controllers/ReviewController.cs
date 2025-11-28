using FootballField.API.Modules.ReviewManagement.Dtos;
using FootballField.API.Modules.ReviewManagement.Services;
using FootballField.API.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FootballField.API.Shared.Middlewares;

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
        [HasPermission("review.moderate")]
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
        public async Task<IActionResult> Create([FromForm] CreateReviewDto createReviewDto)
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

        /// Xóa review của mình
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

        /// Xóa bất kỳ review nào
        [HttpDelete("admin/{id}")]
        [HasPermission("review.delete_any")]
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

        /// Ẩn/Hiện review
        [HttpPatch("admin/{id}/visibility")]
        [HasPermission("review.moderate")]
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

        /// Lấy danh sách review của complex với pagination và statistics
        [HttpGet("complex/{complexId}/paginated")]
        public async Task<IActionResult> GetComplexReviewsPaginated(
            int complexId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var (data, totalCount) = await _reviewService.GetComplexReviewsWithPaginationAsync(
                    complexId, pageIndex, pageSize);
                
                // Wrap in custom response structure as per requirement
                var response = new
                {
                    success = true,
                    message = "Lấy đánh giá thành công",
                    statusCode = 200,
                    data = new
                    {
                        reviews = data.Reviews,
                        statistics = data.Statistics
                    },
                    pagination = new
                    {
                        pageIndex,
                        pageSize,
                        totalRecords = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                        hasPreviousPage = pageIndex > 1,
                        hasNextPage = pageIndex < (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"Lỗi: {ex.Message}", 500));
            }
        }

        /// Vote review là hữu ích
        [HttpPost("{reviewId}/vote-helpful")]
        [Authorize]
        public async Task<IActionResult> VoteHelpful(int reviewId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<string>.Fail("Không thể xác thực người dùng", 401));
                }

                var success = await _reviewService.VoteHelpfulAsync(reviewId, userId);
                if (success)
                {
                    return Ok(ApiResponse<string>.Ok(null, "Đã vote review hữu ích"));
                }
                else
                {
                    return BadRequest(ApiResponse<string>.Fail("Bạn đã vote review này rồi", 400));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }

        /// <summary>
        /// [USER] Hủy vote review hữu ích
        /// </summary>
        [HttpDelete("{reviewId}/vote-helpful")]
        [Authorize]
        public async Task<IActionResult> UnvoteHelpful(int reviewId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<string>.Fail("Không thể xác thực người dùng", 401));
                }

                var success = await _reviewService.UnvoteHelpfulAsync(reviewId, userId);
                if (success)
                {
                    return Ok(ApiResponse<string>.Ok(null, "Đã hủy vote review hữu ích"));
                }
                else
                {
                    return BadRequest(ApiResponse<string>.Fail("Bạn chưa vote review này", 400));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }
    }
}