using DoAn.Core.Application.Interfaces.Recommendation;
using DoAn.Core.Application.DTOs.Recommendation;
using DoAn.Core.Domain.Entities;
using DoAn.Core.Application.Interfaces.Field;
using DoAn.Core.Application.Interfaces.Booking;
using DoAn.Core.Application.Interfaces.Review;
using DoAn.Core.Application.Interfaces.Complex;

namespace DoAn.Core.Application.Services.Recommendation;

public class RecommendationService : IRecommendationService
{
    private readonly IFieldRepository _fieldRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IComplexRepository _complexRepository;

    public RecommendationService(
        IFieldRepository fieldRepository,
        IBookingRepository bookingRepository,
        IReviewRepository reviewRepository,
        IComplexRepository complexRepository)
    {
        _fieldRepository = fieldRepository;
        _bookingRepository = bookingRepository;
        _reviewRepository = reviewRepository;
        _complexRepository = complexRepository;
    }

    #region Item-to-Item Recommendation (Sân tương tự)

    /// <summary>
    /// STRATEGY 1: Item-to-Item Similarity
    /// Gợi ý sân tương tự dựa trên vector đặc trưng của sân
    /// </summary>
    public async Task<RecommendationResponse> GetSimilarFieldsAsync(int fieldId, int topK = 10)
    {
        // Lấy sân hiện tại với đầy đủ thông tin
        var currentField = await _fieldRepository.GetFieldWithDetailsForRecommendationAsync(fieldId);

        if (currentField == null)
        {
            return new RecommendationResponse
            {
                RecommendationType = "item-to-item",
                Fields = new List<FieldRecommendationDto>(),
                Message = "Không tìm thấy sân"
            };
        }

        // Vector hóa sân hiện tại
        var currentVector = VectorizeField(currentField);

        // Lấy tất cả sân khác (cùng tỉnh để tăng độ chính xác)
        var otherFields = (await _fieldRepository.GetAllActiveFieldsWithDetailsAsync(currentField.Complex.Province))
            .Where(f => f.Id != fieldId)
            .ToList();

        // Tính similarity cho từng sân
        var similarities = new List<(FieldEntity field, double score, int randomOrder)>();
        var random = new Random();
        
        foreach (var field in otherFields)
        {
            var fieldVector = VectorizeField(field);
            var similarity = CosineSimilarity(currentVector, fieldVector);
            similarities.Add((field, similarity, random.Next()));
        }

        // Lấy Top-K sân có similarity cao nhất
        // Nếu similarity bằng nhau, shuffle ngẫu nhiên thay vì theo ID
        var topFields = similarities
            .OrderByDescending(x => x.score)
            .ThenBy(x => x.randomOrder) // Thêm random tiebreaker
            .Take(topK)
            .Select(x => MapToRecommendationDto(x.field, x.score))
            .ToList();

        return new RecommendationResponse
        {
            RecommendationType = "item-to-item",
            Fields = topFields,
            Message = $"Tìm thấy {topFields.Count} sân tương tự"
        };
    }

    #endregion

    #region Location-based + Popularity Recommendation (User mới)

