using FootballField.API.Modules.ComplexManagement.Dtos;
using FootballField.API.Modules.ComplexManagement.Entities;
using FootballField.API.Modules.ComplexManagement.Repositories;
using FootballField.API.Shared.Storage;

namespace FootballField.API.Modules.ComplexManagement.Services;

public class ComplexImageService : IComplexImageService
{
    private readonly IComplexImageRepository _complexImageRepository;
    private readonly IComplexRepository _complexRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<ComplexImageService> _logger;

    public ComplexImageService(
        IComplexImageRepository complexImageRepository,
        IComplexRepository complexRepository,
        IStorageService storageService,
        ILogger<ComplexImageService> logger)
    {
        _complexImageRepository = complexImageRepository;
        _complexRepository = complexRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<ComplexImageResponseDto> CreateAsync(ComplexImageCreateDto dto)
    {
        var complexImage = new ComplexImage
        {
            ComplexId = dto.ComplexId,
            ImageUrl = dto.ImageUrl,
            IsMain = dto.IsMain
        };

        var createdImage = await _complexImageRepository.AddAsync(complexImage);

        return new ComplexImageResponseDto
        {
            Id = createdImage.Id,
            ComplexId = createdImage.ComplexId,
            ImageUrl = createdImage.ImageUrl,
            IsMain = createdImage.IsMain
        };
    }

    public async Task<ComplexImageResponseDto> GetByIdAsync(int id)
    {
        var complexImage = await _complexImageRepository.GetByIdAsync(id);

        if (complexImage == null)
        {
            throw new Exception("Không tìm thấy ảnh!");
        }

        return new ComplexImageResponseDto
        {
            Id = complexImage.Id,
            ComplexId = complexImage.ComplexId,
            ImageUrl = complexImage.ImageUrl,
            IsMain = complexImage.IsMain
        };
    }

    public async Task<List<ComplexImageResponseDto>> GetByComplexIdAsync(int complexId)
    {
        var complexImages = await _complexImageRepository.GetByComplexIdAsync(complexId);

        return complexImages.Select(ci => new ComplexImageResponseDto
        {
            Id = ci.Id,
            ComplexId = ci.ComplexId,
            ImageUrl = ci.ImageUrl,
            IsMain = ci.IsMain
        }).ToList();
    }

    public async Task DeleteAsync(int id)
    {
        var complexImage = await _complexImageRepository.GetByIdAsync(id);

        if (complexImage == null)
        {
            throw new Exception("Không tìm thấy ảnh!");
        }

        await _complexImageRepository.DeleteAsync(complexImage);
    }

    public async Task<ComplexImageResponseDto> UploadImageAsync(int complexId, IFormFile file, int userId, string? description = null)
    {
        // Validate file
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("Vui lòng chọn file để upload!");
        }

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
        {
            throw new ArgumentException("Chỉ chấp nhận file ảnh (JPEG, PNG, WEBP)!");
        }

        // Validate file size (5MB)
        if (file.Length > 5 * 1024 * 1024)
        {
            throw new ArgumentException("Kích thước file không được vượt quá 5MB!");
        }

        // Check ownership
        var complex = await _complexRepository.GetByIdAsync(complexId);
        if (complex == null)
        {
            throw new ArgumentException("Không tìm thấy complex!");
        }
        
        if (complex.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền upload ảnh cho complex này!");
        }

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
            Description = description,
            IsMain = false
        };

        var result = await CreateAsync(complexImageDto);
        
        // Trả về full URL cho client
        result.ImageUrl = _storageService.GetFullUrl(result.ImageUrl);

        _logger.LogInformation("Uploaded image for complex {ComplexId}: {ImageId}", complexId, result.Id);

        return result;
    }

    public async Task<List<ComplexImageResponseDto>> GetImagesByComplexIdAsync(int complexId)
    {
        var images = await GetByComplexIdAsync(complexId);
        
        // Chuyển relative path thành full URL trước khi trả về
        foreach (var image in images)
        {
            image.ImageUrl = _storageService.GetFullUrl(image.ImageUrl);
        }
        
        return images;
    }

    public async Task DeleteImageAsync(int imageId, int userId)
    {
        var image = await GetByIdAsync(imageId);

        // Check ownership
        var complex = await _complexRepository.GetByIdAsync(image.ComplexId);
        if (complex == null)
        {
            throw new ArgumentException("Không tìm thấy complex!");
        }
        
        if (complex.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xóa ảnh của complex này!");
        }

        // Delete from MinIO - parse object name từ relative path
        if (!string.IsNullOrEmpty(image.ImageUrl))
        {
            // ImageUrl giờ là: /football-field-booking-media/complexes/complex-1-xxx.webp
            var bucketName = "football-field-booking-media";
            var relativePath = image.ImageUrl.TrimStart('/');
            
            if (relativePath.StartsWith(bucketName + "/"))
            {
                var objectName = relativePath.Substring(bucketName.Length + 1);
                await _storageService.DeleteAsync(objectName);
                _logger.LogInformation("Deleted image from storage: {ObjectName}", objectName);
            }
        }

        // Delete from database
        await DeleteAsync(imageId);
        
        _logger.LogInformation("Deleted complex image: {ImageId}", imageId);
    }
}