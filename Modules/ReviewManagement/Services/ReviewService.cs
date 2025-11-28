using AutoMapper;
using FootballField.API.Modules.BookingManagement.Entities;
using FootballField.API.Modules.BookingManagement.Repositories;
using FootballField.API.Modules.ReviewManagement.Dtos;
using FootballField.API.Modules.ReviewManagement.Entities;
using FootballField.API.Modules.ReviewManagement.Repositories;
using FootballField.API.Shared.Dtos;
using FootballField.API.Shared.Storage;
using System.Security.Claims;

namespace FootballField.API.Modules.ReviewManagement.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IReviewHelpfulVoteRepository _voteRepository;
        private readonly IMapper _mapper;
        private readonly IBookingRepository _bookingRepository;
        private readonly IStorageService _storageService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReviewService(
            IReviewRepository reviewRepository, 
            IReviewHelpfulVoteRepository voteRepository,
            IMapper mapper, 
            IBookingRepository bookingRepository,
            IStorageService storageService,
            IHttpContextAccessor httpContextAccessor)
        {
            _reviewRepository = reviewRepository;
            _voteRepository = voteRepository;
            _mapper = mapper;
            _bookingRepository = bookingRepository;
            _storageService = storageService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<ReviewDto>> GetAllReviewsAsync()
        {
            var reviews = await _reviewRepository.GetAllAsync();
            return reviews.Select(MapToReviewDto).ToList();
        }

        public async Task<ReviewDto?> GetReviewByIdAsync(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            return review == null ? null : MapToReviewDto(review);
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByFieldIdAsync(int fieldId)
        {
            var reviews = await _reviewRepository.GetByFieldIdAsync(fieldId);
            return reviews.Select(MapToReviewDto).ToList();
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByComplexIdAsync(int complexId)
        {
            var reviews = await _reviewRepository.GetByComplexIdAsync(complexId);
            return reviews.Select(MapToReviewDto).ToList();
        }

        public async Task<IEnumerable<ReviewDto>> GetMyReviewsAsync(int customerId)
        {
            var reviews = await _reviewRepository.GetByCustomerIdAsync(customerId);
            return reviews.Select(MapToReviewDto).ToList();
        }

        public async Task<double> GetAverageRatingByFieldIdAsync(int fieldId)
        {
            return await _reviewRepository.GetAverageRatingByFieldIdAsync(fieldId);
        }

        public async Task<double> GetAverageRatingByComplexIdAsync(int complexId)
        {
            return await _reviewRepository.GetAverageRatingByComplexIdAsync(complexId);
        }

        public async Task<(GetComplexReviewsResponseDto Data, int TotalCount)> GetComplexReviewsWithPaginationAsync(
            int complexId, int pageIndex, int pageSize)
        {
            var (reviews, totalCount) = await _reviewRepository.GetComplexReviewsWithPaginationAsync(
                complexId, pageIndex, pageSize);
            
            var statistics = await _reviewRepository.GetReviewStatisticsAsync(complexId);
            
            var reviewDtos = reviews.Select(MapToReviewDto).ToList();
            
            var data = new GetComplexReviewsResponseDto
            {
                Reviews = reviewDtos,
                Statistics = statistics
            };

            return (data, totalCount);
        }

        private ReviewDto MapToReviewDto(Review review)
        {
            var dto = _mapper.Map<ReviewDto>(review);
            
            // Map User
            dto.User = new ReviewUserDto
            {
                Id = review.Booking.Customer.Id,
                Name = $"{review.Booking.Customer.LastName} {review.Booking.Customer.FirstName}",
                Avatar = review.Booking.Customer.AvatarUrl != null 
                    ? _storageService.GetFullUrl(review.Booking.Customer.AvatarUrl) 
                    : null,
                Role = DetermineCustomerRole(review.Booking.CustomerId, review.Booking.Field.ComplexId).Result
            };
            
            // Map Images
            dto.Images = review.Images
                .Select(img => _storageService.GetFullUrl(img.ImageUrl))
                .ToList();
            
            // Check if current user has voted
            var currentUserId = GetCurrentUserId();
            if (currentUserId.HasValue)
            {
                dto.IsVotedByCurrentUser = _voteRepository.HasVotedAsync(review.Id, currentUserId.Value).Result;
            }
            
            return dto;
        }
        
        private int? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        private async Task<string> DetermineCustomerRole(int customerId, int complexId)
        {
            var completedBookingsCount = await _reviewRepository
                .GetCustomerCompletedBookingsCountAsync(customerId, complexId);
            
            return completedBookingsCount >= 3 ? "Khách hàng thường xuyên" : "Khách hàng mới";
        }

        public async Task<ReviewDto> CreateReviewAsync(int customerId, CreateReviewDto createReviewDto)
        {
            // 1. Kiểm tra booking tồn tại
            var booking = await _bookingRepository.GetByIdAsync(createReviewDto.BookingId);
            if (booking == null)
            {
                throw new Exception("Không tìm thấy booking");
            }

            // 2. Kiểm tra booking thuộc về customer
            if (booking.CustomerId != customerId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền đánh giá booking này");
            }

            // 3. Kiểm tra booking đã completed
            if (booking.BookingStatus != BookingStatus.Completed)
            {
                throw new Exception("Chỉ có thể đánh giá sau khi hoàn thành trận đấu");
            }

            // 4. Kiểm tra đã review chưa
            if (await _reviewRepository.HasReviewForBookingAsync(createReviewDto.BookingId))
            {
                throw new Exception("Bạn đã đánh giá booking này rồi");
            }

            // 5. Tạo review
            var review = _mapper.Map<Review>(createReviewDto);
            review.CreatedAt = DateTime.UtcNow.AddHours(7);
            review.UpdatedAt = DateTime.UtcNow.AddHours(7);

            var created = await _reviewRepository.AddAsync(review);

            // 6. Upload images nếu có
            if (createReviewDto.Images != null && createReviewDto.Images.Any())
            {
                foreach (var imageFile in createReviewDto.Images)
                {
                    if (imageFile.Length > 0)
                    {
                        var fileName = $"reviews/{created.Id}/{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                        using var stream = imageFile.OpenReadStream();
                        var relativePath = await _storageService.UploadAsync(
                            stream, 
                            fileName, 
                            imageFile.ContentType);

                        var reviewImage = new ReviewImage
                        {
                            ReviewId = created.Id,
                            ImageUrl = relativePath,
                            CreatedAt = DateTime.UtcNow.AddHours(7)
                        };
                        
                        created.Images.Add(reviewImage);
                    }
                }
                
                await _reviewRepository.UpdateAsync(created);
            }
            
            // Load navigation properties for mapping
            var reviewWithDetails = await _reviewRepository.GetByIdAsync(created.Id);
            return MapToReviewDto(reviewWithDetails!);
        }

        public async Task UpdateReviewAsync(int id, int customerId, UpdateReviewDto updateReviewDto)
        {
            var existingReview = await _reviewRepository.GetByIdAsync(id);
            if (existingReview == null)
                throw new Exception("Không tìm thấy đánh giá");

            if (existingReview.IsDeleted)
                throw new Exception("Đánh giá đã bị xóa");

            // Kiểm tra quyền sở hữu - lấy CustomerId từ Booking
            if (existingReview.Booking.CustomerId != customerId)
                throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa đánh giá này");

            _mapper.Map(updateReviewDto, existingReview);
            existingReview.UpdatedAt = DateTime.UtcNow.AddHours(7);

            await _reviewRepository.UpdateAsync(existingReview);
        }

        public async Task DeleteReviewAsync(int id, int customerId)
        {
            var existingReview = await _reviewRepository.GetByIdAsync(id);
            if (existingReview == null)
                throw new Exception("Không tìm thấy đánh giá");

            if (existingReview.IsDeleted)
                throw new Exception("Đánh giá đã bị xóa");

            // Kiểm tra quyền sở hữu - lấy CustomerId từ Booking
            if (existingReview.Booking.CustomerId != customerId)
                throw new UnauthorizedAccessException("Bạn không có quyền xóa đánh giá này");

            await _reviewRepository.SoftDeleteAsync(id);
        }

        public async Task AdminDeleteReviewAsync(int id)
        {
            var existingReview = await _reviewRepository.GetByIdAsync(id);
            if (existingReview == null)
                throw new Exception("Không tìm thấy đánh giá");

            if (existingReview.IsDeleted)
                throw new Exception("Đánh giá đã bị xóa");

            await _reviewRepository.SoftDeleteAsync(id);
        }

        public async Task AdminToggleVisibilityAsync(int id, bool isVisible)
        {
            var existingReview = await _reviewRepository.GetByIdAsync(id);
            if (existingReview == null)
                throw new Exception("Không tìm thấy đánh giá");

            if (existingReview.IsDeleted)
                throw new Exception("Đánh giá đã bị xóa");

            existingReview.IsVisible = isVisible;
            existingReview.UpdatedAt = DateTime.UtcNow.AddHours(7);

            await _reviewRepository.UpdateAsync(existingReview);
        }

        public async Task<bool> VoteHelpfulAsync(int reviewId, int userId)
        {
            // Kiểm tra review tồn tại
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null || review.IsDeleted || !review.IsVisible)
                throw new Exception("Không tìm thấy đánh giá");

            // Kiểm tra đã vote chưa
            if (await _voteRepository.HasVotedAsync(reviewId, userId))
                return false; // Đã vote rồi

            var vote = new ReviewHelpfulVote
            {
                ReviewId = reviewId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow.AddHours(7)
            };

            await _voteRepository.AddAsync(vote);
            return true;
        }

        public async Task<bool> UnvoteHelpfulAsync(int reviewId, int userId)
        {
            var vote = await _voteRepository.GetVoteAsync(reviewId, userId);
            if (vote == null)
                return false; // Chưa vote

            await _voteRepository.DeleteAsync(vote);
            return true;
        }
    }
}