    /// <summary>
    /// STRATEGY 2: Location-based + Popularity
    /// Gợi ý sân cho user mới dựa trên vị trí và độ phổ biến
    /// </summary>
    public async Task<RecommendationResponse> GetRecommendationsForNewUserAsync(
        string? province, 
        string? ward, 
        int topK = 10)
    {
        // Lấy tất cả sân active
        var fields = (await _fieldRepository.GetAllActiveFieldsWithDetailsAsync(province)).ToList();

        // Lọc theo ward nếu có
        if (!string.IsNullOrEmpty(ward))
        {
            fields = fields.Where(f => f.Complex.Ward == ward).ToList();
        }

        if (!fields.Any())
        {
            return new RecommendationResponse
            {
                RecommendationType = "location-popularity",
                Fields = new List<FieldRecommendationDto>(),
                Message = "Không tìm thấy sân phù hợp"
            };
        }

        // Tính popularity score cho mỗi sân
        var random = new Random();
        var scoredFields = fields.Select(field => 
        {
            var bookingCount = field.Bookings.Count;
            var avgRating = GetAverageRating(field.ComplexId);
            
            // Công thức popularity: 0.6 * normalized_booking + 0.4 * normalized_rating
            var maxBooking = fields.Max(f => f.Bookings.Count);
            var normalizedBooking = maxBooking > 0 ? (double)bookingCount / maxBooking : 0;
            var normalizedRating = avgRating / 5.0;
            
            var popularityScore = 0.6 * normalizedBooking + 0.4 * normalizedRating;
            
            return (field, popularityScore, randomOrder: random.Next());
        })
        .OrderByDescending(x => x.popularityScore)
        .ThenByDescending(x => x.randomOrder) // Thêm randomness khi score bằng nhau
        .Take(topK)
        .Select(x => MapToRecommendationDto(x.field, x.popularityScore))
        .ToList();

        return new RecommendationResponse
        {
            RecommendationType = "location-popularity",
            Fields = scoredFields,
            Message = $"Gợi ý {scoredFields.Count} sân phổ biến"
        };
    }

    #endregion

    #region Content-based User Recommendation (User có lịch sử)

    /// <summary>
    /// STRATEGY 3: Content-Based Filtering
    /// Gợi ý cá nhân hóa dựa trên lịch sử booking của user
    /// </summary>
    public async Task<RecommendationResponse> GetPersonalizedRecommendationsAsync(
        int userId, 
        int topK = 10, 
        string? province = null)
    {
        // Lấy lịch sử booking của user
        var userBookings = (await _bookingRepository.GetUserBookingHistoryAsync(userId)).ToList();

        if (!userBookings.Any())
        {
            // Fallback to location-based nếu user chưa có booking
            return await GetRecommendationsForNewUserAsync(province, null, topK);
        }

        // Tạo user vector từ trung bình các sân đã đặt
        var userVector = CreateUserVector(userBookings.Select(b => b.Field).ToList());

        // Lấy tất cả sân chưa từng đặt
        var bookedFieldIds = userBookings.Select(b => b.FieldId).ToHashSet();
        
        var candidateFields = (await _fieldRepository.GetAllActiveFieldsWithDetailsAsync(province))
            .Where(f => !bookedFieldIds.Contains(f.Id))
            .ToList();

        if (!candidateFields.Any())
        {
            return new RecommendationResponse
            {
                RecommendationType = "content-based-user",
                Fields = new List<FieldRecommendationDto>(),
                Message = "Không có sân mới để gợi ý"
            };
        }

        // Tính similarity giữa user vector và từng sân
        var random = new Random();
        var recommendations = candidateFields
            .Select(field => 
            {
                var fieldVector = VectorizeField(field);
                var similarity = CosineSimilarity(userVector, fieldVector);
                return (field, similarity, randomOrder: random.Next());
            })
            .OrderByDescending(x => x.similarity)
            .ThenBy(x => x.randomOrder) // Random tiebreaker
            .Take(topK)
            .Select(x => MapToRecommendationDto(x.field, x.similarity))
            .ToList();

        return new RecommendationResponse
        {
            RecommendationType = "content-based-user",
            Fields = recommendations,
            Message = $"Gợi ý {recommendations.Count} sân phù hợp với bạn"
        };
    }

    #endregion

    #region Helper Methods - Vector Operations

