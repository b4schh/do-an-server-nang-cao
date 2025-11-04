using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FootballField.API.Dtos;
using System.Security.Claims;
using FootballField.API.Services.Interfaces;
using FootballField.API.Storage;

namespace FootballField.API.Controllers
{
    [ApiController]
    [Route("api/complex-images")]
    public class ComplexImagesController : ControllerBase
    {
        private readonly IStorageService _storageService;
        private readonly IComplexImageService _complexImageService;
        private readonly IComplexService _complexService;

        public ComplexImagesController(
            IStorageService storageService,
            IComplexImageService complexImageService,
            IComplexService complexService)
        {
            _storageService = storageService;
            _complexImageService = complexImageService;
            _complexService = complexService;
        }

        [HttpPost("{complexId:int}/upload")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> UploadComplexImage(int complexId, IFormFile file, string? description = null)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                {
                    return BadRequest(ApiResponse<object>.Fail("Vui lòng chọn file để upload!"));
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest(ApiResponse<object>.Fail("Chỉ chấp nhận file ảnh (JPEG, PNG, WEBP)!"));
                }

                // Validate file size (5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(ApiResponse<object>.Fail("Kích thước file không được vượt quá 5MB!"));
                }

                // Check if complex exists and user owns it
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized(ApiResponse<object>.Fail("Không thể xác thực người dùng!"));
                }

                var complex = await _complexService.GetComplexByIdAsync(complexId);

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = $"complex-{complexId}-{Guid.NewGuid()}{fileExtension}";
                var objectName = $"complexes/{fileName}";

                // Upload to MinIO - nhận về relative path
                string relativePath;
                using (var stream = file.OpenReadStream())
                {
                    relativePath = await _storageService.UploadAsync(stream, objectName, file.ContentType);
                }

                // Save relative path to database
                var complexImageDto = new ComplexImageCreateDto
                {
                    ComplexId = complexId,
                    ImageUrl = relativePath, // Lưu relative path
                    Description = description
                };

                var result = await _complexImageService.CreateAsync(complexImageDto);
                
                // Trả về full URL cho client
                result.ImageUrl = _storageService.GetFullUrl(result.ImageUrl);

                return Ok(ApiResponse<ComplexImageResponseDto>.Ok(result, "Upload ảnh thành công!"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"Đã xảy ra lỗi: {ex.Message}"));
            }
        }

        [HttpGet("{complexId:int}")]
        public async Task<IActionResult> GetComplexImages(int complexId)
        {
            try
            {
                var images = await _complexImageService.GetByComplexIdAsync(complexId);
                
                // Chuyển relative path thành full URL trước khi trả về
                foreach (var image in images)
                {
                    image.ImageUrl = _storageService.GetFullUrl(image.ImageUrl);
                }
                
                return Ok(ApiResponse<List<ComplexImageResponseDto>>.Ok(images, "Lấy danh sách ảnh thành công!"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"Đã xảy ra lỗi: {ex.Message}"));
            }
        }

        [HttpDelete("{imageId:int}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> DeleteComplexImage(int imageId)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized(ApiResponse<object>.Fail("Không thể xác thực người dùng!"));
                }

                var image = await _complexImageService.GetByIdAsync(imageId);

                // Delete from MinIO - parse object name từ relative path
                if (!string.IsNullOrEmpty(image.ImageUrl))
                {
                    // ImageUrl giờ là: /football-field-images/complexes/complex-1-xxx.webp
                    // Cần extract: complexes/complex-1-xxx.webp
                    var bucketName = "football-field-images";
                    var relativePath = image.ImageUrl.TrimStart('/');
                    
                    if (relativePath.StartsWith(bucketName + "/"))
                    {
                        var objectName = relativePath.Substring(bucketName.Length + 1);
                        await _storageService.DeleteAsync(objectName);
                    }
                }

                // Delete from database
                await _complexImageService.DeleteAsync(imageId);

                return Ok(ApiResponse<object?>.Ok(null, "Xóa ảnh thành công!"));
            }

            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"Đã xảy ra lỗi: {ex.Message}"));
            }
        }
    }
}