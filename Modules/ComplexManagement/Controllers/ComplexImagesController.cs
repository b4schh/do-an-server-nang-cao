using Microsoft.AspNetCore.Mvc;
using FootballField.API.Shared.Dtos;
using FootballField.API.Modules.ComplexManagement.Services;
using FootballField.API.Modules.ComplexManagement.Dtos;
using FootballField.API.Shared.Middlewares;
using System.Security.Claims;

namespace FootballField.API.Modules.ComplexManagement.Controllers
{
    [ApiController]
    [Route("api/complex-images")]
    public class ComplexImagesController : ControllerBase
    {
        private readonly IComplexImageService _complexImageService;

        public ComplexImagesController(IComplexImageService complexImageService)
        {
            _complexImageService = complexImageService;
        }

        [HttpPost("{complexId:int}/upload")]
        [HasPermission("complex.upload_images")]
        public async Task<IActionResult> UploadComplexImage(int complexId, IFormFile file, string? description = null)
        {
            var userId = GetUserId();
            var result = await _complexImageService.UploadImageAsync(complexId, file, userId, description);
            return Ok(ApiResponse<ComplexImageResponseDto>.Ok(result, "Upload ảnh thành công!"));
        }

        [HttpGet("{complexId:int}")]
        public async Task<IActionResult> GetComplexImages(int complexId)
        {
            var images = await _complexImageService.GetImagesByComplexIdAsync(complexId);
            return Ok(ApiResponse<List<ComplexImageResponseDto>>.Ok(images, "Lấy danh sách ảnh thành công!"));
        }

        [HttpDelete("{imageId:int}")]
        [HasPermission("complex.upload_images")]
        public async Task<IActionResult> DeleteComplexImage(int imageId)
        {
            var userId = GetUserId();
            await _complexImageService.DeleteImageAsync(imageId, userId);
            return Ok(ApiResponse<object?>.Ok(null, "Xóa ảnh thành công!"));
        }

        // Helper method to get userId from JWT claims
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException("Không thể xác thực user");
            return userId;
        }
    }
}