    /// <summary>
    /// Vector hóa sân bóng thành feature vector
    /// Vector = [field_size, avg_price, surface_natural, surface_artificial, has_bookings, popularity_score]
    /// </summary>
    private double[] VectorizeField(FieldEntity field)
    {
        // Feature 1: Field Size (chuẩn hóa: 5 -> 0.5, 7 -> 0.7, 11 -> 1.0)
        var fieldSizeValue = field.FieldSize?.ToLower() switch
        {
            "5" => 0.5,
            "7" => 0.7,
            "11" => 1.0,
            _ => 0.5
        };

        // Feature 2: Average Price (chuẩn hóa 0-1)
        var avgPrice = field.TimeSlots.Any() 
            ? (double)field.TimeSlots.Average(ts => ts.Price) 
            : 0;
        var normalizedPrice = Math.Min(avgPrice / 1000000, 1.0); // Max 1 triệu

        // Feature 3: Surface Type (one-hot encoding)
        var surfaceNatural = field.SurfaceType?.ToLower().Contains("tự nhiên") == true ? 1.0 : 0.0;
        var surfaceArtificial = field.SurfaceType?.ToLower().Contains("nhân tạo") == true ? 1.0 : 0.0;

        // Feature 4: Has bookings (indicator)
        var hasBookings = field.Bookings.Any() ? 1.0 : 0.0;

        // Feature 5: Popularity score (more nuanced)
        var completedBookings = field.Bookings.Count(b => b.BookingStatus == BookingStatus.Completed);
        var normalizedBooking = Math.Min(completedBookings / 50.0, 1.0); // Scale to 50 bookings

        // Feature 6: Price tier (thêm diversity)
        var priceTier = avgPrice switch
        {
            < 200000 => 0.33,  // Giá rẻ
            < 400000 => 0.67,  // Giá trung
            _ => 1.0           // Giá cao
        };

        return new double[] 
        { 
            fieldSizeValue, 
            normalizedPrice, 
            surfaceNatural, 
            surfaceArtificial, 
            hasBookings,
            normalizedBooking,
            priceTier
        };
    }

    /// <summary>
    /// Tạo user vector từ trung bình các sân đã đặt
    /// </summary>
    private double[] CreateUserVector(List<FieldEntity> bookedFields)
    {
        if (!bookedFields.Any())
            return new double[7]; // Thay đổi từ 6 thành 7

        var vectors = bookedFields.Select(VectorizeField).ToList();
        var dimension = vectors[0].Length;
        var userVector = new double[dimension];

        for (int i = 0; i < dimension; i++)
        {
            userVector[i] = vectors.Average(v => v[i]);
        }

        return userVector;
    }

    /// <summary>
    /// Tính Cosine Similarity giữa 2 vector
    /// </summary>
    private double CosineSimilarity(double[] vectorA, double[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            return 0;

        var dotProduct = 0.0;
        var magnitudeA = 0.0;
        var magnitudeB = 0.0;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        if (magnitudeA == 0 || magnitudeB == 0)
            return 0;

        return dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
    }

    #endregion

    #region Helper Methods - Data Mapping

    private FieldRecommendationDto MapToRecommendationDto(FieldEntity field, double score)
    {
        var avgPrice = field.TimeSlots.Any() 
            ? field.TimeSlots.Average(ts => ts.Price) 
            : 0;

        var avgRating = GetAverageRating(field.ComplexId);
        var bookingCount = field.Bookings.Count(b => b.BookingStatus == BookingStatus.Completed);
        
        var imageUrl = field.Complex.ComplexImages?.FirstOrDefault()?.ImageUrl;

        return new FieldRecommendationDto
        {
            Id = field.Id,
            Name = field.Name,
            ComplexId = field.ComplexId,
            ComplexName = field.Complex.Name,
            Province = field.Complex.Province,
            Ward = field.Complex.Ward,
            Price = avgPrice,
            FieldSize = int.TryParse(field.FieldSize, out var size) ? size : 0,
            SurfaceType = field.SurfaceType,
            AverageRating = avgRating,
            BookingCount = bookingCount,
            SimilarityScore = Math.Round(score, 3),
            ImageUrl = imageUrl,
            IsActive = field.IsActive
        };
    }

    private double GetAverageRating(int complexId)
    {
        // Get all bookings for this complex, then get reviews
        var reviews = _reviewRepository.GetAllAsync(r => 
            !r.IsDeleted && 
            r.Booking.Field.ComplexId == complexId
        ).Result;
        
        return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
    }

    #endregion
}
