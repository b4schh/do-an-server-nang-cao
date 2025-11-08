using AutoMapper;
using FootballField.API.Entities;
using FootballField.API.Dtos.Review;
using FootballField.API.Repositories.Interfaces;
using FootballField.API.Services.Interfaces;

namespace FootballField.API.Services.Implements
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
            var reviews = await _reviewRepository.GetAllAsync(r => !r.IsDeleted);
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

        public async Task<double> GetAverageRatingByFieldIdAsync(int fieldId)
        {
            return await _reviewRepository.GetAverageRatingByFieldIdAsync(fieldId);
        }

        public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto createReviewDto)
        {
            var booking = await _bookingRepository.GetByIdAsync(createReviewDto.BookingId);
            if (booking == null || !booking.IsCompleted)
            {
                throw new Exception("Cannot create review for a booking that is not completed.");
            }

            var review = _mapper.Map<Review>(createReviewDto);
            review.CreatedAt = DateTime.Now;
            review.UpdatedAt = DateTime.Now;

            var created = await _reviewRepository.AddAsync(review);
            return _mapper.Map<ReviewDto>(created);
        }

        public async Task UpdateReviewAsync(int id, UpdateReviewDto updateReviewDto)
        {
            var existingReview = await _reviewRepository.GetByIdAsync(id);
            if (existingReview == null)
                throw new Exception("Review not found");

            _mapper.Map(updateReviewDto, existingReview);
            existingReview.UpdatedAt = DateTime.Now;

            await _reviewRepository.UpdateAsync(existingReview);
        }

        public async Task SoftDeleteReviewAsync(int id)
        {
            await _reviewRepository.SoftDeleteAsync(id);
        }
    }
}
