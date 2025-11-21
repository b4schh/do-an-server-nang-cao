using AutoMapper;
using FootballField.API.Modules.BookingManagement.Entities;
using FootballField.API.Modules.BookingManagement.Repositories;
using FootballField.API.Modules.ReviewManagement.Dtos;
using FootballField.API.Modules.ReviewManagement.Entities;
using FootballField.API.Modules.ReviewManagement.Repositories;

namespace FootballField.API.Modules.ReviewManagement.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IMapper _mapper;
        private readonly IBookingRepository _bookingRepository;

        public ReviewService(IReviewRepository reviewRepository, IMapper mapper, IBookingRepository bookingRepository)
        {
            _reviewRepository = reviewRepository;
            _mapper = mapper;
            _bookingRepository = bookingRepository;
        }

        public async Task<IEnumerable<ReviewDto>> GetAllReviewsAsync()
        {
            var reviews = await _reviewRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<ReviewDto?> GetReviewByIdAsync(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            return review == null ? null : _mapper.Map<ReviewDto>(review);
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByFieldIdAsync(int fieldId)
        {
            var reviews = await _reviewRepository.GetByFieldIdAsync(fieldId);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByComplexIdAsync(int complexId)
        {
            var reviews = await _reviewRepository.GetByComplexIdAsync(complexId);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<IEnumerable<ReviewDto>> GetMyReviewsAsync(int customerId)
        {
            var reviews = await _reviewRepository.GetByCustomerIdAsync(customerId);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<double> GetAverageRatingByFieldIdAsync(int fieldId)
        {
            return await _reviewRepository.GetAverageRatingByFieldIdAsync(fieldId);
        }

        public async Task<double> GetAverageRatingByComplexIdAsync(int complexId)
        {
            return await _reviewRepository.GetAverageRatingByComplexIdAsync(complexId);
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

            // 5. Tạo review (không cần set CustomerId, FieldId, ComplexId nữa - lấy từ Booking)
            var review = _mapper.Map<Review>(createReviewDto);
            review.CreatedAt = DateTime.UtcNow.AddHours(7);
            review.UpdatedAt = DateTime.UtcNow.AddHours(7);

            var created = await _reviewRepository.AddAsync(review);
            
            // Load navigation properties for mapping
            var reviewWithDetails = await _reviewRepository.GetByIdAsync(created.Id);
            return _mapper.Map<ReviewDto>(reviewWithDetails);
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
    }
}